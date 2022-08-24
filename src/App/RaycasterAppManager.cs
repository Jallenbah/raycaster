using PixelWindowSystem;
using SFML.Graphics;

internal class RaycasterAppManager : IPixelWindowAppManager
{
    public void OnLoad(RenderWindow renderWindow)
    {
    }

    public void Update(double frameTime)
    {
    }

    public void FixedUpdate(double timeStep)
    {
    }

    public void Render(PixelData pixelData, double frameTime)
    {
        pixelData.Clear();
    }
}