using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Mana.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Mana
{
    /// <summary>
    /// Represents a class that handles input management for a <see cref="ManaWindow"/> instance.
    /// </summary>
    public class ManaWindowInputProvider : IInputProvider
    {
        private static Logger _log = new Logger("Input");

        public const int SUPPORTED_GAMEPAD_COUNT = 4;

        private bool _isMouseInsideWindow = false;

        public event Action<char> KeyTyped;
        public event Action<int> GamePadConnected;
        public event Action<int> GamePadDisconnected;

        public ManaWindowInputProvider(ManaWindow window)
        {
            Window = window;
            Window.MouseEnter += WindowOnMouseEnter;
            Window.MouseLeave += WindowOnMouseLeave;
            Window.MouseMove += WindowOnMouseMove;

            KeyTyped += Input.OnKeyTyped;
        }

        private void WindowOnMouseMove(MouseMoveEventArgs obj)
        {
        }

        private void WindowOnMouseLeave()
        {
            _isMouseInsideWindow = false;
        }

        private void WindowOnMouseEnter()
        {
            _isMouseInsideWindow = true;
        }

        public ManaWindow Window { get; }

        public bool MainWindowHovered => _isMouseInsideWindow;

        public Vector2 MousePosition => Window.MousePosition;

        public Vector2 MousePositionDelta => Window.MouseState.Delta;

        public bool MouseLeft => Window.IsMouseButtonDown(MouseButton.Left);
        public bool MouseMiddle => Window.IsMouseButtonDown(MouseButton.Middle);
        public bool MouseRight => Window.IsMouseButtonDown(MouseButton.Right);
        
        public int MouseWheel { get; }
        public int MouseWheelDelta { get; }

        public void Update()
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyDown(Keys key)
        {
            if (!Window.IsFocused)
                return false;

            return Window.IsKeyDown(key);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyUp(Keys key)
        {
            if (!Window.IsFocused)
                return true;

            return !Window.IsKeyDown(key);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAnyKeyDown()
        {
            if (!Window.IsFocused)
                return false;

            return Window.IsAnyKeyDown;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WasKeyPressed(Keys key)
        {
            if (!Window.IsFocused)
                return false;

            return Window.KeyboardState.WasKeyPressed(key);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WasKeyReleased(Keys key)
        {
            if (!Window.IsFocused)
                return false;

            return Window.KeyboardState.WasKeyReleased(key);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseDown(MouseButton button)
        {
            if (!Window.IsFocused)
                return false;

            return Window.MouseState.IsButtonDown(button);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseUp(MouseButton button)
        {
            if (!Window.IsFocused)
                return true;

            return !Window.MouseState.IsButtonDown(button);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WasMousePressed(MouseButton button)
        {
            if (!Window.IsFocused)
                return false;

            return Window.MouseState.IsButtonDown(button) && !Window.MouseState.WasButtonDown(button);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WasMouseReleased(MouseButton button)
        {
            if (!Window.IsFocused)
                return false;

            return !Window.MouseState.IsButtonDown(button) && Window.MouseState.WasButtonDown(button);
        }

        public bool IsButtonDown(int button, int index = 0)
        {
            throw new NotImplementedException();
        }

        public bool Control => Window.IsFocused && (IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl));

        public bool Alt => Window.IsFocused && (IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt));

        public bool Shift =>  Window.IsFocused && (IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift));
    }
}
