using Godot;

public partial class Knob : Node2D
{
    #region Variables

    [Export] private Sprite2D _knobSprite = null!;

    private int _currentValue = 0;

    #endregion

    [Signal]
    public delegate void PressedEventHandler(int value);

    #region Callbacks

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (!@event.IsActionPressed("focus")) return;

        if (_currentValue >= 2)
            _currentValue = 0;
        else _currentValue++;

        _knobSprite.RotationDegrees = 90 * _currentValue;

        EmitSignal(SignalName.Pressed, _currentValue);
    }

    #endregion
}