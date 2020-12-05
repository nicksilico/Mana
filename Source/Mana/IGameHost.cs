using Mana.Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Mana
{
    public interface IGameHost
    {
        int Width { get; set; }
        int Height { get; set; }
        string Title { get; set; }
        float AspectRatio { get; }
        FullscreenState FullscreenState { get; set; }
        VSyncMode VSync { get; set; }

        RenderContext RenderContext { get; }
        ref Matrix4 ProjectionMatrix { get; }

        bool IsFocused { get; }

        void Run(Game game);

        void Close();

        void Maximize();
    }
}
