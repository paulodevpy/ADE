// Resolve as ambiguidades entre WPF e WinForms no projeto ADE.UI.
// Sempre que houver conflito, prevalece a versão do WPF (ou Microsoft.Win32),
// que é a usada pela interface da aplicação.
global using Application = System.Windows.Application;
global using MessageBox = System.Windows.MessageBox;
global using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
global using SaveFileDialog = Microsoft.Win32.SaveFileDialog;