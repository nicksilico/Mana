using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Mana.Graphics.Vertex;
using Mana.Utilities;
using Mana.Utilities.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace Mana.Graphics
{
    /// <summary>
    /// Represents an OpenGL context capable of being made current on a thread, as well as encapsulating the
    /// OpenGL context's state.
    /// </summary>
    public partial class RenderContext : IDisposable
    {
        private RenderContext(ManaWindow window, IGraphicsContext openGLContext)
        {
            Window = window;
            OpenGLContext = openGLContext;

            InitializeState();

            _textureUnits = new TextureUnit[GLInfo.MaxTextureImageUnits];
        }

        public ManaWindow Window { get; }

        public IGraphicsContext OpenGLContext { get; }

        /// <summary>
        /// Gets a value that indicates whether this instance is current on the calling thread.
        /// </summary>
        public bool IsCurrent
        {
            get
            {
                bool contextCurrent = OpenGLContext.IsCurrent;

                return contextCurrent;
            }
        }

        /// <summary>
        /// Creates a <see cref="RenderContext"/> that wraps a window's own GraphicsContext object.
        /// </summary>
        /// <param name="window">The window to wrap.</param>
        /// <returns>The newly created <see cref="RenderContext"/> object.</returns>
        public static RenderContext WrapWindowContext(ManaWindow window)
        {
            if (window.Context == null)
                throw new InvalidOperationException();
            
            window.Context.MakeCurrent();
            
            return new RenderContext(window, window.Context);
        }

        private void InitializeState()
        {
            DebugMessageHandler.Initialize();

            GLInfo.Initialize();

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            if (GLInfo.HasDebug)
            {
                string labelName = "Unused VertexArray";
                GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, vao, labelName.Length, labelName);
            }

            DepthTest = true;
            CullBackfaces = true;
            Blend = true;
            SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void Dispose()
        {
        }
    }
}
