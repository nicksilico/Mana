using System;
using Mana.Graphics.Buffers;
using Mana.Graphics.Shaders;
using Mana.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;

using TextureUnitGL = OpenTK.Graphics.OpenGL4.TextureUnit;

namespace Mana.Graphics
{
    public partial class RenderContext
    {
        private GLHandle _vertexBufferHandle = GLHandle.Zero;
        private VertexBuffer _vertexBuffer = null;

        private GLHandle _indexBufferHandle = GLHandle.Zero;
        private IndexBuffer _indexBuffer = null;

        private GLHandle _pixelBufferHandle = GLHandle.Zero;
        private PixelBuffer _pixelBuffer = null;

        private GLHandle _frameBufferHandle = GLHandle.Zero;
        private FrameBuffer _frameBuffer = null;

        private GLHandle _shaderProgramHandle = GLHandle.Zero;
        private ShaderProgram _shaderProgram = null;

        private ManaTextureType _boundTextureType = ManaTextureType.None;
        private readonly TextureUnit[] _textureUnits;
        private int _activeTexture = 0;

        public FrameBuffer FrameBuffer => _frameBuffer;

        #region VertexBuffer

        /// <summary>
        /// Binds the given <see cref="VertexBuffer"/> object to the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="vbo">The <see cref="VertexBuffer"/> object to bind.</param>
        public void BindVertexBuffer(VertexBuffer vbo)
        {
            if (vbo == null)
                throw new ArgumentNullException(nameof(vbo));

            vbo.EnsureUndisposed();

            if (_vertexBufferHandle == vbo.Handle)
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.Handle);

            _vertexBufferHandle = vbo.Handle;
            _vertexBuffer = vbo;
            _vertexBuffer.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="VertexBuffer"/> object is unbound.
        /// </summary>
        /// <param name="vbo">The <see cref="VertexBuffer"/> object to ensure is unbound.</param>
        public void EnsureVertexBufferUnbound(VertexBuffer vbo)
        {
            if (vbo == null)
                throw new ArgumentNullException(nameof(vbo));

            if (_vertexBufferHandle == vbo.Handle)
                UnbindVertexBuffer();
        }

        /// <summary>
        /// Ensures that there is no active <see cref="VertexBuffer"/> object bound to the RenderContext.
        /// </summary>
        public void UnbindVertexBuffer()
        {
            if (_vertexBufferHandle == GLHandle.Zero)
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, GLHandle.Zero);

            _vertexBufferHandle = GLHandle.Zero;
            _vertexBuffer.BoundContext = null;
            _vertexBuffer = null;
        }

        #endregion

        #region IndexBuffer

        /// <summary>
        /// Binds the given <see cref="IndexBuffer"/> object to the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="ibo">The <see cref="IndexBuffer"/> object to bind.</param>
        public void BindIndexBuffer(IndexBuffer ibo)
        {
            if (ibo == null)
                throw new ArgumentNullException(nameof(ibo));

            ibo.EnsureUndisposed();

            if (_indexBufferHandle == ibo.Handle)
                return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo.Handle);

            _indexBufferHandle = ibo.Handle;
            _indexBuffer = ibo;
            _indexBuffer.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="IndexBuffer"/> object is unbound.
        /// </summary>
        /// <param name="ibo">The <see cref="IndexBuffer"/> object to ensure is unbound.</param>
        public void EnsureIndexBufferUnbound(IndexBuffer ibo)
        {
            if (ibo == null)
                throw new ArgumentNullException(nameof(ibo));

            if (_indexBufferHandle == ibo.Handle)
                UnbindIndexBuffer();
        }

        /// <summary>
        /// Ensures that there is no active <see cref="IndexBuffer"/> object bound to the RenderContext.
        /// </summary>
        public void UnbindIndexBuffer()
        {
            if (_indexBufferHandle == GLHandle.Zero)
                return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLHandle.Zero);

            _indexBufferHandle = GLHandle.Zero;
            _indexBuffer.BoundContext = null;
            _indexBuffer = null;
        }

        #endregion

        #region PixelBuffer

        /// <summary>
        /// Binds the given <see cref="PixelBuffer"/> object to the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="pbo">The <see cref="PixelBuffer"/> object to bind.</param>
        public void BindPixelBuffer(PixelBuffer pbo)
        {
            if (pbo == null)
                throw new ArgumentNullException(nameof(pbo));

            pbo.EnsureUndisposed();

            if (_pixelBufferHandle == pbo.Handle)
                return;

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pbo.Handle);

            _pixelBufferHandle = pbo.Handle;
            _pixelBuffer = pbo;
            _pixelBuffer.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="PixelBuffer"/> object is unbound.
        /// </summary>
        /// <param name="pbo">The <see cref="PixelBuffer"/> object to ensure is unbound.</param>
        public void EnsurePixelBufferUnbound(PixelBuffer pbo)
        {
            if (pbo == null)
                throw new ArgumentNullException(nameof(pbo));

            if (_pixelBufferHandle == pbo.Handle)
                UnbindPixelBuffer();
        }

        /// <summary>
        /// Ensures that there is no active <see cref="PixelBuffer"/> object bound to the RenderContext.
        /// </summary>
        public void UnbindPixelBuffer()
        {
            if (_pixelBufferHandle == GLHandle.Zero)
                return;

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, GLHandle.Zero);

            _pixelBufferHandle = GLHandle.Zero;
            _pixelBuffer.BoundContext = null;
            _pixelBuffer = null;
        }

        #endregion

        #region FrameBuffer

        /// <summary>
        /// Binds the given <see cref="_frameBuffer"/> object to the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="fbo">The <see cref="_frameBuffer"/> object to bind.</param>
        public void BindFrameBuffer(FrameBuffer fbo)
        {
            if (fbo == null)
            {
                UnbindFrameBuffer();
                return;
            }

            fbo.EnsureUndisposed();

            if (_frameBufferHandle == fbo.Handle)
                return;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo.Handle);

            _frameBufferHandle = fbo.Handle;
            _frameBuffer = fbo;
            _frameBuffer.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="_frameBuffer"/> object is unbound.
        /// </summary>
        /// <param name="fbo">The <see cref="_frameBuffer"/> object to ensure is unbound.</param>
        public void EnsureFrameBufferUnbound(FrameBuffer fbo)
        {
            if (fbo == null)
                throw new ArgumentNullException(nameof(fbo));

            if (_frameBufferHandle == fbo.Handle)
                UnbindFrameBuffer();
        }

        /// <summary>
        /// Ensures that there is no active <see cref="Buffers.FrameBuffer"/> object bound to the RenderContext.
        /// </summary>
        public void UnbindFrameBuffer()
        {
            if (_frameBufferHandle == GLHandle.Zero)
                return;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLHandle.Zero);

            _frameBufferHandle = GLHandle.Zero;
            _frameBuffer.BoundContext = null;
            _frameBuffer = null;
        }

        #endregion

        #region ShaderProgram

        /// <summary>
        /// Binds the given <see cref="ShaderProgram"/> object to the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="program">The <see cref="ShaderProgram"/> object to bind.</param>
        public void BindShaderProgram(ShaderProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            program.EnsureUndisposed();

            if (_shaderProgramHandle == program.Handle)
                return;

            GL.UseProgram(program.Handle);

            _shaderProgramHandle = program.Handle;
            _shaderProgram = program;
            _shaderProgram.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="ShaderProgram"/> object is unbound.
        /// </summary>
        /// <param name="program">The <see cref="ShaderProgram"/> object to ensure is unbound.</param>
        public void EnsureShaderProgramUnbound(ShaderProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            if (_shaderProgramHandle != program.Handle)
                return;

            GL.UseProgram(GLHandle.Zero);

            _shaderProgramHandle = GLHandle.Zero;
            _shaderProgram.BoundContext = null;
            _shaderProgram = null;
        }

        /// <summary>
        /// Ensures that there is no active <see cref="ShaderProgram"/> object bound to the RenderContext.
        /// </summary>
        public void UnbindShaderProgram()
        {
            if (_shaderProgramHandle == GLHandle.Zero)
                return;

            GL.UseProgram(GLHandle.Zero);

            _shaderProgramHandle = GLHandle.Zero;
            _shaderProgram.BoundContext = null;
            _shaderProgram = null;
        }

        #endregion

        #region Texture2D

        /// <summary>
        /// Binds the given <see cref="Texture2D"/> to the GraphicsDevice at the given texture unit location.
        /// </summary>
        /// <param name="slot">The texture unit slot that the texture will be bound to.</param>
        /// <param name="texture2D">The <see cref="Texture2D"/> to bind.</param>
        public void BindTexture2D(int slot, Texture2D texture2D)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (texture2D == null)
                throw new ArgumentNullException(nameof(texture2D));

            texture2D.EnsureUndisposed();

            SetActiveTextureSlot(slot);

            if (_textureUnits[slot].Texture2DHandle == texture2D.Handle)
                return;

            ClearOtherTextureTypes(slot, ManaTextureType.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, texture2D.Handle);

            _boundTextureType = ManaTextureType.Texture2D;
            _textureUnits[slot].Texture2DHandle = texture2D.Handle;
            _textureUnits[slot].Texture2D = texture2D;
            _textureUnits[slot].Texture2D.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="Texture2D"/> object is unbound on the given slot.
        /// </summary>
        public void EnsureTexture2DUnbound(int slot, Texture2D texture2D)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (texture2D == null)
                throw new ArgumentNullException(nameof(texture2D));

            if (_textureUnits[slot].Texture2DHandle == texture2D.Handle)
                UnbindTexture2D(slot);
        }

        /// <summary>
        /// Ensures that the given <see cref="Texture2D"/> object is unbound from all slots.
        /// </summary>
        public void EnsureTexture2DUnbound(Texture2D texture2D)
        {
            if (texture2D == null)
                throw new ArgumentNullException(nameof(texture2D));

            for (int i = 0; i < GLInfo.MaxTextureImageUnits; i++)
            {
                if (_textureUnits[i].Texture2DHandle == texture2D.Handle)
                {
                    UnbindTexture2D(i);
                }
            }
        }

        /// <summary>
        /// Ensures that there is no active <see cref="Texture2D"/> object bound to the RenderContext's given texture
        /// unit slot.
        /// </summary>
        public void UnbindTexture2D(int slot)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (_textureUnits[slot].Texture2DHandle == GLHandle.Zero)
                return;

            SetActiveTextureSlot(slot);

            GL.BindTexture(TextureTarget.Texture2D, GLHandle.Zero);

            _boundTextureType = ManaTextureType.None;
            _textureUnits[slot].Texture2DHandle = GLHandle.Zero;
            _textureUnits[slot].Texture2D.BoundContext = null;
            _textureUnits[slot].Texture2D = null;
        }

        #endregion

        #region TextureCubeMap

        /// <summary>
        /// Binds the given <see cref="TextureCubeMap"/> to the GraphicsDevice at the given texture unit location.
        /// </summary>
        /// <param name="slot">The texture unit slot that the texture will be bound to.</param>
        /// <param name="textureCubeMap">The <see cref="TextureCubeMap"/> to bind.</param>
        public void BindTextureCubeMap(int slot, TextureCubeMap textureCubeMap)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (textureCubeMap == null)
                throw new ArgumentNullException(nameof(textureCubeMap));

            textureCubeMap.EnsureUndisposed();

            SetActiveTextureSlot(slot);

            if (_textureUnits[slot].TextureCubeMapHandle == textureCubeMap.Handle)
                return;

            ClearOtherTextureTypes(slot, ManaTextureType.TextureCubeMap);

            GL.BindTexture(TextureTarget.TextureCubeMap, textureCubeMap.Handle);

            _boundTextureType = ManaTextureType.TextureCubeMap;
            _textureUnits[slot].TextureCubeMapHandle = textureCubeMap.Handle;
            _textureUnits[slot].TextureCubeMap = textureCubeMap;
            _textureUnits[slot].TextureCubeMap.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="TextureCubeMap"/> object is unbound on the given slot.
        /// </summary>
        public void EnsureTextureCubeMapUnbound(int slot, TextureCubeMap textureCubeMap)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (textureCubeMap == null)
                throw new ArgumentNullException(nameof(textureCubeMap));

            if (_textureUnits[slot].TextureCubeMapHandle == textureCubeMap.Handle)
                UnbindTextureCubeMap(slot);
        }

        /// <summary>
        /// Ensures that the given <see cref="TextureCubeMap"/> object is unbound from all slots.
        /// </summary>
        public void EnsureTextureCubeMapUnbound(TextureCubeMap textureCubeMap)
        {
            if (textureCubeMap == null)
                throw new ArgumentNullException(nameof(textureCubeMap));

            for (int i = 0; i < GLInfo.MaxTextureImageUnits; i++)
            {
                if (_textureUnits[i].TextureCubeMapHandle == textureCubeMap.Handle)
                {
                    UnbindTextureCubeMap(i);
                }
            }
        }

        /// <summary>
        /// Ensures that there is no active <see cref="TextureCubeMap"/> object bound to the RenderContext's given
        /// texture unit slot.
        /// </summary>
        public void UnbindTextureCubeMap(int slot)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (_textureUnits[slot].TextureCubeMapHandle == GLHandle.Zero)
                return;

            SetActiveTextureSlot(slot);

            GL.BindTexture(TextureTarget.TextureCubeMap, GLHandle.Zero);

            _boundTextureType = ManaTextureType.None;
            _textureUnits[slot].TextureCubeMapHandle = GLHandle.Zero;
            _textureUnits[slot].TextureCubeMap.BoundContext = null;
            _textureUnits[slot].TextureCubeMap = null;
        }

        #endregion

        #region Texture2DArray

        /// <summary>
        /// Binds the given <see cref="Texture2DArray"/> to the GraphicsDevice at the given texture unit location.
        /// </summary>
        /// <param name="slot">The texture unit slot that the texture will be bound to.</param>
        /// <param name="texture2DArray">The <see cref="Texture2DArray"/> to bind.</param>
        public void BindTexture2DArray(int slot, Texture2DArray texture2DArray)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (texture2DArray == null)
                throw new ArgumentNullException(nameof(texture2DArray));

            texture2DArray.EnsureUndisposed();

            SetActiveTextureSlot(slot);

            if (_textureUnits[slot].Texture2DArrayHandle == texture2DArray.Handle)
                return;

            ClearOtherTextureTypes(slot, ManaTextureType.Texture2DArray);

            GL.BindTexture(TextureTarget.Texture2DArray, texture2DArray.Handle);

            _boundTextureType = ManaTextureType.Texture2DArray;
            _textureUnits[slot].Texture2DArrayHandle = texture2DArray.Handle;
            _textureUnits[slot].Texture2DArray = texture2DArray;
            _textureUnits[slot].Texture2DArray.BoundContext = this;
        }

        /// <summary>
        /// Ensures that the given <see cref="Texture2DArray"/> object is unbound on the given slot.
        /// </summary>
        public void EnsureTexture2DArrayUnbound(int slot, Texture2DArray texture2DArray)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (texture2DArray == null)
                throw new ArgumentNullException(nameof(texture2DArray));

            if (_textureUnits[slot].Texture2DArrayHandle == texture2DArray.Handle)
                UnbindTexture2DArray(slot);
        }

        /// <summary>
        /// Ensures that the given <see cref="Texture2DArray"/> object is unbound from all slots.
        /// </summary>
        public void EnsureTexture2DArrayUnbound(Texture2DArray texture2DArray)
        {
            if (texture2DArray == null)
                throw new ArgumentNullException(nameof(texture2DArray));

            for (int i = 0; i < GLInfo.MaxTextureImageUnits; i++)
            {
                if (_textureUnits[i].Texture2DArrayHandle == texture2DArray.Handle)
                {
                    UnbindTexture2DArray(i);
                }
            }
        }

        /// <summary>
        /// Ensures that there is no active <see cref="Texture2DArray"/> object bound to the RenderContext's given
        /// texture unit slot.
        /// </summary>
        public void UnbindTexture2DArray(int slot)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (_textureUnits[slot].Texture2DArrayHandle == GLHandle.Zero)
                return;

            SetActiveTextureSlot(slot);

            GL.BindTexture(TextureTarget.Texture2DArray, GLHandle.Zero);

            _boundTextureType = ManaTextureType.None;
            _textureUnits[slot].Texture2DArrayHandle = GLHandle.Zero;
            _textureUnits[slot].Texture2DArray.BoundContext = null;
            _textureUnits[slot].Texture2DArray = null;
        }

        #endregion

        /// <summary>
        /// Sets the active texture unit slot.
        /// </summary>
        /// <param name="slot">The slot to set as the active texture unit.</param>
        public void SetActiveTextureSlot(int slot)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (_activeTexture == slot)
                return;

            _activeTexture = slot;

            GL.ActiveTexture((TextureUnitGL)((int)TextureUnitGL.Texture0 + slot));
        }

        /// <summary>
        /// Ensures that no Textures are bound to the given texture slot.
        /// </summary>
        public void ClearTextureSlot(int slot)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            UnbindTexture2D(slot);
            UnbindTextureCubeMap(slot);
            UnbindTexture2DArray(slot);
        }

        /// <summary>
        /// Ensures that no Textures are bound to any texture slot.
        /// </summary>
        public void ClearTextureSlots()
        {
            for (int i = 0; i < GLInfo.MaxTextureImageUnits; i++)
            {
                if (_textureUnits[i].Texture2DHandle != GLHandle.Zero)
                    UnbindTexture2D(i);

                if (_textureUnits[i].TextureCubeMapHandle != GLHandle.Zero)
                    UnbindTextureCubeMap(i);

                if (_textureUnits[i].Texture2DArrayHandle != GLHandle.Zero)
                    UnbindTexture2DArray(i);
            }
        }

        public Texture GetCurrentTexture(int slot)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            ref TextureUnit unit = ref _textureUnits[slot];

            switch (_boundTextureType)
            {
                case ManaTextureType.None:
                    return null;
                case ManaTextureType.Texture2D:
                    return unit.Texture2D;
                case ManaTextureType.TextureCubeMap:
                    return unit.TextureCubeMap;
                case ManaTextureType.Texture2DArray:
                    return unit.Texture2DArray;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearOtherTextureTypes(int slot, ManaTextureType type)
        {
            if (slot < 0 || slot >= GLInfo.MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(slot));

            if (type != ManaTextureType.Texture2D)
                UnbindTexture2D(slot);

            if (type != ManaTextureType.TextureCubeMap)
                UnbindTextureCubeMap(slot);

            if (type != ManaTextureType.Texture2DArray)
                UnbindTexture2DArray(slot);
        }

        internal struct TextureUnit
        {
            public GLHandle Texture2DHandle;
            public Texture2D Texture2D;

            public GLHandle TextureCubeMapHandle;
            public TextureCubeMap TextureCubeMap;

            public GLHandle Texture2DArrayHandle;
            public Texture2DArray Texture2DArray;
        }
    }
}