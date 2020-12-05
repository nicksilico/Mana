using System.Drawing;
using OpenTK.Mathematics;

namespace Mana
{
    public static class Vector2iExtensions
    {
        public static Point ToPoint(this Vector2i vector)
        {
            return new Point(vector.X, vector.Y);
        }
    }
}