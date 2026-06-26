using System.Drawing;
using System.Drawing.Imaging;

namespace ADE.Capture;

public static class ScreenCaptureManager
{
    /// <summary>
    /// Retorna a lista de monitores detectados, com um rótulo amigável
    /// e o respectivo índice em <see cref="System.Windows.Forms.Screen.AllScreens"/>.
    /// Mantém a dependência de WinForms encapsulada nesta camada.
    /// </summary>
    public static IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var screens =
            System.Windows.Forms.Screen.AllScreens;

        var result =
            new List<MonitorInfo>(screens.Length);

        for (int i = 0; i < screens.Length; i++)
        {
            var s = screens[i];

            string label =
                $"Monitor {i + 1} " +
                $"({s.Bounds.Width}x{s.Bounds.Height})" +
                (s.Primary ? " - Principal" : "");

            result.Add(
                new MonitorInfo(
                    i,
                    label,
                    s.DeviceName));
        }

        return result;
    }

    public static string Capture(
        string destinationFolder,
        int monitorIndex = 0)
    {
        Directory.CreateDirectory(destinationFolder);

        var screens =
            System.Windows.Forms.Screen.AllScreens;

        if (screens.Length == 0)
            throw new InvalidOperationException(
                "Nenhum monitor foi encontrado.");

        if (monitorIndex < 0)
            monitorIndex = 0;

        if (monitorIndex >= screens.Length)
            monitorIndex = screens.Length - 1;

        var screen =
            screens[monitorIndex];

        var bounds =
            screen.Bounds;

        string destination =
            Path.Combine(
                destinationFolder,
                $"SCREEN_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        using Bitmap bitmap =
            new(
                bounds.Width,
                bounds.Height,
                PixelFormat.Format32bppArgb);

        using Graphics graphics =
            Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            bounds.Left,
            bounds.Top,
            0,
            0,
            bounds.Size,
            CopyPixelOperation.SourceCopy);

        bitmap.Save(
            destination,
            ImageFormat.Png);

        return destination;
    }

    /// Captura todos os monitores combinados em uma única imagem.
    public static string CaptureAllMonitors(
        string destinationFolder)
    {
        Directory.CreateDirectory(destinationFolder);

        var screens =
            System.Windows.Forms.Screen.AllScreens;

        if (screens.Length == 0)
            throw new InvalidOperationException(
                "Nenhum monitor foi encontrado.");

        int left =
            screens.Min(s => s.Bounds.Left);

        int top =
            screens.Min(s => s.Bounds.Top);

        int right =
            screens.Max(s => s.Bounds.Right);

        int bottom =
            screens.Max(s => s.Bounds.Bottom);

        Rectangle virtualBounds =
            new(
                left,
                top,
                right - left,
                bottom - top);

        string destination =
            Path.Combine(
                destinationFolder,
                $"SCREEN_ALL_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        using Bitmap bitmap =
            new(
                virtualBounds.Width,
                virtualBounds.Height,
                PixelFormat.Format32bppArgb);

        using Graphics graphics =
            Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            virtualBounds.Left,
            virtualBounds.Top,
            0,
            0,
            virtualBounds.Size,
            CopyPixelOperation.SourceCopy);

        bitmap.Save(
            destination,
            ImageFormat.Png);

        return destination;
    }
}
