using Godot;
using System;

public enum BubbleType
{
    Happier,
    Sadder
}

[GlobalClass]
public partial class ThinkBubble : Sprite2D
{
    #region Variables

    [Export] private Sprite2D _bubbleSprite = null!;

    [ExportGroup("Textures")] [Export] private Texture2D _happierTexture = null!;

    [Export] private Texture2D _sadderTexture = null!;

    private float _desiredOpacity = 0.0f;
    private float _showingTimer = 0.0f;

    public const float OpacityChangeRate = 2f;

    public const float ShowingTime = 2.0f;

    #endregion

    public override void _Ready()
    {
        _bubbleSprite.SelfModulate = new Color(1, 1, 1, 0);
    }

    public override void _Process(double delta)
    {
        if (_showingTimer > 0.0f)
        {
            _showingTimer -= (float)delta;

            if (_showingTimer <= 0.0f)
            {
                _desiredOpacity = 0.0f;
                _showingTimer = -1.0f;
            }
        }

        var currentOpacity = _bubbleSprite.SelfModulate.A;
        var newOpacity = currentOpacity > _desiredOpacity
            ? Math.Max(currentOpacity - (float)delta * OpacityChangeRate, _desiredOpacity)
            : Math.Min(currentOpacity + (float)delta * OpacityChangeRate, _desiredOpacity);

        _bubbleSprite.SelfModulate = new Color(1, 1, 1, newOpacity);
    }

    public void ShowBubble(BubbleType type)
    {
        _desiredOpacity = 1.0f;
        _showingTimer = ShowingTime;

        _bubbleSprite.SelfModulate = new Color(1, 1, 1, 0);
        _bubbleSprite.Texture = type switch
        {
            BubbleType.Happier => _happierTexture,
            BubbleType.Sadder => _sadderTexture,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}