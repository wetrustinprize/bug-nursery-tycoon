using Godot;

[GlobalClass]
public partial class DebugPetSpawn : Resource
{
    [Export] public int TerrariumIndex { get; private set; }
    [Export] public PetType Pet { get; private set; }
}
