using ADE.Core.Audit;
using ADE.Core.Models;

namespace ADE.Reporting.Pdf;

public class ReportModel
{
    public string CaseId { get; set; } = "";

    public string UnitName { get; set; } = "";

    public string BoNumber { get; set; } = "";

    public string ProcedureNumber { get; set; } = "";

    public string OfficerName { get; set; } = "";

    public DateTime GeneratedAtUtc { get; set; }

    public string CaseFolder { get; set; } = "";

    public string MasterSha256 { get; set; } = "";

    public string MasterSha512 { get; set; } = "";

    public List<EvidenceFile> Evidences { get; set; }
        = new();

    public List<TimelineEntry> Timeline { get; set; }
        = new();
}