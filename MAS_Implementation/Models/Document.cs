using System.ComponentModel.DataAnnotations;
using System.IO;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

public class Document
{
    [Key]
    public int Id { get; set; }

    public string Filename { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; 
    public DocumentType Type { get; set; } 
    public DateTime UploadedAt { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? ValidUntil { get; set; }

    public Document(string filename, string filePath,
        DocumentType type, string? issuingAuthority = null, DateTime? validUntil = null)
    {
        Filename = filename;
        FilePath = filePath;
        ContentType = Path.GetExtension(filePath).TrimStart('.');
        Type = type;
        UploadedAt = DateTime.Now;
        IssuingAuthority = issuingAuthority;
        ValidUntil = validUntil;
    }
    
    protected Document() { }
}