using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

namespace ADE.UI;

public class SystemTrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly MainWindow _mainWindow;
    private bool _disposed;

    private static System.Drawing.Icon LoadTrayIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/ade.ico", UriKind.Absolute);
            var info = System.Windows.Application.GetResourceStream(uri);
            if (info != null)
            {
                using var stream = info.Stream;
                return new System.Drawing.Icon(stream);
            }
        }
        catch
        {
            // cai no fallback abaixo
        }
        return System.Drawing.SystemIcons.Application;
    }

    public SystemTrayIcon(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        _contextMenu = new ContextMenuStrip();
        
        var restoreItem = new ToolStripMenuItem("Restaurar Janela");
        restoreItem.Click += Restore_Click;
        _contextMenu.Items.Add(restoreItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var openCaptureItem = new ToolStripMenuItem("Abrir Captura de Tela");
        openCaptureItem.Click += OpenCapture_Click;
        _contextMenu.Items.Add(openCaptureItem);

        var windowCaptureItem = new ToolStripMenuItem("Capturar Janela");
        windowCaptureItem.Click += WindowCapture_Click;
        _contextMenu.Items.Add(windowCaptureItem);

        var monitorCaptureItem = new ToolStripMenuItem("Capturar Monitor");
        monitorCaptureItem.Click += MonitorCapture_Click;
        _contextMenu.Items.Add(monitorCaptureItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Fechar Aplicação");
        exitItem.Click += Exit_Click;
        _contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            ContextMenuStrip = _contextMenu,
            Text = "ADE - Ata Digital de Evidências",
            Visible = true
        };

        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        RestoreWindow();
    }

    private void Restore_Click(object? sender, EventArgs e)
    {
        RestoreWindow();
    }

    private void OpenCapture_Click(object? sender, EventArgs e)
    {
        RestoreWindow();
        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.CapturarTela_Click(null, null);
        });
    }

    private void WindowCapture_Click(object? sender, EventArgs e)
    {
        RestoreWindow();
        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.CapturarJanela_Click(null, null);
        });
    }

    private void MonitorCapture_Click(object? sender, EventArgs e)
    {
        RestoreWindow();
        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.CapturarMonitor_Click(null, null);
        });
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/F /PID {currentProcess.Id}",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show($"Erro ao encerrar aplicação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Current.Shutdown();
        }
    }

    private void RestoreWindow()
    {
        _mainWindow.Dispatcher.Invoke(() =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        });
    }

    public void HideWindow()
    {
        _mainWindow.Hide();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        _contextMenu?.Dispose();
    }
}