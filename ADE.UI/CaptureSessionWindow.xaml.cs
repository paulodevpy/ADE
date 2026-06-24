using System.IO;
using System.Windows;

using ADE.Core.Audit;
using ADE.Core.Models;
using ADE.Core.Security;
using ADE.Capture;

namespace ADE.UI;

/// <summary>
/// Barra flutuante de coleta. Permanece sobre as demais janelas
/// (Topmost) para permitir capturas de tela durante a interação com
/// a fonte da prova (ex.: WhatsApp Web), preservando a cadeia de
/// custódia via registro de auditoria a cada captura.
/// </summary>
public partial class CaptureSessionWindow : Window
{
    private readonly string? _casePath;

    /// <summary>
    /// Disparado a cada evidência capturada, permitindo que a janela
    /// principal atualize a lista e o manifesto.
    /// </summary>
    public event Action<EvidenceFile>? EvidenceCaptured;

    public CaptureSessionWindow()
    {
        InitializeComponent();
    }

    public CaptureSessionWindow(string casePath)
        : this()
    {
        _casePath = casePath;
    }

    private void BtnCapture_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_casePath is null)
        {
            MessageBox.Show(
                "Nenhuma coleta ativa.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        string evidencias =
            Path.Combine(
                _casePath,
                "evidencias");

        // Oculta a própria barra para não aparecer na captura.
        this.Hide();

        System.Threading.Thread.Sleep(400);

        string arquivo =
            ScreenCaptureManager.Capture(
                evidencias,
                0);

        this.Show();

        var info =
            new FileInfo(arquivo);

        var evidence =
            new EvidenceFile
            {
                FileName =
                    info.Name,

                OriginalPath =
                    arquivo,

                ImportedPath =
                    arquivo,

                RelativePath =
                    Path.Combine(
                        "evidencias",
                        info.Name),

                SizeBytes =
                    info.Length,

                EvidenceType =
                    info.Extension,

                Sha256 =
                    IntegrityManager
                        .CalculateSha256(arquivo),

                Sha512 =
                    IntegrityManager
                        .CalculateSha512(arquivo),

                CollectedAtUtc =
                    DateTime.UtcNow,

                CollectionMethod =
                    "CAPTURA_SESSAO"
            };

        AuditService.AppendEvent(
            Path.Combine(
                _casePath,
                "logs",
                "audit.jsonl"),
            "SESSION_CAPTURE",
            evidence.Sha256);

        EvidenceCaptured?.Invoke(evidence);
    }



    private void BtnFinish_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_casePath is not null)
        {
            AuditService.AppendEvent(
                Path.Combine(
                    _casePath,
                    "logs",
                    "audit.jsonl"),
                "SESSION_FINISHED",
                "Sessão de coleta encerrada.");
        }

        this.Close();
    }
}
