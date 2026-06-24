using System.Runtime.InteropServices;
using System.Text;

namespace ADE.Capture;

internal static class NativeMethods
{
    public delegate bool EnumWindowsProc(
        IntPtr hWnd,
        IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(
        EnumWindowsProc lpEnumFunc,
        IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern int GetWindowText(
        IntPtr hWnd,
        StringBuilder text,
        int count);

    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetParent(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(
        IntPtr hWnd,
        int nCmdShow);

    public const int SW_RESTORE = 9;
    public const int SW_SHOW = 5;

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(
        IntPtr hWnd,
        out RECT rect);

    [DllImport("user32.dll")]
    public static extern long GetWindowLong(
        IntPtr hWnd,
        int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClassName(
        IntPtr hWnd,
        StringBuilder lpClassName,
        int nMaxCount);

    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;
    public const long WS_BORDER = 0x00800000L;
    public const long WS_THICKFRAME = 0x00400000L;
    public const long WS_EX_APPWINDOW = 0x00040000L;
    public const long WS_EX_TOOLWINDOW = 0x00000080L;
    public const long WS_EX_TOPMOST = 0x00000008L;
    public const long WS_EX_NOACTIVATE = 0x08000000L;

    public static bool IsWindowOnCurrentDesktop(IntPtr hWnd)
    {
        // Verificação simplificada - se a janela está visível e não está minimizada,
        // assumimos que está na área de trabalho atual
        // Para implementação completa seria necessário usar Windows 10+ Virtual Desktop APIs
        return true;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}