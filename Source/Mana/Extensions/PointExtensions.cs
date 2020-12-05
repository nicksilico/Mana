using System.Drawing;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;

namespace Mana
{
    public static class PointExtensions
    {
        /// <summary>
        /// Converts the <see cref="Point"/> to a <see cref="System.Numerics.Vector2"/>.
        /// </summary>
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }
        
        /// <summary>
        /// Converts the <see cref="Point"/> to a <see cref="Vector2i"/>.
        /// </summary>
        public static Vector2i ToVector2i(this Point point)
        {
            return new Vector2i(point.X, point.Y);
        }
    }
}
