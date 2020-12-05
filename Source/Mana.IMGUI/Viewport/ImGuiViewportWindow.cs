using Mana.Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Mana.IMGUI.Viewport
{
    public class ImGuiViewportWindow : GameWindow
    {
        private ImGuiViewportWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        public static ImGuiViewportWindow CreateWindow(int width, int height, RenderContext renderContext)
        {
            var window = new ImGuiViewportWindow(GameWindowSettings.Default, 
                                           new NativeWindowSettings
            {
                Size = new Vector2i(width, height),
                SharedContext = renderContext.OpenGLContext as IGLFWGraphicsContext,
            });
            
            renderContext.OpenGLContext.MakeCurrent();

            return window;
        }
    }
}