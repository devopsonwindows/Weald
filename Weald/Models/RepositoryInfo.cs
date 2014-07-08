using System;

namespace Weald.Models
{
    public class RepositoryInfo
    {
        public string Name;
        public int LatestRevision;
        public string LatestChangeUsername;
        public DateTime LatestChangeTimestamp;
        public int SizeInBytes;
        public string Url;
    }
}