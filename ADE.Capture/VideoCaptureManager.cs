using System.Diagnostics;
using System.IO;

namespace ADE.Capture;

/// <summary>
/// Gerenciador de captura de vídeo com cadeia de custódia.
/// Implementação básica que pode ser expandida com FFmpeg ou similar.
/// </summary>
public class VideoCaptureManager
{
    private Process? _recordingProcess;
    private string? _outputPath;

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
    /// <param name="captureType">Tipo de captura (SCREEN, MONITOR, WINDOW).</param>
    /// <param name="monitorIndex">Índice do monitor (0 para primário).</param>
    /// <param name="windowHandle">Handle da janela para captura de janela específica.</param>
    public void StartRecording(
        string destinationFolder,
        string captureType = "SCREEN",
        int monitorIndex = 0,
        nint? windowHandle = null)
    {
        if (!IsVideoRecordingSupported())
        {
            throw new InvalidOperationException(
                "FFmpeg não está instalado. Não é possível gravar vídeos.");
        }

        Directory.CreateDirectory(destinationFolder);

        string fileName = $"VIDEO_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
        _outputPath = Path.Combine(destinationFolder, fileName);

        // Configurar argumentos do FFmpeg para captura de tela
        string arguments = captureType switch
        {
            "MONITOR" when monitorIndex > 0 => 
                $"-f gdigrab -framerate 30 -i desktop -offset_x {GetMonitorOffsetX(monitorIndex)} -offset_y {GetMonitorOffsetY(monitorIndex)} -video_size {GetMonitorResolution(monitorIndex)} -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\"",
            "WINDOW" when windowHandle.HasValue => 
                $"-f gdigrab -framerate 30 -i title={windowHandle.Value} -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\"",
            _ => 
                $"-f gdigrab -framerate 30 -i desktop -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\""
        };

        _recordingProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true
            }
        };

        _recordingProcess.Start();
    }

    /// <summary>
    /// Para a gravação de vídeo em andamento.
    /// </summary>
    /// <returns>Caminho do arquivo gravado.</returns>
    public string StopRecording()
    {
        if (_recordingProcess == null || _recordingProcess.HasExited)
            return _outputPath ?? string.Empty;

        try
        {
            // Enviar comando 'q' para o FFmpeg parar graciosamente
            _recordingProcess.StandardInput.WriteLine("q");
            
            // Aguarda um pouco para o processo finalizar
            if (!_recordingProcess.WaitForExit(5000))
            {
                _recordingProcess.Kill();
            }
        }
        catch
        {
            try
            {
                _recordingProcess.Kill();
            }
            catch
            {
                // Ignorar erros ao tentar matar o processo
            }
        }

        return _outputPath ?? string.Empty;
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
