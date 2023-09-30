using Godot;

[GlobalClass]
public partial class Player : Node2D
{
    #region Variables

    [Export] private Camera2D _camera2D = null!;
    [Export] private Vector2 _focusZoom;
    [Export] private float _focusSpeed = 4.0f;
    
    private Vector2 _desiredPosition;
    private Vector2 _desiredZoom;

    private Vector2 _initialPosition;
    private Vector2 _initialZoom;

    public static Player Instance { get; private set; } = null!;
    
    #endregion

    public override void _Ready()
    {
        Instance = this;
        
        _initialPosition = Position;
        _initialZoom = _camera2D.Zoom;

        _desiredPosition = _initialPosition;
        _desiredZoom = _initialZoom;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = Position.Lerp(_desiredPosition, (float)delta * _focusSpeed);
        _camera2D.Zoom = _camera2D.Zoom.Lerp(_desiredZoom, (float)delta * _focusSpeed);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("unfocus"))
            Unfocus();
    }

    #region Camera Focus

    public void Unfocus()
    {
        _desiredPosition = _initialPosition;
        _desiredZoom = _initialZoom;
    }

    public void FocusAt(Vector2 position)
    {
        _desiredPosition = position;
        _desiredZoom = _focusZoom;
    }

    #endregion
}