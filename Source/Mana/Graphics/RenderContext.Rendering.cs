using Mana.Graphics.Buffers;
using Mana.Graphics.Geometry;
using Mana.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics
{
    public partial class RenderContext
    {
        public void Clear(Color color)
        {
            ClearColor = color;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Render(PrimitiveType primitiveType, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, ShaderProgram shaderProgram)
        {
            BindVertexBuffer(vertexBuffer);
            BindIndexBuffer(indexBuffer);
            BindShaderProgram(shaderProgram);

            vertexBuffer.VertexTypeInfo.Apply(shaderProgram);

            GL.DrawElements(primitiveType, indexBuffer.Count, indexBuffer.DrawElementsType, 0);
        }

        public void Render(Model model, ShaderProgram shaderProgram)
        {
            foreach (var mesh in model.Meshes)
            {
                mesh.Render(this, shaderProgram);
            }
        }
    }
}