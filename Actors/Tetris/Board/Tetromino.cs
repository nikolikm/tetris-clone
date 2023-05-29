using Godot;
using System;
using System.Linq;

public enum TetrominoType {I, O, J, L, S, Z, T}


/// <summary>
/// Controls TetrominoTiles and their arrangement.
/// </summary>
public partial class Tetromino : Node2D
{
    public TetrominoType Type;
    public TetrominoTile[] Tiles;

    
    private bool DebugIndexOn;


    [Signal]
    public delegate void TetrominoLockedEventHandler();

    public void Create(TetrominoType type)
    {
        // Initialize each Tetromino tile
        Type = type;
        Color color;
        Vector2I startPosition = new Vector2I((int)(Board.instance.Size.X/2.0f), 0) * Board.instance.TileSize;
        Vector2I[] tilePositions = { Vector2I.Zero, Board.instance.TileSize, 
                                    Board.instance.TileSize, Board.instance.TileSize };            


        // Choose the right color and position offsets for the current Tetromino type
        switch (type)
        {
            case TetrominoType.I:
                tilePositions[1] *= Vector2I.Left;
                tilePositions[2] *= Vector2I.Right;
                tilePositions[3] *= Vector2I.Right * 2;
                color = new Color("#67f8ff");
                break;

            case TetrominoType.O:
                tilePositions[1] *= Vector2I.Right;
                tilePositions[2] *= Vector2I.Right + Vector2I.Up;
                tilePositions[3] *= Vector2I.Up;
                startPosition += Vector2I.Down * Board.instance.TileSize;
                color = new Color("#ffd853");
                break;

            case TetrominoType.J:
                tilePositions[1] *= Vector2I.Left;
                tilePositions[2] *= Vector2I.One;
                tilePositions[3] *= Vector2I.Right;
                startPosition += Vector2I.Right * Board.instance.TileSize;
                color = new Color("#2267f9");
                break;

            case TetrominoType.L:
                tilePositions[1] *= Vector2I.Left;
                tilePositions[2] *= new Vector2I(-1, 1);
                tilePositions[3] *= Vector2I.Right;
                startPosition += Vector2I.Right * Board.instance.TileSize;
                color = new Color("#f49b1e");
                break;

            case TetrominoType.S:
                tilePositions[1] *= Vector2I.Down;
                tilePositions[2] *= new Vector2I(-1, 1);
                tilePositions[3] *= Vector2I.Right;
                startPosition += Vector2I.Right * Board.instance.TileSize;
                color = new Color("#2fd11d");
                break;

            case TetrominoType.Z:
                tilePositions[1] *= Vector2I.Left;
                tilePositions[2] *= Vector2I.One;
                tilePositions[3] *= Vector2I.Down;
                startPosition += Vector2I.Right * Board.instance.TileSize;
                color = new Color("#d72e2e");
                break;

            case TetrominoType.T:
                tilePositions[1] *= Vector2I.Left;
                tilePositions[2] *= Vector2I.Up;
                tilePositions[3] *= Vector2I.Right;
                startPosition += (Vector2I.Right + Vector2I.Down) * Board.instance.TileSize;
                color = new Color("#ab38ec");
                break;

            default: // Debug cube
                tilePositions[1] = Vector2I.Zero;
                tilePositions[2] = Vector2I.Zero;
                tilePositions[3] = Vector2I.Zero;
                color = new Color("#000000");
                break;
        }


        // Initialize all tiles with their indexes, positions, and color
        Tiles = new TetrominoTile[4];

        for (int i = 0; i < Tiles.Length; i++)
        {
            Tiles[i] = new TetrominoTile();
            Tiles[i].Name = "Tile" + i;
            AddChild(Tiles[i]);

            Tiles[i].Init(color, startPosition + tilePositions[i], i);
        }


        // Initialize debug vars
        DebugIndexOn = true;
    }


    public void Rotate(bool clockwise, bool checkForOffset)
    {
        
    }

    public bool Move(Vector2I direction)
    {  
        foreach (TetrominoTile Tile in Tiles)
        {
            // Check if moving the tile would result in it colliding with the board frame
            if (!Tile.CanMoveTo(direction + ((Position + Tile.Position) / Board.instance.TileSize)))
            {
                // If any tile would hit the floor, the Tetromino needs to be locked in place
                if (direction == Vector2I.Down)
                    Lock();

                return false;
            }
        }

        // If the Tetromino is allowed to move, move it
        foreach (TetrominoTile Tile in Tiles)
        {
            Tile.Move(direction);
        }

        return true;
    }

    public void Lock()
    {
        foreach (TetrominoTile Tile in Tiles)
        {
            // Get the internal position of the tile
            Vector2 internalPosition = Tile.PositionOnBoardGrid;

            // Mark this position as filled
            Board.instance.GameGrid[(int)internalPosition.X, (int)internalPosition.Y].free = false;

            // "Snap" the tile to the board
            Sprite2D tileToLock = Tile.GetNode<Sprite2D>("Sprite2D");
            tileToLock.Position = (internalPosition * Board.instance.TileSize) + Board.instance.TileSize;

            Tile.RemoveChild(tileToLock);
            Board.instance.AddChild(tileToLock);

            // Destroy the active tile
            Tile.QueueFree();
        }

        // Tell the Tetromino spawner that this Tetromino has been locked
        EmitSignal(SignalName.TetrominoLocked);
    }


    public override void _Process(double delta)
    {
        // DEBUG: Show & Hide tile indexes
        if (Input.IsActionJustPressed("debug1"))
        {
            DebugIndexOn = !DebugIndexOn;

            foreach (TetrominoTile x in Tiles)
            {
                x.GetNode<ColorRect>("ColorRect").Visible = DebugIndexOn;
                x.GetNode<Label>("Index").Visible = DebugIndexOn;
            }
        }

        base._Process(delta);
    }
}
