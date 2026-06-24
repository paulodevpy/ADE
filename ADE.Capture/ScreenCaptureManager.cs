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
        var screens =
            System.Windows.Forms.Screen.AllScreens;

        if (monitorIndex >= screens.Length)
            monitorIndex = 0;

        var bounds =
            screens[monitorIndex]
                .Bounds;

        string fileName =
            $"SCREEN_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        string destination =
            Path.Combine(
                destinationFolder,
                fileName);

        using Bitmap bitmap =
            new(
                bounds.Width,
                bounds.Height);

        using Graphics graphics =
            Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            bounds.Location,
            Point.Empty,
            bounds.Size);

        bitmap.Save(
            destination,
            ImageFormat.Png);

        return destination;
    }
}
