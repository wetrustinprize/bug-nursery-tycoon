using System;
using Godot;

public partial class PetInformation : Control
{
    #region Variables

    [Export] private AudioStreamPlayer _clickSound = null!;

    [ExportGroup("Header")] [Export] private Label _nameLabel = null!;
    [Export] private TextureRect _petGraphic = null!;
    [Export] private TextureRect _biomeGraphic = null!;
    [Export] private TextureRect _temperatureGraphic = null!;
    [Export] private TextureRect _foodGraphic = null!;
    [Export] private Label _happinessDecayLabel = null!;

    [ExportGroup("Stats")] [Export] private ProgressBar _happinessProgressBar = null!;
    [Export] private ProgressBar _angerProgressBar = null!;
    [Export] private ProgressBar _deathProgressBar = null!;

    [ExportGroup("Personality")] [Export] private Container _personalityContainer = null!;

    [ExportGroup("Textures")] [ExportSubgroup("Traits")] [Export]
    private Texture2D _desertBiomeTexture = null!;

    [Export] private Texture2D _forestBiomeTexture = null!;
    [Export] private Texture2D _swampBiomeTexture = null!;
    [Export] private Texture2D _coldTemperatureTexture = null!;
    [Export] private Texture2D _hotTemperatureTexture = null!;
    [Export] private Texture2D _normalTemperatureTexture = null!;
    [Export] private Texture2D _carnivoreTexture = null!;
    [Export] private Texture2D _herbivoreTexture = null!;

    [ExportSubgroup("Personalities")] [Export]
    private Texture2D _agressiveTexture = null!;

    [Export] private Texture2D _aloneTexture = null!;
    [Export] private Texture2D _socialTexture = null!;

    private Pet? _pet;

    public static PetInformation Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_pet == null) return;

        _happinessProgressBar.Value = _pet.Happiness * 100;
        _angerProgressBar.Value = _pet.Anger * 100;
        _deathProgressBar.Value = _pet.DeathTimer / Pet.DeathTimerMax * 100;
        _happinessDecayLabel.Text = _pet.HappinessRate * 100 + "% / sec";
    }

    public void HidePet()
    {
        _pet = null;
        Visible = false;
    }

    public void ShowPet(Pet pet)
    {
        Visible = true;

        _pet = pet;

        _nameLabel.Text = pet.PetType.Name;
        _petGraphic.Texture = pet.PetType.Graphic;

        _biomeGraphic.Texture = pet.PetType.Biome switch
        {
            PetBiome.Desert => _desertBiomeTexture,
            PetBiome.Forest => _forestBiomeTexture,
            PetBiome.Swamp => _swampBiomeTexture,
            _ => throw new ArgumentOutOfRangeException($"Unknow biome {pet.PetType.Biome}")
        };

        _temperatureGraphic.Texture = pet.PetType.Temperature switch
        {
            PetTemperature.Cold => _coldTemperatureTexture,
            PetTemperature.Hot => _hotTemperatureTexture,
            PetTemperature.Normal => _normalTemperatureTexture,
            _ => throw new ArgumentOutOfRangeException($"Unknown temperature {pet.PetType.Temperature}")
        };

        _foodGraphic.Texture = pet.PetType.FoodType switch
        {
            PetFoodType.Carnivore => _carnivoreTexture,
            PetFoodType.Herbivore => _herbivoreTexture,
            _ => throw new ArgumentOutOfRangeException($"Unknown food type {pet.PetType.FoodType}")
        };

        foreach (var child in _personalityContainer.GetChildren())
        {
            _personalityContainer.RemoveChild(child);
            child.QueueFree();
        }

        if (pet.PetType.Personality.HasFlag(PetPersonality.Agressive))
            AddPersonality(_agressiveTexture);

        if (pet.PetType.Personality.HasFlag(PetPersonality.Alone))
            AddPersonality(_aloneTexture);

        if (pet.PetType.Personality.HasFlag(PetPersonality.Social))
            AddPersonality(_socialTexture);
    }

    private void AddPersonality(Texture2D texture)
    {
        var textureRect = new TextureRect
        {
            Texture = texture,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
            CustomMinimumSize = new Vector2(32, 32)
        };

        _personalityContainer.AddChild(textureRect);
    }

    public void MoveTo(int terrarium)
    {
        if (_pet == null) return;
        _clickSound.Play();

        Game.Instance.MovePet(_pet, terrarium);
    }
}