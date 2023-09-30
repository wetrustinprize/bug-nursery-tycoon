using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Game : Node2D
{
    #region Variables

    [ExportGroup("Terrarium")] [Export] private Terrarium[] _terrariums = null!;

    [ExportGroup("Pet")] [Export] private PackedScene _petScene = null!;
    [Export] private DebugPetSpawn[] _debugPets = null!;

    private readonly List<Pet> _pets = new();

    public Terrarium? SelectedTerrarium { get; private set; }

    public int SelectedTerrariumIndex =>
        SelectedTerrarium != null ? _terrariums.ToList().IndexOf(SelectedTerrarium) : -1;

    public static Game Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;

        // DEBUG
        foreach (var debugPet in _debugPets)
            AddPet(debugPet.Pet, debugPet.TerrariumIndex);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("unfocus"))
            SelectedTerrarium = null;
    }

    public void AddPet(PetType pet, int terrariumIndex = -1)
    {
        var petNode = _petScene.Instantiate<Pet>();

        petNode.SetTerrariumIndex(terrariumIndex);
        petNode.SetPetType(pet);

        if (terrariumIndex > -1)
            _terrariums[terrariumIndex].AddChild(petNode);

        _pets.Add(petNode);
    }

    public void SelectTerrarium(Terrarium terrarium)
    {
        SelectedTerrarium = terrarium;
        Player.Instance.FocusAt(terrarium.Position);
    }
}