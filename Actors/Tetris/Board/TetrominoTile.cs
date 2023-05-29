using Godot;
using System;

public partial class TetrominoTile : Node2D
{
    private static string ResTetrominoTile = "res://Assets/Tetris/Board/tetromino_block.png";


    public int Index;
    public Vector2 PositionOnBoardGrid;


    public override void _Ready()
    {
        // Create Tetromino tile sprite
        Sprite2D sprite = new Sprite2D();
        sprite.Name = "Sprite2D";
        sprite.Texture = ResourceLoader.Load(ResTetrominoTile) as Texture2D;
        sprite.TextureFilter = TextureFilterEnum.Linear;
        sprite.Centered = false;
        AddChild(sprite);

        // DEBUG: Draw tile index
        ColorRect colorRect = new ColorRect();
        colorRect.Name = "ColorRect";
        colorRect.SetSize(new Vector2(10, 10));
        colorRect.Color = Color.Color8(0, 0, 0, 170);
        AddChild(colorRect);

        Label label = new Label();
        label.Name = "Index";
        label.TextureFilter = TextureFilterEnum.Linear;
        label.Scale /= new Vector2(2, 2);
        label.Set("custom_colors/font_color", Colors.White);
        AddChild(label);
    }


    public void Init(Color color, Vector2I posOffset, int index)
    {
        GetNode<Sprite2D>("Sprite2D").SelfModulate = color;
        Position += posOffset;

        Index = index;

        // DEBUG: Set index text
        GetNode<Label>("Index").Text = Index.ToString();
    }


    public bool CanMoveTo(Vector2 position)
    {
        return Board.instance.IsValidPos(position);
    }

    public void Move(Vector2 direction)
    {
        Position += direction * Board.instance.TileSize;
        PositionOnBoardGrid = Position / Board.instance.TileSize;
    }

    public void Rotate(Vector2 origin, bool clockwise)
    {
        // TO IMPLEMENT
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
