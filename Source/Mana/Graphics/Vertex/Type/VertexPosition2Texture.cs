using System.Numerics;
using System.Runtime.InteropServices;

namespace Mana.Graphics.Vertex.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPosition2Texture : IVertexType
    {
        public Vector2 Position;
        public Vector2 TexCoord;

        public VertexPosition2Texture(Vector2 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
    }
}