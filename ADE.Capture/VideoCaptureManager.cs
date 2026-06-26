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
    /// <param name="captureType">Tipo de captura (SCREEN, MONITOR, WINDOW, ALL_MONITORS).</param>
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

        string fileName = captureType == "ALL_MONITORS" 
            ? $"VIDEO_ALL_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"
            : $"VIDEO_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
        _outputPath = Path.Combine(destinationFolder, fileName);

        // Configurar argumentos do FFmpeg para captura de tela
        string arguments = captureType switch
        {
            "MONITOR" => 
                $"-f gdigrab -framerate 30 -i desktop -offset_x {GetMonitorOffsetX(monitorIndex)} -offset_y {GetMonitorOffsetY(monitorIndex)} -video_size {GetMonitorResolution(monitorIndex)} -draw_mouse 1 -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\"",
            "ALL_MONITORS" => 
                $"-f gdigrab -framerate 30 -i desktop -offset_x {GetAllMonitorsOffsetX()} -offset_y {GetAllMonitorsOffsetY()} -video_size {GetAllMonitorsResolution()} -draw_mouse 1 -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\"",
            "WINDOW" when windowHandle.HasValue => 
                $"-f gdigrab -framerate 30 -i title={windowHandle.Value} -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\"",
            _ => 
                $"-f gdigrab -framerate 30 -i desktop -draw_mouse 1 -c:v libx264 -preset ultrafast -crf 22 \"{_outputPath}\""
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
        if (_recordingProcess == null)
            return string.Empty;

        if (_recordingProcess.HasExited)
            return _outputPath ?? string.Empty;

        try
        {
            _recordingProcess.StandardInput.WriteLine("q");
            _recordingProcess.StandardInput.Flush();

            if (!_recordingProcess.WaitForExit(15000))
            {
                _recordingProcess.Kill(true);
                _recordingProcess.WaitForExit();
            }
        }
        finally
        {
            _recordingProcess.Dispose();
            _recordingProcess = null;
        }

        if (string.IsNullOrWhiteSpace(_outputPath))
            return string.Empty;

        var timeout =
            DateTime.UtcNow.AddSeconds(10);

        while (DateTime.UtcNow < timeout)
        {
            try
            {
                using var stream =
                    new FileStream(
                        _outputPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read);

                if (stream.Length > 0)
                    break;
            }
            catch
            {
            }

            Thread.Sleep(250);
        }

        return _outputPath;
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

    /// <summary>
    /// Obtém o offset horizontal de todos os monitores combinados.
    /// </summary>
    private static int GetAllMonitorsOffsetX()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        return screens.Min(s => s.Bounds.X);
    }

    /// <summary>
    /// Obtém o offset vertical de todos os monitores combinados.
    /// </summary>
    private static int GetAllMonitorsOffsetY()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        return screens.Min(s => s.Bounds.Y);
    }

    /// <summary>
    /// Obtém a resolução total de todos os monitores combinados.
    /// </summary>
    private static string GetAllMonitorsResolution()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        int minX = screens.Min(s => s.Bounds.X);
        int minY = screens.Min(s => s.Bounds.Y);
        int maxX = screens.Max(s => s.Bounds.X + s.Bounds.Width);
        int maxY = screens.Max(s => s.Bounds.Y + s.Bounds.Height);
        return $"{maxX - minX}x{maxY - minY}";
    }
}
