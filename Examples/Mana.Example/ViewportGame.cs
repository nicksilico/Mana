using System.Threading;
using ImGuiNET;
using Mana.Graphics;
using Mana.IMGUI;
using Mana.IMGUI.Viewport;
using Mana.Utilities;

namespace Mana.Example
{
    public class ViewportGame : ExampleGame
    {
        private static Logger _log = Logger.Create();

        private ImGuiViewportWindow _window;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float time, float deltaTime)
        {
        }

        public override void Render(float time, float deltaTime)
        {
            RenderContext.Clear(Color.CornflowerBlue);

            ImGuiHelper.BeginGlobalDocking(false);
                
            ImGui.Begin("Viewport");

            ImGui.End();
            
            ImGui.ShowMetricsWindow();
            
        }
    }
}