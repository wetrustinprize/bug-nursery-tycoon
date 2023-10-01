using Godot;

public partial class Knob : Node2D
{
    #region Variables

    [Export] private Sprite2D _knobSprite = null!;

    private int _currentValue;

    #endregion

    public int CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            _knobSprite.RotationDegrees = -90 + value * 45;
        }
    }

    public override void _Ready()
    {
        _knobSprite.RotationDegrees = -90;
    }
}