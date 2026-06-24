namespace ADE.Core.Models;

public class CaseMetadata
{
    public string CaseId { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public string BoNumber { get; set; } = string.Empty;

    public string ProcedureNumber { get; set; } = string.Empty;

    public string OfficerName { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string ComputerName { get; set; } = string.Empty;

    public string WindowsUser { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}