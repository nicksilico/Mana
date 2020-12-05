using System;
using System.IO;
using Mana.Graphics;
using Mana.Graphics.Textures;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mana.Asset.Loaders
{
    public class TextureCubeMapLoader : IAssetLoader<TextureCubeMap>
    {
        public TextureCubeMap Load(AssetManager assetManager, RenderContext renderContext, Stream stream, string sourcePath)
        {
            using StreamReader streamReader = new StreamReader(stream);

            var description = JsonConvert.DeserializeObject<CubeMapDescription>(streamReader.ReadToEnd());

            string containingPath = Path.GetDirectoryName(sourcePath)
                                    ?? throw new Exception($"Error getting parent for path: {sourcePath}");

            var cubemap = new TextureCubeMap(renderContext);

            using var front = LoadImage(assetManager, description.Front, containingPath);
            using var back = LoadImage(assetManager, description.Back, containingPath);
            using var left = LoadImage(assetManager, description.Left, containingPath);
            using var right = LoadImage(assetManager, description.Right, containingPath);
            using var up = LoadImage(assetManager, description.Up, containingPath);
            using var down = LoadImage(assetManager, description.Down, containingPath);

            cubemap.SetImages(renderContext, front, back, left, right, up, down);

            return cubemap;
        }

        private Image<Rgba32> LoadImage(AssetManager assetManager, string relativePath, string containingPath)
        {
            var facePath = Path.Combine(containingPath, relativePath);
            return Image.Load<Rgba32>(assetManager.GetStreamFromPath(facePath));
        }

        [Serializable]
        public class CubeMapDescription
        {
            public string Front;
            public string Back;
            public string Left;
            public string Right;
            public string Up;
            public string Down;
        }
    }
}
