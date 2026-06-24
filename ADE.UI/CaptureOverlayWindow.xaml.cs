using System.Windows;
using System.IO;
using ADE.Core.Audit;
using ADE.Core.Evidence;
using ADE.Core.Security;
using ADE.Core.Models;

namespace ADE.UI;

public partial class CaptureOverlayWindow : Window
{
    private readonly string _casePath;
    private readonly string _captureType;
    private readonly int? _monitorIndex;
    private readonly nint? _windowHandle;
    private readonly Action<EvidenceFile>? _onEvidenceCaptured;
    private readonly Action<string>? _onStatusUpdate;

    public CaptureOverlayWindow(
        string casePath,
        string captureType,
        int? monitorIndex = null,
        nint? windowHandle = null,
        Action<EvidenceFile>? onEvidenceCaptured = null,
        Action<string>? onStatusUpdate = null)
    {
        InitializeComponent();
        _casePath = casePath;
        _captureType = captureType;
        _monitorIndex = monitorIndex;
        _windowHandle = windowHandle;
        _onEvidenceCaptured = onEvidenceCaptured;
        _onStatusUpdate = onStatusUpdate;

        // Ajustar título baseado no tipo de captura
        Title = captureType switch
        {
            "SCREEN" => "Captura de Tela",
            "MONITOR" => $"Captura de Monitor {(monitorIndex ?? 0) + 1}",
            "WINDOW" => "Captura de Janela",
            _ => "Captura"
        };

        Loaded += (s, e) =>
        {
            // Captura automática ao carregar a janela
            Capture_Click(this, new RoutedEventArgs());
        };
    }

    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        string evidencias = Path.Combine(_casePath, "evidencias");
        string arquivo = string.Empty;

        try
        {
            // A janela já está invisível, mas esconde explicitamente para garantir
            this.Hide();
            System.Threading.Thread.Sleep(200);

            switch (_captureType)
            {
                case "SCREEN":
                    arquivo = ADE.Capture.ScreenCaptureManager.Capture(evidencias);
                    break;
                case "MONITOR":
                    arquivo = ADE.Capture.ScreenCaptureManager.Capture(evidencias, _monitorIndex ?? 0);
                    break;
                case "WINDOW":
                    if (_windowHandle.HasValue)
                    {
                        arquivo = ADE.Capture.WindowCaptureManager.Capture(_windowHandle.Value, evidencias);
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(arquivo))
            {
                var info = new FileInfo(arquivo);
                var evidence = CreateEvidence(info, arquivo, "CAPTURE");
                _onEvidenceCaptured?.Invoke(evidence);
                _onStatusUpdate?.Invoke("CAPTURA REALIZADA".ToUpper());

                AuditService.AppendEvent(
                    Path.Combine(_casePath, "logs", "audit.jsonl"),
                    $"{_captureType}_CAPTURE",
                    evidence.Sha256);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao capturar: {ex.Message}", "Erro");
        }
        finally
        {
            // Fecha a janela automaticamente após a captura
            this.Close();
        }
    }

    private EvidenceFile CreateEvidence(FileInfo info, string arquivo, string methodSuffix)
    {
        return new EvidenceFile
        {
            FileName = info.Name,
            OriginalPath = arquivo,
            ImportedPath = arquivo,
            RelativePath = Path.Combine("evidencias", info.Name),
            SizeBytes = info.Length,
            EvidenceType = info.Extension,
            Sha256 = IntegrityManager.CalculateSha256(arquivo),
            Sha512 = IntegrityManager.CalculateSha512(arquivo),
            CollectedAtUtc = DateTime.UtcNow,
            CollectionMethod = $"{_captureType}_{methodSuffix}"
        };
    }
}
