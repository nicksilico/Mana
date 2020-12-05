using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mana.Asset;
using Mana.Graphics.Buffers;
using Mana.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Mana.Graphics.Textures
{
    public class TextureCubeMap : Texture, IAsset
    {
        public TextureCubeMap(RenderContext renderContext)
            : base(renderContext)
        {
        }

        internal override TextureTarget? TextureTargetType => TextureTarget.TextureCubeMap;

        public override void Bind(int slot, RenderContext renderContext)
        {
            renderContext.BindTextureCubeMap(slot, this);
        }

        public override void EnsureUnbound(int slot, RenderContext renderContext)
        {
            renderContext.EnsureTextureCubeMapUnbound(slot, this);
        }

        public override void EnsureUnbound(RenderContext renderContext)
        {
            renderContext.EnsureTextureCubeMapUnbound(this);
        }

        public string SourcePath { get; set; }

        public AssetManager AssetManager { get; set; }

        public void SetImages(RenderContext renderContext,
                                     Image<Rgba32> front,
                                     Image<Rgba32> back,
                                     Image<Rgba32> left,
                                     Image<Rgba32> right,
                                     Image<Rgba32> up,
                                     Image<Rgba32> down)

        {
            SetFaceData(renderContext, CubeMapDirection.Front, front);
            SetFaceData(renderContext, CubeMapDirection.Back, back);
            SetFaceData(renderContext, CubeMapDirection.Left, left);
            SetFaceData(renderContext, CubeMapDirection.Right, right);
            SetFaceData(renderContext, CubeMapDirection.Up, up);
            SetFaceData(renderContext, CubeMapDirection.Down, down);

            GLHelper.TextureParameter(renderContext, TextureTarget.TextureCubeMap, this, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GLHelper.TextureParameter(renderContext, TextureTarget.TextureCubeMap, this, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GLHelper.TextureParameter(renderContext, TextureTarget.TextureCubeMap, this, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GLHelper.TextureParameter(renderContext, TextureTarget.TextureCubeMap, this, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GLHelper.TextureParameter(renderContext, TextureTarget.TextureCubeMap, this, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GenerateMipmaps(renderContext);
        }

        private void GenerateMipmaps(RenderContext renderContext)
        {
            var previousTexture = renderContext.GetCurrentTexture(0);
            renderContext.BindTextureCubeMap(0, this);

            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

            if (previousTexture != null)
            {
                previousTexture.Bind(0, renderContext);
            }
            else
            {
                renderContext.UnbindTextureCubeMap(0);
            }
        }

        private unsafe void SetFaceData(RenderContext renderContext, CubeMapDirection direction, Image<Rgba32> image)
        {
            fixed (void* data = &MemoryMarshal.GetReference(ImageHelper.GetPixelSpan(image)))
            {
                var previousTexture = renderContext.GetCurrentTexture(0);
                renderContext.BindTextureCubeMap(0, this);

                GL.TexImage2D(GetTextureTarget(direction),
                              0,
                              PixelInternalFormat.Rgba,
                              image.Width,
                              image.Height,
                              0,
                              PixelFormat.Rgba,
                              PixelType.UnsignedByte,
                              new IntPtr(data));

                if (previousTexture != null)
                {
                    previousTexture.Bind(0, renderContext);
                }
                else
                {
                    renderContext.UnbindTextureCubeMap(0);
                }
            }
        }

        private TextureTarget GetTextureTarget(CubeMapDirection dir)
        {
            switch (dir)
            {
                case CubeMapDirection.Front:
                    return TextureTarget.TextureCubeMapPositiveX;
                case CubeMapDirection.Back:
                    return TextureTarget.TextureCubeMapNegativeX;
                case CubeMapDirection.Up:
                    return TextureTarget.TextureCubeMapPositiveY;
                case CubeMapDirection.Down:
                    return TextureTarget.TextureCubeMapNegativeY;
                case CubeMapDirection.Right:
                    return TextureTarget.TextureCubeMapPositiveZ;
                case CubeMapDirection.Left:
                    return TextureTarget.TextureCubeMapNegativeZ;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }

        private enum CubeMapDirection
        {
            Front = 0,
            Back,
            Up,
            Down,
            Right,
            Left,
        }
    }
}
