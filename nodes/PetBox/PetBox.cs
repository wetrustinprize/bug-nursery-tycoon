using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PetBox : Node2D
{
    #region Variables

    [Export] private Sprite2D _boxInnerSprite = null!;

    public bool BoxVisible = false;
    public const float OpacityChangeRate = 2f;

    public List<Pet> Pets { get; } = new();

    #endregion

    public override void _Process(double delta)
    {
        var currentOpacity = _boxInnerSprite.SelfModulate.A;
        var desiredOpacity = !BoxVisible ? 1.0f : 0.0f;
        var newOpacity = currentOpacity > desiredOpacity
            ? Math.Max(currentOpacity - (float)delta * OpacityChangeRate, desiredOpacity)
            : Math.Min(currentOpacity + (float)delta * OpacityChangeRate, desiredOpacity);

        _boxInnerSprite.SelfModulate = new Color(1, 1, 1, newOpacity);
    }

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus") && Pets.Count > 0)
            Game.Instance.FocusPetBox();
    }
}