using System;

namespace ConcordCloud.Core.DTOs;
public class FileDto
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool HasActiveShare { get; set; }
}

public class FileUploadResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public FileDto? File { get; set; }
}

public class FileShareDto
{
    public Guid FileId { get; set; }
    public int ExpirationDays { get; set; } = 7;
}

public class FileShareResultDto
{
    public string? ShareCode { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? FileName { get; set; }
}

public class FileRenameDto
{
    public Guid FileId { get; set; }
    public string? NewFileName { get; set; }
}
