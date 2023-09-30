using Godot;

[GlobalClass]
public partial class Pet : RigidBody2D
{
    #region Variables

    private PetType _type = null!;

    public float Happiness { get; private set; } = 1.0f;
    public float Anger { get; private set; } = 0.0f;

    public int TerrariumIndex { get; private set; } = -1;

    public static float AngerPerSecond { get; } = 0.1f;

    #endregion

    public void SetPetType(PetType type)
    {
        _type = type;
    }

    public void SetTerrariumIndex(int index)
    {
        TerrariumIndex = index;

        var petSelected = index == -1;
        Freeze = petSelected;
    }

    #region Callbacks

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (TerrariumIndex == -1) return;
        if (!@event.IsActionPressed("focus")) return;

        if (Game.Instance.SelectedTerrariumIndex == TerrariumIndex)
        {
            // Will show a modal to change the pet position
        }
    }

    #endregion
}