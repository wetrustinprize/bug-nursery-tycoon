using Godot;
using System;

public struct EndOfDayResult
{
    public int CurrentLevel { get; init; }
    public int TotalLevels { get; init; }
    public float AverageHappiness { get; init; }
}

public partial class EndOfDayDialog : Control
{
    #region Variables

    [Export] private Label _dayLabel = null!;
    [Export] private Label _avgLabel = null!;
    [Export] private AudioStreamPlayer _clickSound = null!;

    private bool _isVisible = false;

    public const float OpacityChangeRate = 2f;
    public static EndOfDayDialog Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;
        Modulate = new Color(1, 1, 1, 0);
        Visible = false;
        HideDialog();
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

    public void Show(EndOfDayResult result)
    {
        _isVisible = true;
        MouseFilter = MouseFilterEnum.Stop;

        _dayLabel.Text = $"Day {result.CurrentLevel + 1}/{result.TotalLevels}";
        _avgLabel.Text = result.AverageHappiness.ToString("P1");
    }

    public void HideDialog()
    {
        _isVisible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void NextDay()
    {
        _clickSound.Play();
        HideDialog();
        Game.Instance.NextLevel();
    }

    public void MainMenu()
    {
        _clickSound.Play();
        foreach (var node in GetTree().GetNodesInGroup("Game"))
            node.QueueFree();

        GetTree().ReloadCurrentScene();
        GetParent().QueueFree();
    }
}