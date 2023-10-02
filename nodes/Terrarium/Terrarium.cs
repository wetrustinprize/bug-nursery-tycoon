using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Terrarium : Node2D
{
    #region Variables

    [Export] private Node2D _petsNode = null!;
    [Export] private AnimationPlayer _terrariumAnimationPlayer = null!;
    [Export] private AnimationPlayer _temperatureAnimationPlayer = null!;
    [Export] public Node2D FocusPoint = null!;
    [Export] private AudioStreamPlayer _temperatureAudioPlayer = null!;

    [ExportGroup("Sprites")] [Export] private Sprite2D _backgroundSprite = null!;
    [Export] private Sprite2D _objectsSprite = null!;
    [Export] private Sprite2D _groundSprite = null!;
    [Export] private Sprite2D _newGroundSprite = null!;
    [Export] private Sprite2D _temperatureSprite = null!;

    [ExportGroup("Textures")] [ExportSubgroup("Desert")] [Export]
    private Texture2D _desertBackgroundTexture = null!;

    [Export] Texture2D _desertObjectsTexture = null!;
    [Export] Texture2D _desertGroundTexture = null!;

    [ExportSubgroup("Forest")] [Export] private Texture2D _forestBackgroundTexture = null!;
    [Export] private Texture2D _forestObjectsTexture = null!;
    [Export] private Texture2D _forestGroundTexture = null!;

    [ExportSubgroup("Swamp")] [Export] private Texture2D _swampBackgroundTexture = null!;
    [Export] private Texture2D _swampObjectsTexture = null!;
    [Export] private Texture2D _swampGroundTexture = null!;

    [ExportSubgroup("Temperature")] [Export]
    private Texture2D _coldTemperatureTexture = null!;

    [Export] private Texture2D _hotTemperatureTexture = null!;
    [Export] private Texture2D _normalTemperatureTexture = null!;

    [ExportGroup("Sounds")] [ExportSubgroup("Temperature")] [Export]
    private AudioStream _coldTemperatureSound = null!;

    [Export] private AudioStream _hotTemperatureSound = null!;
    [Export] private AudioStream _normalTemperatureSound = null!;

    public bool IsSelected => Game.Instance.SelectedFocus == this;
    public bool CanChangeBiome => !_terrariumAnimationPlayer.IsPlaying();
    public bool CanChangeTemperature => !_temperatureAnimationPlayer.IsPlaying();

    private PetBiome _biome = PetBiome.Forest;
    private PetTemperature _temperature = PetTemperature.Normal;

    public List<Pet> Pets = new();

    #endregion

    public PetBiome Biome
    {
        get => _biome;
        set => _biome = value;
    }

    public PetTemperature Temperature
    {
        get => _temperature;
        set => _temperature = value;
    }

    public void UpdatePetDeathTimers()
    {
        var hasCarnivore = Pets.Any(p => p.PetType.FoodType == PetFoodType.Carnivore);

        Pets
            .FindAll(terrariumPets => terrariumPets.PetType.FoodType == PetFoodType.Herbivore)
            .ForEach(terrariumPets =>
            {
                if (hasCarnivore)
                    terrariumPets.StartDeathTimer();
                else
                    terrariumPets.StopDeathTimer();
            });
    }

    public void AddPet(Pet pet)
    {
        Pets.Add(pet);
        _petsNode.AddChild(pet);
        pet.Position = Vector2.Zero;
    }

    public void RemovePet(Pet pet)
    {
        Pets.Remove(pet);
        _petsNode.RemoveChild(pet);
    }

    #region Animation Methods

    public void UpdateGroundGraphics()
    {
        _groundSprite.Texture = Biome switch
        {
            PetBiome.Desert => _desertGroundTexture,
            PetBiome.Forest => _forestGroundTexture,
            PetBiome.Swamp => _swampGroundTexture,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void UpdateObjectsGraphics()
    {
        _objectsSprite.Texture = Biome switch
        {
            PetBiome.Desert => _desertObjectsTexture,
            PetBiome.Forest => _forestObjectsTexture,
            PetBiome.Swamp => _swampObjectsTexture,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void UpdateBackgroundGraphics()
    {
        _backgroundSprite.Texture = Biome switch
        {
            PetBiome.Desert => _desertBackgroundTexture,
            PetBiome.Forest => _forestBackgroundTexture,
            PetBiome.Swamp => _swampBackgroundTexture,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void UpdateTemperatureGraphics()
    {
        _temperatureSprite.Texture = Temperature switch
        {
            PetTemperature.Cold => _coldTemperatureTexture,
            PetTemperature.Normal => _normalTemperatureTexture,
            PetTemperature.Hot => _hotTemperatureTexture,
            _ => throw new ArgumentOutOfRangeException()
        };

        _temperatureAudioPlayer.Stream = Temperature switch
        {
            PetTemperature.Cold => _coldTemperatureSound,
            PetTemperature.Normal => _normalTemperatureSound,
            PetTemperature.Hot => _hotTemperatureSound,
            _ => throw new ArgumentOutOfRangeException()
        };
        _temperatureAudioPlayer.Play();
    }

    #endregion

    #region Callbacks

    public void UpdateBiome(PetBiome biome)
    {
        if (!CanChangeBiome) return;
        if (biome == Biome) return;
        Biome = biome;

        _newGroundSprite.Texture = biome switch
        {
            PetBiome.Desert => _desertGroundTexture,
            PetBiome.Forest => _forestGroundTexture,
            PetBiome.Swamp => _swampGroundTexture,
            _ => throw new ArgumentOutOfRangeException()
        };

        _terrariumAnimationPlayer.Play("Switch");
    }

    public void UpdateTemperature(PetTemperature temperature)
    {
        if (!CanChangeTemperature) return;
        if (temperature == Temperature) return;
        Temperature = temperature;

        _temperatureAnimationPlayer.Play("Switch");
    }

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus"))
            Game.Instance.FocusTerrarium(this);
    }

    #endregion
}