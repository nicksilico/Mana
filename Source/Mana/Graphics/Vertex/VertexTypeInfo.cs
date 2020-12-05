using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mana.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Vertex
{
    public class VertexTypeInfo
    {
        private static Dictionary<Type, VertexTypeInfo> _vertexTypeInfoCache = new Dictionary<Type, VertexTypeInfo>();

        internal readonly int VertexStride;
        internal readonly VertexAttributeInfo[] Attributes;

        public VertexTypeInfo(Type type)
        {
            var fields = type.GetFields();
            Attributes = new VertexAttributeInfo[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                Attributes[i] = VertexHelper.GetVertexAttributeInfo(fields[i].FieldType);
                VertexStride += Attributes[i].Size * Attributes[i].ComponentCount;
            }
        }

        public static VertexTypeInfo Get<T>()
        {
            if (_vertexTypeInfoCache.TryGetValue(typeof(T), out VertexTypeInfo vertexTypeInfo))
                return vertexTypeInfo;

            var info = new VertexTypeInfo(typeof(T));

            _vertexTypeInfoCache.Add(typeof(T), info);

            return info;
        }

        public void Apply(ShaderProgram program)
        {
            int location = 0;
            for (uint i = 0; i < Attributes.Length; i++)
            {
                EnableDisableAttributes(program, i);

                VertexAttributeInfo attribute = Attributes[i];

                GL.VertexAttribPointer(i,
                                       attribute.ComponentCount,
                                       attribute.Type,
                                       attribute.Normalize,
                                       VertexStride,
                                       new IntPtr(location));

                location += attribute.Size * attribute.ComponentCount;
            }
        }

        public void Apply(ShaderProgram program, IntPtr offset)
        {
            int location = 0;
            for (uint i = 0; i < Attributes.Length; i++)
            {
                EnableDisableAttributes(program, i);

                VertexAttributeInfo attribute = Attributes[i];

                GL.VertexAttribPointer(i,
                                       attribute.ComponentCount,
                                       attribute.Type,
                                       attribute.Normalize,
                                       VertexStride,
                                       offset + location);

                location += attribute.Size * attribute.ComponentCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnableDisableAttributes(ShaderProgram program, uint i)
        {
            if (program.AttributesByLocation.TryGetValue(i, out _))
            {
                GL.EnableVertexAttribArray(i);
            }
            else
            {
                GL.DisableVertexAttribArray(i);
            }
        }
    }
}
