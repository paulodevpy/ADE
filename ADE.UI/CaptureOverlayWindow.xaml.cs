using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using ADE.Core.Audit;
using ADE.Core.Evidence;
using ADE.Core.Security;
using ADE.Core.Models;
using ADE.Capture;

namespace ADE.UI;

public partial class CaptureOverlayWindow : Window
{
    // --- Interop do atalho global (Ctrl+Shift+A) ---
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 0x0ADE;        // id arbitrário
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;
    private const uint VK_A = 0x41;              // tecla "A"
    private const int WM_HOTKEY = 0x0312;

    private HwndSource? _hwndSource;
    private bool _hotkeyRegistered;

    private readonly string _casePath;
    private readonly string _captureType;
    private readonly int? _monitorIndex;
    private readonly nint? _windowHandle;
    private readonly Action<EvidenceFile>? _onEvidenceCaptured;
    private readonly Action<string>? _onStatusUpdate;
    private ADE.Capture.VideoCaptureManager? _videoCaptureManager;

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

        Title = captureType switch
        {
            "SCREEN" => "Captura de Tela",
            "MONITOR" => $"Captura de Monitor {(monitorIndex ?? 0) + 1}",
            "WINDOW" => "Captura de Janela",
            _ => "Captura"
        };
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        this.Topmost = true;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Não traz a janela de volta automaticamente.
        // Isso impede que o overlay apareça na gravação.
        // O usuário restaura com Ctrl+Shift+A.
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        this.Topmost = true;

        // Registra o atalho global no HWND DESTA janela.
        // O HWND continua válido mesmo após Hide(), então o WM_HOTKEY
        // é entregue ao message loop do WPF mesmo com a janela escondida.
        var helper = new WindowInteropHelper(this);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(HwndHook);

        _hotkeyRegistered = RegisterHotKey(
            helper.Handle,
            HOTKEY_ID,
            MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT,
            VK_A);

        if (!_hotkeyRegistered)
        {
            // Outro app pode já estar usando o atalho. Avisa sem travar.
            _onStatusUpdate?.Invoke("ATALHO CTRL+SHIFT+A INDISPONÍVEL");
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            RestoreOverlay();
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void RestoreOverlay()
    {
        this.Dispatcher.Invoke(() =>
        {
            this.Show();
            this.Visibility = Visibility.Visible;
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
            this.Topmost = true;
            this.Activate();
        });
    }

    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        string evidencias = Path.Combine(_casePath, "evidencias");
        string arquivo = string.Empty;

        try
        {
            this.Hide();
            Thread.Sleep(200);

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
                        arquivo = ADE.Capture.WindowCaptureManager.Capture(_windowHandle.Value, evidencias);
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
            this.Show();
        }
    }

    private void StartVideo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string evidencias = Path.Combine(_casePath, "evidencias");

            _videoCaptureManager = new ADE.Capture.VideoCaptureManager();

            switch (_captureType)
            {
                case "SCREEN":
                    _videoCaptureManager.StartRecording(evidencias, "SCREEN");
                    break;
                case "MONITOR":
                    _videoCaptureManager.StartRecording(evidencias, "MONITOR", _monitorIndex ?? 0);
                    break;
                case "WINDOW":
                    if (_windowHandle.HasValue)
                        _videoCaptureManager.StartRecording(evidencias, "WINDOW", windowHandle: _windowHandle.Value);
                    break;
            }

            StartVideoButton.IsEnabled = false;
            StopVideoButton.IsEnabled = true;
            CaptureButton.IsEnabled = false;

            _onStatusUpdate?.Invoke("GRAVANDO VÍDEO (CTRL+SHIFT+A RESTAURA)".ToUpper());

            AuditService.AppendEvent(
                Path.Combine(_casePath, "logs", "audit.jsonl"),
                $"{_captureType}_VIDEO_START",
                "Iniciada gravação de vídeo");

            // Esconde a janela para não aparecer na gravação.
            // O HWND permanece vivo, então o atalho global continua funcionando.
            this.Topmost = false;
            this.Hide();
            Thread.Sleep(500);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao iniciar gravação: {ex.Message}", "Erro");
        }
    }

    private void StopVideo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_videoCaptureManager != null)
            {
                string arquivo = _videoCaptureManager.StopRecording();

                if (!string.IsNullOrEmpty(arquivo) && File.Exists(arquivo))
                {
                    var info = new FileInfo(arquivo);
                    var evidence = CreateEvidence(info, arquivo, "VIDEO");
                    _onEvidenceCaptured?.Invoke(evidence);
                    _onStatusUpdate?.Invoke("VÍDEO GRAVADO".ToUpper());

                    AuditService.AppendEvent(
                        Path.Combine(_casePath, "logs", "audit.jsonl"),
                        $"{_captureType}_VIDEO_STOP",
                        evidence.Sha256);
                }
            }

            StartVideoButton.IsEnabled = true;
            StopVideoButton.IsEnabled = false;
            CaptureButton.IsEnabled = true;

            RestoreOverlay();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao parar gravação: {ex.Message}", "Erro");
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (_videoCaptureManager != null)
        {
            try { _videoCaptureManager.StopRecording(); } catch { }
        }

        this.Show();
        this.Close();
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

    protected override void OnClosed(EventArgs e)
    {
        // Remove o atalho global e o hook ao fechar.
        if (_hotkeyRegistered)
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
            _hotkeyRegistered = false;
        }

        _hwndSource?.RemoveHook(HwndHook);
        _hwndSource = null;

        base.OnClosed(e);
    }
}