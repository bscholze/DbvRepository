using System;
using RepositoryWrapper;

namespace DbvCmd
{
    public class DbvEntry : IIdentifiable 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public byte[] Data { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedTms { get; set; }
    }
}