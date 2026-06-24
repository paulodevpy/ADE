namespace ADE.Capture;

/// <summary>
/// Representa um monitor disponível para captura, com rótulo amigável
/// e o índice correspondente usado por <see cref="ScreenCaptureManager.Capture"/>.
/// </summary>
public sealed class MonitorInfo
{
    public MonitorInfo(
        int index,
        string label,
        string deviceName)
    {
        Index = index;
        Label = label;
        DeviceName = deviceName;
    }

    public int Index
    {
        get;
    }

    public string Label
    {
        get;
    }

    public string DeviceName
    {
        get;
    }
}
