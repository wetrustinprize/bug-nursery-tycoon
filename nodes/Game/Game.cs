using System.Collections.Generic;
using System.Linq;
using Godot;

public enum GameState
{
    Preparing,
    Running,
    GameOver,
    Shop,
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
    public float RoundTime { get; private set; } = RoundTimer;

    private readonly List<Pet> _pets = new();

    public Node2D? SelectedFocus { get; private set; }
    public bool SelectedIsPetBox => SelectedFocus == _petBox;

    public static Game Instance { get; private set; } = null!;

    public const float RoundTimer = 60f * 3;

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
        // CheckHoverPet();

        switch (State)
        {
            case GameState.Running:
                UpdateTimer(delta);
                UpdatePets(delta);
                break;
        }
    }

    #region Game Loop

    public void NextDay()
    {
        DeleteAllPets();

        // shall go to the shop, but currently will only reset
        foreach (var debugPet in _debugPets)
            CreatePet(debugPet);
    }

    public void GameOver(Pet diedPet)
    {
        State = GameState.GameOver;
        Timer.Instance.TimerVisible = false;

        GameOverDialog.Instance.Show(diedPet);
        FocusTerrarium(diedPet.Terrarium);
    }

    private void UpdateTimer(double delta)
    {
        RoundTime -= (float)delta;
        Timer.Instance.SetTimer(RoundTime);

        if (RoundTime > 0) return;

        State = GameState.Shop;
        Timer.Instance.TimerVisible = false;
        FocusTerrarium(null);
        EndOfDayDialog.Instance.Show(100);
    }

    private void UpdatePets(double delta)
    {
        foreach (var pet in _pets)
        {
            var died = pet.UpdateStats(delta);

            if (died) GameOver(pet);
        }
    }

    #endregion

    #region Pet Handling

    void CheckHoverPet()
    {
        var space = GetWorld2D().DirectSpaceState;
        var mousePos = GetGlobalMousePosition();

        var options = new PhysicsPointQueryParameters2D
        {
            Position = mousePos,
            CollideWithAreas = true,
            CollideWithBodies = false
        };

        var result = space.IntersectPoint(options, 5);
        if (result.Count > 0)
        {
            var collidedPets = new List<Pet>();

            foreach (var dictionary in result)
            {
                if (!dictionary.TryGetValue("collider", out var collider)) continue;

                if (collider.As<Node2D>().GetParent() is Pet)
                    collidedPets.Add(collider.As<Node2D>().GetParent<Pet>());
            }

            collidedPets.Sort((pet1, pet2) => pet1.Transform.Y < pet2.Transform.Y ? 1 : -1);

            Pet.HoveredPet = !collidedPets.Any() ? null : collidedPets.First();
        }
        else
            Pet.HoveredPet = null;
    }

    void CheckPetBox()
    {
        if (State != GameState.Preparing) return;
        if (_petBox.Pets.Count > 0) return;

        FocusTerrarium(null);
        State = GameState.Running;
        Timer.Instance.TimerVisible = true;
    }

    public void DeleteAllPets()
    {
        foreach (var terrarium in Terrariums)
            terrarium.Pets.Clear();

        _pets.ForEach(p => p.QueueFree());
    }

    public void CreatePet(PetType pet, int terrariumIndex = -1)
    {
        var petNode = _petScene.Instantiate<Pet>();

        petNode.PetType = (PetType)pet.Duplicate();

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
            pet.Position = Vector2.Zero;
            pet.StopDeathTimer();

            terrarium.Pets.Add(pet);
            terrarium.UpdatePetDeathTimers();

            if (State == GameState.Running)
                pet.MakeAngry();

            PetInformation.Instance.HidePet();
        }

        CheckPetBox();
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