using OpenTK.Windowing.Common;

namespace Mana
{
    public class InitializationParameters
    {
        public static InitializationParameters Default => new InitializationParameters();

        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;
        public string Title { get; set; } = "Mana Window";
        public FullscreenState FullscreenState { get; set; } = FullscreenState.Windowed;
        public int OpenGLMajorVersion { get; set; } = 4;
        public int OpenGLMinorVersion { get; set; } = 6;
        public VSyncMode VSync { get; set; } = VSyncMode.On;
    }
}