using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class PetBox : Node2D
{
    #region Variables

    [Export] private Sprite2D _outerSprite = null!;

    public bool IsViewingInside = false;

    public const float OpacityChangeRate = 2f;

    public List<Pet> Pets { get; } = new();

    #endregion

    public override void _Process(double delta)
    {
        Modulate = new Color(
            1, 1, 1,
            CalculateDesiredOpacity(Modulate.A, Pets.Count > 0 ? 1.0f : 0.0f, delta)
        );

        _outerSprite.Modulate = new Color(
            1, 1, 1,
            CalculateDesiredOpacity(_outerSprite.Modulate.A, !IsViewingInside ? 1.0f : 0.0f, delta)
        );
    }

    float CalculateDesiredOpacity(float from, float to, double delta)
    {
        return from > to
            ? Math.Max(from - (float)delta * OpacityChangeRate, to)
            : Math.Min(from + (float)delta * OpacityChangeRate, to);
    }

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus") && Pets.Count > 0)
            Game.Instance.FocusPetBox();
    }
}