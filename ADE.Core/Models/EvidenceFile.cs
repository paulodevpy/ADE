namespace ADE.Core.Models;

public class EvidenceFile
{
    public string FileName { get; set; } = string.Empty;

    public string OriginalPath { get; set; } = string.Empty;

    public string ImportedPath { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string EvidenceType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string Sha256 { get; set; } = string.Empty;

    public string Sha512 { get; set; } = string.Empty;

    public DateTime CollectedAtUtc { get; set; }

    public string CollectionMethod { get; set; }
        = "IMPORTAÇÃO";

    public string SourceDescription { get; set; }
        = "";
}