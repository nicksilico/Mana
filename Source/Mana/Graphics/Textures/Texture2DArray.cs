using System;
using System.IO;
using System.Runtime.InteropServices;
using Mana.Asset;
using Mana.Utilities;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Mana.Graphics.Textures
{
    public class Texture2DArray : Texture, IAsset
    {
        private TextureMinFilter _minFilter;
        private TextureMagFilter _magFilter;
        private TextureWrapMode _wrapModeS;
        private TextureWrapMode _wrapModeT;

        private Texture2DArray(RenderContext renderContext)
            : base(renderContext)
        {
        }

        public int Width { get; private set; } = -1;

        public int Height { get; private set; } = -1;

        public int Depth { get; private set; } = -1;

        internal override TextureTarget? TextureTargetType => TextureTarget.Texture2DArray;

        public override void Bind(int slot, RenderContext renderContext)
        {
            renderContext.BindTexture2DArray(slot, this);
        }

        public override void EnsureUnbound(int slot, RenderContext renderContext)
        {
            renderContext.EnsureTexture2DArrayUnbound(slot, this);
        }

        public override void EnsureUnbound(RenderContext renderContext)
        {
            renderContext.EnsureTexture2DArrayUnbound(this);
        }

        public static Texture2DArray CreateEmpty(RenderContext renderContext,
                                                 int width,
                                                 int height,
                                                 int depth)
        {
            var textureArray = new Texture2DArray(renderContext)
            {
                Width = width,
                Height = height,
                Depth = depth,
            };
            
            var previousTexture = renderContext.GetCurrentTexture(0);
            renderContext.BindTexture2DArray(0, textureArray);

            GL.TexImage3D(TextureTarget.Texture2DArray,
                          0,
                          PixelInternalFormat.Rgba,
                          textureArray.Width,
                          textureArray.Height,
                          depth,
                          0,
                          PixelFormat.Rgba,
                          PixelType.UnsignedByte,
                          IntPtr.Zero);

            if (previousTexture != null)
            {
                previousTexture.Bind(0, renderContext);
            }
            else
            {
               renderContext.UnbindTexture2DArray(0);
            }

            return textureArray;
        }

        public unsafe void SetDataFromStream(RenderContext renderContext, int depth, Stream stream)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            if (image.Width != Width || image.Height != Height)
            {
                throw new InvalidOperationException("The image loaded from the stream is not the correct size.\n"
                                                    + $"Expected: ({Width}, {Height})\n"
                                                    + $"Actual: ({image.Width}, {image.Height})");
            }

            fixed (void* data = &MemoryMarshal.GetReference(ImageHelper.GetPixelSpan(image)))
            {
                var previousTexture = renderContext.GetCurrentTexture(0);
                renderContext.BindTexture2DArray(0, this);

                GL.TexSubImage3D(TextureTarget.Texture2DArray,
                                 0,
                                 0,
                                 0,
                                 depth,
                                 Width,
                                 Height,
                                 1,
                                 PixelFormat.Rgba,
                                 PixelType.UnsignedByte,
                                 new IntPtr(data));

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

                if (previousTexture != null)
                {
                    previousTexture.Bind(0, renderContext);
                }
                else
                {
                    renderContext.UnbindTexture2DArray(0);
                }
            }
        }

        public string SourcePath { get; set; }

        public AssetManager AssetManager { get; set; }

        public void SetMinFilter(RenderContext renderContext, TextureMinFilter minFilter)
        {
            if (_minFilter != minFilter)
            {
                GLHelper.TextureParameter(renderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureMinFilter,
                                          (int)minFilter);

                _minFilter = minFilter;
            }
        }

        public TextureMinFilter GetMinFilter() => _minFilter;

        public void SetMagFilter(RenderContext renderContext, TextureMagFilter magFilter)
        {
            if (_magFilter != magFilter)
            {
                GLHelper.TextureParameter(renderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureMagFilter,
                                          (int)magFilter);

                _magFilter = magFilter;
            }
        }

        public TextureMagFilter GetMagFilter() => _magFilter;

        public void SetWrapModeS(RenderContext renderContext, TextureWrapMode wrapModeS)
        {
            if (_wrapModeS != wrapModeS)
            {
                GLHelper.TextureParameter(renderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureWrapS,
                                          (int)wrapModeS);

                _wrapModeS = wrapModeS;
            }
        }
        
        public TextureWrapMode GetWrapModeS() => _wrapModeS;

        public void SetWrapModeT(RenderContext renderContext, TextureWrapMode wrapModeT)
        {
            if (_wrapModeT != wrapModeT)
            {
                GLHelper.TextureParameter(renderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureWrapT,
                                          (int)wrapModeT);

                _wrapModeT = wrapModeT;
            }
        }

        public TextureWrapMode GetWrapModeT() => _wrapModeT;

        private void SetDefaultTextureParameters(RenderContext renderContext)
        {
            SetMinFilter(renderContext, TextureMinFilter.Linear);
            SetMagFilter(renderContext, TextureMagFilter.Linear);

            SetWrapModeS(renderContext, TextureWrapMode.Repeat);
            SetWrapModeT(renderContext, TextureWrapMode.Repeat);
        }
    }
}
