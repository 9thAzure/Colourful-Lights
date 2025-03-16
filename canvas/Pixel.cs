using Godot;
using System;

public partial class Pixel : Node2D
{
    public CollisionShape2D CollisionShape { get; private set; } = null!;

    public Color NextColor { get; set; } = Colors.Black;

    public override void _Ready()
    {
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    public override void _Draw()
    {
        CollisionShape.Shape.Draw(GetCanvasItem(), Colors.White);
    }
}
