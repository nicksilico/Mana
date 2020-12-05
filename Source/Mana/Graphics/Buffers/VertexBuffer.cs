using System;
using Mana.Graphics.Vertex;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Buffers
{
    /// <summary>
    /// Represents an OpenGL vertex buffer object.
    /// </summary>
    public class VertexBuffer : Buffer
    {
        public readonly VertexTypeInfo VertexTypeInfo;

        private VertexBuffer(RenderContext parentContext, VertexTypeInfo vertexTypeInfo)
            : base(parentContext)
        {
            VertexTypeInfo = vertexTypeInfo;
        }

        internal override BufferTarget BufferTarget => BufferTarget.ArrayBuffer;

        public static VertexBuffer Create<T>(RenderContext renderContext,
                                             T[] data,
                                             BufferUsageHint bufferUsageHint,
                                             bool immutable,
                                             BufferStorageFlags flags = BufferStorageFlags.None)
            where T : unmanaged
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var vbo = new VertexBuffer(renderContext, VertexTypeInfo.Get<T>());
            vbo.Allocate<T>(data, bufferUsageHint, immutable, flags);

            return vbo;
        }

        public static unsafe VertexBuffer Create<T>(RenderContext renderContext,
                                                    int capacity,
                                                    BufferUsageHint bufferUsageHint,
                                                    bool immutable,
                                                    BufferStorageFlags flags = BufferStorageFlags.None)
            where T : unmanaged
        {
            if (renderContext == null)
                throw new ArgumentNullException(nameof(renderContext));

            var vbo = new VertexBuffer(renderContext, VertexTypeInfo.Get<T>());
            vbo.Allocate<T>(capacity * sizeof(T), bufferUsageHint, immutable, flags);

            return vbo;
        }

        public override void Bind(RenderContext renderContext) => renderContext.BindVertexBuffer(this);

        public override void EnsureUnbound(RenderContext renderContext) => renderContext.EnsureVertexBufferUnbound(this);
    }
}
