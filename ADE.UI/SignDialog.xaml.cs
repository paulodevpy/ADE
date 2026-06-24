using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using Microsoft.Win32;

namespace ADE.UI;

public partial class SignDialog : Window
{
    /// <summary>
    /// Caminho do arquivo PFX (certificado A1 ICP-Brasil) selecionado pelo usuário.
    /// Preenchido apenas após validação bem-sucedida.
    /// </summary>
    public string? PfxPath { get; private set; }

    /// <summary>
    /// Senha do certificado validada com sucesso.
    /// </summary>
    public string? Password { get; private set; }

    /// <summary>
    /// Nome do titular do certificado (subject) para registro na cadeia de custódia.
    /// </summary>
    public string? CertificateSubject { get; private set; }

    public SignDialog()
    {
        InitializeComponent();
    }

    private void SelectPfx_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog =
            new OpenFileDialog
            {
                Title = "Selecionar Certificado Digital (A1)",
                Filter = "Certificado PFX/PKCS#12 (*.pfx;*.p12)|*.pfx;*.p12"
            };

        if (dialog.ShowDialog() == true)
        {
            PfxPathTextBox.Text = dialog.FileName;
        }
    }

    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        string path = PfxPathTextBox.Text;
        string password = PfxPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(
                "Selecione um arquivo de certificado (.pfx) válido.",
                "Certificado",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            // Valida o certificado e a senha carregando o PKCS#12.
            // X509CertificateLoader é a API atual (o construtor X509Certificate2
            // a partir de arquivo está obsoleto no .NET 10).
            using var certificate =
                X509CertificateLoader.LoadPkcs12FromFile(
                    path,
                    password,
                    X509KeyStorageFlags.EphemeralKeySet);

            if (!certificate.HasPrivateKey)
            {
                MessageBox.Show(
                    "O certificado selecionado não possui chave privada e " +
                    "não pode ser usado para assinatura.",
                    "Certificado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (certificate.NotAfter < DateTime.Now)
            {
                MessageBox.Show(
                    $"O certificado está expirado (validade até " +
                    $"{certificate.NotAfter:dd/MM/yyyy}).",
                    "Certificado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            PfxPath = path;
            Password = password;
            CertificateSubject = certificate.Subject;

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível abrir o certificado. Verifique se a senha " +
                $"está correta.\n\nDetalhe: {ex.Message}",
                "Certificado",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
