using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TerrariumInformation : Control
{
    #region Variable

    [Export] private AudioStreamPlayer _clickSound = null!;
    [Export] private Button[] _biomeButtons = null!;
    [Export] private Button[] _temperatureButtons = null!;
    [Export] private Godot.Timer _biomeTimer = null!;
    [Export] private Godot.Timer _temperatureTimer = null!;

    private Terrarium? _terrarium;

    public static TerrariumInformation Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;
        Visible = false;
    }

    public void ShowTerrarium(Terrarium terrarium)
    {
        Visible = true;

        _terrarium = terrarium;
    }

    public void HideTerrarium()
    {
        Visible = false;
    }

    void SetButtons(IEnumerable<Button> buttons, bool disabled)
    {
        foreach (var button in buttons)
            button.Disabled = disabled;
    }

    public void BiomeTimeout()
    {
        SetButtons(_biomeButtons, false);
    }

    public void TemperatureTimeout()
    {
        SetButtons(_temperatureButtons, false);
    }

    public void UpdateBiome(int biome)
    {
        if (_terrarium == null) return;
        if (!_terrarium.CanChangeBiome) return;

        var newBiome = biome switch
        {
            0 => PetBiome.Forest,
            1 => PetBiome.Desert,
            2 => PetBiome.Swamp,
            _ => throw new ArgumentOutOfRangeException(nameof(biome), biome, null)
        };

        _terrarium.UpdateBiome(newBiome);
        SetButtons(_biomeButtons, true);
        _biomeTimer.Start();
        _clickSound.Play();
    }

    public void UpdateTemperature(int temperature)
    {
        if (_terrarium == null) return;
        if (!_terrarium.CanChangeTemperature) return;

        var newTemperature = temperature switch
        {
            0 => PetTemperature.Cold,
            1 => PetTemperature.Normal,
            2 => PetTemperature.Hot,
            _ => throw new ArgumentOutOfRangeException(nameof(temperature), temperature, null)
        };

        _terrarium.UpdateTemperature(newTemperature);
        SetButtons(_temperatureButtons, true);
        _temperatureTimer.Start();
        _clickSound.Play();
    }
}