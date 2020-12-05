using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Assimp;
using Mana.Asset;
using Mana.Asset.Loaders;
using Mana.Asset.Reloading;
using Mana.Graphics.Buffers;
using Mana.Utilities;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace Mana.Graphics.Textures
{
    /// <summary>
    /// Represents an OpenGL Texture2D object.
    /// </summary>
    public class Texture2D : Texture, IReloadableAsset
    {
        private static Logger _log = Logger.Create();

        private TextureMinFilter _minFilter;
        private TextureMagFilter _magFilter;
        private TextureWrapMode _wrapModeS;
        private TextureWrapMode _wrapModeT;

        private Texture2D(RenderContext renderContext)
            : base(renderContext)
        {
        }

        internal override TextureTarget? TextureTargetType => TextureTarget.Texture2D;

        protected override ObjectLabelIdentifier? LabelType => ObjectLabelIdentifier.Texture;

        public int Width => _width;

        public int Height => _height;

        internal int _width = -1;

        internal int _height = -1;

        public string SourcePath { get; set; }

        public AssetManager AssetManager { get; set; }

        public override void Bind(int slot, RenderContext renderContext)
        {
            renderContext.BindTexture2D(slot, this);
        }

        public override void EnsureUnbound(int slot, RenderContext renderContext)
        {
            renderContext.EnsureTexture2DUnbound(slot, this);
        }

        public override void EnsureUnbound(RenderContext renderContext)
        {
            renderContext.EnsureTexture2DUnbound(this);
        }

        #region Create Methods

        public static Texture2D CreateEmpty(RenderContext renderContext, int width, int height)
        {
            var texture = new Texture2D(renderContext)
            {
                _width = width,
                _height = height,
            };

            if (GLInfo.HasDirectStateAccess)
            {
                GL.TextureStorage2D(texture.Handle,
                                    1,
                                    SizedInternalFormat.Rgba8,
                                    texture._width,
                                    texture._height);

                GL.ClearTexImage(texture.Handle,
                                 0,
                                 PixelFormat.Rgba,
                                 PixelType.UnsignedByte,
                                 IntPtr.Zero);
            }
            else
            {
                var previousTexture = renderContext.GetCurrentTexture(0);

                renderContext.BindTexture2D(0, texture);

                GL.TexImage2D(TextureTarget.Texture2D,
                              0,
                              PixelInternalFormat.Rgba,
                              texture._width,
                              texture._height,
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
                    renderContext.UnbindTexture2D(0);
                }
            }

            texture.SetDefaultTextureParameters(renderContext);

            return texture;
        }

        public static unsafe Texture2D CreateFromStream(RenderContext renderContext, Stream stream)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var texture = new Texture2D(renderContext)
            {
                _width = image.Width,
                _height = image.Height,
            };

            fixed (void* data = &MemoryMarshal.GetReference(ImageHelper.GetPixelSpan(image)))
            {
                if (GLInfo.HasDirectStateAccess)
                {
                    GL.TextureStorage2D(texture.Handle,
                                        1,
                                        SizedInternalFormat.Rgba8,
                                        texture._width,
                                        texture._height);

                    GL.TextureSubImage2D(texture.Handle,
                                         0,
                                         0,
                                         0,
                                         texture._width,
                                         texture._height,
                                         PixelFormat.Rgba,
                                         PixelType.UnsignedByte,
                                         new IntPtr(data));

                    GL.GenerateTextureMipmap(texture.Handle);
                }
                else
                {
                    var previousTexture = renderContext.GetCurrentTexture(0);

                    renderContext.BindTexture2D(0, texture);

                    GL.TexImage2D(TextureTarget.Texture2D,
                                  0,
                                  PixelInternalFormat.Rgba,
                                  texture._width,
                                  texture._height,
                                  0,
                                  PixelFormat.Rgba,
                                  PixelType.UnsignedByte,
                                  new IntPtr(data));

                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    if (previousTexture != null)
                    {
                        previousTexture.Bind(0, renderContext);
                    }
                    else
                    {
                        renderContext.UnbindTexture2D(0);
                    }
                }
            }

            texture.SetDefaultTextureParameters(renderContext);

            return texture;
        }

        public static unsafe Texture2D CreateFromStreamUnsynchronized(RenderContext renderContext, Stream stream)
        {
            using var image = Image.Load<Rgba32>(stream);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var texture = new Texture2D(renderContext)
            {
                _width = image.Width,
                _height = image.Height,
            };

            var span = ImageHelper.GetPixelSpan(image);
            int size = span.Length * sizeof(Rgba32);

            fixed (void* data = &MemoryMarshal.GetReference(span))
            {
                var start = new IntPtr(data);

                using var pixelBuffer = PixelBuffer.Create<Rgba32>(renderContext, size, true, true);

                GLHandle prevPixelBuffer = (GLHandle)GL.GetInteger(GetPName.PixelUnpackBufferBinding);
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pixelBuffer.Handle);

                // When we load assets asynchronously, using SubData to set the data will cause the main thread
                // to wait for the GL call to complete, so we use a PBO and fill it with a mapped memory range to
                // prevent OpenGL synchronization from causing frame drops on the main thread.

                IntPtr pixelPointer = GL.MapBufferRange(BufferTarget.PixelUnpackBuffer,
                                                  IntPtr.Zero,
                                                  size,
                                                  BufferAccessMask.MapWriteBit
                                                  | BufferAccessMask.MapUnsynchronizedBit
                                                  | BufferAccessMask.MapInvalidateRangeBit);

                if (pixelPointer == IntPtr.Zero)
                {
                    throw new Exception("Could not map PixelUnbackBuffer range.");
                }

                // We send the data to mapped memory in multiple batches instead of one large one for the same
                // reason.

                int remaining = size;
                const int step = 2048;
                while (remaining > 0)
                {
                    int currentStep = Math.Min(remaining, step);
                    int point = size - remaining;
                    void* dest = (void*)IntPtr.Add(pixelPointer, point);
                    void* src = (void*)IntPtr.Add(start, point);
                    Unsafe.CopyBlock(dest, src, (uint)currentStep);

                    remaining -= step;
                }

                GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);

                if (GLInfo.HasDirectStateAccess)
                {
                    GL.TextureStorage2D(texture.Handle,
                                        1,
                                        SizedInternalFormat.Rgba8,
                                        texture._width,
                                        texture._height);

                    GL.TextureSubImage2D(texture.Handle,
                                         0,
                                         0,
                                         0,
                                         texture._width,
                                         texture._height,
                                         PixelFormat.Rgba,
                                         PixelType.UnsignedByte,
                                         IntPtr.Zero);

                    GL.GenerateTextureMipmap(texture.Handle);
                }
                else
                {
                    var previousTexture = renderContext.GetCurrentTexture(0);

                    renderContext.BindTexture2D(0, texture);

                    GL.TexImage2D(TextureTarget.Texture2D,
                                  0,
                                  PixelInternalFormat.Rgba,
                                  texture._width,
                                  texture._height,
                                  0,
                                  PixelFormat.Rgba,
                                  PixelType.UnsignedByte,
                                  IntPtr.Zero);

                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    if (previousTexture != null)
                    {
                        previousTexture.Bind(0, renderContext);
                    }
                    else
                    {
                        renderContext.UnbindTexture2D(0);
                    }
                }

                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, prevPixelBuffer);
            }

            texture.SetDefaultTextureParameters(renderContext);

            GL.Finish();

            return texture;
        }

        public static unsafe Texture2D CreateFromRGBAPointer(RenderContext renderContext,
                                                             int width,
                                                             int height,
                                                             byte* pixelData,
                                                             bool generateMipMaps = false)
        {
            var texture = new Texture2D(renderContext)
            {
                _width = width,
                _height = height
            };

            if (GLInfo.HasDirectStateAccess)
            {
                GL.TextureStorage2D(texture.Handle,
                                    1,
                                    SizedInternalFormat.Rgba8,
                                    texture._width,
                                    texture._height);

                GL.TextureSubImage2D(texture.Handle,
                                     0,
                                     0,
                                     0,
                                     texture._width,
                                     texture._height,
                                     PixelFormat.Rgba,
                                     PixelType.UnsignedByte,
                                     new IntPtr(pixelData));

                if (generateMipMaps)
                {
                    GL.GenerateTextureMipmap(texture.Handle);
                }
            }
            else
            {
                var previousTexture = renderContext.GetCurrentTexture(0);
                renderContext.BindTexture2D(0, texture);

                GL.TexImage2D(TextureTarget.Texture2D,
                              0,
                              PixelInternalFormat.Rgba,
                              texture._width,
                              texture._height,
                              0,
                              PixelFormat.Rgba,
                              PixelType.UnsignedByte,
                              new IntPtr(pixelData));

                if (generateMipMaps)
                {
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                if (previousTexture != null)
                {
                    previousTexture.Bind(0, renderContext);
                }
                else
                {
                    renderContext.UnbindTexture2D(0);
                }
            }

            texture.SetDefaultTextureParameters(renderContext);

            return texture;
        }

        #endregion

        public void SetDataFromAlphaByteArray(RenderContext renderContext,
                                              byte[] pixelData)
        {
            SetDataFromAlphaByteArray(renderContext, 0, 0, _width, _height, pixelData);
        }

        public unsafe void SetDataFromAlphaByteArray(RenderContext renderContext,
                                                     int xOffset,
                                                     int yOffset,
                                                     int width,
                                                     int height,
                                                     byte[] pixelData)
        {
            if (width * height != pixelData.Length)
                throw new ArgumentException($"Invalid size of pixel data.");

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            fixed (void* data = &pixelData[0])
            {
                if (GLInfo.HasDirectStateAccess)
                {
                    GL.TextureSubImage2D(Handle,
                                         0,
                                         xOffset,
                                         yOffset,
                                         width,
                                         height,
                                         PixelFormat.Alpha,
                                         PixelType.UnsignedByte,
                                         new IntPtr(data));
                }
                else
                {
                    var previousTexture = renderContext.GetCurrentTexture(0);
                    renderContext.BindTexture2D(0, this);

                    GL.TexSubImage2D(TextureTarget.Texture2D,
                                     0,
                                     xOffset,
                                     yOffset,
                                     width,
                                     height,
                                     PixelFormat.Alpha,
                                     PixelType.UnsignedByte,
                                     new IntPtr(data));

                    if (previousTexture != null)
                    {
                        previousTexture.Bind(0, renderContext);
                    }
                    else
                    {
                        renderContext.UnbindTexture2D(0);
                    }
                }
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
        }

        public Image<Rgba32> ToImage(RenderContext renderContext)
        {
            var image = new Image<Rgba32>(_width, _height);

            var previousTexture = renderContext.GetCurrentTexture(0);
            renderContext.BindTexture2D(0, this);

            GL.GetTexImage(TextureTarget.Texture2D,
                           0,
                           PixelFormat.Rgba,
                           PixelType.UnsignedByte,
                           ref MemoryMarshal.GetReference(ImageHelper.GetPixelSpan(image)));

            image.Mutate(c => c.Flip(FlipMode.Vertical));

            if (previousTexture != null)
            {
                previousTexture.Bind(0, renderContext);
            }
            else
            {
                renderContext.UnbindTexture2D(0);
            }

            return image;
        }



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

        // internal void SetDataFromBitmap(BitmapData data)
        // {
        //     GL.TextureSubImage2D(Handle,
        //                          0,
        //                          0,
        //                          0,
        //                          Width,
        //                          Height,
        //                          PixelFormat.Bgra,
        //                          PixelType.UnsignedByte,
        //                          data.Scan0);
        // }

        public void Reload(AssetManager assetManager)
        {
            Texture2D texture = null;

            if (BoundContext != null)
            {
                EnsureUnbound(BoundContext);
            }

            var loader = AssetManager.AssetLoaders[typeof(Texture2D)] as Texture2DLoader
                         ?? throw new InvalidOperationException("Cannot reload Texture2D.");

            try
            {
                using var stream = assetManager.GetStreamFromPath(SourcePath);

                var createdTexture = loader.Load(assetManager, assetManager.RenderContext, stream, SourcePath);
                texture = createdTexture;
            }
            catch (Exception e)
            {
                _log.Error("Error reloading Texture2D: " + e.Message);
            }

            if (texture != null)
            {
                var initialHandle = Handle;

                GL.DeleteTexture(Handle);

                Handle = texture.Handle;
                _height = texture._height;
                _width = texture._width;

                GLHelper.TextureParameter(assetManager.RenderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureMinFilter,
                                          (int)_minFilter);

                GLHelper.TextureParameter(assetManager.RenderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureMagFilter,
                                          (int)_magFilter);

                GLHelper.TextureParameter(assetManager.RenderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureWrapS,
                                          (int)_wrapModeS);

                GLHelper.TextureParameter(assetManager.RenderContext,
                                          TextureTarget.Texture2D,
                                          this,
                                          TextureParameterName.TextureWrapT,
                                          (int)_wrapModeT);

                if (GLInfo.HasDirectStateAccess)
                {
                    GL.GenerateTextureMipmap(Handle);
                }
                else
                {
                    var previousTexture = assetManager.RenderContext.GetCurrentTexture(0);
                    assetManager.RenderContext.BindTexture2D(0, this);

                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    if (previousTexture != null)
                    {
                        previousTexture.Bind(0, assetManager.RenderContext);
                    }
                    else
                    {
                        assetManager.RenderContext.UnbindTexture2D(0);
                    }
                }

                _log.Info($"Texture reloaded successfully: {initialHandle} -> {Handle}");
            }
        }

        public string[] GetLiveReloadAssetPaths()
        {
            return new[] { SourcePath };
        }

        public void SetInfoFromTextureSlot(RenderContext renderContext, ref TextureSlot textureSlot)
        {
            switch (textureSlot.WrapModeU)
            {
                case Assimp.TextureWrapMode.Wrap:
                    SetWrapModeS(renderContext, TextureWrapMode.Repeat);
                    break;
                case Assimp.TextureWrapMode.Clamp:
                    SetWrapModeS(renderContext, TextureWrapMode.ClampToEdge);
                    break;
                case Assimp.TextureWrapMode.Mirror:
                    SetWrapModeS(renderContext, TextureWrapMode.MirroredRepeat);
                    break;
                case Assimp.TextureWrapMode.Decal:
                    SetWrapModeS(renderContext, TextureWrapMode.ClampToBorder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (textureSlot.WrapModeV)
            {
                case Assimp.TextureWrapMode.Wrap:
                    SetWrapModeT(renderContext, TextureWrapMode.Repeat);
                    break;
                case Assimp.TextureWrapMode.Clamp:
                    SetWrapModeT(renderContext, TextureWrapMode.ClampToEdge);
                    break;
                case Assimp.TextureWrapMode.Mirror:
                    SetWrapModeT(renderContext, TextureWrapMode.MirroredRepeat);
                    break;
                case Assimp.TextureWrapMode.Decal:
                    SetWrapModeT(renderContext, TextureWrapMode.ClampToBorder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}