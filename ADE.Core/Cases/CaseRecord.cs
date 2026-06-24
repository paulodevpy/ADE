namespace ADE.Core.Cases;

public class CaseRecord
{
    public string CaseId { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public string OccurrenceNumber { get; set; } = string.Empty;

    public string ProcedureNumber { get; set; } = string.Empty;

    public string OfficerName { get; set; } = string.Empty;

    public string BadgeNumber { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}