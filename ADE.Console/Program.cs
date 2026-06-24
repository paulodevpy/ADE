using ADE.Core.Security;
using ADE.Core.Cases;
using ADE.Core.Audit;

Console.WriteLine();
Console.WriteLine("====================================");
Console.WriteLine(" ATA DIGITAL DE EVIDÊNCIAS - CONSOLE");
Console.WriteLine("====================================");
Console.WriteLine();

if (args.Length == 0)
{
    MostrarAjuda();
    return;
}

string comando =
    args[0].ToLowerInvariant();

switch (comando)
{
    case "verify":
        ExecutarVerificacao(args);
        break;

    case "export":
        ExecutarExportacao(args);
        break;

    default:
        MostrarAjuda();
        break;
}

static void ExecutarVerificacao(
    string[] args)
{
    string caseFolder;

    if (args.Length < 2)
    {
        Console.WriteLine();
        Console.Write("Informe o caminho do caso: ");

        caseFolder =
            Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(caseFolder))
            return;
    }
    else
    {
        caseFolder = args[1];
    }

    if (!Directory.Exists(caseFolder))
    {
        Console.WriteLine(
            "Diretório não encontrado.");

        return;
    }

    Console.WriteLine();
    Console.WriteLine(
        $"Validando: {caseFolder}");
    Console.WriteLine();

    var result =
        IntegrityVerifier.Verify(
            caseFolder);

    foreach (var msg in result.Messages)
    {
        Console.WriteLine(msg);
    }

    Console.WriteLine();

    string auditFile =
        Path.Combine(
            caseFolder,
            "logs",
            "audit.jsonl");

    var auditResult =
        AuditChainVerifier.Verify(
            auditFile);

    foreach (var msg in auditResult.Messages)
    {
        Console.WriteLine(msg);
    }

    Console.WriteLine();

    Console.WriteLine(
        result.Success &&
        auditResult.Success
            ? "STATUS: ÍNTEGRO"
            : "STATUS: FALHA DE INTEGRIDADE");
}

static void ExecutarExportacao(
    string[] args)
{
    string caseFolder;

    if (args.Length < 2)
    {
        Console.WriteLine();
        Console.Write("Informe o caminho do caso: ");

        caseFolder =
            Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(caseFolder))
            return;
    }
    else
    {
        caseFolder = args[1];
    }

    if (!Directory.Exists(caseFolder))
    {
        Console.WriteLine(
            "Diretório não encontrado.");

        return;
    }

    string zipFile =
        Path.Combine(
            Directory.GetParent(caseFolder)!.FullName,
            Path.GetFileName(caseFolder) + ".zip");

    string zip =
        CaseExporter.Export(
            caseFolder,
            zipFile);

    Console.WriteLine();
    Console.WriteLine("ZIP criado:");
    Console.WriteLine(zipFile);

    Console.WriteLine();
    Console.WriteLine("Arquivos de integridade:");

    Console.WriteLine(
        zipFile + ".sha256");

    Console.WriteLine(
        zipFile + ".sha512");
}

static void MostrarAjuda()
{
    Console.WriteLine(
        "Comandos disponíveis:");
    Console.WriteLine();

    Console.WriteLine(
        "verify <diretorio-do-caso>");

    Console.WriteLine(
        "  Valida a integridade do caso.");

    Console.WriteLine();

    Console.WriteLine(
        "export <diretorio-do-caso>");

    Console.WriteLine(
        "  Exporta o caso para ZIP.");

    Console.WriteLine();

    Console.WriteLine(
        "Exemplos:");

    Console.WriteLine(
        "dotnet run --project .\\ADE.Console verify");

    Console.WriteLine(
        "dotnet run --project .\\ADE.Console export");

    Console.WriteLine();
}