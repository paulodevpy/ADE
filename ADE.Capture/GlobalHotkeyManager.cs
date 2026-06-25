using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ADE.Capture;

public class GlobalHotkeyManager : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _proc;
    private Action? _onHotkeyPressed;
    private bool _ctrlPressed;
    private bool _shiftPressed;
    private bool _aPressed;

    public void RegisterHotkey(Action onHotkeyPressed)
    {
        _onHotkeyPressed = onHotkeyPressed;
        _proc = HookCallback;
        _hookId = SetHook(_proc);
    }

    private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        if (curModule != null)
        {
            return NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                proc,
                NativeMethods.GetModuleHandle(curModule.ModuleName),
                0);
        }
        return IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            // Check for key events
            if (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN)
            {
                if (vkCode == 0x11) // VK_CONTROL
                    _ctrlPressed = true;
                else if (vkCode == 0x10) // VK_SHIFT
                    _shiftPressed = true;
                else if (vkCode == 0x41) // VK_A
                    _aPressed = true;

                // Check if Ctrl+Shift+A is pressed
                if (_ctrlPressed && _shiftPressed && _aPressed)
                {
                    _onHotkeyPressed?.Invoke();
                    // Reset to prevent multiple triggers
                    _aPressed = false;
                }
            }
            else if (wParam == (IntPtr)NativeMethods.WM_KEYUP || wParam == (IntPtr)NativeMethods.WM_SYSKEYUP)
            {
                if (vkCode == 0x11) // VK_CONTROL
                    _ctrlPressed = false;
                else if (vkCode == 0x10) // VK_SHIFT
                    _shiftPressed = false;
                else if (vkCode == 0x41) // VK_A
                    _aPressed = false;
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }
}
