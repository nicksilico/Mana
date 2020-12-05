using System;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Mana.IMGUI
{
    public static class ImGuiKeyHelper
    {
        public static Keys ToOpenTKKey(ImGuiKey key)
        {
            switch (key)
            {
                case ImGuiKey.Tab:
                    return Keys.Tab;
                case ImGuiKey.LeftArrow:
                    return Keys.Left;
                case ImGuiKey.RightArrow:
                    return Keys.Right;
                case ImGuiKey.UpArrow:
                    return Keys.Up;
                case ImGuiKey.DownArrow:
                    return Keys.Down;
                case ImGuiKey.PageUp:
                    return Keys.PageUp;
                case ImGuiKey.PageDown:
                    return Keys.PageDown;
                case ImGuiKey.Home:
                    return Keys.Home;
                case ImGuiKey.End:
                    return Keys.End;
                case ImGuiKey.Insert:
                    return Keys.Insert;
                case ImGuiKey.Delete:
                    return Keys.Delete;
                case ImGuiKey.Backspace:
                    return Keys.Backspace;
                case ImGuiKey.Space:
                    return Keys.Space;
                case ImGuiKey.Enter:
                    return Keys.Enter;
                case ImGuiKey.Escape:
                    return Keys.Escape;
                case ImGuiKey.A:
                    return Keys.A;
                case ImGuiKey.C:
                    return Keys.C;
                case ImGuiKey.V:
                    return Keys.V;
                case ImGuiKey.X:
                    return Keys.X;
                case ImGuiKey.Y:
                    return Keys.Y;
                case ImGuiKey.Z:
                    return Keys.Z;
                case ImGuiKey.KeyPadEnter:
                    return Keys.KeyPadEnter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
