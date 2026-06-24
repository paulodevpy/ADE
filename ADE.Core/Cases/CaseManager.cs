namespace ADE.Core.Cases;

public class CaseManager
{
    public string CaseId { get; private set; } = string.Empty;

    public string RootPath { get; private set; } = string.Empty;

    public void CreateCase(string baseDirectory)
    {
        CaseId =
            $"ADE-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}";

        RootPath =
            Path.Combine(baseDirectory, CaseId);

        Directory.CreateDirectory(RootPath);

        Directory.CreateDirectory(
            Path.Combine(RootPath, "evidencias"));

        Directory.CreateDirectory(
            Path.Combine(RootPath, "logs"));

        Directory.CreateDirectory(
            Path.Combine(RootPath, "metadata"));

        Directory.CreateDirectory(
            Path.Combine(RootPath, "relatorio"));

        Directory.CreateDirectory(
            Path.Combine(RootPath, "integridade"));
    }
}