using System.Diagnostics;
using System.IO;

namespace ADE.Capture;

/// <summary>
/// Gerenciador de captura de vídeo com cadeia de custódia.
/// Implementação básica que pode ser expandida com FFmpeg ou similar.
/// </summary>
public static class VideoCaptureManager
{
    /// <summary>
    /// Verifica se há suporte para gravação de vídeo (FFmpeg instalado).
    /// </summary>
    public static bool IsVideoRecordingSupported()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Inicia a gravação de vídeo da tela completa.
    /// </summary>
    /// <param name="destinationFolder">Pasta de destino para o vídeo.</param>
    /// <param name="monitorIndex">Índice do monitor (0 para primário).</param>
    /// <returns>Processo de gravação em andamento.</returns>
    public static Process StartRecording(
        string destinationFolder,
        int monitorIndex = 0)
    {
        if (!IsVideoRecordingSupported())
        {
            throw new InvalidOperationException(
                "FFmpeg não está instalado. Não é possível gravar vídeos.");
        }

        Directory.CreateDirectory(destinationFolder);

        string fileName = $"VIDEO_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
        string outputPath = Path.Combine(destinationFolder, fileName);

        // Configurar argumentos do FFmpeg para captura de tela
        // Nota: Esta é uma implementação básica. Ajustes podem ser necessários
        // dependendo do sistema operacional e configurações.
        string arguments = monitorIndex == 0
            ? $"-f gdigrab -framerate 30 -i desktop -c:v libx264 -preset ultrafast -crf 22 \"{outputPath}\""
            : $"-f gdigrab -framerate 30 -i desktop -offset_x {GetMonitorOffsetX(monitorIndex)} -offset_y {GetMonitorOffsetY(monitorIndex)} -video_size {GetMonitorResolution(monitorIndex)} -c:v libx264 -preset ultrafast -crf 22 \"{outputPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        return process;
    }

    /// <summary>
    /// Para a gravação de vídeo em andamento.
    /// </summary>
    /// <param name="recordingProcess">Processo de gravação.</param>
    public static void StopRecording(Process recordingProcess)
    {
        if (recordingProcess == null || recordingProcess.HasExited)
            return;

        try
        {
            // Enviar comando 'q' para o FFmpeg parar graciosamente
            recordingProcess.StandardInput.WriteLine("q");
            
            // Aguarda um pouco para o processo finalizar
            if (!recordingProcess.WaitForExit(5000))
            {
                recordingProcess.Kill();
            }
        }
        catch
        {
            try
            {
                recordingProcess.Kill();
            }
            catch
            {
                // Ignorar erros ao tentar matar o processo
            }
        }
    }

    /// <summary>
    /// Obtém o offset horizontal do monitor especificado.
    /// </summary>
    private static int GetMonitorOffsetX(int monitorIndex)
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (monitorIndex >= 0 && monitorIndex < screens.Length)
        {
            return screens[monitorIndex].Bounds.X;
        }
        return 0;
    }

    /// <summary>
    /// Obtém o offset vertical do monitor especificado.
    /// </summary>
    private static int GetMonitorOffsetY(int monitorIndex)
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (monitorIndex >= 0 && monitorIndex < screens.Length)
        {
            return screens[monitorIndex].Bounds.Y;
        }
        return 0;
    }

    /// <summary>
    /// Obtém a resolução do monitor especificado.
    /// </summary>
    private static string GetMonitorResolution(int monitorIndex)
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (monitorIndex >= 0 && monitorIndex < screens.Length)
        {
            var bounds = screens[monitorIndex].Bounds;
            return $"{bounds.Width}x{bounds.Height}";
        }
        return "1920x1080";
    }
}
