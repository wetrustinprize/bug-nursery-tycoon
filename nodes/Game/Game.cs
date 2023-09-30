using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Game : Node2D
{
    #region Variables

    [ExportGroup("Terrarium")] [Export] public Terrarium[] Terrariums = null!;

    [ExportGroup("Pet")] [Export] private PackedScene _petScene = null!;
    [Export] private DebugPetSpawn[] _debugPets = null!;

    private readonly List<Pet> _pets = new();

    public Terrarium? SelectedTerrarium { get; private set; }

    public int SelectedTerrariumIndex =>
        SelectedTerrarium != null ? Terrariums.ToList().IndexOf(SelectedTerrarium) : -1;

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
        if (!@event.IsActionPressed("unfocus")) return;

        SelectTerrarium(null);
        PetInformation.Instance.HidePet();
    }

    public override void _Process(double delta)
    {
        UpdatePets(delta);
    }

    #region Game Loop

    private void UpdatePets(double delta)
    {
        foreach (var pet in _pets)
            pet.UpdateStats(delta);
    }

    #endregion

    #region Pet Handling

    public void AddPet(PetType pet, int terrariumIndex = -1)
    {
        var petNode = _petScene.Instantiate<Pet>();

        petNode.Terrarium = Terrariums[terrariumIndex];
        petNode.PetType = pet.CreateInstance();

        petNode.PetType.RandomizePersonality();

        if (terrariumIndex > -1)
        {
            var terrarium = Terrariums[terrariumIndex];

            terrarium.AddChild(petNode);
            terrarium.Pets.Add(petNode);
        }

        _pets.Add(petNode);
    }

    public void MovePet(Pet pet, int terrariumIndex)
    {
        if (!_pets.Exists(p => p == pet))
        {
            GD.PrintErr($"Pet {pet.Name} is not being managed by Game!");
            return;
        }

        if (pet.Anger > 0)
        {
            GD.PrintErr($"Pet {pet.Name} is angry, can't be moved!");
            return;
        }

        if (pet.Terrarium != null)
        {
            pet.Terrarium.RemoveChild(pet);
            pet.Terrarium.Pets.RemoveAll(p => p == pet);
        }

        Terrariums[terrariumIndex].AddChild(pet);
        pet.Terrarium = Terrariums[terrariumIndex];

        pet.Anger = 1.0f;

        SelectTerrarium(Terrariums[terrariumIndex], false);
    }

    #endregion

    public void SelectTerrarium(Terrarium? terrarium, bool resetHud = true)
    {
        if (terrarium == SelectedTerrarium) return;

        SelectedTerrarium = terrarium;

        if (resetHud)
            PetInformation.Instance.HidePet();

        if (terrarium == null)
            Player.Instance.Unfocus();
        else
            Player.Instance.FocusAt(terrarium.Position);
    }
}