using SFML.Graphics;
using System.Numerics;

/// <summary>
/// Handles input for the <see cref="RaycasterAppManager"/>
/// </summary>
internal class InputController
{
    private RenderWindow? _renderWindow;
    private SFML.System.Vector2i _windowCentre;
    private float _mouseSensitivity = 3.0f;

    /// <summary>
    /// Whether the mouse is currently hidden and held in the middle of the screen
    /// </summary>
    public bool MouseCaptured { get; private set; }

    public InputController(RenderWindow renderWindow)
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

    /// <summary>
    /// Hides the cursor and holds it in the middle of the screen so movement can be used for analogue input each frame
    /// </summary>
    private void CaptureMouse(bool capture)
    {
        _renderWindow!.SetMouseCursorGrabbed(capture);
        _renderWindow!.SetMouseCursorVisible(!capture);
        SFML.Window.Mouse.SetPosition(_windowCentre, _renderWindow);
        MouseCaptured = capture;
    }

    // Public flags checking if the WASD keys are pressed and the mouse is captured, for movement
    public bool MoveForward => MouseCaptured && SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.W);
    public bool MoveLeft => MouseCaptured && SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.A);
    public bool MoveBackwards => MouseCaptured && SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.S);
    public bool MoveRight => MouseCaptured && SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.D);

    // Hold space to show debug info
    public bool ShowDebug => MouseCaptured && SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Space);

    /// <summary>
    /// Gets a mouse sensitivity adjusted mouse movement vector, which can be used for analogue input
    /// </summary>
    /// <returns></returns>
    public Vector2 GetSensitivityAdjustedMouseDeltaAndResetToCentre()
    {
        var mousePos = SFML.Window.Mouse.GetPosition(_renderWindow);
        SFML.Window.Mouse.SetPosition(_windowCentre, _renderWindow);
        var mouseDelta = mousePos - _windowCentre;
        var sensitivityAdjustedDelta = new Vector2(
            SensitivityAdjustMouseMovement(mouseDelta.X),
            SensitivityAdjustMouseMovement(mouseDelta.Y));
        return sensitivityAdjustedDelta;
    }

    // Convert from pixels moved to a sensitivity adjusted floating point value for analogue input
    private float SensitivityAdjustMouseMovement(int value) => value / 1000f * _mouseSensitivity;
}
