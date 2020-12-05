using Mana.Asset;

namespace Mana.Graphics.Geometry
{
    public class Model : IAsset
    {
        public readonly Mesh[] Meshes;

        public Model(Mesh[] meshes)
        {
            Meshes = meshes;
        }

        public Model(Mesh mesh)
        {
            Meshes = new[] { mesh };
        }

        public string SourcePath { get; set; }

        public AssetManager AssetManager { get; set; }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
            {
                mesh.Dispose();
            }
        }
    }
}