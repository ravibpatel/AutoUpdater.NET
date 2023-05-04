using System;
using System.Windows;
using System.Windows.Interop;

namespace AutoUpdaterDotNET
{
    internal class Wpf32Window : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public Wpf32Window(Window wpfWindow)
        {
            Handle = new WindowInteropHelper(wpfWindow).Handle;
        }
    }
}
