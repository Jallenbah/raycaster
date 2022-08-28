using PixelWindowSystem;
using SFML.Graphics;
using System.Numerics;
using VectorMaths;

internal class RaycasterAppManager : IPixelWindowAppManager
{
    private RenderWindow? _renderWindow;
    private bool _mouseCaptured;
    private SFML.System.Vector2i _windowCentre;
    private float _mouseSensitivity = 3.0f;

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

        _windowCentre = new SFML.System.Vector2i((int)_renderWindow!.Size.X / 2, (int)_renderWindow!.Size.Y / 2);

        _renderWindow.KeyPressed += KeyPressedHandler;
        _renderWindow.MouseButtonPressed += MousePressedHandler;
        _renderWindow.MouseButtonReleased += MouseReleasedHandler;
    }

    private void MouseReleasedHandler(object? sender, SFML.Window.MouseButtonEventArgs e)
    {
        switch (e.Button)
        {
            case SFML.Window.Mouse.Button.Right:
                CaptureMouse(false);
                break;
            default:
                break;
        }
    }

    private void MousePressedHandler(object? sender, SFML.Window.MouseButtonEventArgs e)
    {
        switch (e.Button)
        {
            case SFML.Window.Mouse.Button.Right:
                CaptureMouse(true);
                break;
            default:
                break;
        }
    }

    private void KeyPressedHandler(object? sender, SFML.Window.KeyEventArgs e)
    {
        switch (e.Code)
        {
            case SFML.Window.Keyboard.Key.Escape:
                _renderWindow!.Close();
                break;
            default:
                break;
        }
    }

    private void CaptureMouse(bool capture)
    {
        _renderWindow!.SetMouseCursorGrabbed(capture);
        _renderWindow!.SetMouseCursorVisible(!capture);
        SFML.Window.Mouse.SetPosition(_windowCentre, _renderWindow);
        _mouseCaptured = capture;
    }

    public void Update(float frameTime)
    {
        if (_mouseCaptured)
        {
            var mousePos = SFML.Window.Mouse.GetPosition(_renderWindow);
            SFML.Window.Mouse.SetPosition(_windowCentre, _renderWindow);
            var mouseDelta = mousePos - _windowCentre;
            RotateCamera(mouseDelta.X / 1000f * _mouseSensitivity);
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

    private void RotateCamera(float radians)
    {
        _cameraDirection = _cameraDirection.Rotate(radians);
        _cameraPlane = _cameraPlane.Rotate(radians);
    }

    private float PixelToViewportCoord(uint pixelCoord, uint pixelScreenLength) => (pixelCoord - (pixelScreenLength / 2f)) / (pixelScreenLength / 2f);

    private Vector2? CastRay(Vector2 cameraPos, Vector2 direction)
    {
        return null;
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