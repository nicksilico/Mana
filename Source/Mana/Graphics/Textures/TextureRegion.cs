using System.Drawing;

namespace Mana.Graphics.Textures
{
    /// <summary>
    /// Represents a sub-region of a Texture2D object, such as a tile in a tile sheet.
    /// </summary>
    public class TextureRegion
    {
        public readonly Texture2D Texture;
        public readonly Rectangle Region;

        public TextureRegion(Texture2D texture, Rectangle region)
        {
            Texture = texture;
            Region = region;
        }
    }
}