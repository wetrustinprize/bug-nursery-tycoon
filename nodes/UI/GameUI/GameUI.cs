using Godot;
using System;

public partial class GameUI : CanvasLayer
{
    [Export] private Button _creatureInBox = null!;

    public override void _Process(double delta)
    {
        _creatureInBox.Disabled = Game.Instance.State != GameState.Preparing;
    }

    public void GoToBox()
    {
        if (!Game.Instance.SelectedIsPetBox)
            Game.Instance.FocusPetBox();
    }
}