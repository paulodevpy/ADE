namespace ADE.Core.Audit;

public class AuditVerificationResult
{
    public bool Success { get; set; }

    public List<string> Messages { get; set; }
        = new();
}