using System.Configuration;
using System.Data;
using System.Windows;

namespace ADE.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        try
        {
            var mainWindow = new MainWindow();
            mainWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting application: {ex.Message}\n\n{ex.StackTrace}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

