using System.Drawing;
using System.Numerics;
using Mana.Graphics.Buffers;
using Mana.Graphics.Shaders;
using Mana.Graphics.Textures;
using Mana.Graphics.Vertex.Types;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Text
{
    internal static class FontAtlasHelper
    {
        public static void FlipFontAtlas(RenderContext renderContext, Texture2D texture)
        {
            var previousFrameBuffer = renderContext.FrameBuffer;
            var previousScissorTest = renderContext.ScissorTest;
            var previousDepthTest = renderContext.DepthTest;
            var previousViewport = renderContext.ViewportRectangle;
            var previousCullBackfaces = renderContext.CullBackfaces;
            var previousBlend = renderContext.Blend;
            renderContext.GetBlendFunc(out var previousSFactor, out var previousDFactor);

            renderContext.ScissorTest = false;
            renderContext.DepthTest = false;
            renderContext.ViewportRectangle = new Rectangle(0, 0, texture._width, texture._height);
            renderContext.CullBackfaces = false;
            renderContext.SetBlendFunc(BlendingFactor.One, BlendingFactor.Zero);

            var shaderProgram = CreateAtlasFlippingProgram(renderContext);

            var tempFrameBuffer = new FrameBuffer(renderContext,
                                                         texture._width,
                                                         texture._height,
                                                         FrameBufferFlags.Color);

            var atlasFrameBuffer = new FrameBuffer(renderContext, texture);

            VertexPosition2Texture[] vertices =
            {
                new VertexPosition2Texture(new Vector2(0, 0), new Vector2(0, 1)),
                new VertexPosition2Texture(new Vector2(texture._width, 0), new Vector2(1, 1)),
                new VertexPosition2Texture(new Vector2(texture._width, texture._height), new Vector2(1, 0)),
                new VertexPosition2Texture(new Vector2(0, texture._height), new Vector2(0, 0)),
            };

            uint[] indices =
            {
                2, 1, 0,
                2, 0, 3,
            };

            var vertexBuffer = VertexBuffer.Create(renderContext, vertices, BufferUsageHint.StaticDraw, true);
            var indexBuffer = IndexBuffer.Create(renderContext, indices, BufferUsageHint.StaticDraw, true);

            // Draw atlas texture to temporary frame buffer.
            renderContext.BindFrameBuffer(tempFrameBuffer);
            renderContext.Clear(Color.Transparent);

            renderContext.BindTexture2D(0, texture);

            shaderProgram.SetUniform("projection",
                                     Matrix4x4.CreateOrthographicOffCenter(0f,
                                                                           texture._width,
                                                                           texture._height,
                                                                           0f,
                                                                           -1f,
                                                                           1f));

            renderContext.Render(PrimitiveType.Triangles, vertexBuffer, indexBuffer, shaderProgram);


            // Draw temporary render target to atlas texture.
            renderContext.BindFrameBuffer(atlasFrameBuffer);
            renderContext.Clear(Color.Transparent);

            renderContext.BindTexture2D(0, tempFrameBuffer.ColorTexture);

            shaderProgram.SetUniform("projection",
                                     Matrix4x4.CreateOrthographicOffCenter(0f,
                                                                           texture._width,
                                                                           0f,
                                                                           texture._height,
                                                                           -1f,
                                                                           1f));

            renderContext.Render(PrimitiveType.Triangles, vertexBuffer, indexBuffer, shaderProgram);

            renderContext.ViewportRectangle = previousViewport;
            renderContext.DepthTest = previousDepthTest;
            renderContext.ScissorTest = previousScissorTest;
            renderContext.CullBackfaces = previousCullBackfaces;
            renderContext.Blend = previousBlend;
            renderContext.SetBlendFunc(previousSFactor, previousDFactor);

            renderContext.BindFrameBuffer(previousFrameBuffer);

            vertexBuffer.Dispose();
            indexBuffer.Dispose();

            tempFrameBuffer.Dispose();
            atlasFrameBuffer.Dispose();
            shaderProgram.Dispose();
        }

        public static ShaderProgram CreateAtlasFlippingProgram(RenderContext renderContext)
        {
            VertexShader vertexShader = new VertexShader(renderContext,
                                                         @"#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 projection;

void main()
{
    gl_Position = projection * vec4(aPos, 1.0, 1.0);
    TexCoord = aTexCoord;
}");

            FragmentShader fragmentShader = new FragmentShader(renderContext,
                                                               @"#version 330 core

out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D texture0;

void main()
{
    vec4 samp = vec4(1.0, 1.0, 1.0, texture(texture0, TexCoord).a);
    FragColor = samp;
}");

            ShaderProgram shaderProgram = new ShaderProgram(renderContext);

            shaderProgram.AttachShader(vertexShader);
            shaderProgram.AttachShader(fragmentShader);

            shaderProgram.Link();

            shaderProgram.DetachShader(vertexShader);
            shaderProgram.DetachShader(fragmentShader);

            vertexShader.Dispose();
            fragmentShader.Dispose();

            shaderProgram.Label = "Font Import ShaderProgram";

            return shaderProgram;
        }
    }
}
