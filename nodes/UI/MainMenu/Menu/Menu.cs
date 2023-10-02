using Godot;
using System;

public partial class Menu : Control
{
    #region Variables

    [Export] private AudioStreamPlayer _clickSound = null!;
    [Export] private PackedScene _gameScene = null!;
    [Export] private Control _credits = null!;
    [Export] private Control _help = null!;

    #endregion

    public override void _Ready()
    {
        _credits.Visible = false;
        _help.Visible = false;
    }

    #region Callbacks

    public void StartGame()
    {
        var game = _gameScene.Instantiate();
        GetTree().Root.AddChild(game);
        QueueFree();
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