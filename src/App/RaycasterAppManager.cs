using PixelWindowSystem;
using SFML.Graphics;
using System.Numerics;
using VectorMaths;

internal class RaycasterAppManager : IPixelWindowAppManager
{
    private RenderWindow? _renderWindow;
    private InputController? _inputController;
    private DebugRenderer? _debugRenderer;

    private Vector2 _cameraPos;
    private Vector2 _cameraDirection;
    private Vector2 _cameraPlane;

    private float _fovDegrees = 90;

    private bool _renderDebug = false;

    public RaycasterAppManager()
    {
        _cameraPos = new Vector2(7.5f, 7.5f);
        _cameraDirection = Vector2.Normalize(new Vector2(1, 1));

        _cameraPlane = _cameraDirection.Rotate(AngleHelper.RadiansToDegrees(90));
        _cameraPlane *= MathF.Tan(AngleHelper.DegreesToRadians(_fovDegrees/2)); // fov adjustment
    }

    public void OnLoad(RenderWindow renderWindow)
    {
        _renderWindow = renderWindow;
        _inputController = new InputController(_renderWindow);
        _debugRenderer = new DebugRenderer(_mapWidth, _mapHeight);
    }

    public void Update(float frameTime)
    {
        // Player rotation
        if (_inputController!.MouseCaptured)
        {
            var mouseDelta = _inputController.GetSensitivityAdjustedMouseDeltaAndResetToCentre();
            RotateCamera(-mouseDelta.X);
        }

        // Player movement
        var moveSpeed = 4f * 0.001f * frameTime;
        var movementVector = new Vector2(0, 0);
        if (_inputController!.MoveForward)   movementVector += _cameraDirection;
        if (_inputController!.MoveBackwards) movementVector -= _cameraDirection;
        if (_inputController!.MoveLeft)      movementVector += _cameraDirection.Rotate(AngleHelper.DegreesToRadians(90));
        if (_inputController!.MoveRight)     movementVector += _cameraDirection.Rotate(AngleHelper.DegreesToRadians(-90));
        movementVector = movementVector.LengthSquared() == 0 ? movementVector : Vector2.Normalize(movementVector);
        _cameraPos += moveSpeed * movementVector;

        _renderDebug = _inputController!.ShowDebug;
    }

    public void FixedUpdate(float timeStep)
    {
    }

    public void Render(PixelData pixelData, float frameTime)
    {
        pixelData.Clear();

        var hitLocations = new Vector2?[pixelData.Width];
        for (uint x = 0; x < pixelData.Width; x++)
        {
            var xAsViewportCoord = PixelToViewportCoord(x, pixelData.Width);
            var rayVector = Vector2.Normalize((xAsViewportCoord * _cameraPlane) + _cameraDirection);

            hitLocations[x] = CastRay(_cameraPos, rayVector, pixelData);
        }

        for (uint x = 0; x < pixelData.Width; x++)
        {
            if (hitLocations[x] == null)
            {
                continue;
            }

            var distance = (hitLocations[x]!.Value - _cameraPos).Length();
            var lineHeight = (uint)((1 / Math.Max(distance, 1)) * pixelData.Height);
            var facesY = (hitLocations[x]!.Value.X - (int)hitLocations[x]!.Value.X) > (hitLocations[x]!.Value.Y - (int)hitLocations[x]!.Value.Y);
            byte brightness = (byte)((1 / Math.Max(distance, 1) * 200) + (facesY ? 55: 0));
            DrawVerticalPixelStrip(pixelData, x, lineHeight, (brightness, brightness, brightness));
        }

        if (_renderDebug)
        {
            // For debugging purposes, casting just 1 ray
            //hitLocations[0] = CastRay(_cameraPos, _cameraDirection, pixelData);

            _debugRenderer!.Render(pixelData, _cameraPos, GetMapEntry, hitLocations);
        }
    }

    /// Draw a vertical pixel strip centered on the middle of the screen (the horizon)
    private void DrawVerticalPixelStrip(PixelData pixelData, uint x, uint lineHeight, (byte r, byte g, byte b) colour)
    {
        var midY = pixelData.Height / 2;
        var startY = midY - (lineHeight / 2);
        var endY = midY + (lineHeight / 2) - 1;
        for (uint y = startY; y <= endY; y++)
        {
            pixelData[x, y] = colour;
        }
    }

    // Rotate the camera left / right. Positive angle is clockwise rotation
    private void RotateCamera(float radians)
    {
        _cameraDirection = _cameraDirection.Rotate(radians);
        _cameraPlane = _cameraPlane.Rotate(radians);
    }

    // Takes a pixel coordinate starting in the top left and converts it to a -1 to 1 float value where 0 is the centre of the screen
    private float PixelToViewportCoord(uint pixelCoord, uint pixelScreenLength) =>
        (pixelCoord - (pixelScreenLength / 2f)) / (pixelScreenLength / 2f);

    /// <summary>
    /// For a given camera position and direction, finds the first point at which the ray hits something
    /// </summary>
    /// <param name="cameraPos">The position of the camera</param>
    /// <param name="direction">The direction in which the camera is facing</param>
    /// <param name="pixelData">Instance of the rendering <see cref="PixelData"/> used for debug rendering of ray checking points</param>
    /// <returns>A <see cref="Vector2"/> of the point at which a non-empty cell was hit, or null if nothing was hit</returns>
    private Vector2? CastRay(Vector2 cameraPos, Vector2 direction, PixelData pixelData)
    {
        const uint iterationLimit = 100;

        direction = Vector2.Normalize(direction); // We need direction to be unit vector, so ensure it is
        var rayPos = cameraPos;

        // Finds the next integer coordinate value based on the current position in the ray and the direction in which it is being cast on that axis
        float calculateNextGridValue(float currentPos, float directionUnitValue)
        {
            float val = currentPos < 0 == directionUnitValue < 0
                ? (int)(currentPos + directionUnitValue)
                : (int)currentPos;

            val = val == currentPos
                ? val + directionUnitValue
                : val;

            return val;
        }

        // Get X and Y polarised direction values i.e. 1, 0, -1
        var polarisedDirectionUnitValues = new Vector2(
            direction.X / MathF.Abs(direction.X),
            direction.Y / MathF.Abs(direction.Y)
        );

        for (var i = 0; i < iterationLimit; i++)
        {
            // Find next gridline intersection point (integer coordinate) for X and Y components
            var nextGridValuesAlongRay = new Vector2(
                calculateNextGridValue(rayPos.X, polarisedDirectionUnitValues.X),
                calculateNextGridValue(rayPos.Y, polarisedDirectionUnitValues.Y)
            );

            // A vector representing what would needed to be added to rayPos to get to the next X gridline along the ray
            var distanceToNextX = new Vector2(
                nextGridValuesAlongRay.X - rayPos.X,
                direction.Y * ((nextGridValuesAlongRay.X - rayPos.X) / direction.X)
            );

            // A vector representing what would needed to be added to rayPos to get to the next Y gridline along the ray
            var distanceToNextY = new Vector2(
                direction.X * ((nextGridValuesAlongRay.Y - rayPos.Y) / direction.Y),
                nextGridValuesAlongRay.Y - rayPos.Y
            );

            // We compare the squared lengths of the 2 vectors as it avoids unnecessary costly sqrts to get the actual lengths
            if (distanceToNextX.LengthSquared() < distanceToNextY.LengthSquared())
            {
                rayPos += distanceToNextX;
            }
            else
            {
                rayPos += distanceToNextY;
            }

            // By stepping the ray forward a tiny amount, we can get the next grid unit.
            // Adjust by 1 if negative as cell occupies x to x+1 so flooring a negative number would wrongly get the next cell over.
            const float cellCheckStepSize = 0.0001f;
            var cellToCheck = (
                X: (int)(rayPos.X + cellCheckStepSize * direction.X) + (rayPos.X < 0 ? -1 : 0),
                Y: (int)(rayPos.Y + cellCheckStepSize * direction.Y) + (rayPos.Y < 0 ? -1 : 0)
            );
            if (GetMapEntry(cellToCheck.X, cellToCheck.Y) == 1)
            {
                return rayPos; // You sunk my battleship
            }

            // Shows every single ray position through the cast. Overkill most of the time
            //if (_renderDebug)
            //{
            //    _debugRenderer!.RenderRayCast(pixelData, rayPos);
            //}
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

    private const int _mapWidth = 16;
    private const int _mapHeight = 16;
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