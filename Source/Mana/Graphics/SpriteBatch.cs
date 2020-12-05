using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Text;
using Mana.Graphics.Buffers;
using Mana.Graphics.Shaders;
using Mana.Graphics.Sprite;
using Mana.Graphics.Text;
using Mana.Graphics.Textures;
using Mana.Graphics.Vertex.Types;
using Mana.Utilities;
using OpenTK.Graphics.OpenGL4;
using BufferUsageHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace Mana.Graphics
{
    /// <summary>
    /// Used for efficient sprite rendering via batches.
    /// </summary>
    public class SpriteBatch : IDisposable
    {
        private static Logger _log = Logger.Create();

        private const int MAX_BATCH_SIZE = 3000;

        private VertexBuffer _vertexBuffer;
        private int _vertexBufferSize;
        private VertexPosition2TextureColor[] _vertexData;

        private IndexBuffer _indexBuffer;
        private int _indexBufferSize;
        private ushort[] _indexData;

        private Texture2D _lastTexture;
        private ShaderProgram _lastShader;

        private int _count = 0;
        private bool _active = false;
        private ShaderProgram _shaderProgram;
        private Matrix4x4 _transform = Matrix4x4.Identity;

        private bool _previousDepthTest;

        public SpriteBatch(RenderContext renderContext)
        {
            RenderContext = renderContext;

            _vertexBufferSize = 64;
            _indexBufferSize = 96;

            CreateVertexBuffer();
            _vertexData = new VertexPosition2TextureColor[_vertexBufferSize];

            CreateIndexBuffer();
            _indexData = new ushort[_indexBufferSize];
        }

        public RenderContext RenderContext { get; }

        public void Begin(ShaderProgram shaderProgram, Matrix4x4 transform)
        {
            if (_active)
                throw new InvalidOperationException("SpriteBatch already began.");

            _count = 0;
            _active = true;
            _shaderProgram = shaderProgram;
            _transform = transform;

            _shaderProgram.TrySetUniform("transform", ref transform);

            _previousDepthTest = RenderContext.DepthTest;
            RenderContext.DepthTest = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Begin(ShaderProgram shaderProgram)
        {
            Begin(shaderProgram, Matrix4x4.Identity);
        }

        public void End()
        {
            if (!_active)
                throw new InvalidOperationException("End() must be called before Begin() may be called.");

            if (_count != 0)
                Flush();

            _active = false;

            RenderContext.DepthTest = _previousDepthTest;
        }

        // public void DrawQuad(Texture2D texture,
        //                      VertexPosition2TextureColor bl,
        //                      VertexPosition2TextureColor br,
        //                      VertexPosition2TextureColor tr,
        //                      VertexPosition2TextureColor tl)
        // {
        //     if (!_active)
        //         throw new InvalidOperationException("Begin() must be called before SpriteBatch may be used for drawing.");
        //
        //     if (texture == null)
        //         throw new ArgumentNullException(nameof(texture));
        //
        //     FlushIfNeeded(texture);
        //
        //     _count++;
        //     EnsureBufferLargeEnough();
        //
        //     int vertexOffset = (_count - 1) * 4;
        //     int indexOffset = (_count - 1) * 6;
        //
        //     _vertexData[vertexOffset + 0] = bl;
        //     _vertexData[vertexOffset + 1] = br;
        //     _vertexData[vertexOffset + 2] = tr;
        //     _vertexData[vertexOffset + 3] = tl;
        //
        //     _indexData[indexOffset + 0] = (ushort)(vertexOffset + 0);
        //     _indexData[indexOffset + 1] = (ushort)(vertexOffset + 1);
        //     _indexData[indexOffset + 2] = (ushort)(vertexOffset + 2);
        //     _indexData[indexOffset + 3] = (ushort)(vertexOffset + 0);
        //     _indexData[indexOffset + 4] = (ushort)(vertexOffset + 2);
        //     _indexData[indexOffset + 5] = (ushort)(vertexOffset + 3);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(TextureRegion textureRegion, Rectangle destination, SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(textureRegion, destination, Color.White, flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(TextureRegion textureRegion, Rectangle destination, Color color, SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            if (textureRegion == null)
                throw new ArgumentNullException(nameof(textureRegion));

            Draw(textureRegion.Texture, destination, textureRegion.Region, color, flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Tileset tileset, int tileX, int tileY, Rectangle destination, Color color, SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            if (tileset == null)
                throw new ArgumentNullException(nameof(tileset));

            Draw(tileset.Texture2D, destination, tileset.GetTileRegion(tileX, tileY), color, flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Texture2D texture, Vector2 position, SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(texture,
                 new Rectangle((int)position.X,
                               (int)position.Y,
                               texture._width,
                               texture._height),
                 new Rectangle(0,
                               0,
                               texture._width,
                               texture._height),
                 Color.White,
                 flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Texture2D texture, Rectangle destination, SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(texture, destination, new Rectangle(0, 0, texture._width, texture._height), Color.White, flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Texture2D texture, Rectangle destination, Color color, SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(texture, destination, new Rectangle(0, 0, texture._width, texture._height), color, flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Texture2D texture,
                         Rectangle destination,
                         Rectangle source,
                         SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(texture, destination, source, Color.White, flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Texture2D texture,
                         Rectangle destination,
                         Rectangle source,
                         Color color,
                         SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(texture,
                 new Vector2(destination.X, destination.Y),
                 new Vector2(destination.X + destination.Width, destination.Y + destination.Height),
                 source,
                 color,
                 flipMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(TextureRegion textureRegion,
                         Rectangle destination,
                         Color color,
                         float rotation,
                         Vector2 origin,
                         SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(textureRegion.Texture, destination, textureRegion.Region, color, rotation, origin, flipMode);
        }

        public void Draw(Texture2D texture,
                         Rectangle destination,
                         Rectangle source,
                         Color color,
                         float rotation,
                         Vector2 origin,
                         SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            float x = destination.X;
            float y = destination.Y;
            float dx = -origin.X;
            float dy = -origin.Y;
            float w = destination.Width;
            float h = destination.Height;
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);

            Vector2 topLeft = new Vector2(x+dx*cos-dy*sin, y+dx*sin+dy*cos);
            Vector2 topRight = new Vector2(x+(dx+w)*cos-dy*sin, y+(dx+w)*sin+dy*cos);
            Vector2 bottomLeft = new Vector2(x+dx*cos-(dy+h)*sin, y+dx*sin+(dy+h)*cos);
            Vector2 bottomRight = new Vector2(x+(dx+w)*cos-(dy+h)*sin, y+(dx+w)*sin+(dy+h)*cos);

            Draw(texture,
                 topLeft,
                 topRight,
                 bottomLeft,
                 bottomRight,
                 source,
                 color,
                 flipMode);
        }

        public void Draw(Texture2D texture,
                          Vector2 topLeft,
                          Vector2 bottomRight,
                          Rectangle source,
                          Color color,
                          SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            Draw(texture, topLeft,
                 new Vector2(bottomRight.X, topLeft.Y),
                 new Vector2(topLeft.X, bottomRight.Y),
                 bottomRight,
                 source,
                 color,
                 flipMode);
        }

        #region Text Rendering Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(Font font, ReadOnlySpan<char> text, Vector2 location, float scale = 1.0f)
        {
            DrawString(font, text, location, Color.White, scale);
        }

        public void DrawString(Font font, ReadOnlySpan<char> text, Vector2 location, Color color, float scale = 1.0f)
        {
            int cursorX = (int)location.X;
            // location.Y += font.Height;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < text.Length; i++)
            {
                if (!font.Characters.TryGetValue(text[i], out var fontCharacter))
                    continue;

                int x = (int)(cursorX + (fontCharacter.Bearing.X * scale));
                int y = (int)(location.Y - (fontCharacter.Bearing.Y * scale));

                cursorX += (int)((fontCharacter.Advance >> 6) * scale);

                if (fontCharacter.Visible)
                {
                    Draw(font.FontAtlas,
                         new Rectangle(x,
                                       y,
                                       (int)(fontCharacter.Bounds.Width * scale),
                                       (int)(fontCharacter.Bounds.Height * scale)),
                         fontCharacter.Bounds,
                         color);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(Font font, StringBuilder text, Vector2 location, float scale = 1.0f)
        {
            DrawString(font, text, location, Color.White, scale);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 location, Color color, float scale = 1.0f)
        {
            int cursorX = (int)location.X;
            // location.Y += font.Height;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < text.Length; i++)
            {
                if (!font.Characters.TryGetValue(text[i], out var fontCharacter))
                    continue;

                int x = (int)(cursorX + (fontCharacter.Bearing.X * scale));
                int y = (int)(location.Y - (fontCharacter.Bearing.Y * scale));

                cursorX += (int)((fontCharacter.Advance >> 6) * scale);

                if (fontCharacter.Visible)
                {
                    Draw(font.FontAtlas,
                         new Rectangle(x,
                                       y,
                                       (int)(fontCharacter.Bounds.Width * scale),
                                       (int)(fontCharacter.Bounds.Height * scale)),
                         fontCharacter.Bounds,
                         color);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(Font font, ref Utf16ValueStringBuilder text, Vector2 location, float scale = 1.0f)
        {
            DrawString(font, ref text, location, Color.White, scale);
        }

        public void DrawString(Font font, ref Utf16ValueStringBuilder text, Vector2 location, Color color, float scale = 1.0f)
        {
            int cursorX = (int)location.X;
            // location.Y += font.Height;

            var textSpan = text.AsSpan();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < text.Length; i++)
            {
                if (!font.Characters.TryGetValue(textSpan[i], out var fontCharacter))
                    continue;

                int x = (int)(cursorX + (fontCharacter.Bearing.X * scale));
                int y = (int)(location.Y - (fontCharacter.Bearing.Y * scale));

                cursorX += (int)((fontCharacter.Advance >> 6) * scale);

                if (fontCharacter.Visible)
                {
                    Draw(font.FontAtlas,
                         new Rectangle(x,
                                       y,
                                       (int)(fontCharacter.Bounds.Width * scale),
                                       (int)(fontCharacter.Bounds.Height * scale)),
                         fontCharacter.Bounds,
                         color);
                }
            }
        }

        #endregion

        public void Draw(Texture2D texture,
                          Vector2 topLeft,
                          Vector2 topRight,
                          Vector2 bottomLeft,
                          Vector2 bottomRight,
                          Rectangle source,
                          Color color,
                          SpriteFlipMode flipMode = SpriteFlipMode.None)
        {
            if (!_active)
                throw new InvalidOperationException("Begin() must be called before SpriteBatch may be used for drawing.");

            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            FlushIfNeeded(texture);

            _count++;
            EnsureBufferLargeEnough();

            int vertexOffset = (_count - 1) * 4;
            int indexOffset = (_count - 1) * 6;

            float uvLeft = source.X / (float)texture._width;
            float uvRight = (source.X + source.Width) / (float)texture._width;
            float uvTop = (texture._height - (source.Y + source.Height)) / (float)texture._height;
            float uvBottom = (texture._height - (source.Y + source.Height) + source.Height) / (float)texture._height;

            if (flipMode == SpriteFlipMode.FlipHorizontally)
                ValueHelper.Swap(ref uvLeft, ref uvRight);

            if (flipMode == SpriteFlipMode.FlipVertically)
                ValueHelper.Swap(ref uvTop, ref uvBottom);

            _vertexData[vertexOffset + 0].Position.X = bottomLeft.X;     // Bottom Left
            _vertexData[vertexOffset + 0].Position.Y = bottomLeft.Y;
            _vertexData[vertexOffset + 0].TexCoord.X = uvLeft;
            _vertexData[vertexOffset + 0].TexCoord.Y = uvTop;
            _vertexData[vertexOffset + 0].Color = color;

            _vertexData[vertexOffset + 1].Position.X = bottomRight.X;    // Bottom Right
            _vertexData[vertexOffset + 1].Position.Y = bottomRight.Y;
            _vertexData[vertexOffset + 1].TexCoord.X = uvRight;
            _vertexData[vertexOffset + 1].TexCoord.Y = uvTop;
            _vertexData[vertexOffset + 1].Color = color;

            _vertexData[vertexOffset + 2].Position.X = topRight.X;    // Top Right
            _vertexData[vertexOffset + 2].Position.Y = topRight.Y;
            _vertexData[vertexOffset + 2].TexCoord.X = uvRight;
            _vertexData[vertexOffset + 2].TexCoord.Y = uvBottom;
            _vertexData[vertexOffset + 2].Color = color;

            _vertexData[vertexOffset + 3].Position.X = topLeft.X;     // Top Left
            _vertexData[vertexOffset + 3].Position.Y = topLeft.Y;
            _vertexData[vertexOffset + 3].TexCoord.X = uvLeft;
            _vertexData[vertexOffset + 3].TexCoord.Y = uvBottom;
            _vertexData[vertexOffset + 3].Color = color;

            _indexData[indexOffset + 0] = (ushort)(vertexOffset + 0);        // Bottom Left
            _indexData[indexOffset + 1] = (ushort)(vertexOffset + 1);        // Bottom Right
            _indexData[indexOffset + 2] = (ushort)(vertexOffset + 2);        // Top Right
            _indexData[indexOffset + 3] = (ushort)(vertexOffset + 0);        // Bottom Left
            _indexData[indexOffset + 4] = (ushort)(vertexOffset + 2);        // Top Right
            _indexData[indexOffset + 5] = (ushort)(vertexOffset + 3);        // Top Left
        }

        private void FlushIfNeeded(Texture2D texture)
        {
            bool flushed = false;

            if (_lastTexture == null)
            {
                _lastTexture = texture;
            }
            else if (_lastTexture != texture)
            {
                Flush();
                flushed = true;
                _lastTexture = texture;
            }

            if (_shaderProgram != _lastShader)
            {
                if (!flushed)
                    Flush();

                _lastShader = _shaderProgram;
            }

            if (_count * 4 + 3 > ushort.MaxValue || _count >= MAX_BATCH_SIZE)
            {
                Flush();
            }
        }

        private void Flush()
        {
            if (_count <= 0)
                return;

            if (_shaderProgram == null)
                throw new InvalidOperationException("Cannot flush SpriteBatch with Shader set to null.");

            if (_lastTexture == null)
                throw new InvalidOperationException("Cannot flush SpriteBatch with _lastTexture set to null.");

            unsafe
            {
                fixed (VertexPosition2TextureColor* vertexPtr = &_vertexData[0])
                fixed (ushort* indexPtr = &_indexData[0])
                {
                    _vertexBuffer.SubData<VertexPosition2TextureColor>((IntPtr)vertexPtr, 0, _count * 4);
                    _indexBuffer.SubData<ushort>((IntPtr)indexPtr, 0, _count * 6);
                }
            }

            RenderContext.BindVertexBuffer(_vertexBuffer);
            RenderContext.BindIndexBuffer(_indexBuffer);
            RenderContext.BindShaderProgram(_shaderProgram);
            RenderContext.BindTexture2D(0, _lastTexture);

            _vertexBuffer.VertexTypeInfo.Apply(_shaderProgram);

            bool prevDepthTest = RenderContext.DepthTest;
            RenderContext.DepthTest = false;

            GL.DrawRangeElements(PrimitiveType.Triangles,
                                 0,
                                 _count * 4,
                                 _count * 6,
                                 DrawElementsType.UnsignedShort,
                                 IntPtr.Zero);

            RenderContext.DepthTest = prevDepthTest;

            unchecked
            {
                // Metrics._drawCalls++;
                // Metrics._primitiveCount += _count * 6;
            }

            _count = 0;
        }

        private unsafe void EnsureBufferLargeEnough()
        {
            if (_count * 4 > _vertexBufferSize)
            {
                //_log.Debug("SpriteBatch capacity increased from " + _vertexBufferSize + " to " + (int)(_vertexBufferSize * 1.5f));
                _vertexBufferSize = (int)(_vertexBufferSize * 1.5f);    // To increase capacity
                _indexBufferSize = (int)(_vertexBufferSize * 1.5f);     // To have 2:3 ratio from vertex to index

                if (_vertexBufferSize > MAX_BATCH_SIZE * 4)
                    _vertexBufferSize = MAX_BATCH_SIZE * 4;

                if (_indexBufferSize > MAX_BATCH_SIZE * 6)
                    _indexBufferSize = MAX_BATCH_SIZE * 6;

                // Increase capacity of _vertexData
                var tempVertexData = new VertexPosition2TextureColor[_vertexData.Length];

                fixed (void* vertexSourcePtr = &_vertexData[0])
                fixed (void* vertexDestPtr = &tempVertexData[0])
                {
                    System.Buffer.MemoryCopy(vertexSourcePtr,
                                      vertexDestPtr,
                                      tempVertexData.Length * sizeof(VertexPosition2TextureColor),
                                      _vertexData.Length * sizeof(VertexPosition2TextureColor));
                }

                // Increase vertexData capacity
                _vertexData = new VertexPosition2TextureColor[_vertexBufferSize];

                fixed (void* vertexSourcePtr = &tempVertexData[0])
                fixed (void* vertexDestPtr = &_vertexData[0])
                {
                    // Store vertex data from temp array back into _vertexData
                    System.Buffer.MemoryCopy(vertexSourcePtr,
                                      vertexDestPtr,
                                      tempVertexData.Length * sizeof(VertexPosition2TextureColor),
                                      tempVertexData.Length * sizeof(VertexPosition2TextureColor));
                }

                // Increase capacity of _indexData

                var tempIndexData = new ushort[_indexData.Length];

                fixed (void* indexSourcePtr = &_indexData[0])
                fixed (void* indexDestPtr = &tempIndexData[0])
                {
                    // Store vertex data in temp array
                    System.Buffer.MemoryCopy(indexSourcePtr,
                                      indexDestPtr,
                                      _indexData.Length * sizeof(ushort),
                                      _indexData.Length * sizeof(ushort));
                }

                // Increase indexData capacity
                _indexData = new ushort[_indexBufferSize];

                fixed (void* indexSourcePtr = &tempIndexData[0])
                fixed (void* indexDestPtr = &_indexData[0])
                {
                    // Store vertex data from temp array back into _vertexData
                    System.Buffer.MemoryCopy(indexSourcePtr,
                                      indexDestPtr,
                                      tempIndexData.Length * sizeof(ushort),
                                      tempIndexData.Length * sizeof(ushort));
                }

                CreateVertexBuffer();
                CreateIndexBuffer();
            }
        }

        private void CreateVertexBuffer()
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = VertexBuffer.Create<VertexPosition2TextureColor>(RenderContext,
                                                                             _vertexBufferSize,
                                                                             BufferUsageHint.StreamDraw,
                                                                             true);
            _vertexBuffer.Label = "SpriteBatch VertexBuffer";
        }

        private void CreateIndexBuffer()
        {
            _indexBuffer?.Dispose();
            _indexBuffer = IndexBuffer.Create<ushort>(RenderContext,
                                                      _indexBufferSize,
                                                      BufferUsageHint.StreamDraw,
                                                      true);
            _indexBuffer.Label = "SpriteBatch IndexBuffer";
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}
