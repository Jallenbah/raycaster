using System.Numerics;

namespace VectorMaths
{
    /// <summary>
    /// Extensions for the <see cref="System.Numerics.Vector2" struct/>
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Extension method to rotates a vector around the origin 0,0
        /// </summary>
        public static Vector2 Rotate(this Vector2 vec, float radians)
        {
            var oldX = vec.X;
            vec.X = vec.X * MathF.Cos(radians) - vec.Y * MathF.Sin(radians);
            vec.Y = oldX * MathF.Sin(radians) + vec.Y * MathF.Cos(radians);
            return vec;
        }
    }
}