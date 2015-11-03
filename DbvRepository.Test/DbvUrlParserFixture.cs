using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace DbvRepository.Test
{

    public class DbvRequestCreator : IWebRequestCreate
    {
        private DbvItemRepositoryLocator _repositoryLocator;

        public DbvRequestCreator(DbvItemRepositoryLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public WebRequest Create(Uri Url)
        {
            return new DbvWebRequest(_repositoryLocator.Repository, Url);
        }
    }


    public class DbvWebRequest : WebRequest
    {
        private DbvItemRepository _repository;
        private string _method;
        private ICredentials _Credentials;
        private long _dwdwContentLength;
        private string _szContentType;
        private IWebProxy _proxy;
        private MemoryStream _requestStream;
        private Uri _requestUri;


        public override String Method
        {
            get { return _method; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(paramName: "Method");
                _method = value;

            }
        }

        public override ICredentials Credentials
        {
            get { return _Credentials; }
            set { _Credentials = value; }
        }

        public override string ConnectionGroupName
        {
            /* override */
            get { throw new NotSupportedException(); }
            /* override */
            set { throw new NotSupportedException(); }
        }

        public override long ContentLength
        {
            /* override */
            get { return _dwdwContentLength; }
            /* override */
            set { _dwdwContentLength = value; }
        }

        public override string ContentType
        {
            /* override */
            get { return _szContentType; }
            /* override */
            set { _szContentType = value; }
        }


        public override IWebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        public override Stream GetRequestStream()
        {
            if (_requestStream == null)
                _requestStream = new MemoryStream();
            else
                throw new InvalidOperationException("request stream already retrieved");

            return _requestStream;
        }

        public DbvWebRequest(DbvItemRepository repository, Uri url)
            : base()
        {
            // do whatever initialization is required here
            if (url.Scheme != "dbv") // This class is only for ftp urls
                throw new NotSupportedException("This protocol is not supported");

            _requestUri = url;
            _repository = repository;
            _method = "checkout"; // default is to retrieve a file
        }

        public override WebResponse GetResponse()
        {
            var item = _repository.Checkout(_requestUri.LocalPath, _requestUri.Port);
            if (item == null)
                throw new WebException("Item not found", WebExceptionStatus.ProtocolError);
            return new DbvWebResponse(item);
        }


    }


    public class DbvWebResponse : WebResponse
    {
        private string _contentType;
        private Stream _responseStream;
        private WebHeaderCollection _headerCollection;


        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int FindMimeFromData(IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved);


        /*
        public static string GetMimeFromBuffer(byte[] content)
        {

            byte[] buffer = new byte[256];
            
            Array.Copy(content, buffer, Math.Min(content.Length, 256));


            try
            {
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mime;
            }
            catch (Exception e)
            {
                return "unknown/unknown";
            }
        }
       */

        public string MimeTypeFrom(byte[] dataBytes, string mimeProposed)
        {
            if (dataBytes == null)
                throw new ArgumentNullException("dataBytes");
            string mimeRet = String.Empty;
            IntPtr suggestPtr = IntPtr.Zero, filePtr = IntPtr.Zero, outPtr = IntPtr.Zero;
            if (mimeProposed != null && mimeProposed.Length > 0)
            {
                //suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
                mimeRet = mimeProposed;
            }
            int ret = FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length, mimeProposed, 0, out outPtr, 0);
            if (ret == 0 && outPtr != IntPtr.Zero)
            {
                //todo: this leaks memory outPtr must be freed
                return Marshal.PtrToStringUni(outPtr);
            }
            return mimeRet;
        }




        internal DbvWebResponse(DbvItem item)
        {
            _contentType = MimeTypeFrom(item.Content, "text/plain");
            _responseStream = new MemoryStream(item.Content);
            _headerCollection = new WebHeaderCollection();
            _headerCollection.Add("X-DBV-Name", item.Name);
            _headerCollection.Add("X-DBV-Revision", item.Revision.ToString());
            _headerCollection.Add("X-DBV-Timestamp", item.Timestamp.ToUniversalTime().ToString("R"));
            _headerCollection.Add("X-DBV-Comments", item.Comments);
            _headerCollection.Add("X-DBV-Author", item.Author);
        }

        public override String ContentType
        {
            get { return _contentType; }
            set { throw new NotSupportedException("This property cannot be set"); }
        }

        public override Stream GetResponseStream()
        {
            if (_responseStream == null)
                throw new ApplicationException("No response stream for this kind of method");

            return _responseStream;
        }

        public override long ContentLength
        {
            get { return _responseStream.Length; } 
            set { throw new NotSupportedException("This property cannot be set"); }
        }

        // maybe this should support version headers ? like X-DBV-Author, X-DBV-Respository, X....
        public override bool SupportsHeaders
        {
            get { return true; }
        }

        public override WebHeaderCollection Headers
        {
            get { return _headerCollection; }
        }
    }


    public class DbvStyleUriParser : GenericUriParser
    {
        public DbvStyleUriParser()
            : base(GenericUriParserOptions.NoFragment |
                   GenericUriParserOptions.NoQuery |
                   GenericUriParserOptions.NoUserInfo |
                   GenericUriParserOptions.DontConvertPathBackslashes |
                   GenericUriParserOptions.AllowEmptyAuthority)
        {
        }
    }

    public class DbvItemRepositoryLocator
    {
        public DbvItemRepository Repository { get; set; }
    }


    [TestFixture]
    public class DbvUrlParserFixture
    {

        public DbvItemRepositoryLocator RepositoryLocator;
        
        [TestFixtureSetUp]
        public void RegiserUriParser()
        {
            UriParser.Register(new DbvStyleUriParser(), "dbv", 0);
            RepositoryLocator = new DbvItemRepositoryLocator();
            WebRequest.RegisterPrefix("dbv", new DbvRequestCreator(RepositoryLocator));
        }


        [Test]
        public void ParseDbvUrl()
        {


            
            var url = "dbv://rules:25/test%20with%20spaces/dummy.csv";

            Uri parsed;
            Assert.IsTrue(Uri.TryCreate(url, UriKind.Absolute, out parsed));

            Assert.AreEqual("dbv", parsed.Scheme);
            Assert.AreEqual("rules", parsed.Host);
            Assert.AreEqual(25, parsed.Port);
            Assert.AreEqual("/test with spaces/dummy.csv", parsed.LocalPath);

        }


        [Test]
        public void TestDbvUrlLatestOnly()
        {
            var url = "dbv://rules/test%20with%20spaces/dummy.csv";

            Uri parsed;
            Assert.IsTrue(Uri.TryCreate(url, UriKind.Absolute, out parsed));

            Assert.AreEqual("dbv", parsed.Scheme);
            Assert.AreEqual("rules", parsed.Host);
            Assert.AreEqual(0, parsed.Port);
            Assert.AreEqual("/test with spaces/dummy.csv", parsed.LocalPath);

        }


        [Test]
        public void TestDbvUrlWithoutRepository()
        {
            var url = "dbv:///test%20with%20spaces/dummy.csv";

            Uri parsed;
            Assert.IsTrue(Uri.TryCreate(url, UriKind.Absolute, out parsed));

            Assert.AreEqual("dbv", parsed.Scheme);
            Assert.AreEqual(string.Empty, parsed.Host);
            Assert.AreEqual(0, parsed.Port);
            Assert.AreEqual("/test with spaces/dummy.csv", parsed.LocalPath);

        }

        [Test]
        public void RegisterWebRequest()
        {
            var url = "dbv://rules/test with space/test.pdf";


            var repository = new DbvItemRepository(new DbvItemMemoryStore());
            repository.Checkin("/test/dummy.csv", Encoding.UTF8.GetBytes("HELLO WORLD"), "Checking", "john doe");

            repository.Checkin("/test with space/test.pdf", Encoding.ASCII.GetBytes("%PDF-"), "Checkin pdf", "jane doe");

            RepositoryLocator.Repository = repository;
            



            var r = WebRequest.Create(url);

            Assert.IsInstanceOf<DbvWebRequest>(r);

            // and the response
            var resp = r.GetResponse();
            Assert.IsInstanceOf<DbvWebResponse>(resp);

            Console.Out.WriteLine(resp.ContentLength);

            var data = new byte[resp.ContentLength];
            resp.GetResponseStream().Read(data, 0, (int) resp.ContentLength);

            var result = Encoding.UTF8.GetString(data);

            Console.Out.WriteLine("result = {0}", result);
            Console.Out.WriteLine("resp.ContentType = {0}", resp.ContentType);
            //Assert.AreEqual("HELLO WORLD", result);

        }


        [Test]
        public void GetDbvItemViaWebClient()
        {
            // get specific version
            var url = "dbv://rules:2/test%20with%20spaces/dummy.csv";

            var repository = new DbvItemRepository(new DbvItemMemoryStore());
            repository.Checkin("/test with spaces/dummy.csv", Encoding.UTF8.GetBytes("first checkin"), "Checking", "john doe");
            repository.Checkin("/test with spaces/dummy.csv", Encoding.UTF8.GetBytes("HELLO WORLD"), "Checking", "john doe");
            repository.Checkin("/test with spaces/dummy.csv", Encoding.UTF8.GetBytes("third checkin"), "Checking", "john doe");

            RepositoryLocator.Repository = repository;

            //WebRequest.RegisterPrefix("dbv", new DbvRequestCreator(RepositoryLocator));


            var wc = new WebClient();
            var result = wc.DownloadString(url);
            
            Console.Out.WriteLine("result = {0}", result);
            Assert.AreEqual("HELLO WORLD", result);

            Assert.Contains("X-DBV-Revision",wc.ResponseHeaders.AllKeys);
            Console.Out.WriteLine("wc.ResponseHeaders['X-DBV-Revision'] = {0}", wc.ResponseHeaders["X-DBV-Revision"]);
            Console.Out.WriteLine("wc.ResponseHeaders['X-DBV-Comments'] = {0}", wc.ResponseHeaders["X-DBV-Comments"]);
            Console.Out.WriteLine("wc.ResponseHeaders['X-DBV-Name'] = {0}", wc.ResponseHeaders["X-DBV-Name"]);
            Console.Out.WriteLine("wc.ResponseHeaders['X-DBV-Timestamp'] = {0}", wc.ResponseHeaders["X-DBV-Timestamp"]);
            Console.Out.WriteLine("wc.ResponseHeaders['Content-Type'] = {0}", wc.ResponseHeaders["Content-Type"]);


            // NO ASYNC IMPLEMENTATION
            /*
            var t = wc.DownloadStringTaskAsync(url);
            Console.Out.WriteLine("t.Result = {0}", t.Result);
            */

        }

        [Test]
        [ExpectedException(typeof(WebException))]
        public void GetDbvItemViaWebClientUnknownItem()
        {
            // get specific version
            var url = "dbv://rules:2/test/dummy.csv";

            var repository = new DbvItemRepository(new DbvItemMemoryStore());
            repository.Checkin("/test with spaces/dummy.csv", Encoding.UTF8.GetBytes("first checkin"), "Checking", "john doe");
            repository.Checkin("/test with spaces/dummy.csv", Encoding.UTF8.GetBytes("HELLO WORLD"), "Checking", "john doe");
            repository.Checkin("/test with spaces/dummy.csv", Encoding.UTF8.GetBytes("third checkin"), "Checking", "john doe");

            RepositoryLocator.Repository = repository;

            //WebRequest.RegisterPrefix("dbv", new DbvRequestCreator(RepositoryLocator));


            var wc = new WebClient();
            var result = wc.DownloadString(url);


        }

    }


}