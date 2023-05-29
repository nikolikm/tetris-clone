using Godot;
using System;


public enum SpawnerState {INACTIVE, ACTIVE}


/// <summary>
/// Spawns Tetromino objects and controls their state, but also handles player movement.
/// </summary>
public partial class TetrominoSpawner : Node2D
{
    public static TetrominoSpawner instance;

    // Offset data for all Tetromino types
    public static Vector2[,] OFFSETS_JLSZT = new Vector2[,]{
            {Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero},
            {Vector2.Zero, new Vector2(1,0), Vector2.Zero, new Vector2(-1,0)},
            {Vector2.Zero, new Vector2(1,-1), Vector2.Zero, new Vector2(-1,-1)},
            {Vector2.Zero, new Vector2(0,2), Vector2.Zero, new Vector2(0,2)},
            {Vector2.Zero, new Vector2(1,2), Vector2.Zero, new Vector2(-1,2)}
        };

    public static Vector2[,] OFFSETS_I = new Vector2[,]{
            {Vector2.Zero, new Vector2(-1,0), new Vector2(-1,1), new Vector2(0,1)},
            {new Vector2(-1,0), Vector2.Zero, new Vector2(1,1), new Vector2(0,1)},
            {new Vector2(2,0), Vector2.Zero, new Vector2(-2, 1), new Vector2(0, 1)},
            {new Vector2(-1,0), new Vector2(0,1), new Vector2(1,0), new Vector2(0,-1)},
            {new Vector2(2,0), new Vector2(0,-2), new Vector2(-2,0), new Vector2(0, 2)}
        };

    public static Vector2[,] OFFSETS_O = new Vector2[,]{
            {Vector2.Zero, Vector2.Down, new Vector2(-1,-1), Vector2.Left}
        };


    private float DropSpeedModifier = -0.0f;
    private string TetrominoSet;
    private Tetromino CurrentTetromino;
    private SpawnerState State;


    public override void _EnterTree()
    {
        instance = this;
    }


    public override void _Ready()
    {
        // Offset so that the origin for Tetrominos starts inside the board frame
        Position += Board.instance.TileSize;

        // Create & initialize timer(s) //

        // Drop timer
        Timer dropTimer = new Timer();
        dropTimer.Name = "DropTimer";
        dropTimer.WaitTime = Board.instance.DropSpeed;
        dropTimer.Timeout += OnDropTimerTimeout;
        dropTimer.Stop();
        AddChild(dropTimer);

        // Input timer
        Timer inputTimer = new Timer();
        inputTimer.Name = "InputTimer";
        inputTimer.WaitTime = 0.07f;
        inputTimer.OneShot = true;
        AddChild(inputTimer);


        State = SpawnerState.INACTIVE;
        TetrominoSet = "";


        Generate();
    }


    public override void _Process(double delta)
    {
        bool inputAllowed = GetNode<Timer>("InputTimer").IsStopped();

        // Spawn new Tetromino if none is being played
        if (State == SpawnerState.INACTIVE)
        {
            Generate();
        }

        // Input Handlers
        if (inputAllowed)
        {
            // Instantly drop Tetromino
            if (Input.IsActionJustPressed("tTetrisDrop"))
            {
                //CurrTetromino.Drop();
            }
            
            // Drop the Tetromino by one tile & reset the autodrop timer
            if (Input.IsActionPressed("tTetrisDown"))
            {
                OnDropTimerTimeout();
                GetNode<Timer>("InputTimer").Start();
            }

            // Move Tetromino left & right
            if (Input.IsActionPressed("tTetrisLeft"))
            {
                Move(Vector2I.Left);
                GetNode<Timer>("InputTimer").Start();
            }
            if (Input.IsActionPressed("tTetrisRight"))
            {
                Move(Vector2I.Right);
                GetNode<Timer>("InputTimer").Start();
            }

            // Rotate Tetromino counterclockwise
            if (Input.IsActionPressed("tTetrisRotateL"))
            {
                CurrentTetromino.Rotate(false, true);
                GetNode<Timer>("InputTimer").Start();
            }

            // Rotate Tetromino clockwise
            if (Input.IsActionPressed("tTetrisRotateR"))
            {
                CurrentTetromino.Rotate(true, true);
                GetNode<Timer>("InputTimer").Start();
            }
        }
    }

    /// <summary>
    /// Generates a new Tetromino (and re-shuffles the set if empty) and adds it as a child.
    /// </summary>
    public void Generate()
    {
        // Re-shuffle set of Tetrominos if depleted
        if (String.IsNullOrEmpty(TetrominoSet))
        {
            for (string s = "0123456"; !String.IsNullOrEmpty(s);)
            {
                int t = (int)GD.RandRange(0, s.Length - 1);
                TetrominoSet += s[t];
                s = s.Remove(t, 1);
            }
        }

        // Create a new Tetromino
        CurrentTetromino = new Tetromino();
        CurrentTetromino.Name = "Tetromino";
        CurrentTetromino.TetrominoLocked += OnTetrominoLocked;
        AddChild(CurrentTetromino);

        CurrentTetromino.Create((TetrominoType)Char.GetNumericValue(TetrominoSet[0]));
        TetrominoSet = TetrominoSet.Remove(0, 1);

        // Start the drop timer and mark the spawner as active
        GetNode<Timer>("DropTimer").Start();
        State = SpawnerState.ACTIVE;
    }


    public void Move(Vector2I direction)
    {
        if (CurrentTetromino != null)
        {
            CurrentTetromino.Move(direction);
        }
    }


    public void Destroy()
    {
        CurrentTetromino.QueueFree();
        State = SpawnerState.INACTIVE;
    }


    public void OnDropTimerTimeout()
    {
        Move(Vector2I.Down);
        GetNode<Timer>("DropTimer").Start(Board.instance.DropSpeed + DropSpeedModifier);
    }

    public void OnTetrominoLocked()
    {
        CurrentTetromino.QueueFree();
        CurrentTetromino = null;
        State = SpawnerState.INACTIVE;
    }
}
