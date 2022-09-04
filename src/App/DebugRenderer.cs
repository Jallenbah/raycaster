using PixelWindowSystem;
using System.Numerics;

/// <summary>
/// For rendering top down data for debug purposes
/// </summary>
internal class DebugRenderer
{
    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public DebugRenderer(int mapWidth, int mapHeight)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    /// <summary>
    /// Renders the map, the player, and the points which the player can see
    /// </summary>
    public void Render(PixelData pixelData, Vector2 cameraPos, Func<int, int, int> GetMapEntry, Vector2?[] hitLocations)
    {
        var midPos = GetRenderMidPosForDebugRender(pixelData.Width, pixelData.Height);
        var renderScale = GetRenderScaleForDebugRender(pixelData.Width, pixelData.Height);

        // Render map as blue blocks
        for (var x = 0; x < _mapWidth; x++)
        {
            for (var y = 0; y < _mapHeight; y++)
            {
                if (GetMapEntry(x, y) != 1)
                {
                    continue;
                }

                var blockPos = GetRenderPosFromWorldPosForDebugRender(new Vector2(x, y), midPos, renderScale, pixelData.Height);

                for (uint bx = (uint)blockPos.X + 1; bx < (blockPos.X + renderScale); bx++)
                {
                    for (uint by = (uint)blockPos.Y - 1; by > (blockPos.Y - renderScale); by--)
                    {
                        pixelData.SetSafe(bx, by, (0, 0, 255));
                    }
                }
            }
        }

        // Show where casted rays have hit
        for (var x = 0; x < hitLocations.Length; x++)
        {
            if (hitLocations[x] != null)
            {
                var renderPos = GetRenderPosFromWorldPosForDebugRender(hitLocations[x]!.Value, midPos, renderScale, pixelData.Height);
                pixelData.SetSafe((uint)renderPos.X, (uint)renderPos.Y, (0, 255, 255));
            }
        }

        // Camera position as green dot
        var playerRenderPosition = GetRenderPosFromWorldPosForDebugRender(cameraPos, midPos, renderScale, pixelData.Height);
        //playerRenderPosition.Y = pixelData.Height - playerRenderPosition.Y;
        pixelData.SetSafe((uint)playerRenderPosition.X, (uint)playerRenderPosition.Y, (0, 255, 0));
    }

    /// <summary>
    /// Renders a single point along a casted ray
    /// </summary>
    public void RenderRayCast(PixelData pixelData, Vector2 rayPos)
    {
        var midPos = GetRenderMidPosForDebugRender(pixelData.Width, pixelData.Height);
        var renderScale = GetRenderScaleForDebugRender(pixelData.Width, pixelData.Height);
        var rayCheckRenderPos = GetRenderPosFromWorldPosForDebugRender(rayPos, midPos, renderScale, pixelData.Height);
        pixelData.SetSafe((uint)rayCheckRenderPos.X, (uint)rayCheckRenderPos.Y, (255, 0, 0));
    }

    /// <summary>
    /// For a given world position, gets the position on the screen for rendering debug data
    /// </summary>
    private Vector2 GetRenderPosFromWorldPosForDebugRender(Vector2 pos, Vector2 midPos, uint renderScale, uint screenHeight)
    {
        var centerOrientedPos = new Vector2(pos.X - _mapWidth / 2, pos.Y - _mapWidth / 2);
        var screenPos = (centerOrientedPos * renderScale) + midPos;
        screenPos.Y = screenHeight - screenPos.Y;
        return screenPos;
    }

    /// <summary>
    /// Gets a scale value by which debug rendering should be adjusted to ensure it is all positioned consistently and so that it fits on the screen
    /// </summary>
    private uint GetRenderScaleForDebugRender(uint canvasWidth, uint canvasHeight) =>
        (uint)Math.Min((canvasWidth / 1.1) / _mapWidth, (canvasHeight / 1.1) / _mapHeight);

    /// <summary>
    /// Gets the middle position of the screen, in canvas coordinates
    /// </summary>
    private Vector2 GetRenderMidPosForDebugRender(uint canvasWidth, uint canvasHeight) =>
        new Vector2(canvasWidth / 2, canvasHeight / 2);
}
