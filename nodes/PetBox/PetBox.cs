using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PetBox : Node2D
{
    #region Variables

    public const float OpacityChangeRate = 2f;

    public List<Pet> Pets { get; } = new();

    #endregion

    public override void _Process(double delta)
    {
        var currentOpacity = SelfModulate.A;
        var desiredOpacity = Pets.Count > 0 ? 1.0f : 0.0f;
        var newOpacity = currentOpacity > desiredOpacity
            ? Math.Max(currentOpacity - (float)delta * OpacityChangeRate, desiredOpacity)
            : Math.Min(currentOpacity + (float)delta * OpacityChangeRate, desiredOpacity);

        SelfModulate = new Color(1, 1, 1, newOpacity);
    }

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus") && Pets.Count > 0)
            Game.Instance.FocusPetBox();
    }
}