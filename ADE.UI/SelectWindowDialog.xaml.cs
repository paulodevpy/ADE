using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using ADE.Capture;

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

        WindowListBox.ItemsSource =
            WindowCaptureManager.GetWindows();

        if (WindowListBox.Items.Count > 0)
            WindowListBox.SelectedIndex = 0;
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
            try
            {
                string capturedFile = WindowCaptureManager.Capture(windowInfo.Handle, Path.GetTempPath());

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
        SelectedWindow =
            WindowListBox.SelectedItem
                as WindowInfo;

        if (SelectedWindow != null)
        {
            // Traz a janela selecionada para frente quando confirmado
            WindowCaptureManager.BringToFront(SelectedWindow.Handle);
            
            // Aguarda um momento para garantir que a janela seja ativada
            System.Threading.Thread.Sleep(300);
        }

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        SelectedWindow = null;
        DialogResult = false;
    }
}