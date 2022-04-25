using Microsoft.Xna.Framework;

namespace Pathfinder.GUI;

public struct DrawData
{
    public Rectangle _rectangle;
    public Rectangle Rectangle { get => _rectangle; set => _rectangle = value; }
    public int X { get => _rectangle.X; set => _rectangle.X = value; }
    public int Y { get => _rectangle.Y; set => _rectangle.Y = value; }
    public int Width { get => _rectangle.Width; set => _rectangle.Width = value; }
    public int Height { get => _rectangle.Height; set => _rectangle.Height = value; }
    public Point Position
    {
        get => new Point(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }
    public Point Size
    {
        get => new Point(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }
    public DrawData Add(int x, int? y = null, int width = 0, int? height = null)
    {
        if(!y.HasValue) y = x;
        if(!height.HasValue) height = width;
        X += x;
        Y += y.Value;
        Width += width;
        Height = height.Value;
        return this;
    }
    public DrawData Set(int? x = null, int? y = null, int? width = null, int? height = null)
    {
        if(x.HasValue)
            X = x.Value;
        if(y.HasValue)
            Y = y.Value;
        if(width.HasValue)
            Width = width.Value;
        if(height.HasValue)
            Height = height.Value;
        return this;
    }
    public static implicit operator Vector2(DrawData d) => new Vector2(d.X, d.Y);
    public static implicit operator Point(DrawData d) => d.Position;
    public static implicit operator Rectangle(DrawData d) => d.Rectangle;
}