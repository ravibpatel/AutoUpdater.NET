using System;
using System.Windows;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace AutoUpdaterDotNET;

internal class Wpf32Window(Window wpfWindow) : IWin32Window
{
    public IntPtr Handle { get; } = new WindowInteropHelper(wpfWindow).EnsureHandle();
}