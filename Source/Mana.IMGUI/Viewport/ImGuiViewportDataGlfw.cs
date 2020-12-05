using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Mana.IMGUI.Viewport
{
    public struct ViewportData
    {
        public IntPtr Window;
        public bool WindowOwned;
        public int IgnoreWindowPosEventFrame;
        public int IgnoreWindowSizeEventFrame;
        public int Key;

        public ViewportData(IntPtr window, bool windowOwned, int ignoreWindowPosEventFrame, int ignoreWindowSizeEventFrame, int key)
        {
            Window = window;
            WindowOwned = windowOwned;
            IgnoreWindowPosEventFrame = ignoreWindowPosEventFrame;
            IgnoreWindowSizeEventFrame = ignoreWindowSizeEventFrame;
            Key = key;
        }

        public static ViewportData Default => new ViewportData(IntPtr.Zero, false, -1, -1, -1);

    }
}