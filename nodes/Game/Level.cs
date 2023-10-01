using Godot;

[GlobalClass]
public partial class Level : Resource
{
    [Export] public PetType[] PetsInLevel = null!;
}