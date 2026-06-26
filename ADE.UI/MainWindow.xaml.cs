using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

using ADE.Core.Audit;
using ADE.Core.Cases;
using ADE.Core.Evidence;
using ADE.Core.Manifest;
using ADE.Core.Metadata;
using ADE.Core.Models;
using ADE.Core.Security;
using ADE.Capture;
using ADE.Reporting.Pdf;
using ADE.Core.Collection;

namespace ADE.UI;

public class BooleanToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDeleted)
        {
            return isDeleted ? "EXCLUÍDO" : "ATIVO";
        }
        return "ATIVO";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class MainWindow : Window
{
   
    private readonly CollectionService _collectionService;
    private readonly CollectionRepository _repository =
        new();
    private readonly CollectionFactory _factory =
        new();
    private readonly CollectionSessionManager _session = new();
    private readonly CollectionEvidenceManager _collectionEvidenceManager;
    private string? _currentCasePath;
    private string? _currentCaseId;
    
    private readonly ObservableCollection<EvidenceFile>
        _evidences = new();

    private SystemTrayIcon? _systemTrayIcon;
    

    public MainWindow()
    {
        InitializeComponent();
        _collectionEvidenceManager =
            new CollectionEvidenceManager(_session);

        _collectionService =
            new CollectionService(
                _session,
                _repository);
        
        EvidenceGrid.ItemsSource =
            _evidences;

        _systemTrayIcon = new SystemTrayIcon(this);

        StateChanged += MainWindow_StateChanged;
        Closing += MainWindow_Closing;

        _session.Close();
        UpdateUIForCollectionState();
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

    private void UpdateUIForCollectionState()
    {
        switch (_session.State)
        {
            case CollectionState.None:

                NovaColetaButton.IsEnabled = true;

                AdicionarEvidenciaButton.IsEnabled = false;
                CapturarTelaButton.IsEnabled = false;
                CapturarMonitorButton.IsEnabled = false;
                CapturarJanelaButton.IsEnabled = false;

                FinalizarColetaButton.IsEnabled = false;
                GerarRelatorioButton.IsEnabled = false;

                VerificarIntegridadeButton.IsEnabled = false;
                ExportarTimelineButton.IsEnabled = false;
                ExportarCasoButton.IsEnabled = false;

                BoTextBox.IsEnabled = true;
                ProcedimentoTextBox.IsEnabled = true;
                UnidadeTextBox.IsEnabled = true;
                OfficerTextBox.IsEnabled = true;
                ObservacoesTextBox.IsEnabled = true;

                break;

            case CollectionState.InProgress:

                NovaColetaButton.IsEnabled = false;

                AdicionarEvidenciaButton.IsEnabled = true;
                CapturarTelaButton.IsEnabled = true;
                CapturarMonitorButton.IsEnabled = true;
                CapturarJanelaButton.IsEnabled = true;

                FinalizarColetaButton.IsEnabled = true;
                GerarRelatorioButton.IsEnabled = true;

                VerificarIntegridadeButton.IsEnabled = true;
                ExportarTimelineButton.IsEnabled = true;
                ExportarCasoButton.IsEnabled = true;

                BoTextBox.IsEnabled = false;
                ProcedimentoTextBox.IsEnabled = false;
                UnidadeTextBox.IsEnabled = false;
                OfficerTextBox.IsEnabled = false;
                ObservacoesTextBox.IsEnabled = false;

                break;

            case CollectionState.Finalized:

                NovaColetaButton.IsEnabled = true;

                AdicionarEvidenciaButton.IsEnabled = false;
                CapturarTelaButton.IsEnabled = false;
                CapturarMonitorButton.IsEnabled = false;
                CapturarJanelaButton.IsEnabled = false;

                FinalizarColetaButton.IsEnabled = false;
                GerarRelatorioButton.IsEnabled = true;

                VerificarIntegridadeButton.IsEnabled = true;
                ExportarTimelineButton.IsEnabled = true;
                ExportarCasoButton.IsEnabled = true;

                BoTextBox.IsEnabled = true;
                ProcedimentoTextBox.IsEnabled = true;
                UnidadeTextBox.IsEnabled = true;
                OfficerTextBox.IsEnabled = true;
                ObservacoesTextBox.IsEnabled = true;

                break;

            case CollectionState.ReportGenerated:

                NovaColetaButton.IsEnabled = true;

                AdicionarEvidenciaButton.IsEnabled = false;
                CapturarTelaButton.IsEnabled = false;
                CapturarMonitorButton.IsEnabled = false;
                CapturarJanelaButton.IsEnabled = false;

                FinalizarColetaButton.IsEnabled = false;
                GerarRelatorioButton.IsEnabled = false;

                VerificarIntegridadeButton.IsEnabled = true;
                ExportarTimelineButton.IsEnabled = true;
                ExportarCasoButton.IsEnabled = true;

                BoTextBox.IsEnabled = true;
                ProcedimentoTextBox.IsEnabled = true;
                UnidadeTextBox.IsEnabled = true;
                OfficerTextBox.IsEnabled = true;
                ObservacoesTextBox.IsEnabled = true;

                break;
        }
    }

    private void SaveCollectionState()
    {
        if (!_session.HasCollection)
            return;

        _collectionService.SaveCollection();

        SaveCaseMetadata();
    }

    private void AddEvidenceToCurrentCollection(EvidenceFile evidence)
    {
        bool added =
            _collectionEvidenceManager.Add(evidence);

        if (!added)
        {
            var result =
                MessageBox.Show(
                    $"A evidência abaixo já existe nesta coleta.\n\n" +
                    $"Arquivo: {evidence.FileName}\n\n" +
                    $"SHA-256:\n{evidence.Sha256}\n\n" +
                    "Deseja manter a duplicata?",
                    "Evidência duplicada",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                try
                {
                    if (File.Exists(evidence.ImportedPath))
                        File.Delete(evidence.ImportedPath);
                }
                catch
                {
                }

                AuditService.AppendEvent(
                    Path.Combine(
                        _currentCasePath!,
                        "logs",
                        "audit.jsonl"),
                    "EVIDENCE_DUPLICATE_DISCARDED",
                    $"{evidence.FileName}|{evidence.Sha256}");

                SetStatus("Duplicata descartada");

                return;
            }

            _session.CurrentCollection!
                .EvidenceFiles
                .Add(evidence);

            AuditService.AppendEvent(
                Path.Combine(
                    _currentCasePath!,
                    "logs",
                    "audit.jsonl"),
                "EVIDENCE_DUPLICATE_ACCEPTED",
                $"{evidence.FileName}|{evidence.Sha256}");
        }
        else
        {
            AuditService.AppendEvent(
                Path.Combine(
                    _currentCasePath!,
                    "logs",
                    "audit.jsonl"),
                "EVIDENCE_ADDED",
                $"{evidence.FileName}|{evidence.Sha256}");
        }

        RefreshEvidenceGrid();

        AtualizarManifesto();

        SaveCollectionState();

        SetStatus(
            $"Evidências: {_collectionEvidenceManager.Count()}");
    }

    private void RefreshEvidenceGrid()
    {
        _evidences.Clear();

        foreach (var item in _collectionEvidenceManager.Active())
            _evidences.Add(item);

        EvidenceGrid.Items.Refresh();
    }

    private void SaveCaseMetadata()
    {
        if (_currentCasePath is null)
            return;

        var metadata =
            new CaseMetadata
            {
                CaseId =
                    _currentCaseId ?? "",

                BoNumber =
                    BoTextBox.Text,

                ProcedureNumber =
                    ProcedimentoTextBox.Text,

                UnitName =
                    UnidadeTextBox.Text,

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

        _collectionService.SaveMetadata(
            _currentCasePath,
            metadata);
    }

    private void LoadCollectionState()
    {
        if (_currentCasePath is null)
            return;

        _session.Load(
            _repository,
            _currentCasePath);

        if (!_session.HasCollection)
        {
            UpdateUIForCollectionState();
            return;
        }

        _currentCaseId =
            _session.CaseId;

        CaseIdTextBlock.Text =
            $"Caso Atual: {_currentCaseId}";

        RefreshEvidenceGrid();

        UpdateUIForCollectionState();
    }

    private void ResumeLastCollection()
    {
        string rootFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop),
                "ADE_Coletas");

        if (!Directory.Exists(rootFolder))
            return;

        foreach (var folder in Directory.GetDirectories(rootFolder))
        {
            if (!_session.Resume(
                _repository,
                folder))
                continue;

            if (_session.State ==
                CollectionState.ReportGenerated)
                continue;

            var result =
                MessageBox.Show(
                    $"Foi encontrada uma coleta em andamento.\n\nCaso:\n{_session.CaseId}\n\nDeseja continuar?",
                    "ADE",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                _session.Close();
                continue;
            }

            _currentCasePath = folder;
            _currentCaseId = _session.CaseId;

            CaseIdTextBlock.Text =
                $"Caso Atual: {_currentCaseId}";

            _evidences.Clear();

            foreach (var evidence in _session.CurrentCollection!.EvidenceFiles)
                _evidences.Add(evidence);

            EvidenceGrid.Items.Refresh();

            AtualizarFormulario();

            UpdateUIForCollectionState();

            SetStatus("COLETA RESTAURADA");

            return;
        }
    }

    private void AtualizarFormulario()
    {
        if (_currentCasePath is null)
            return;

        var metadata =
            _collectionService.LoadMetadata(
                _currentCasePath);

        if (metadata is null)
            return;

        BoTextBox.Text =
            metadata.BoNumber;

        ProcedimentoTextBox.Text =
            metadata.ProcedureNumber;

        UnidadeTextBox.Text =
            metadata.UnitName;

        OfficerTextBox.Text =
            metadata.OfficerName;

        ObservacoesTextBox.Text =
            metadata.Notes;
    }

    private void NovaColeta_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_session.HasActiveCollection())
        {
            MessageBox.Show(
                "Existe uma coleta aberta.\n\nFinalize-a antes de iniciar outra.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        SetStatus("CRIANDO CASO");

        string destino =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop),
                "ADE_Coletas");

        var result =
            _factory.Create(
                destino,
                UnidadeTextBox.Text,
                BoTextBox.Text,
                ProcedimentoTextBox.Text,
                OfficerTextBox.Text,
                ObservacoesTextBox.Text);

        _session.Reset();

        _evidences.Clear();

        EvidenceGrid.Items.Refresh();

        _currentCaseId =
            result.CaseId;

        _currentCasePath =
            result.RootPath;

        var collection = new CollectionRecord
        {
            CaseId = result.CaseId,
            GeneratedAtUtc   = DateTime.UtcNow,
            CollectionState = CollectionState.InProgress,
            EvidenceFiles = new List<EvidenceFile>()
        };

        _session.Start(
            collection,
            result.RootPath);

        _collectionService.SaveCollection();

        CaseIdTextBlock.Text =
            $"Caso Atual: {_currentCaseId}";

        var metadata =
            new CaseMetadata
            {
                CaseId = result.CaseId,
                UnitName = UnidadeTextBox.Text,
                BoNumber = BoTextBox.Text,
                ProcedureNumber = ProcedimentoTextBox.Text,
                OfficerName = OfficerTextBox.Text,
                Notes = ObservacoesTextBox.Text,
                ComputerName = Environment.MachineName,
                WindowsUser = Environment.UserName,
                CreatedAtUtc = DateTime.UtcNow
            };

        new CaseMetadataManager()
            .Save(
                metadata,
                Path.Combine(
                    result.RootPath,
                    "metadata"));

        var audit =
            new AuditChain();

        audit.AddEvent(
            "CASE_CREATED",
            result.CaseId);

        audit.Save(
            Path.Combine(
                result.RootPath,
                "logs",
                "audit.jsonl"));
        
        RefreshEvidenceGrid();

        AtualizarManifesto();

        SaveCollectionState();

        LoadCollectionState();

        UpdateUIForCollectionState();

        SetStatus("CASO EM ANDAMENTO");

        MessageBox.Show(
            $"Novo caso criado.\n\n{result.CaseId}",
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

            AddEvidenceToCurrentCollection(evidence);
        }    
            
    }

    private void AtualizarManifesto()
    {
        if (_currentCasePath is null)
            return;

        if (!_session.HasCollection)
            return;

        var manifest =
            new ManifestModel
            {
                CaseId = _currentCaseId ?? "",

                GeneratedAtUtc = DateTime.UtcNow,

                EvidenceFiles =
                    _collectionEvidenceManager
                        .ReportFiles()
                        .ToList()
            };

        new ManifestManager()
            .SaveManifest(
                manifest,
                Path.Combine(
                    _currentCasePath,
                    "metadata"));

        MasterIntegrityManager.Generate(
            _currentCasePath);

        SaveCollectionState();
    }

    private void FinalizarColeta()
    {
        if (!_session.CanFinalizeCollection)
        {
            MessageBox.Show(
                "Não existe coleta em andamento.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (MessageBox.Show(
            "Deseja finalizar esta coleta?",
            "ADE",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question)
            != MessageBoxResult.Yes)
            return;

        _session.FinalizeCollection();

        _session.Save(_repository);

        UpdateUIForCollectionState();

        AuditService.AppendEvent(
            Path.Combine(
                _currentCasePath!,
                "logs",
                "audit.jsonl"),
            "COLLECTION_FINALIZED",
            _currentCaseId!);

        SetStatus("COLETA FINALIZADA");
    }

    private void FinalizarColeta_Click(object sender, RoutedEventArgs e)
    {
        FinalizarColeta();
    }

    private void DeleteEvidence_Click(
        object sender,
        RoutedEventArgs e)
    {
        var evidence =
            (sender as FrameworkElement)?.DataContext as EvidenceFile
            ?? EvidenceGrid.SelectedItem as EvidenceFile;

        if (evidence is null)
            return;

        if (!_session.CanDeleteEvidence)
        {
            MessageBox.Show(
                "A coleta já foi finalizada.\n\nNão é permitido excluir evidências.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var result =
            MessageBox.Show(
                $"Deseja excluir a evidência\n\n{evidence.FileName} ?",
                "ADE",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            if (File.Exists(evidence.ImportedPath))
                File.Delete(evidence.ImportedPath);

            _collectionEvidenceManager.Delete(evidence);

            RefreshEvidenceGrid();

            AtualizarManifesto();

            SaveCollectionState();

            AuditService.AppendEvent(
                Path.Combine(
                    _currentCasePath!,
                    "logs",
                    "audit.jsonl"),
                "EVIDENCE_DELETED",
                evidence.FileName);

            SetStatus("EVIDÊNCIA EXCLUÍDA");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
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
                AddEvidenceToCurrentCollection(evidence);
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

        if (_session.State != CollectionState.Finalized)
        {
            MessageBox.Show(
                "A coleta deve ser finalizada antes da geração do relatório.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
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
                        _evidences
                            .Where(e => !e.IsDeleted)
                            .ToList(),

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
                TimelineService.Build(auditFile);

            PdfReportManager.Generate(
                model,
                pdfFile);

            string sha256 =
                IntegrityManager.CalculateSha256(pdfFile);

            string sha512 =
                IntegrityManager.CalculateSha512(pdfFile);

            File.WriteAllText(
                pdfFile + ".sha256",
                sha256);

            File.WriteAllText(
                pdfFile + ".sha512",
                sha512);

            _session.ReportGenerated();

            SaveCollectionState();

            _evidences.Clear();

            EvidenceGrid.Items.Refresh();

            _session.Close();

            _currentCasePath = null;

            _currentCaseId = null;

            EvidenceGrid.SelectedItem = null;

            UpdateUIForCollectionState();

            AuditService.AppendEvent(
                auditFile,
                "PDF_GENERATED",
                pdfFile);

            SetStatus("RELATÓRIO GERADO");

            MessageBox.Show(
                "Relatório gerado com sucesso.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao gerar relatório:\n\n{ex.Message}",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
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
        if (dialog.ShowDialog() != true)
            return;

        string captureType = dialog.CaptureAllMonitors ? "ALL_MONITORS" : "MONITOR";
        int? monitorIndex = dialog.CaptureAllMonitors ? null : dialog.SelectedMonitor?.Index;

        var overlay = new CaptureOverlayWindow(
            _currentCasePath,
            captureType,
            monitorIndex: monitorIndex,
            onEvidenceCaptured: evidence =>
            {
                AddEvidenceToCurrentCollection(evidence);
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
                    AddEvidenceToCurrentCollection(evidence);
                },
                onStatusUpdate: status =>
                {
                    SetStatus(status ?? "CAPTURA REALIZADA");
                });

            overlay.Show();
        }
}
