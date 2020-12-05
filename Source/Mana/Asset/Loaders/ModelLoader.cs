using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Assimp;
using Mana.Graphics;
using Mana.Graphics.Geometry;
using Mana.Graphics.Textures;
using Mana.Graphics.Vertex.Types;
using OpenTK.Graphics.OpenGL4;
using AssimpMesh = Assimp.Mesh;
using AssimpFace = Assimp.Face;
using Mesh = Mana.Graphics.Geometry.Mesh;
using AssimpMaterial = Assimp.Material;
using Matrix4x4 = Assimp.Matrix4x4;

namespace Mana.Asset.Loaders
{
    public class ModelLoader : IAssetLoader<Model>
    {
        public readonly AssimpContext Importer;

        private const bool LOG_TEXTURES = false;

        public ModelLoader()
        {
            Importer = new AssimpContext();
        }

        public Model Load(AssetManager assetManager, RenderContext renderContext, Stream stream, string sourcePath)
        {
            Scene scene = Importer.ImportFile(sourcePath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

            if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            {
                throw new Exception("Assimp Error.");
            }

            var meshes = new List<Mesh>();

            string directory = Path.GetDirectoryName(sourcePath);

            Texture2D LoadFromTextureSlot(TextureSlot textureSlot, TextureType textureType)
            {
                var texture = assetManager.Load<Texture2D>(Path.Combine(directory, textureSlot.FilePath));
                texture.SetInfoFromTextureSlot(renderContext, ref textureSlot);

                texture.SetMinFilter(renderContext, TextureMinFilter.NearestMipmapLinear);
                return texture;
            }

            List<Texture2D> LoadMaterialTextures(AssimpMaterial material, TextureType textureType)
            {
                List<Texture2D> textures = new List<Texture2D>();
                for (int i = 0; i < material.GetMaterialTextureCount(textureType); i++)
                {
                    material.GetMaterialTexture(textureType, i, out var textureSlot);

                    if (textureSlot.FilePath != null)
                    {
                        if (textureSlot.FilePath.Contains("*"))
                        {
                            throw new InvalidOperationException();
                        }

                        var texture = assetManager.Load<Texture2D>(Path.Combine(directory, textureSlot.FilePath));
                        texture.SetInfoFromTextureSlot(renderContext, ref textureSlot);
                        textures.Add(texture);
                    }
                }
                return textures;
            }

            Mesh ProcessMesh(AssimpMesh mesh, System.Numerics.Matrix4x4 transformMatrix)
            {
                VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[mesh.VertexCount];
                List<uint> indices = new List<uint>();

                // Process vertices
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    Vector3D position = mesh.Vertices[i];
                    vertices[i].Position = Unsafe.As<Vector3D, Vector3>(ref position);
                    Vector3D normal = mesh.Normals[i];
                    vertices[i].Normal = Unsafe.As<Vector3D, Vector3>(ref normal);

                    if (mesh.HasTextureCoords(0))
                    {
                        Vector3D texCoord = mesh.TextureCoordinateChannels[0][i];
                        vertices[i].TexCoord = new Vector2(texCoord.X, 1 - texCoord.Y);
                    }
                }

                // Process indices
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    AssimpFace face = mesh.Faces[i];
                    for (int j = 0; j < face.IndexCount; j++)
                    {
                        indices.Add((uint)face.Indices[j]);
                    }
                }

                Mesh.MeshTextureInfo textures = new Mesh.MeshTextureInfo();

                // Process Material
                if (mesh.MaterialIndex >= 0)
                {
                    AssimpMaterial material = scene.Materials[mesh.MaterialIndex];

                    var texturezzz = material.GetAllMaterialTextures();

                    texturezzz = texturezzz.Where(x => x.FilePath != null).ToArray();

                    if (material.HasTextureDiffuse)
                    {
                        textures.Diffuse = LoadFromTextureSlot(material.TextureDiffuse, TextureType.Diffuse);
                    }

                    if (material.HasTextureSpecular)
                    {
                        textures.Specular = LoadFromTextureSlot(material.TextureSpecular, TextureType.Specular);
                    }

                    if (material.HasTextureLightMap)
                    {
                        textures.MetallicRoughness = LoadFromTextureSlot(material.TextureLightMap, TextureType.Lightmap);
                    }
                }

                return new Mesh(renderContext, vertices, indices.ToArray(), textures, transformMatrix);
            }

            void ProcessNode(Node node, System.Numerics.Matrix4x4 transform)
            {
                var t = node.Transform;
                var thisTransform = Unsafe.As<Matrix4x4, System.Numerics.Matrix4x4>(ref t);

                // Process all of the node's meshes
                for (int i = 0; i < node.MeshCount; i++)
                {
                    AssimpMesh mesh = scene.Meshes[node.MeshIndices[i]];
                    meshes.Add(ProcessMesh(mesh, transform * thisTransform));
                }

                // Recurse
                for (int i = 0; i < node.ChildCount; i++)
                {
                    ProcessNode(node.Children[i], transform * thisTransform);
                }
            }

            ProcessNode(scene.RootNode, System.Numerics.Matrix4x4.Identity);

            return new Model(meshes.ToArray());
        }
    }
}
