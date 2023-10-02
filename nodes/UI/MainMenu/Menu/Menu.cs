using Godot;
using System;

public partial class Menu : Control
{
    #region Variables

    [Export] private AudioStreamPlayer _clickSound = null!;
    [Export] private PackedScene _gameScene = null!;
    [Export] private Control _credits = null!;
    [Export] private Control _help = null!;
    [Export] private Terrarium _terrarium = null!;
    [Export] private PetType[] _displayPets = null!;
    [Export] private Node[] _queueOnStart = null!;
    [Export] private PackedScene _petScene = null!;

    #endregion

    public override void _Ready()
    {
        _credits.Visible = false;
        _help.Visible = false;

        foreach (var pet in _displayPets)
        {
            var petNode = _petScene.Instantiate<Pet>();
            petNode.PetType = pet;

            _terrarium.AddPet(petNode);
        }
    }

    #region Callbacks

    public void StartGame()
    {
        var game = _gameScene.Instantiate();
        GetTree().Root.AddChild(game);
        QueueFree();

        foreach (var node in _queueOnStart)
            node.QueueFree();
    }

    public void ChangeBiomeTimeout()
    {
        _terrarium.UpdateBiome(
            _terrarium.Biome switch
            {
                PetBiome.Desert => PetBiome.Forest,
                PetBiome.Forest => PetBiome.Swamp,
                PetBiome.Swamp => PetBiome.Desert,
                _ => throw new ArgumentOutOfRangeException()
            }
        );
    }

    public void OpenHelp()
    {
        _clickSound.Play();
        _help.Visible = true;
    }

    public void OpenCredits()
    {
        _clickSound.Play();
        _credits.Visible = true;
    }

    public void CloseCredits()
    {
        _clickSound.Play();
        _credits.Visible = false;
    }

    public void CloseHelp()
    {
        _clickSound.Play();
        _help.Visible = false;
    }

    public void ExitGame()
    {
        _clickSound.Play();
        GetTree().Quit();
    }

    #endregion
}