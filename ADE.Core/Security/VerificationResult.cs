namespace ADE.Core.Security;

public class VerificationResult
{
    public bool Success { get; set; }

    public List<string> Messages { get; set; }
        = new();
}