using System;
using Mana.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Buffers
{
    /// <summary>
    /// Represents an OpenGL frame buffer object.
    /// </summary>
    public class FrameBuffer : GLResource
    {
        public readonly GLHandle DepthHandle;
        public readonly Texture2D ColorTexture;
        public readonly FrameBufferFlags Flags;

        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// Creates a FrameBuffer with components depending on the FrameBufferFlags given.
        /// </summary>
        public FrameBuffer(RenderContext renderContext, int width, int height, FrameBufferFlags flags)
            : base(renderContext)
        {
            Width = width;
            Height = height;
            Flags = flags;

            Handle = GLHelper.CreateFrameBuffer();
            GLHelper.EnsureValid(Handle);

            // Initialize Color Component (Texture2D)
            if ((Flags & FrameBufferFlags.Color) != 0)
            {
                ColorTexture = Texture2D.CreateEmpty(renderContext, width, height);

                GLHandle prevFrameBuffer = (GLHandle)GL.GetInteger(GetPName.FramebufferBinding);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                                        FramebufferAttachment.ColorAttachment0,
                                        TextureTarget.Texture2D,
                                        ColorTexture.Handle,
                                        0);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFrameBuffer);
            }

            // Initialize Depth Component (Renderbuffer)
            if ((Flags & FrameBufferFlags.Depth) != 0)
            {
                DepthHandle = (GLHandle)GL.GenRenderbuffer();

                GLHandle prevFrameBuffer = (GLHandle)GL.GetInteger(GetPName.FramebufferBinding);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

                GLHandle prevRenderBuffer = (GLHandle)GL.GetInteger(GetPName.RenderbufferBinding);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthHandle);

                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                                       RenderbufferStorage.DepthComponent,
                                       width,
                                       height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                                           FramebufferAttachment.DepthAttachment,
                                           RenderbufferTarget.Renderbuffer,
                                           DepthHandle);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFrameBuffer);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, prevRenderBuffer);
            }

            if (!IsComplete())
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Creates a FrameBuffer that wraps a given Texture2D object. This Texture2D will not be disposed when the
        /// FrameBuffer itself is disposed, unlike other constructors.
        /// </summary>
        public FrameBuffer(RenderContext renderContext, Texture2D wrappedTexture)
            : base(renderContext)
        {
            Width = wrappedTexture._width;
            Height = wrappedTexture._height;
            Flags = FrameBufferFlags.Color;

            Handle = GLHelper.CreateFrameBuffer();
            GLHelper.EnsureValid(Handle);

            GLHandle prevFrameBuffer = (GLHandle)GL.GetInteger(GetPName.FramebufferBinding);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                                    FramebufferAttachment.ColorAttachment0,
                                    TextureTarget.Texture2D,
                                    wrappedTexture.Handle,
                                    0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFrameBuffer);

            if (!IsComplete())
            {
                throw new InvalidOperationException();
            }
        }

        public void Bind(RenderContext renderContext) => renderContext.BindFrameBuffer(this);

        public void EnsureUnbound(RenderContext renderContext) => renderContext.EnsureFrameBufferUnbound(this);

        protected override void Dispose(bool disposing)
        {
            EnsureUndisposed();

            if (BoundContext != null)
            {
                EnsureUnbound(BoundContext);
                BoundContext = null;
            }

            GL.DeleteFramebuffer(Handle);

            ColorTexture?.Dispose();

            if (DepthHandle != GLHandle.Zero)
            {
                GL.DeleteRenderbuffer(DepthHandle);
            }
        }

        private bool IsComplete()
        {
            GLHandle prevFBO = (GLHandle)GL.GetInteger(GetPName.FramebufferBinding);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFBO);

            return status == FramebufferErrorCode.FramebufferComplete;
        }
    }
}
