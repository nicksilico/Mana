using System;
using System.Numerics;
using Mana.Graphics.Buffers;
using Mana.Graphics.Shaders;
using Mana.Graphics.Textures;
using Mana.Graphics.Vertex.Types;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Geometry
{
    public class Mesh : IDisposable
    {
        public VertexPositionNormalTexture[] Vertices;
        public uint[] Indices;
        public Matrix4x4 Transform;
        public MeshTextureInfo Textures;

        public readonly VertexBuffer VertexBuffer;
        public readonly IndexBuffer IndexBuffer;

        public Mesh(RenderContext parentContext,
                    VertexPositionNormalTexture[] vertices,
                    uint[] indices)
            : this(parentContext, vertices, indices, null, Matrix4x4.Identity)
        {
        }

        public Mesh(RenderContext parentContext,
                    VertexPositionNormalTexture[] vertices,
                    uint[] indices,
                    MeshTextureInfo textures,
                    Matrix4x4 transform)
        {
            Vertices = vertices;
            Indices = indices;

            VertexBuffer = VertexBuffer.Create(parentContext,
                                                vertices,
                                                BufferUsageHint.StaticDraw,
                                                true);

            IndexBuffer = IndexBuffer.Create(parentContext,
                                              indices,
                                              BufferUsageHint.StaticDraw,
                                              true);

            Textures = textures;
            Transform = transform;
        }

        public void Render(RenderContext renderContext, ShaderProgram shaderProgram)
        {
            shaderProgram.TrySetUniform("transform", ref Transform);
            renderContext.Render(PrimitiveType.Triangles, VertexBuffer, IndexBuffer, shaderProgram);
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public class MeshTextureInfo
        {
            public Texture2D Diffuse;
            public Texture2D Specular;
            public Texture2D MetallicRoughness;
        }
    }
}
