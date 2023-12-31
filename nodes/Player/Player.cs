using Godot;

[GlobalClass]
public partial class Player : Node2D
{
    #region Variables

    [Export] private Camera2D _camera2D = null!;
    [Export] private float _focusZoom = 0.9f;
    [Export] private float _focusSpeed = 4.0f;
    [Export] private Vector2 _offset = new Vector2(0, -900);

    public bool AddOffset = false;

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
        Position = Position.Lerp(
            _desiredPosition + (AddOffset ? _offset : Vector2.Zero),
            (float)delta * _focusSpeed
        );

        _camera2D.Zoom = _camera2D.Zoom.Lerp(_desiredZoom, (float)delta * _focusSpeed);
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
        _desiredZoom = new Vector2(_focusZoom, _focusZoom);
    }

    #endregion
}