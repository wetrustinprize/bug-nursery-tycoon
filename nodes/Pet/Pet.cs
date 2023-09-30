using System.Linq;
using Godot;

[GlobalClass]
public partial class Pet : RigidBody2D
{
    #region Variables

    [Export] private Sprite2D _sprite2D;

    public float Happiness = 1.0f;
    public float Anger = 0.0f;
    public Terrarium? Terrarium;

    private PetType _petType = null!;

    public static float AngerRegenRate = 0.05f;
    public static float HappinessDecayRate = 0.005f;

    #endregion

    public PetType PetType
    {
        get => _petType;
        set
        {
            _petType = value;
            _sprite2D.Texture = value.Graphic;
        }
    }

    public void UpdateStats(double delta)
    {
        var newAnger = Anger - AngerRegenRate * (float)delta;

        Anger = Mathf.Clamp(newAnger, 0.0f, 1.0f);

        if (Terrarium == null) return;

        var unHappinessTotal = 0;

        var isAlone = Terrarium.Pets.Count == 1 && Terrarium.Pets.First() == this;
        var allSameType = Terrarium.Pets.All(pet => pet.PetType.Name == PetType.Name);

        var correctBiome = Terrarium.Biome == PetType.Biome;
        var correctTemperature = Terrarium.Temperature == PetType.Temperature;

        #region Unhapiness calculations

        if (!correctBiome) unHappinessTotal++;
        if (!correctTemperature) unHappinessTotal++;

        if (PetType.Personality.HasFlag(PetPersonality.Alone) && !isAlone)
            unHappinessTotal++;

        if (PetType.Personality.HasFlag(PetPersonality.Social) && isAlone)
            unHappinessTotal++;

        if (PetType.Personality.HasFlag(PetPersonality.Agressive) && !allSameType)
            unHappinessTotal++;

        #endregion

        var newUnHappiness = Happiness - HappinessDecayRate * unHappinessTotal * (float)delta;
        Happiness = Mathf.Clamp(newUnHappiness, 0.0f, 1.0f);
    }

    #region Callbacks

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (Terrarium == null) return;
        if (!@event.IsActionPressed("focus")) return;

        if (Game.Instance.SelectedTerrarium == Terrarium)
            PetInformation.Instance.ShowPet(this);
    }

    public void Area2DMouseEnter()
    {
    }

    public void Area2DMouseLeave()
    {
    }

    #endregion
}