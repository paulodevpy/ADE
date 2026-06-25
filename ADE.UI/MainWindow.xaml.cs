using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

using ADE.Core.Audit;
using ADE.Core.Cases;
using ADE.Core.Evidence;
using ADE.Core.Manifest;
using ADE.Core.Metadata;
using ADE.Core.Models;
using ADE.Core.Security;
using ADE.Capture;
using ADE.Reporting.Pdf;

namespace ADE.UI;

public partial class MainWindow : Window
{
    private string? _currentCasePath;
    private string? _currentCaseId;

    private readonly ObservableCollection<EvidenceFile>
        _evidences = new();

    private SystemTrayIcon? _systemTrayIcon;

    public MainWindow()
    {
        InitializeComponent();

        EvidenceGrid.ItemsSource =
            _evidences;

        _systemTrayIcon = new SystemTrayIcon(this);

        StateChanged += MainWindow_StateChanged;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            _systemTrayIcon?.HideWindow();
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            var result = MessageBox.Show(
                "Deseja minimizar para a bandeja do sistema ou fechar completamente?",
                "ADE",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                _systemTrayIcon?.Dispose();
            }
        }
        else
        {
            _systemTrayIcon?.Dispose();
        }
    }

    private void SetStatus(string status)
    {
        StatusTextBlock.Text = status?.ToUpper().Trim() ?? "AGUARDANDO";
    }

    private void NovaColeta_Click(
        object sender,
        RoutedEventArgs e)
    {
        // limpa completamente a sessão anterior

        _evidences.Clear();

        EvidenceGrid.Items.Refresh();

        SetStatus("CRIANDO CASO");

        var caseManager =
            new CaseManager();

        string destino =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop),
                "ADE_Coletas");

        caseManager.CreateCase(destino);

        _currentCasePath =
            caseManager.RootPath;

        _currentCaseId =
            caseManager.CaseId;

        CaseIdTextBlock.Text =
            $"Caso Atual: {_currentCaseId}";

        var metadata =
            new CaseMetadata
            {
                CaseId =
                    caseManager.CaseId,

                UnitName =
                    UnidadeTextBox.Text,

                BoNumber =
                    BoTextBox.Text,

                ProcedureNumber =
                    ProcedimentoTextBox.Text,

                OfficerName =
                    OfficerTextBox.Text,

                Notes =
                    ObservacoesTextBox.Text,

                ComputerName =
                    Environment.MachineName,

                WindowsUser =
                    Environment.UserName,

                CreatedAtUtc =
                    DateTime.UtcNow
            };

        new CaseMetadataManager()
            .Save(
                metadata,
                Path.Combine(
                    caseManager.RootPath,
                    "metadata"));

        var audit =
            new AuditChain();

        audit.AddEvent(
            "CASE_CREATED",
            caseManager.CaseId);

        audit.Save(
            Path.Combine(
                caseManager.RootPath,
                "logs",
                "audit.jsonl"));

        AtualizarManifesto();

        SetStatus("CASO EM ANDAMENTO");

        MessageBox.Show(
            $"Novo caso criado.\n\n{caseManager.CaseId}",
            "ADE",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void AdicionarEvidencia_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_currentCasePath is null)
            return;

        var dialog =
            new OpenFileDialog
            {
                Multiselect = true
            };

        if (dialog.ShowDialog() != true)
            return;

        var evidenceManager =
            new EvidenceManager();

        foreach (var file in dialog.FileNames)
        {
            var evidence =
                evidenceManager.ImportFile(
                    file,
                    Path.Combine(
                        _currentCasePath,
                        "evidencias"));

            _evidences.Add(
                evidence);

            SetStatus("EVIDÊNCIA ADICIONADA");

            AuditService.AppendEvent(
                Path.Combine(
                    _currentCasePath,
                    "logs",
                    "audit.jsonl"),
                "FILE_IMPORTED",
                evidence.Sha256);
        }

        AtualizarManifesto();
    }

    private void AtualizarManifesto()
    {
        if (_currentCasePath is null)
            return;

        if (_currentCaseId is null)
            return;

        var manifest =
            new ManifestModel
            {
                CaseId =
                    _currentCaseId,

                GeneratedAtUtc =
                    DateTime.UtcNow,

                EvidenceFiles =
                    _evidences.ToList(),
            };

        var manager =
            new ManifestManager();

        manager.SaveManifest(
            manifest,
            Path.Combine(
                _currentCasePath,
                "metadata"));

        MasterIntegrityManager.Generate(
            _currentCasePath);
    }

    public void CapturarTela_Click(
    object? sender,
    RoutedEventArgs? e)
    {
        if (_currentCasePath is null)
            return;

        var overlay = new CaptureOverlayWindow(
            _currentCasePath,
            "SCREEN",
            onEvidenceCaptured: evidence =>
            {
                _evidences.Add(evidence);
                AtualizarManifesto();
            },
            onStatusUpdate: status =>
            {
                SetStatus(status ?? "CAPTURA REALIZADA");
            });

        overlay.Show();
    }

    private void GerarRelatorio_Click(
        object sender,
        RoutedEventArgs e)
        {
        if (_currentCasePath is null)
            return;
        
        string reportFolder =
            Path.Combine(
                _currentCasePath,
                "relatorio");

        Directory.CreateDirectory(
            reportFolder);

        var model =
            new ReportModel
            {
                CaseId =
                    _currentCaseId ?? "",

                UnitName =
                    UnidadeTextBox.Text,

                BoNumber =
                    BoTextBox.Text,

                ProcedureNumber =
                    ProcedimentoTextBox.Text,

                OfficerName =
                    OfficerTextBox.Text,

                GeneratedAtUtc =
                    DateTime.UtcNow,

                Evidences =
                    _evidences.ToList(),

                CaseFolder =
                    _currentCasePath,

                MasterSha256 =
                    File.Exists(
                        Path.Combine(
                            _currentCasePath,
                            "integridade",
                            "master.sha256"))
                        ? File.ReadAllText(
                            Path.Combine(
                                _currentCasePath,
                                "integridade",
                                "master.sha256"))
                        : "",

                MasterSha512 =
                    File.Exists(
                        Path.Combine(
                            _currentCasePath,
                            "integridade",
                            "master.sha512"))
                        ? File.ReadAllText(
                            Path.Combine(
                                _currentCasePath,
                                "integridade",
                                "master.sha512"))
                        : ""
            };

        string pdfFile =
            Path.Combine(
                reportFolder,
                $"{_currentCaseId}.pdf");

        string auditFile =
            Path.Combine(
                _currentCasePath,
                "logs",
                "audit.jsonl");

        model.Timeline =
            TimelineService.Build(
                auditFile);

        PdfReportManager.Generate(
            model,
            pdfFile);


        string sha256 =
            IntegrityManager
        .CalculateSha256(
            pdfFile);

        string sha512 =
            IntegrityManager
                .CalculateSha512(
                    pdfFile);

        File.WriteAllText(
            pdfFile + ".sha256",
            sha256);

        File.WriteAllText(
            pdfFile + ".sha512",
            sha512);

        SetStatus("RELATÓRIO GERADO");

        AuditService.AppendEvent(
            Path.Combine(
                _currentCasePath,
                "logs",
                "audit.jsonl"),
            "PDF_GENERATED",
            pdfFile);

        MessageBox.Show(
            "Relatório gerado com sucesso.");
    }

    private void VerificarIntegridade_Click(
        object sender,
        RoutedEventArgs e)
        {
        if (_currentCasePath is null)
            return;

        var result =
            IntegrityVerifier.Verify(
                _currentCasePath);

        string message =
            string.Join(
                Environment.NewLine,
                result.Messages);

        AuditService.AppendEvent(
            Path.Combine(
                _currentCasePath,
                "logs",
                "audit.jsonl"),
            "INTEGRITY_CHECK",
            result.Success
                ? "SUCCESS"
                : "FAIL");

        if (result.Success)
        {
            SetStatus("ÍNTEGRO");
        }
        else
        {
            SetStatus("COMPROMETIDO");
        }

        MessageBox.Show(
            message,
            result.Success
                ? "Integridade OK"
                : "Integridade Comprometida");
    }

    private void ExportarTimeline_Click(
        object sender,
        RoutedEventArgs e)
        {
        if (_currentCasePath is null)
            return;

        string auditFile =
            Path.Combine(
                _currentCasePath,
                "logs",
                "audit.jsonl");

        string timelineFile =
            Path.Combine(
                _currentCasePath,
                "relatorio",
                "timeline.csv");

        Directory.CreateDirectory(
            Path.GetDirectoryName(
                timelineFile)!);

        var timeline =
            TimelineService.Build(
                auditFile);

        using var writer =
            new StreamWriter(
                timelineFile);

        writer.WriteLine(
            "TimestampUtc;Evento;Descricao");

        foreach (var item in timeline)
        {
            writer.WriteLine(
                $"{item.TimestampUtc:o};" +
                $"{item.EventType};" +
                $"{item.Description}");
        }

        AuditService.AppendEvent(
            auditFile,
            "TIMELINE_EXPORTED",
            timelineFile);

        MessageBox.Show(
            "Timeline exportada.");
    }
    private void ExportarCaso_Click(
        object sender,
        RoutedEventArgs e)
        {
            if (_currentCasePath is null)
                return;

            try
            {
                var dialog =
                    new SaveFileDialog
                    {
                        Filter =
                            "Arquivo ZIP (*.zip)|*.zip",

                        FileName =
                            $"{_currentCaseId}.zip"
                    };

                if (dialog.ShowDialog() != true)
                    return;

                string zip =
                    CaseExporter.Export(
                        _currentCasePath,
                        dialog.FileName);

                AuditService.AppendEvent(
                    Path.Combine(
                        _currentCasePath,
                        "logs",
                        "audit.jsonl"),
                    "CASE_EXPORTED",
                    zip);

                MessageBox.Show(
                    $"Caso exportado:\n\n{zip}",
                    "ADE");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro");
            }
        }
    public void CapturarMonitor_Click(
        object? sender,
        RoutedEventArgs? e)
    {
        if (_currentCasePath is null)
            return;

        // Mostrar diálogo para selecionar monitor
        var dialog = new SelectMonitorDialog();
        if (dialog.ShowDialog() != true || dialog.SelectedMonitor == null)
            return;

        var overlay = new CaptureOverlayWindow(
            _currentCasePath,
            "MONITOR",
            monitorIndex: dialog.SelectedMonitor.Index,
            onEvidenceCaptured: evidence =>
            {
                _evidences.Add(evidence);
                AtualizarManifesto();
            },
            onStatusUpdate: status =>
            {
                SetStatus(status ?? "CAPTURA REALIZADA");
            });

        overlay.Show();
    }
    public void CapturarJanela_Click(
        object? sender,
        RoutedEventArgs? e)
        {
            if (_currentCasePath is null)
                return;

            var dialog = new SelectWindowDialog();
            if (dialog.ShowDialog() != true || dialog.SelectedWindow is null)
                return;

            var overlay = new CaptureOverlayWindow(
                _currentCasePath,
                "WINDOW",
                windowHandle: dialog.SelectedWindow.Handle,
                onEvidenceCaptured: evidence =>
                {
                    _evidences.Add(evidence);
                    AtualizarManifesto();
                },
                onStatusUpdate: status =>
                {
                    SetStatus(status ?? "CAPTURA REALIZADA");
                });

            overlay.Show();
        }
}
