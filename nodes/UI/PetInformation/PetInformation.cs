using System;
using Godot;

public partial class PetInformation : Control
{
    #region Variables

    [ExportGroup("Header")] [Export] private Label _nameLabel = null!;
    [Export] private TextureRect _petGraphic = null!;
    [Export] private TextureRect _biomeGraphic = null!;
    [Export] private TextureRect _temperatureGraphic = null!;

    [ExportGroup("Stats")] [Export] private ProgressBar _happinessProgressBar = null!;
    [Export] private ProgressBar _angerProgressBar = null!;

    [ExportGroup("Textures")] [Export] private Texture2D _desertBiomeTexture = null!;
    [Export] private Texture2D _forestBiomeTexture = null!;
    [Export] private Texture2D _swampBiomeTexture = null!;
    [Export] private Texture2D _coldTemperatureTexture = null!;
    [Export] private Texture2D _hotTemperatureTexture = null!;
    [Export] private Texture2D _normalTemperatureTexture = null!;

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
    }

    public void MoveTo(int terrarium)
    {
        if (_pet == null) return;

        Game.Instance.MovePet(_pet, terrarium);
    }
}