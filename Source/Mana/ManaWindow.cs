using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Mana.Audio;
using Mana.Audio.OpenAL;
using Mana.Graphics;
using Mana.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Rectangle = System.Drawing.Rectangle;

namespace Mana
{
    public class ManaWindow : GameWindow, IGameHost
    {
        private static Logger _log = Logger.Create();
        
        private InitializationParameters _pendingInitializationParameters;

        private GLFWCallbacks.CharCallback _glfwCharCallback;

        private Game _game;
        private float _elapsedTime;
        private Matrix4 _projectionMatrix;

        public ManaWindow(InitializationParameters initializationParameters)
            : base(GameWindowSettings.Default, GetNativeWindowSettings(initializationParameters))
        {
            _pendingInitializationParameters = initializationParameters;

            if (MainWindow == null)
            {
                MainWindow = this;
            }
            else
            {
                throw new InvalidOperationException("Only one ManaWindow is supported at this time.");
            }
            
            Console.Clear();
            Console.Title = "Mana Console";

            _log.Info($"Current Working Directory: {Directory.GetCurrentDirectory()}");

            InputProvider = new ManaWindowInputProvider(this);
            Input.CurrentProvider = InputProvider;
            
            RenderContext = RenderContext.WrapWindowContext(this);

            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0f, Width, Height, 0f, -1f, 1f);
        }
        
        public static ManaWindow MainWindow { get; private set; }
        
        public bool RenderOnResize { get; set; } = true;
        
        public bool ExitOnEscape { get; set; } = true;

        protected override void OnLoad()
        {
            base.OnLoad();
            
            VSync = _pendingInitializationParameters.VSync;
            if (_pendingInitializationParameters.FullscreenState == FullscreenState.Fullscreen)
            {
                WindowState = WindowState.Fullscreen;
            }
            
            unsafe
            {
                _glfwCharCallback = GLFW_CharCallback;
                GLFW.SetCharCallback(this.WindowPtr, _glfwCharCallback);                
            }
            
            _game.OnBeforeRun(this);
        }

        /// <summary>
        /// Gets the <see cref="InputProvider"/> for this <see cref="ManaWindow"/>.
        /// </summary>
        public IInputProvider InputProvider { get; }

        public RenderContext RenderContext { get; }

        public int Height
        {
            get => Size.Y;
            set
            {
                var newSize = Size;
                newSize.Y = value;
                Size = newSize;
            }
        }
        
        public int Width
        {
            get => Size.X;
            set
            {
                var newSize = Size;
                newSize.X = value;
                Size = newSize;
            }
        }
        
        public float AspectRatio => Width / (float)Height;

        private FullscreenState _fullscreenState;
        private FullscreenState _pendingFullscreenState;

        public FullscreenState FullscreenState
        {
            get => _fullscreenState;
            set => _pendingFullscreenState = value;
        }

        public ref Matrix4 ProjectionMatrix => ref _projectionMatrix;

        public void Maximize()
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        public void Run(Game game)
        {
            _game = game;
         
            AudioBackend audioBackend = new OpenALBackend();
            _game.AddGameSystem(audioBackend);
            
            Run();
        }

        public Image<Rgba32> TakeScreenshot()
        {
            var image = new Image<Rgba32>(ClientSize.X, ClientSize.Y);
            
            GL.ReadPixels(0,
                          0,
                          image.Width,
                          image.Height,
                          PixelFormat.Rgba,
                          PixelType.UnsignedByte,
                          ref MemoryMarshal.GetReference(ImageHelper.GetPixelSpan(image)));

            image.Mutate(c => c.Flip(FlipMode.Vertical));

            return image;
        }

        public void ToggleFullscreen()
        {
            if (FullscreenState == FullscreenState.Fullscreen)
            {
                FullscreenState = FullscreenState.Windowed;
            }
            else
            {
                FullscreenState = FullscreenState.Fullscreen;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            InputProvider.Update();

            base.OnUpdateFrame(e);

            _elapsedTime += (float)e.Time;

            _game?.UpdateBase(_elapsedTime, (float)e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _game?.RenderBase(_elapsedTime, (float)e.Time);

            if (_pendingFullscreenState != _fullscreenState)
            {
                ApplyFullscreenState(_pendingFullscreenState);
                _fullscreenState = _pendingFullscreenState;
            }

            unsafe
            {
                GLFW.SwapBuffers(this.WindowPtr);                
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (ExitOnEscape && e.Key == Keys.Escape)
                Environment.Exit(0);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            if (RenderContext == null)
                return;
            
            RenderContext.ViewportRectangle = new Rectangle(0, 0, Width, Height);
            RenderContext.ScissorRectangle = new Rectangle(0, 0, Width, Height);
            
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0f, Width, Height, 0f, -1f, 1f);
            
            if (RenderOnResize && _elapsedTime > 5)
            {
                unsafe
                {
                    _game?.RenderBase(_elapsedTime, float.Epsilon);
                    GLFW.SwapBuffers(this.WindowPtr);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            AudioBackend.Instance.Dispose();
            
            _game.Dispose();

            base.OnClosing(e);
        }
        
        private unsafe void GLFW_CharCallback(Window* window, uint codepoint)
        {
            Input.OnKeyTyped((char)codepoint);
        }

        private void ApplyFullscreenState(FullscreenState fullscreenState)
        {
            switch (fullscreenState)
            {
                case FullscreenState.Windowed:
                    WindowBorder = WindowBorder.Resizable;
                    WindowState = WindowState.Normal;
                    break;
                case FullscreenState.Fullscreen:
                    WindowState = WindowState.Fullscreen;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fullscreenState), fullscreenState, null);
            }
        }
        
        private static NativeWindowSettings GetNativeWindowSettings(InitializationParameters init)
        {
            return new NativeWindowSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(init.OpenGLMajorVersion, init.OpenGLMinorVersion),
                AutoLoadBindings = true,
                Size = new Vector2i(init.Width, init.Height),
                Title = init.Title,
            };
        }
    }
}
