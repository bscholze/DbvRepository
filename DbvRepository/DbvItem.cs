using System;

namespace DbvRepository
{
    public class DbvItemBase
    {
        public int Revision { get; internal set; }
        public string Name { get; internal set; }
        public string Author { get; internal set; }
        public string Comments { get; internal set; }
        public char Deleted { get; internal set; }
        public DateTime Timestamp { get; internal set; }

        public override string ToString()
        {
            return string.Format("Revision: {0}, Name: {1}, Author: {2}, Comments: {3}, Deleted: {4}, Timestamp: {5}", Revision, Name, Author, Comments, Deleted, Timestamp);
        }
    }

    public class DbvItem : DbvItemBase
    {
        public byte[] Content { get; internal set; }
    }

}