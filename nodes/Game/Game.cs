using System.Collections.Generic;
using System.Linq;
using Godot;

public enum GameState
{
    Preparing,
    Running,
    Ending,
    GameOver,
}

[GlobalClass]
public partial class Game : Node2D
{
    #region Variables

    [ExportGroup("Game")] [Export] public Level[] _levels = null!;
    [Export] private AudioStreamPlayer _chimesAudioPlayer = null!;

    [ExportGroup("Terrarium")] [Export] public Terrarium[] Terrariums = null!;

    [ExportGroup("Pet")] [Export] private PackedScene _petScene = null!;
    [Export] private PetBox _petBox = null!;

    public int CurrentLevel { get; private set; } = -1;

    public GameState State { get; private set; } = GameState.Preparing;
    public float RoundTime { get; private set; } = RoundTimer;

    private readonly List<Pet> _pets = new();

    public Node2D? SelectedFocus { get; private set; }
    public bool SelectedIsPetBox => SelectedFocus == _petBox;

    public static Game Instance { get; private set; } = null!;

    public const float RoundTimer = 60f;

    #endregion

    public override void _Ready()
    {
        Instance = this;
        NextLevel();
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
                UpdateTimer(delta);
                UpdatePets(delta);
                break;
        }
    }

    #region Game Loop

    public void RestartGame()
    {
        DeleteAllPets();
        FocusTerrarium(null);
        CurrentLevel = -1;

        NextLevel();
    }

    public void NextLevel()
    {
        DeleteAllPets();
        FocusTerrarium(null);

        CurrentLevel++;

        if (CurrentLevel >= _levels.Length)
            CurrentLevel = 0;

        var level = _levels[CurrentLevel];

        foreach (var pet in level.PetsInLevel)
            CreatePet(pet);

        State = GameState.Preparing;
        _chimesAudioPlayer.Play();
    }

    public void GameOver(Pet diedPet)
    {
        State = GameState.GameOver;
        Timer.Instance.TimerVisible = false;

        PetInformation.Instance.HidePet();
        GameOverDialog.Instance.Show(diedPet);
        FocusTerrarium(diedPet.Terrarium);
    }

    private void UpdateTimer(double delta)
    {
        RoundTime -= (float)delta;
        Timer.Instance.SetTimer(RoundTime);

        if (RoundTime > 0) return;

        State = GameState.Ending;
        Timer.Instance.TimerVisible = false;
        FocusTerrarium(null);
        EndOfDayDialog.Instance.Show(new EndOfDayResult
        {
            CurrentLevel = CurrentLevel,
            TotalLevels = _levels.Length,
            AverageHappiness = _pets.Average(p => p.Happiness)
        });
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
        _pets.Clear();
    }

    public void CreatePet(PetType pet, int terrariumIndex = -1)
    {
        var petNode = _petScene.Instantiate<Pet>();

        petNode.PetType = (PetType)pet.Duplicate();

        if (terrariumIndex > -1)
        {
            var terrarium = Terrariums[terrariumIndex];

            petNode.Terrarium = terrarium;

            terrarium.AddPet(petNode);
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
            pet.Terrarium.RemovePet(pet);
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

            pet.Terrarium = terrarium;
            pet.StopDeathTimer();

            terrarium.AddPet(pet);
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
    }

    public void FocusTerrarium(Terrarium? terrarium, bool resetHud = true)
    {
        if (terrarium == SelectedFocus && terrarium != null) return;

        SelectedFocus = terrarium;

        if (resetHud)
            PetInformation.Instance.HidePet();


        if (terrarium == null)
        {
            Player.Instance.Unfocus();
            TerrariumInformation.Instance.HideTerrarium();
        }
        else
        {
            Player.Instance.FocusAt(terrarium.FocusPoint.GlobalPosition);
            TerrariumInformation.Instance.ShowTerrarium(terrarium);
        }
    }

    #endregion
}