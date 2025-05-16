using System;

namespace ConcordCloud.Core.Entities;
public class ShareFile
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public virtual UserFile File { get; set; }
    public string ShareCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive => DateTime.UtcNow < ExpiresAt;
}
