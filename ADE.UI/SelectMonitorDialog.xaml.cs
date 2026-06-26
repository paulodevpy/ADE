using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using ADE.Capture;
using System.Windows.Threading;

namespace ADE.UI;

public partial class SelectMonitorDialog : Window
{
    /// <summary>
    /// Índice do monitor selecionado (compatível com
    /// <see cref="ScreenCaptureManager.Capture"/>).
    /// Permanece -1 quando o usuário cancela.
    /// </summary>
    public int SelectedMonitorIndex
    {
        get;
        private set;
    } = -1;

    public MonitorInfo? SelectedMonitor
    {
        get;
        private set;
    }

    /// <summary>
    /// Indica se todos os monitores devem ser capturados combinados.
    /// </summary>
    public bool CaptureAllMonitors
    {
        get;
        private set;
    }

    private DispatcherTimer? _previewTimer;

    public SelectMonitorDialog()
    {
        InitializeComponent();

        // Lista os monitores físicos detectados pela camada de captura.
        // O ListBox usa DisplayMemberPath="DeviceName".
        MonitorListBox.ItemsSource =
            ScreenCaptureManager.GetMonitors();

        if (MonitorListBox.Items.Count > 0)
            MonitorListBox.SelectedIndex = 0;

        // Configurar timer para preview em tempo real
        _previewTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500) // Atualizar a cada 500ms
        };
        _previewTimer.Tick += (s, e) => 
        {
            if (!CaptureAllMonitors && MonitorListBox.SelectedItem is MonitorInfo selectedMonitor)
            {
                UpdatePreview(selectedMonitor);
            }
            else if (CaptureAllMonitors)
            {
                UpdateAllMonitorsPreview();
            }
        };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _previewTimer?.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _previewTimer?.Stop();
        _previewTimer = null;
        base.OnClosed(e);
    }

    private void MonitorListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (MonitorListBox.SelectedItem is MonitorInfo selectedMonitor)
        {
            SelectedMonitor = selectedMonitor;
            if (!CaptureAllMonitors)
            {
                UpdatePreview(selectedMonitor);
            }
        }
    }

    private void AllMonitorsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        CaptureAllMonitors = true;
        MonitorListBox.IsEnabled = false;
        UpdateAllMonitorsPreview();
    }

    private void AllMonitorsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        CaptureAllMonitors = false;
        MonitorListBox.IsEnabled = true;
        if (MonitorListBox.SelectedItem is MonitorInfo selectedMonitor)
        {
            UpdatePreview(selectedMonitor);
        }
    }

    private void UpdatePreview(MonitorInfo monitorInfo)
    {
        try
        {
            // Criar captura temporária para preview
            string capturedFile = ScreenCaptureManager.Capture(Path.GetTempPath(), monitorInfo.Index);
            
            if (File.Exists(capturedFile))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(capturedFile);
                bitmap.EndInit();
                bitmap.Freeze(); // Importante para permitir atualizações
                
                PreviewImage.Source = bitmap;
                
                // Limpar arquivo temporário após carregar
                try
                {
                    File.Delete(capturedFile);
                }
                catch { }
            }
            else
            {
                PreviewImage.Source = null;
            }
        }
        catch
        {
            PreviewImage.Source = null;
        }
    }

    private void UpdateAllMonitorsPreview()
    {
        try
        {
            // Criar captura temporária para preview de todos os monitores
            string capturedFile = ScreenCaptureManager.CaptureAllMonitors(Path.GetTempPath());
            
            if (File.Exists(capturedFile))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(capturedFile);
                bitmap.EndInit();
                bitmap.Freeze(); // Importante para permitir atualizações
                
                PreviewImage.Source = bitmap;
                
                // Limpar arquivo temporário após carregar
                try
                {
                    File.Delete(capturedFile);
                }
                catch { }
            }
            else
            {
                PreviewImage.Source = null;
            }
        }
        catch
        {
            PreviewImage.Source = null;
        }
    }

    private void Selecionar_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (MonitorListBox.SelectedItem is not MonitorInfo monitor)
        {
            MessageBox.Show(
                "Selecione um monitor para continuar.",
                "ADE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        SelectedMonitorIndex = monitor.Index;
        SelectedMonitor = monitor;

        DialogResult = true;
    }

    private void Cancelar_Click(
        object sender,
        RoutedEventArgs e)
    {
        SelectedMonitorIndex = -1;
        SelectedMonitor = null;

        DialogResult = false;
    }
}
