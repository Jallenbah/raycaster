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

        Vector2 midPos = new Vector2(pixelData.Width / 2, pixelData.Height / 2);

        for (uint x = 0; x < pixelData.Width; x++)
        {
            var xAsViewportCoord = PixelToViewportCoord(x, pixelData.Width);
            var rayVector = Vector2.Normalize((xAsViewportCoord * _cameraPlane) + _cameraDirection);

            // This will be needed but for now its easier for debugging to cast just 1 ray below
            //var hit = CastRay(_cameraPos, rayVector, pixelData);
        }

        // TODO - REMOVE: For debugging purposes, casting just 1 ray
        var singleRayHit = CastRayBad(_cameraPos, _cameraDirection, pixelData);

        if (_renderDebug)
        {
            const int mapWidth = 16;
            const int mapHeight = 16;
            var renderScale = Math.Min((pixelData.Width / 2) / mapWidth, (pixelData.Height / 2) / mapHeight);

            Vector2 getRenderPosFromWorldPos(Vector2 pos)
            {
                var centerOrientedPos = new Vector2(pos.X - mapWidth / 2, pos.Y - mapWidth / 2);
                return (centerOrientedPos * renderScale) + midPos;
            };

            // Camera position as green dot
            var playerRenderPosition = getRenderPosFromWorldPos(_cameraPos);
            pixelData[(uint)playerRenderPosition.X, (uint)playerRenderPosition.Y] = (0, 255, 0);

            for (var x = 0; x < mapWidth; x++)
            {
                for (var y = 0; y < mapHeight; y++)
                {
                    if (GetMapEntry(x, y) != 1)
                    {
                        continue;
                    }

                    var blockPos = getRenderPosFromWorldPos(new Vector2(x, y));

                    for (uint bx = (uint)blockPos.X; bx < (blockPos.X + renderScale) - 1; bx++)
                    {
                        for (uint by = (uint)blockPos.Y; by < (blockPos.Y + renderScale) - 1; by++)
                        {
                            pixelData[bx, by] = (0, 0, 255);
                        }
                    }
                }
            }

            if(singleRayHit != null)
            {
                var rayHitRenderPos = getRenderPosFromWorldPos(singleRayHit.Value);
                pixelData[(uint)rayHitRenderPos.X, (uint)rayHitRenderPos.Y] = (255, 0, 0);
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
    private Vector2? CastRayBad(Vector2 cameraPos, Vector2 direction, PixelData pixelData)
    {
        const uint cellStepLimit = 100;
        var midPos = new Vector2(pixelData.Width / 2, pixelData.Height / 2);

        direction = Vector2.Normalize(direction); // We need direction to be unit vector, so ensure it is
        var rayPos = cameraPos;
        for (uint i = 0; i < cellStepLimit; i++)
        {
            //if (_renderDebug)
            //{
            //    var offsetPos = rayPos + midPos - _cameraPos;
            //    if (offsetPos.X > 0 && offsetPos.Y > 0 && offsetPos.X < pixelData.Width && offsetPos.Y < pixelData.Height)
            //    {
            //        pixelData[(uint)offsetPos.X, (uint)offsetPos.Y] = (255, 0, 0);
            //    }
            //}

            if (GetMapEntry((int)(rayPos.X), (int)(rayPos.Y)) == 1)
            {
                return rayPos;
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