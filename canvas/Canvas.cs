using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Canvas : Node2D
{
    private PackedScene _pixelScene = GD.Load<PackedScene>("res://canvas/pixel.tscn");
    private List<Agent> agents = new();
    
    [Export] public Vector2I Dimensions { get; set; } = Vector2I.One * 50;
    [Export] public float DarkeningFactor { get; set; } = 0.0005f;
    [Export] public float SpreadFactor { get; set; } = 0.2f;
    [Export] public float AgentSpeed { get; set; } = 400f;

    public Pixel[,] Pixels { get; private set; }
    
    public Vector2 TileSize
    {
        get
        {
            Vector2 size = GetWindow().Size;
            return new Vector2(size.X / Dimensions.X, size.Y / Dimensions.Y);
        }
    }
    public override void _Ready()
    {
        Pixels = new Pixel[Dimensions.X, Dimensions.Y];
        Vector2 size = TileSize;

        for (int y = 0; y < Dimensions.Y; y++)
        {
            for (int x = 0; x < Dimensions.X; x++)
            {
                Pixel instance = _pixelScene.Instantiate<Pixel>();
                AddChild(instance);
                instance.Position = new Vector2(x * size.X, y * size.Y) + size / 2;
                instance.CollisionShape.Position = size;
                instance.CollisionShape.Shape = new RectangleShape2D()
                {
                    Size = size,
                };
                instance.QueueRedraw();
                Pixels[x, y] = instance;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            AddAgent();
        }
    }

    public void AddAgent()
    {
        Agent agent = new Agent();
        agents.Add(agent);
        
        Vector2 windowSize = GetWindow().Size;
        agent.Position = new(GD.Randf() * windowSize.X, GD.Randf() * windowSize.Y);
        float rotation = GD.Randf() * MathF.Tau;
        agent.Velocity = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * AgentSpeed;
        agent.Color = Color.FromHsv(GD.Randf(), 1, 1);
    }
    public override void _Process(double delta)
    {
        // GD.Print("fps: ", 1 / delta);
        foreach (Agent agent in agents)
        {
            Vector2 windowSize = GetWindow().Size;
            var position = agent.Position + agent.Velocity / 30f;
            if (position.X < 0)
            {
                position.X += windowSize.X;
                agent.Velocity = agent.Velocity.Rotated(GD.Randf());
            }

            if (position.X >= windowSize.X)
            {
                position.X -= windowSize.X;
                agent.Velocity = agent.Velocity.Rotated(GD.Randf());
            }

            if (position.Y < 0)
            {
                position.Y += windowSize.Y;
                agent.Velocity = agent.Velocity.Rotated(GD.Randf());
            }

            if (position.Y >= windowSize.Y)
            {
                position.Y -= windowSize.Y;
                agent.Velocity = agent.Velocity.Rotated(GD.Randf());
            }

            agent.Position = position;

            int x = (int)(position.X / windowSize.X * Dimensions.X);
            int y = (int)(position.Y / windowSize.Y * Dimensions.Y); 
            x = Mathf.Clamp(x,0, Dimensions.X - 1);
            y = Mathf.Clamp(y, 0, Dimensions.Y - 1);
            // GD.Print(position);
            // Pixels[x,y].NextColor = Pixels[x, y].NextColor.Blend(agent.Color);
            Color color = Pixels[x, y].NextColor;
            if (!color.IsEqualApprox(Colors.Black))
            {
                color.S = 1;
                color.V = 1;
                agent.Color = agent.Color.Blend(color);
            }

            Pixels[x, y].NextColor = agent.Color;
            // GD.Print(agent.Color, " | ", Pixels[x, y].NextColor);
        }
        // Parallel.For(0, Dimensions.Y, y =>
        for (int y = 0; y < Dimensions.Y; y++)
        {
            // Parallel.For(0, Dimensions.X, x =>
            for (int x = 0; x < Dimensions.X; x++)
            {
                Pixel instance = Pixels[x, y];
                if (x != 0)
                {
                    instance.NextColor = instance.NextColor.Lerp(Pixels[x - 1, y].Modulate, SpreadFactor);
                }
                if (y != 0)
                {
                    instance.NextColor = instance.NextColor.Lerp(Pixels[x, y - 1].Modulate, SpreadFactor);
                }
                if (x != Dimensions.X - 1)
                {
                    instance.NextColor = instance.NextColor.Lerp(Pixels[x + 1, y].Modulate, SpreadFactor);
                }
                if (y != Dimensions.Y - 1)
                {
                    instance.NextColor = instance.NextColor.Lerp(Pixels[x, y + 1].Modulate, SpreadFactor);
                }
                instance.NextColor = instance.NextColor.Darkened(DarkeningFactor);
            }
        }
        // Parallel.For(0, Dimensions.Y, y =>
        for (int y = 0; y < Dimensions.Y; y++)
        {
            // Parallel.For(0, Dimensions.X, x =>
            for (int x = 0; x < Dimensions.X; x++)
            {
                Pixel instance = Pixels[x, y];
                instance.Modulate = instance.NextColor;
            }
        }
    }
    
}
