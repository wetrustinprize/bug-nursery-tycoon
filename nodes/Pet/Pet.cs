using System.Linq;
using Godot;

[GlobalClass]
public partial class Pet : RigidBody2D
{
    #region Variables

    [Export] private Sprite2D _petGraphic = null!;
    [Export] private Sprite2D _deathTimerGraphic = null!;
    [Export] private ThinkBubble _thinkBubble = null!;

    public float Happiness = 1.0f;
    public float Anger = 0.0f;
    public float DeathTimer = 0.0f;

    public Terrarium? Terrarium;

    private PetType _petType = null!;
    private float _hapinessDiff = 0.0f;

    public const float AngerRegenRate = 0.05f;
    public const float HappinessRegenRate = 0.005f;
    public const float HappinessDecayRate = 0.01f;
    public const float DeathTimerMax = 15.0f;
    public const float ThinkBubbleDiff = 0.2f;

    #endregion

    public PetType PetType
    {
        get => _petType;
        set
        {
            _petType = value;
            _petGraphic.Texture = value.Graphic;
        }
    }

    #region Death Timer

    public bool UpdateDeathTimer(double delta)
    {
        if (DeathTimer <= 0.0f) return false;

        DeathTimer -= (float)delta;

        _deathTimerGraphic.Visible = DeathTimer <= 10.0f;

        if (DeathTimer <= 0.0f)
            DeathTimer = 1.0f;

        return DeathTimer <= 0.0f;
    }

    public void StartDeathTimer()
    {
        if (DeathTimer <= 0.0f)
            DeathTimer = DeathTimerMax;
    }

    public void StopDeathTimer()
    {
        DeathTimer = -1.0f;
    }

    #endregion

    #region Update Stats

    public bool UpdateStats(double delta)
    {
        UpdateRage(delta);
        UpdateHappiness(delta);
        var died = UpdateDeathTimer(delta);

        return Happiness <= 0.0f || died;
    }

    private void UpdateRage(double delta)
    {
        var newAnger = Anger - AngerRegenRate * (float)delta;

        Anger = Mathf.Clamp(newAnger, 0.0f, 1.0f);
    }

    private void UpdateHappiness(double delta)
    {
        if (Terrarium == null)
        {
            GD.PrintErr("Pet is not in a terrarium, can't update its happiness");
            return;
        }

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

        var newUnHappiness = Happiness;

        var diff = unHappinessTotal > 0
            ? HappinessDecayRate * unHappinessTotal * (float)delta * -1
            : HappinessRegenRate * (float)delta;

        _hapinessDiff += diff;
        newUnHappiness += diff;

        Happiness = Mathf.Clamp(newUnHappiness, 0.0f, 1.0f);

        if (_hapinessDiff is >= ThinkBubbleDiff or <= -ThinkBubbleDiff)
        {
            _thinkBubble.ShowBubble(diff > ThinkBubbleDiff ? BubbleType.Happier : BubbleType.Sadder);
            _hapinessDiff = 0.0f;
        }
    }

    #endregion

    #region Callbacks

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (!@event.IsActionPressed("focus")) return;

        if (Game.Instance.SelectedFocus == Terrarium || Game.Instance.SelectedIsPetBox)
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