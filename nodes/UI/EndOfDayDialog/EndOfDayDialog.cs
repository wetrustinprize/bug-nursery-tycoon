using Godot;
using System;

public partial class EndOfDayDialog : Control
{
    #region Variables

    [Export] private Label _coinsLabel = null!;

    private bool _isVisible = false;

    public const float OpacityChangeRate = 2f;
    public static EndOfDayDialog Instance { get; private set; } = null!;

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

    public void Show(int totalCoins)
    {
        _isVisible = true;
        MouseFilter = MouseFilterEnum.Stop;
        _coinsLabel.Text = $"{totalCoins} coins";
    }

    public void Hide()
    {
        _isVisible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void NextDay()
    {
        Game.Instance.NextDay();
    }
}