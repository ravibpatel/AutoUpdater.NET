using System;
using System.Windows;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace AutoUpdaterDotNET;

internal class Wpf32Window : IWin32Window
{
    public Wpf32Window(Window wpfWindow)
    {
        Handle = new WindowInteropHelper(wpfWindow).EnsureHandle();
    }

    public IntPtr Handle { get; }
}