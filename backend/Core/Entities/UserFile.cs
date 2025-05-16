using System;

namespace ConcordCloud.Core.Entities
{
    public class UserFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string StoragePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid OwnerId { get; set; }
        public virtual User Owner { get; set; }
        public virtual FileShare Share { get; set; }
    }
} 