using PixelWindowSystem;
using SFML.Graphics;
using System.Numerics;
using VectorMaths;

internal class RaycasterAppManager : IPixelWindowAppManager
{
    private Vector2 _secondHandDirection = new Vector2(0, -1);

    public void OnLoad(RenderWindow renderWindow)
    {
    }

    public void Update(double frameTime)
    {
    }

    public void FixedUpdate(double timeStep)
    {
        var stepsPerMinute = (1000 / (float)timeStep) * 60;
        _secondHandDirection = _secondHandDirection.Rotate(AngleHelper.DegreesToRadians(360/stepsPerMinute));
    }

    public void Render(PixelData pixelData, double frameTime)
    {
        const uint offset = 30;
        const uint secondHandLength = 25;

        var secondHandEnd = (_secondHandDirection * secondHandLength) + new Vector2(offset, offset);

        pixelData.Clear();
        pixelData[offset, offset] = (255, 0, 0);
        pixelData[(uint)secondHandEnd.X, (uint)secondHandEnd.Y] = (0, 255, 0);
    }
}