using PixelWindowSystem;
using SFML.Graphics;
using System.Numerics;
using VectorMaths;

internal class RaycasterAppManager : IPixelWindowAppManager
{
    private RenderWindow? _renderWindow;
    private InputController? _inputController;

    private Vector2 _cameraPos;
    private Vector2 _cameraDirection;
    private Vector2 _cameraPlane;

    private float _fovDegrees = 90;

    public RaycasterAppManager()
    {
        _cameraPos = new Vector2(8, 8);
        _cameraDirection = Vector2.Normalize(new Vector2(1, 1));

        _cameraPlane = _cameraDirection.Rotate(AngleHelper.RadiansToDegrees(90));
        _cameraPlane *= MathF.Tan(AngleHelper.DegreesToRadians(_fovDegrees/2)); // fov adjustment
    }

    public void OnLoad(RenderWindow renderWindow)
    {
        _renderWindow = renderWindow;

        _inputController = new InputController(_renderWindow);
    }

    public void Update(float frameTime)
    {
        if (_inputController!.MouseCaptured)
        {
            var mouseDelta = _inputController.GetSensitivityAdjustedMouseDeltaAndResetToCentre();
            RotateCamera(mouseDelta.X);
        }
    }

    public void FixedUpdate(float timeStep)
    {
    }

    public void Render(PixelData pixelData, float frameTime)
    {
        pixelData.Clear();

        var mult = pixelData.Width / 5;
        (uint x, uint y) midPos = (pixelData.Width / 2, pixelData.Height / 2);
        pixelData[midPos.x, midPos.y] = (0, 255, 0);

        for (uint x = 0; x < pixelData.Width; x++)
        {
            var xAsViewportCoord = PixelToViewportCoord(x, pixelData.Width);
            var rayVector = Vector2.Normalize((xAsViewportCoord * _cameraPlane) + _cameraDirection);

            var test = rayVector * mult;

            pixelData[(uint)(midPos.x + MathF.Round(test.X)), (uint)(midPos.y + MathF.Round(test.Y))] = (255, 0, 0);
        }
    }

    // Rotate the camera left / right. Positive angle is clockwise rotation
    private void RotateCamera(float radians)
    {
        _cameraDirection = _cameraDirection.Rotate(radians);
        _cameraPlane = _cameraPlane.Rotate(radians);
    }

    // Takes a pixel coordinate starting in the top left and converts it to a -1 to 1 float value where 0 is the centre of the screen
    private float PixelToViewportCoord(uint pixelCoord, uint pixelScreenLength) => (pixelCoord - (pixelScreenLength / 2f)) / (pixelScreenLength / 2f);

    private Vector2? CastRay(Vector2 cameraPos, Vector2 direction)
    {
        return null; // todo
    }

    private readonly int[,] _map = {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,0,0,0,0,1,0,1,0,1,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
    };
}