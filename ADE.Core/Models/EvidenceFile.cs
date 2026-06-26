namespace ADE.Core.Models;

public class EvidenceFile
{
    public string FileName { get; set; } = string.Empty;

    public string OriginalPath { get; set; } = string.Empty;

    public string ImportedPath { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;
    public string ThumbnailRelativePath { get; set; } = "";
    public string EvidenceType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string Sha256 { get; set; } = string.Empty;

    public string Sha512 { get; set; } = string.Empty;

    public DateTime CollectedAtUtc { get; set; }

    public CollectionMethodType CollectionMethod { get; set; }
        = CollectionMethodType.Unknown;

    public EvidenceSourceType SourceType { get; set; }
        = EvidenceSourceType.Unknown;

    public string SourceDescription { get; set; } = "";

    /// <summary>
    /// Indica se o arquivo foi excluído durante a coleta
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data e hora da exclusão (se aplicável)
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }
}