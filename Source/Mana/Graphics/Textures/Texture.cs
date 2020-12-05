using System;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Textures
{
    /// <summary>
    /// Represents an OpenGL Texture object.
    /// </summary>
    public abstract class Texture : GLResource
    {
        protected Texture(RenderContext renderContext)
            : base(renderContext)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            if (!TextureTargetType.HasValue)
                throw new InvalidOperationException("Texture subtype does not specify a TextureTarget.");

            // ReSharper disable once VirtualMemberCallInConstructor
            Handle = GLHelper.CreateTexture(TextureTargetType.Value);
        }

        internal abstract TextureTarget? TextureTargetType { get; }

        public abstract void Bind(int slot, RenderContext renderContext);
        public abstract void EnsureUnbound(int slot, RenderContext renderContext);
        public abstract void EnsureUnbound(RenderContext renderContext);

        public bool IsBound(int slot, RenderContext renderContext)
        {
            if (slot < 0 || slot >=  GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            return renderContext.GetCurrentTexture(slot) == this;
        }

        protected override void Dispose(bool disposing)
        {
            if (BoundContext != null)
            {
                EnsureUnbound(BoundContext);
                BoundContext = null;
            }
            
            GL.DeleteTexture(Handle);
        }
    }
}