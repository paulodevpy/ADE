using ADE.Core.Models;

namespace ADE.Core.Manifest;

public class ManifestModel
{
    public string CaseId { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public List<EvidenceFile> EvidenceFiles { get; set; }
        = new();
}