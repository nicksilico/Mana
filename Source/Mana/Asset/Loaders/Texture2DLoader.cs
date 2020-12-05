using System.IO;
using Mana.Graphics;
using Mana.Graphics.Textures;

namespace Mana.Asset.Loaders
{
    public class Texture2DLoader : IAssetLoader<Texture2D>
    {
        public Texture2D Load(AssetManager manager,
                              RenderContext renderContext,
                              Stream sourceStream,
                              string sourcePath)
        {
            return Texture2D.CreateFromStream(renderContext, sourceStream);
        }
    }
}