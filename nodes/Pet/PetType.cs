using System;
using Godot;

public enum PetFoodType
{
    Carnivore,
    Herbivore,
}

public enum PetBiome
{
    Desert,
    Forest,
    Swamp,
}

public enum PetTemperature
{
    Cold,
    Hot,
    Normal,
}

[Flags]
public enum PetPersonality
{
    Agressive = 1 << 1,
    Alone = 1 << 2,
    Social = 1 << 3
}

[GlobalClass]
public partial class PetType : Resource
{
    #region Variables

    [ExportGroup("Pet Information")]
    [Export]
    public string Name { get; private set; } = null!;

    [Export] public PetFoodType FoodType { get; private set; }
    [Export] public PetBiome Biome { get; private set; }
    [Export] public PetTemperature Temperature { get; private set; }
    [Export] public PetPersonality Personality { get; private set; }

    [ExportGroup("Graphics")] [Export] public Texture2D Graphic { get; private set; } = null!;

    #endregion

    public void RandomizePersonality()
    {
        // Change the personality of the pet randomly
        // It cannot be Social and Alone at the same time, select one of them randomly
        // It can also have no personality at all

        var random = new Random();
        var personality = (PetPersonality)random.Next(1, 8);

        if (personality.HasFlag(PetPersonality.Alone) && personality.HasFlag(PetPersonality.Social))
        {
            personality &= ~PetPersonality.Alone;
            personality &= ~PetPersonality.Social;
            personality |= (PetPersonality)random.Next(1, 3);
        }

        Personality = personality;
    }
}