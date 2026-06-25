using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Diagnostics;

namespace ADE.Capture;

public static class WindowCaptureManager
{
    public static List<WindowInfo> GetWindows()
    {
        var result =
            new List<WindowInfo>();

        NativeMethods.EnumWindows(
            (hWnd, lParam) =>
            {
                // Verificar se a janela está visível
                if (!NativeMethods.IsWindowVisible(hWnd))
                    return true;

                // Verificar se a janela está minimizada
                if (NativeMethods.IsIconic(hWnd))
                    return true;

                // Additional check: verify window is actually on screen and has a proper rectangle
                NativeMethods.GetWindowRect(hWnd, out RECT rect);
                if (rect.Right <= rect.Left || rect.Bottom <= rect.Top)
                    return true;

                // Check if window has a reasonable size (not tiny hidden windows)
                if (rect.Right - rect.Left < 100 || rect.Bottom - rect.Top < 100)
                    return true;

                // Verificar se a janela está na área de trabalho visível (em algum monitor)
                bool isVisibleOnAnyScreen = false;
                foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
                {
                    var screenBounds = screen.Bounds;
                    // Check if window intersects with this screen
                    if (!(rect.Right <= screenBounds.Left || rect.Left >= screenBounds.Right ||
                          rect.Bottom <= screenBounds.Top || rect.Top >= screenBounds.Bottom))
                    {
                        isVisibleOnAnyScreen = true;
                        break;
                    }
                }
                
                if (!isVisibleOnAnyScreen)
                    return true;

                int length =
                    NativeMethods.GetWindowTextLength(hWnd);

                if (length == 0)
                    return true;

                StringBuilder builder =
                    new(length + 1);

                NativeMethods.GetWindowText(
                    hWnd,
                    builder,
                    builder.Capacity);

                string title =
                    builder.ToString();

                if (string.IsNullOrWhiteSpace(title))
                    return true;

                // Obter classe da janela para filtrar classes de sistema
                StringBuilder className = new StringBuilder(256);
                NativeMethods.GetClassName(hWnd, className, className.Capacity);
                string windowClass = className.ToString().ToLowerInvariant();
                
                // Filtrar classes de janela que são tipicamente do sistema
                string[] systemWindowClasses = 
                {
                    "shell_traywnd",
                    "workerw",
                    "progmman",
                    "dv2controlhost",
                    "msedgeui",
                    "edgeui",
                    "windows.ui.core.corewindow",
                    "applicationframewindow",
                    "shell__embeddingframe",
                    "shell__multitaskingframe",
                    "shell__thumbnailframe",
                    "clockflyoutwindow",
                    "searchapp",
                    "xaml_windowedpopupclass",
                    "windowsinternal_shellcortanaexperiencewindow",
                    "shell__notifyoverlapwindow",
                    "textinputhostwindow",
                    "immersivebackgroundwindow",
                    "immersivefocuswindow",
                    "shellwindow",
                    "sidebarwindow",
                    "gdi+",
                    "static",
                    "button",
                    "edit",
                    "combobox",
                    "listbox",
                    "scrollbar",
                    "msctls_statusbar32",
                    "msctls_progress32",
                    "msctls_updown32",
                    "msctls_hotkey32",
                    "msctls_trackbar32",
                    "tooltips_class32",
                    "sysshadow",
                    "popup",
                    "menu",
                    "#32768",
                    "#32770",
                    "msorm",
                    "ncserver",
                    "dwm",
                    "msaut",
                    "system__class",
                    "ime",
                    "imeui",
                    "msctls_hotkey32",
                    "msctls_trackbar32",
                    "richedit",
                    "richedit20a",
                    "richedit20w",
                    "richedit50w"
                };
                
                bool isSystemClass = systemWindowClasses.Any(sc => windowClass.Contains(sc));

                // Filtrar apenas janelas de sistema essenciais - permitir aplicações do usuário
                string titleLower = title.ToLowerInvariant();
                string[] essentialSystemWindows = 
                {
                    "program manager",
                    "desktop",
                    "dwm",
                    "microsoft text input application",
                    "windows security notification",
                    "notification",
                    " Cortana",
                    "search",
                    "start",
                    "task switcher",
                    "task view",
                    "windows shell experience host"
                };

                // Verificar se o título contém palavras de sistema essenciais
                bool isEssentialSystemWindow = essentialSystemWindows.Any(sw => titleLower.Contains(sw));

                // Additional filtering: check process name to exclude background processes
                NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
                string processName = "";
                try
                {
                    var process = Process.GetProcessById((int)processId);
                    processName = process.ProcessName.ToLowerInvariant();
                }
                catch { }

                // Filter out common background processes
                string[] backgroundProcesses = 
                {
                    "svchost",
                    "dllhost",
                    "conhost",
                    "csrss",
                    "wininit",
                    "services",
                    "lsass",
                    "explorer",
                    "system",
                    "smss",
                    "rundll32",
                    "wmiprvse",
                    "spoolsv",
                    "taskhost",
                    "audiodg",
                    "dwm",
                    "atiesorr",
                    "nvcontainer",
                    "nvidia",
                    "amd",
                    "intel",
                    "googleupdate",
                    "chrome",
                    "msedge",
                    "firefox",
                    "browser",
                    "update",
                    "installer",
                    "setup",
                    "uninstall",
                    "powershell",
                    "cmd",
                    "bash",
                    "wsl",
                    "windows Terminal",
                    "terminal"
                };

                bool isBackgroundProcess = backgroundProcesses.Any(bp => processName.Contains(bp) && !titleLower.Contains(bp));

                // Allow browser windows but filter out their background processes
                if (processName.Contains("chrome") || processName.Contains("msedge") || processName.Contains("firefox"))
                {
                    // Only include if title contains visible window indicators
                    if (!titleLower.Contains("chrome") && !titleLower.Contains("edge") && !titleLower.Contains("firefox"))
                    {
                        isBackgroundProcess = true;
                    }
                }

                // Verificar se é uma janela sem borda (geralmente overlays de sistema)
                long style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_STYLE);
                long exStyle = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);
                
                bool hasBorder = (style & NativeMethods.WS_BORDER) != 0 || 
                                 (style & NativeMethods.WS_THICKFRAME) != 0;
                
                bool isAppWindow = (exStyle & NativeMethods.WS_EX_APPWINDOW) != 0;
                bool isToolWindow = (exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0;
                bool isNoActivate = (exStyle & NativeMethods.WS_EX_NOACTIVATE) != 0;

                // Verificar se a janela tem um pai (janelas filhas não devem ser listadas)
                IntPtr parent = NativeMethods.GetParent(hWnd);
                bool hasParent = parent != IntPtr.Zero;

                // Verificar se a janela tem um owner (janelas com owner geralmente são diálogos)
                IntPtr owner = NativeMethods.GetWindow(hWnd, NativeMethods.GW_OWNER);
                bool hasOwner = owner != IntPtr.Zero && owner != hWnd;

                // Incluir apenas janelas que:
                // 1. Não são janelas de sistema essenciais
                // 2. Não são classes de janela de sistema
                // 3. São janelas de aplicação ou têm bordas visíveis
                // 4. Não são janelas de ferramenta (tool windows)
                // 5. Não são janelas que não podem ser ativadas
                // 6. Não são janelas filhas
                // 7. Não são janelas com owner (diálogos)
                // 8. Não são processos em segundo plano
                if (!isEssentialSystemWindow && !isSystemClass && (hasBorder || isAppWindow) && !isToolWindow && !isNoActivate && !hasParent && !hasOwner && !isBackgroundProcess)
                {
                    result.Add(
                        new WindowInfo
                        {
                            Handle = hWnd,
                            Title = title
                        });
                }

                return true;
            },
            IntPtr.Zero);

        return result
            .OrderBy(x => x.Title)
            .ToList();
    }

    public static void BringToFront(IntPtr hWnd)
    {
        // Restaura a janela se estiver minimizada
        if (NativeMethods.IsIconic(hWnd))
        {
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
        }

        // Traz a janela para frente
        NativeMethods.SetForegroundWindow(hWnd);
    }

    public static string Capture(
        IntPtr windowHandle,
        string destinationFolder)
    {
        NativeMethods.GetWindowRect(
            windowHandle,
            out RECT rect);

        int width =
            rect.Right - rect.Left;

        int height =
            rect.Bottom - rect.Top;

        string fileName =
            $"WINDOW_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        string destination =
            Path.Combine(
                destinationFolder,
                fileName);

        using Bitmap bitmap =
            new(width, height);

        using Graphics graphics =
            Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            new Point(rect.Left, rect.Top),
            Point.Empty,
            new Size(width, height));

        bitmap.Save(
            destination,
            ImageFormat.Png);

        return destination;
    }
}