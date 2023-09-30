using Godot;

[GlobalClass]
public partial class Terrarium : Node2D
{
    #region Variables

    public bool IsSelected => Game.Instance.SelectedTerrarium == this;

    #endregion

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus"))
            Game.Instance.SelectTerrarium(this);
    }
}