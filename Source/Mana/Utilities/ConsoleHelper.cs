using System;
using System.Runtime.InteropServices;

namespace Mana.Utilities
{
    public static class ConsoleHelper
    {
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOACTIVATE = 0x10;

        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        /// <summary>
        /// Sets the location and size of the program's console window. This method only has an effect on Windows.
        /// </summary>
        /// <param name="x">The new X position of the console window.</param>
        /// <param name="y">The new Y position of the console window.</param>
        /// <param name="width">The new width of the console window.</param>
        /// <param name="height">The new height of the console window.</param>
        public static void SetWindowPosition(int x, int y, int width, int height)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetWindowPos(ConsoleWindowHandle, IntPtr.Zero, x, y, width, height, SWP_NOACTIVATE | SWP_NOZORDER);
            }
        }

        private static IntPtr ConsoleWindowHandle => GetConsoleWindow();

        public static void Write(string message, ConsoleColor foregroundColor)
        {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            Console.Write(message);

            Console.ForegroundColor = oldForegroundColor;
        }

        public static void Write(string message, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;
            ConsoleColor oldBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            Console.Write(message);

            Console.BackgroundColor = oldBackgroundColor;
            Console.ForegroundColor = oldForegroundColor;
        }

        public static void WriteLine(string message, ConsoleColor foregroundColor)
        {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            Console.WriteLine(message);

            Console.ForegroundColor = oldForegroundColor;
        }

        public static void WriteLine(string message, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;
            ConsoleColor oldBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            Console.WriteLine(message);

            Console.BackgroundColor = oldBackgroundColor;
            Console.ForegroundColor = oldForegroundColor;
        }
    }
}
