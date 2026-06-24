namespace ADE.Core.Models;

public class CollectionRecord
{
    public string CaseId { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public List<EvidenceFile> EvidenceFiles { get; set; }
        = new();
}