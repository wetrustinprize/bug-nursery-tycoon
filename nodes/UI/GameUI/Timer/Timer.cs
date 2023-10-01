using Godot;
using System;

[GlobalClass]
public partial class Timer : Control
{
    #region Variables

    [Export] private Label _timerLabel;

    public bool TimerVisible = false;
    public const float OpacityChangeRate = 2f;

    public static Timer Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        var currentOpacity = _timerLabel.Modulate.A;
        var desiredOpacity = TimerVisible ? 1.0f : 0.0f;
        var newOpacity = currentOpacity > desiredOpacity
            ? Math.Max(currentOpacity - (float)delta * OpacityChangeRate, desiredOpacity)
            : Math.Min(currentOpacity + (float)delta * OpacityChangeRate, desiredOpacity);

        _timerLabel.Modulate = new Color(1, 1, 1, newOpacity);
    }

    public void SetTimer(float time)
    {
        var minutes = Mathf.FloorToInt(time / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        var secondsString = seconds.ToString();
        if (seconds < 10)
            secondsString = "0" + secondsString;

        _timerLabel.Text = $"{minutes}:{secondsString}";
    }
}