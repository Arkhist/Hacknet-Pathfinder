using Microsoft.Xna.Framework;

namespace Pathfinder.GUI;

public struct PluginOptionDrawData
{
    public int? X;
    public int? Y;
    public int? Width;
    public int? Height;
    public Rectangle Rectangle
    {
        get => new Rectangle(X.GetValueOrDefault(), Y.GetValueOrDefault(), Width.GetValueOrDefault(), Height.GetValueOrDefault());
        set
        {
            X = value.X;
            Y = value.Y;
            Width = value.Width;
            Height = value.Height;
        }
    }
    public Vector2 Vector2
    {
        get => new Vector2(X.GetValueOrDefault(), Y.GetValueOrDefault());
        set
        {
            X = (int)value.X;
            Y = (int)value.Y;
        }
    }
    public Point Position
    {
        get => new Point(X.GetValueOrDefault(), Y.GetValueOrDefault());
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }
    public Point Size
    {
        get => new Point(Width.GetValueOrDefault(), Height.GetValueOrDefault());
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public PluginOptionDrawData QuickAdd(int x = 0, int y = 0, int width = 0, int height = 0)
    {
        X = x + X.GetValueOrDefault();
        Y = y + Y.GetValueOrDefault();
        Width = width + Width.GetValueOrDefault();
        Height = height + Height.GetValueOrDefault();
        return this;
    }

    public PluginOptionDrawData Add(int? x, int? y = null, int? width = null, int? height = null, bool xycombo = false, bool whcombo = false)
    {
        if(xycombo && (x == null || y == null) && x != y)
        {
            if(x.HasValue) y = x;
            else x = y;
        }
        if(whcombo && (width == null || height == null) && width != height)
        {
            if(width.HasValue) height = width;
            else width = height;
        }

        if(x != null)
        {
            if(X == null) X = 0;
            X += x;
        }
        if(y != null)
        {
            if(Y == null) Y = 0;
            Y += y.Value;
        }
        if(width != null)
        {
            if(Width == null) Width = 0;
            Width += width;
        }
        if(height != null)
        {
            if(Height == null) Height = 0;
            Height = height.Value;
        }
        return this;
    }
    public PluginOptionDrawData Set(int? x = null, int? y = null, int? width = null, int? height = null)
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
    public static implicit operator Vector2(PluginOptionDrawData d) => d.Vector2;
    public static implicit operator Point(PluginOptionDrawData d) => d.Position;
    public static implicit operator Rectangle(PluginOptionDrawData d) => d.Rectangle;
}