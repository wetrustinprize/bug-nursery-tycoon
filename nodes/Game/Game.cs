using System.Collections.Generic;
using Godot;

public enum GameState
{
    Preparing,
    Running,
}

[GlobalClass]
public partial class Game : Node2D
{
    #region Variables

    [ExportGroup("Terrarium")] [Export] public Terrarium[] Terrariums = null!;

    [ExportGroup("Pet")] [Export] private PackedScene _petScene = null!;
    [Export] private PetBox _petBox = null!;
    [Export] private PetType[] _debugPets = null!;

    public GameState State { get; private set; } = GameState.Preparing;

    private readonly List<Pet> _pets = new();

    public Node2D? SelectedFocus { get; private set; }
    public bool SelectedIsPetBox => SelectedFocus == _petBox;

    public static Game Instance { get; private set; } = null!;

    #endregion

    public override void _Ready()
    {
        Instance = this;

        // DEBUG
        foreach (var debugPet in _debugPets)
            CreatePet(debugPet);
    }

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("unfocus")) return;

        FocusTerrarium(null);
        PetInformation.Instance.HidePet();
    }

    public override void _Process(double delta)
    {
        switch (State)
        {
            case GameState.Running:
                UpdatePets(delta);
                break;
        }
    }

    #region Game Loop

    private void UpdatePets(double delta)
    {
        foreach (var pet in _pets)
            pet.UpdateStats(delta);
    }

    #endregion

    #region Pet Handling

    public void CreatePet(PetType pet, int terrariumIndex = -1)
    {
        var petNode = _petScene.Instantiate<Pet>();

        petNode.PetType = (PetType)pet.Duplicate();
        petNode.PetType.RandomizePersonality();

        if (terrariumIndex > -1)
        {
            var terrarium = Terrariums[terrariumIndex];

            petNode.Terrarium = terrarium;
            terrarium.AddChild(petNode);
            terrarium.Pets.Add(petNode);
            terrarium.UpdatePetDeathTimers();
        }
        else
        {
            _petBox.AddChild(petNode);
            _petBox.Pets.Add(petNode);
        }

        _pets.Add(petNode);
    }

    public void MovePet(Pet pet, int terrariumIndex)
    {
        if (!_pets.Exists(p => p == pet))
        {
            GD.PrintErr($"Pet {pet.Name} is not being managed by Game, can't be moved.");
            return;
        }

        if (pet.Anger > 0)
        {
            GD.PrintErr($"Pet {pet.Name} is angry, can't be moved.");
            return;
        }

        if (pet.Terrarium != null)
        {
            pet.Terrarium.RemoveChild(pet);
            pet.Terrarium.Pets.RemoveAll(p => p == pet);
            pet.Terrarium.UpdatePetDeathTimers();
        }
        else
        {
            _petBox.RemoveChild(pet);
            _petBox.Pets.RemoveAll(p => p == pet);
        }

        if (terrariumIndex != -1)
        {
            var terrarium = Terrariums[terrariumIndex];
            terrarium.AddChild(pet);

            pet.Terrarium = terrarium;

            terrarium.Pets.Add(pet);
            terrarium.UpdatePetDeathTimers();

            pet.Anger = 1.0f;

            PetInformation.Instance.HidePet();
        }
    }

    #endregion

    #region Focus

    public void FocusPetBox()
    {
        if (SelectedIsPetBox) return;

        FocusTerrarium(null);

        SelectedFocus = _petBox;

        Player.Instance.FocusAt(_petBox.Position);
        _petBox.BoxVisible = true;
    }

    public void FocusTerrarium(Terrarium? terrarium, bool resetHud = true)
    {
        if (terrarium == SelectedFocus && terrarium != null) return;

        SelectedFocus = terrarium;
        _petBox.BoxVisible = false;

        if (resetHud)
            PetInformation.Instance.HidePet();

        if (terrarium == null)
            Player.Instance.Unfocus();
        else
            Player.Instance.FocusAt(terrarium.Position);
    }

    #endregion
}