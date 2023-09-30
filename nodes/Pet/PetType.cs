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

[GlobalClass]
public partial class PetType : Resource
{
    [ExportGroup("Pet Information")]
    [Export]
    public string Name { get; private set; } = null!;

    [Export] public PetFoodType FoodType { get; private set; }
    [Export] public PetBiome Biome { get; private set; }
    [Export] public PetTemperature Temperature { get; private set; }

    [ExportGroup("Graphics")] [Export] public Texture2D Graphic { get; private set; } = null!;
}