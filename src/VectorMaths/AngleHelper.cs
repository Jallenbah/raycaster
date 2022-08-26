namespace VectorMaths
{
    /// <summary>
    /// Helper class for generic functionality for handling angles
    /// </summary>
    public static class AngleHelper
    {
        /// <summary>
        /// Takes an angle in degrees and converts it to radians
        /// </summary>
        public static float DegreesToRadians(float degrees) => (MathF.PI / 180) * degrees;

        /// <summary>
        /// Takes an angle in radians and converts it to degrees
        /// </summary>
        public static float RadiansToDegrees(float radians) => (180 / MathF.PI) * radians;
    }
}
