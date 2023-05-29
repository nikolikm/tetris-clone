using Godot;
using Godot.Collections;


public enum GameState {RUNNING, PAUSED, GAMEOVER}


/// <summary>
/// A class that represents a single tile on the internal grid.
/// <para/> Contains a reference to a locked TetrominoTile.
/// </summary>
public partial class GameTile
{
    /// <summary>A reference to a locked tile in this game tile's location. 
    /// <para/> Null if the game tile is empty.</summary>
    public Sprite2D lockedTile;

    /// <summary>Whether this tile on the grid is free or not.</summary>
    public bool free;

    public GameTile()
    {
        lockedTile = null;
        free = true;
    }
}


/// <summary>
/// The main game board.
/// <para>This class draws the board graphics, as well as keeps track of the gameplay grid
/// and other gameplay components.</para>
/// </summary>
public partial class Board : Node2D
{    
    public static Board instance;
    
    private static string ResBoardBackground = "res://Assets/Tetris/Board/board_tileset.tres";


    [Export(PropertyHint.Range, "4,20")]
    public Vector2I Size = new Vector2I(10, 20);

    [Export]
    public float DropSpeed = 2.0f;

    [Export]
    public Vector2I TileSize = new Vector2I(16, 16);


    public GameTile[,] GameGrid;
    public GameState State;


    public override void _EnterTree()
    {
        instance = this;
    }

    
    public override void _Ready()
    {
        // Set up the board background
        TileMap tileMap = new TileMap();
        tileMap.Name = "BackgroundTilemap";
        tileMap.TileSet = ResourceLoader.Load(ResBoardBackground) as TileSet;

        // Paint the tiled board background
        Array<Vector2I> cellsToPaint = new Array<Vector2I>();
        for (int x = 0; x < Size.X + 2; x++)
        {
            for (int y = 0; y < Size.Y + 2; y++)
            {
                cellsToPaint.Add(new Vector2I(x, y));         
            }
        }

        tileMap.SetCellsTerrainConnect(0, cellsToPaint, 0, 0);
        AddChild(tileMap);

         // Initialize the gameplay grid & add blank gameplay tiles to it
        GameGrid = new GameTile[(int)Size.X, (int)Size.Y];
        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                GameGrid[x, y] = new GameTile();
            }
        }

        // Iniitalize the Tetromino Spawner
        TetrominoSpawner Spawner = new TetrominoSpawner();
        Spawner.Name = "TetrominoSpawner";
        AddChild(Spawner);

        State = GameState.RUNNING;
    }

    public bool IsValidPos(Vector2 position)
    {
        // Check if position coordinate is in bounds
        if (position.X < 0 || position.X >= Size.X || position.Y < 0 || position.Y >= Size.Y)
        {
            return false;
        }

        // Check if the position isn't already occupied by a tile
        if (!GameGrid[(int)position.X, (int)position.Y].free)
        {
            return false;
        }

        return true;
    }

    
}
