using System.Linq;
using Godot;

[GlobalClass]
public partial class Pet : RigidBody2D
{
    #region Variables

    [Export] private Sprite2D _petGraphic = null!;
    [Export] private ThinkBubble _thinkBubble = null!;
    [Export] private DeathBubble _deathBubble = null!;
    [Export] private AudioStreamPlayer _deathSound = null!;

    public float Happiness = 1.0f;
    public const float HappinessRegenRate = 0.01f;
    public const float HappinessDecayRate = 0.001f;

    public float Anger = 0.0f;
    public const float AngerRegenRate = 0.05f;

    public float DeathTimer = 0.0f;
    public const float DeathTimerMax = 30.0f;
    public const float ShowDeathTimerAt = 10.0f;

    public Terrarium? Terrarium;

    private PetType _petType = null!;
    private float _hapinessDiff = 0.0f;
    private Vector2 _movingDirection = Vector2.Zero;
    private float _walkAnimationOffset = 0.0f;

    public static Pet? HoveredPet = null;
    public const float ThinkBubbleDiff = 0.2f;
    public const float PetSpeed = 50.0f;

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

    public float HappinessRate
    {
        get
        {
            if (Terrarium == null) return 0.0f;

            var rate = 0.0f;

            var isAlone = Terrarium.Pets.Count == 1 && Terrarium.Pets.First() == this;
            var allSameType = Terrarium.Pets.All(pet => pet.PetType.Name == PetType.Name);

            var correctBiome = Terrarium.Biome == PetType.Biome;
            var correctTemperature = Terrarium.Temperature == PetType.Temperature;

            var satisfiedPersonalities = (int)0;
            var unsatisfiedPersonalities = (int)0;

            if (PetType.Personality.HasFlag(PetPersonality.Alone))
                if (isAlone)
                    satisfiedPersonalities++;
                else
                    unsatisfiedPersonalities++;

            if (PetType.Personality.HasFlag(PetPersonality.Social))
                if (!isAlone)
                    satisfiedPersonalities++;
                else
                    unsatisfiedPersonalities++;

            if (PetType.Personality.HasFlag(PetPersonality.Agressive))
                if (allSameType)
                    satisfiedPersonalities++;
                else
                    unsatisfiedPersonalities++;

            rate += correctBiome ? 1.0f : -2.0f;
            rate += correctTemperature ? 1.0f : -1.0f;

            rate += unsatisfiedPersonalities * -1.0f;
            rate += satisfiedPersonalities * 1.0f;

            return rate / 100;
        }
    }

    public override void _Ready()
    {
        GetNewRandomDirection();

        _walkAnimationOffset = GD.RandRange(-100, 100);
    }

    public override void _Process(double delta)
    {
        _petGraphic.Skew = Mathf.Sin(Time.GetTicksMsec() / 500.0f + _walkAnimationOffset) / 10.0f;
        _petGraphic.Scale = HoveredPet == this ? new Vector2(1.1f, 1.1f) : Vector2.One;
    }

    public override void _PhysicsProcess(double delta)
    {
        var result = MoveAndCollide(_movingDirection * PetSpeed * (float)delta);

        if (result != null)
        {
            var normal = result
                .GetNormal();

            var maxRandom = 0.5f;
            var x = Mathf.Clamp(_movingDirection.X + (float)GD.RandRange(-maxRandom, maxRandom), -1.0f, 1.0f);
            var y = Mathf.Clamp(_movingDirection.Y + (float)GD.RandRange(-maxRandom, maxRandom), -1.0f, 1.0f);

            _movingDirection = new Vector2(x, y);
            _movingDirection = _movingDirection.Normalized();

            _petGraphic.FlipH = _movingDirection.X > 0.0f;
        }
    }

    private void GetNewRandomDirection()
    {
        var x = Mathf.Clamp(_movingDirection.X + (float)GD.RandRange(-1.0f, 1.0f), -1.0f, 1.0f);
        var y = Mathf.Clamp(_movingDirection.Y + (float)GD.RandRange(-1.0f, 1.0f), -1.0f, 1.0f);

        var newDirection = new Vector2(x, y);
        newDirection.Normalized();

        _movingDirection = newDirection;
        _petGraphic.FlipH = _movingDirection.X > 0.0f;
    }

    #region Death Timer

    public bool UpdateDeathTimer(double delta)
    {
        if (DeathTimer <= 0.0f) return false;

        DeathTimer -= (float)delta;

        var showDeath = DeathTimer <= ShowDeathTimerAt;

        if (showDeath && !_deathSound.Playing)
            _deathSound.Play();

        _deathBubble.IsVisible = showDeath;
        _deathBubble.SetProgress(DeathTimer / ShowDeathTimerAt);

        var timeIsOver = DeathTimer <= 0.0f;
        if (!timeIsOver) return false;

        DeathTimer = -1.0f;
        return true;
    }

    public void StartDeathTimer()
    {
        if (DeathTimer <= 0.0f)
            DeathTimer = DeathTimerMax;
    }

    public void StopDeathTimer()
    {
        _deathBubble.IsVisible = false;
        _deathSound.Stop();
        DeathTimer = -1.0f;
    }

    #endregion

    #region Update Stats

    public void MakeAngry()
    {
        Anger = 1.0f;
        _thinkBubble.ShowBubble(BubbleType.Angry);
    }

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

        var diff = HappinessRate * (float)delta;

        _hapinessDiff += diff;
        Happiness = Mathf.Clamp(Happiness + diff, 0.0f, 1.0f);

        if (_hapinessDiff is >= ThinkBubbleDiff or <= -ThinkBubbleDiff)
        {
            _thinkBubble.ShowBubble(_hapinessDiff > ThinkBubbleDiff ? BubbleType.Happier : BubbleType.Sadder);
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

    #endregion
}