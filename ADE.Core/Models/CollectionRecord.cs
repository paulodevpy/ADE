namespace ADE.Core.Models;
using ADE.Core.Collection;

public class CollectionRecord
{
    
    public CollectionState CollectionState { get; set; }
    = CollectionState.None;
    
    public string CaseId { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public List<EvidenceFile> EvidenceFiles { get; set; }
        = new();
    
    /// <summary>
    /// Data e hora quando a coleta foi finalizada
    /// </summary>
    public DateTime? FinalizedAtUtc { get; set; }

    /// <summary>
    /// Data e hora quando o relatório foi gerado
    /// </summary>
    public DateTime? ReportGeneratedAtUtc { get; set; }
}