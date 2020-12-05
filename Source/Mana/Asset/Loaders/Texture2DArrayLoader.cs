using System;
using System.IO;
using System.Linq;
using Assimp.Configs;
using Mana.Graphics;
using Mana.Graphics.Textures;
using Newtonsoft.Json;

namespace Mana.Asset.Loaders
{
    public class Texture2DArrayLoader : IAssetLoader<Texture2DArray>
    {
        public Texture2DArray Load(AssetManager assetManager,
                                   RenderContext renderContext,
                                   Stream stream,
                                   string sourcePath)
        {
            using StreamReader streamReader = new StreamReader(stream);

            var description = JsonConvert.DeserializeObject<Texture2DArrayDescription>(streamReader.ReadToEnd());

            if (description.Layers.Length == 0)
            {
                throw new InvalidOperationException("Cannot create Texture2DArray with no layers. Is the description invalid?");
            }

            if (description.Width <= 0 || description.Height <= 0)
            {
                throw new InvalidOperationException("Cannot create a Texture2DArray with a dimension of less than one.");
            }

            string containingPath = Path.GetDirectoryName(sourcePath)
                                    ?? throw new Exception($"Error getting parent for path: {sourcePath}");

            string[] paths = description.Layers
                                        .Select(x => Path.Combine(containingPath, x))
                                        .ToArray();

            var array = Texture2DArray.CreateEmpty(renderContext,
                                                   description.Width,
                                                   description.Height,
                                                   description.Layers.Length);

            for (int i = 0; i < paths.Length; i++)
            {
                string currentPath = paths[i];

                using var subStream = File.OpenRead(currentPath);

                array.SetDataFromStream(renderContext, i, subStream);
            }

            return array;
        }

    }

    [Serializable]
    internal class Texture2DArrayDescription
    {
        public int Width { get; set; } = -1;
        public int Height { get; set; } = -1;
        public string[] Layers { get; set; } = { };
    }
}
