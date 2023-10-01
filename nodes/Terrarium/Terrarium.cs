using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Terrarium : Node2D
{
    #region Variables

    [Export] private Node2D _petsNode = null!;
    [Export] private AnimationPlayer _animationPlayer = null!;
    [Export] public Node2D FocusPoint = null!;

    [ExportGroup("Sprites")] [Export] private Sprite2D _backgroundSprite = null!;
    [Export] private Sprite2D _objectsSprite = null!;
    [Export] private Sprite2D _groundSprite = null!;
    [Export] private Sprite2D _newGroundSprite = null!;

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

    public bool IsSelected => Game.Instance.SelectedFocus == this;

    private PetBiome _biome = PetBiome.Forest;
    private PetTemperature _temperature = PetTemperature.Cold;

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

    public void UpdateGround()
    {
        _groundSprite.Texture = Biome switch
        {
            PetBiome.Desert => _desertGroundTexture,
            PetBiome.Forest => _forestGroundTexture,
            PetBiome.Swamp => _swampGroundTexture,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void UpdateObjects()
    {
        _objectsSprite.Texture = Biome switch
        {
            PetBiome.Desert => _desertObjectsTexture,
            PetBiome.Forest => _forestObjectsTexture,
            PetBiome.Swamp => _swampObjectsTexture,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void UpdateBackground()
    {
        _backgroundSprite.Texture = Biome switch
        {
            PetBiome.Desert => _desertBackgroundTexture,
            PetBiome.Forest => _forestBackgroundTexture,
            PetBiome.Swamp => _swampBackgroundTexture,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion

    #region Callbacks

    public void UpdateBiome(int biome)
    {
        var newBiome = biome switch
        {
            0 => PetBiome.Forest,
            1 => PetBiome.Desert,
            2 => PetBiome.Swamp,
            _ => throw new ArgumentOutOfRangeException(nameof(biome), biome, null)
        };

        if (newBiome == Biome) return;
        Biome = newBiome;

        _newGroundSprite.Texture = newBiome switch
        {
            PetBiome.Desert => _desertGroundTexture,
            PetBiome.Forest => _forestGroundTexture,
            PetBiome.Swamp => _swampGroundTexture,
            _ => throw new ArgumentOutOfRangeException()
        };

        _animationPlayer.Play("Switch");
    }

    public void UpdateTemperature(int temperature)
    {
        Temperature = temperature switch
        {
            0 => PetTemperature.Cold,
            1 => PetTemperature.Normal,
            2 => PetTemperature.Hot,
            _ => throw new ArgumentOutOfRangeException(nameof(temperature), temperature, null)
        };
    }

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus"))
            Game.Instance.FocusTerrarium(this);
    }

    #endregion
}