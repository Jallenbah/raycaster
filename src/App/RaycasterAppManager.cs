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

    private bool _renderDebug = true;

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

            // This will be needed but for now its easier for debugging to cast just 1 ray below
            //var hit = CastRay(_cameraPos, rayVector, pixelData);
        }

        // TODO - REMOVE: For debugging purposes, casting just 1 ray
        CastRay(_cameraPos, _cameraDirection, pixelData);

        if (_renderDebug)
        {
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    var offsetPos = new Vector2(x, y) + new Vector2(midPos.x, midPos.y) - _cameraPos;
                    if (GetMapEntry(x, y) == 1)
                    {
                        pixelData[(uint)offsetPos.X, (uint)offsetPos.Y] = (0, 0, 255);
                    }
                }
            }
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

    // Returns a vector of the hit location, or null if nothing is hit
    // TODO - DOESN'T WORK - Draws a great line but won't catch every square when tracing for a hit. Needs to be rewritten
    // to step along X or Y depending on which one is closer to a grid-line. Currently it always makes a whole step along
    // one axis, which means you jump over the corner of some cells.
    private Vector2? CastRay(Vector2 cameraPos, Vector2 direction, PixelData pixelData)
    {
        const uint cellStepLimit = 100;
        var midPos = new Vector2(pixelData.Width / 2, pixelData.Height / 2);

        direction = Vector2.Normalize(direction); // We need direction to be unit vector, so ensure it is
        var rayPos = cameraPos;
        for (uint i = 0; i < cellStepLimit; i++)
        {
            if (_renderDebug)
            {
                var offsetPos = rayPos + midPos - _cameraPos;
                if (offsetPos.X > 0 && offsetPos.Y > 0 && offsetPos.X < pixelData.Width && offsetPos.Y < pixelData.Height)
                {
                    pixelData[(uint)offsetPos.X, (uint)offsetPos.Y] = (255, 0, 0);
                }
            }

            if (GetMapEntry((int)(rayPos.X), (int)(rayPos.Y)) == 1)
            {
                break;
            }

            if (MathF.Abs(direction.Y) <= MathF.Abs(direction.X))
            {
                rayPos.X += (direction.X / MathF.Abs(direction.X));
                rayPos.Y += (1 / MathF.Abs(direction.X)) * direction.Y;
            }
            else
            {
                rayPos.Y += (direction.Y / MathF.Abs(direction.Y));
                rayPos.X += (1 / MathF.Abs(direction.Y)) * direction.X;
            }
        }

        return null;
    }

    private int GetMapEntry(int x, int y)
    {
        if (x < 0 || y < 0 || x >= 16 || y >= 16)
        {
            return -1;
        }
        else
        {
            return _map[x, y];
        }
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