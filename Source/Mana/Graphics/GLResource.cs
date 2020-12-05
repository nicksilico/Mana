using System;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics
{
    /// <summary>
    /// Represents an OpenGL resource with a corresponding <see cref="GLHandle"/> handle.
    /// </summary>
    public abstract class GLResource : IDisposable
    {
        /// <summary>
        /// The <see cref="RenderContext"/> assigned to this GraphicsResource. When operations are performed on the
        /// resource that will affect graphics state (such as binding), this context should be set to the current
        /// context on the calling thread.
        /// </summary>
        public RenderContext ParentContext;

        /// <summary>
        /// The <see cref="RenderContext"/> that the GraphicsResource is currently bound to, if any.
        /// </summary>
        public RenderContext BoundContext;

        private string _label;

        protected GLResource(RenderContext parentContext)
        {
            ParentContext = parentContext;
        }

        ~GLResource()
        {
            throw new InvalidOperationException("Finalized called in GLResource: Disposal leak.");
        }

        /// <summary>
        /// Gets a value representing the OpenGL handle for this resource.
        /// </summary>
        public GLHandle Handle { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the resource has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Gets or sets the OpenGL object label string (used for logging and debugging).
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                if (GLInfo.HasDebug)
                {
                    if (!LabelType.HasValue)
                    {
                        throw new InvalidOperationException("ObjectLabelIdentifier must be set in order to set label.");
                    }

                    GL.ObjectLabel(LabelType.Value, Handle, value.Length, value);
                    _label = value;
                }
            }
        }

        /// <summary>
        /// Gets the OpenGL ObjectLabelIdentifier associated with this graphics resource.
        /// </summary>
        protected virtual ObjectLabelIdentifier? LabelType => null;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);

            Disposed = true;
        }

        protected abstract void Dispose(bool disposing);

        public void EnsureUndisposed()
        {
            if (Disposed)
            {
                throw new InvalidOperationException("Cannot perform this operation on a disposed GraphicsResource");
            }
        }
    }
}
