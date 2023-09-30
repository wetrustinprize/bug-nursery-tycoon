using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Terrarium : Node2D
{
    #region Variables

    public bool IsSelected => Game.Instance.SelectedTerrarium == this;

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

    #region Callbacks

    public void UpdateBiomeKnob(int value)
    {
        Biome = value switch
        {
            0 => PetBiome.Forest,
            1 => PetBiome.Desert,
            2 => PetBiome.Swamp,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public void UpdateTemperatureKnob(int value)
    {
        Temperature = value switch
        {
            0 => PetTemperature.Cold,
            1 => PetTemperature.Normal,
            2 => PetTemperature.Hot,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public void Area2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("focus"))
            Game.Instance.SelectTerrarium(this);
    }

    #endregion
}