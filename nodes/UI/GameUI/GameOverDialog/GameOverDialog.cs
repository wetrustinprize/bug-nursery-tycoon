using Godot;
using System;

public partial class GameOverDialog : Control
{
    #region Variables

    [Export] private TextureRect _petSprite = null!;
    [Export] private Label _petNameLabel = null!;
    [Export] private AudioStreamPlayer _clickSound = null!;

    private bool _isVisible = false;

    public const float OpacityChangeRate = 2f;

    public static GameOverDialog Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;
        Modulate = new Color(1, 1, 1, 0);
        Visible = false;
        Hide();
    }

    public override void _Process(double delta)
    {
        var currentOpacity = Modulate.A;
        var desiredOpacity = _isVisible ? 1.0f : 0.0f;
        var newOpacity = currentOpacity > desiredOpacity
            ? Math.Max(currentOpacity - (float)delta * OpacityChangeRate, desiredOpacity)
            : Math.Min(currentOpacity + (float)delta * OpacityChangeRate, desiredOpacity);

        Modulate = new Color(1, 1, 1, newOpacity);
        Visible = newOpacity > 0.1f;
    }

    public void Restart()
    {
        _clickSound.Play();
        Hide();
        Game.Instance.RestartGame();
    }

    public void MainMenu()
    {
        _clickSound.Play();
        foreach (var node in GetTree().GetNodesInGroup("Game"))
            node.QueueFree();

        GetTree().ReloadCurrentScene();
        GetParent().QueueFree();
    }

    public void Show(Pet diedPet)
    {
        _isVisible = true;
        MouseFilter = MouseFilterEnum.Stop;

        _petSprite.Texture = diedPet.PetType.Graphic;
        _petNameLabel.Text = $"You let {diedPet.PetType.Name} die.";
    }

    public void Hide()
    {
        _isVisible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }
}