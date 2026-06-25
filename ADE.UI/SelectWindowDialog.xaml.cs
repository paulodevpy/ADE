using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using ADE.Capture;
using System.ComponentModel;
using System.Windows.Threading;

namespace ADE.UI;

public partial class SelectWindowDialog : Window
{
    public WindowInfo? SelectedWindow
    {
        get;
        private set;
    }

    public SelectWindowDialog()
    {
        InitializeComponent();

        this.Loaded += (s, e) =>
        {
            // Small delay to ensure window is fully loaded
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (t, args) =>
            {
                timer.Stop();
                WindowListBox.ItemsSource =
                    WindowCaptureManager.GetWindows();

                if (WindowListBox.Items.Count > 0)
                    WindowListBox.SelectedIndex = 0;
            };
            timer.Start();
        };
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        // Keep window as topmost when activated
        this.Topmost = true;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Bring window back to foreground when deactivated
        this.Topmost = true;
        this.Activate();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // Ensure window stays on top
        this.Topmost = true;
    }

    private void WindowListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (WindowListBox.SelectedItem is WindowInfo selectedWindow)
        {
            UpdatePreview(selectedWindow);
        }
    }

    private void UpdatePreview(WindowInfo windowInfo)
    {
        try
        {
            // Bring the window to front temporarily for capture
            WindowCaptureManager.BringToFront(windowInfo.Handle);
            Thread.Sleep(300);

            string capturedFile = WindowCaptureManager.Capture(windowInfo.Handle, Path.GetTempPath());

            // Bring this dialog back to front
            this.Activate();
            Thread.Sleep(150);

            if (File.Exists(capturedFile))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(capturedFile);
                bitmap.EndInit();
                bitmap.Freeze();

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
            // Ensure dialog is back on front even if capture fails
            this.Activate();
        }
    }

    private void Selecionar_Click(
        object sender,
        RoutedEventArgs e)
    {
        SelectedWindow =
            WindowListBox.SelectedItem
                as WindowInfo;

        if (SelectedWindow != null)
        {
            // Traz a janela selecionada para frente quando confirmado
            WindowCaptureManager.BringToFront(SelectedWindow.Handle);

            // Aguarda um momento para garantir que a janela seja ativada
            Thread.Sleep(300);
        }

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        SelectedWindow = null;
        DialogResult = false;
    }
}