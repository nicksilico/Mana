using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Mana
{
    public static class Vector2Extensions
    {
        /// <summary>
        /// Returns the normalized vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalized(this Vector2 vector)
        {
            return Vector2.Normalize(vector);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3"/> object from the <see cref="Vector2"/> object,
        /// using the given float as the Z component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vector2 vector, float z)
        {
            return new Vector3(vector, z);
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> object from the <see cref="Point"/> object, truncating each component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        /// <summary>
        /// Transforms the vector by a specified 4x4 matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(this Vector2 vector, Matrix4x4 matrix)
        {
            return Vector2.Transform(vector, matrix);
        }
    }
}
