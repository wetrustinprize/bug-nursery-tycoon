using Godot;
using System;

[GlobalClass]
public partial class DeathBubble : Control
{
    #region Variables

    [Export] private TextureProgressBar _progressBar;
    [Export] private Sprite2D _deathSprite;

    public bool IsVisible = false;

    private float _desiredProgress = 0.0f;
    private float _progress = 0.0f;

    public const float OpacityChangeRate = 2f;

    #endregion

    public override void _Process(double delta)
    {
        var currentOpacity = Modulate.A;
        var desiredOpacity = IsVisible ? 1.0f : 0.0f;
        var newOpacity = currentOpacity > desiredOpacity
            ? Math.Max(currentOpacity - (float)delta * OpacityChangeRate, desiredOpacity)
            : Math.Min(currentOpacity + (float)delta * OpacityChangeRate, desiredOpacity);

        Modulate = new Color(1, 1, 1, newOpacity);
    }

    public void SetProgress(float progress)
    {
        _desiredProgress = progress * 100;
        _progressBar.Value = progress * 100;
    }
}