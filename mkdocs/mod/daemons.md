# Creating custom daemons

## Base daemon

```CSharp
public class CustomDaemon : Pathfinder.Daemon.BaseDaemon
{
    public CustomDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

    public override string Identifier => "Custom Daemon";

    [XMLStorage]
    public string DisplayString;

    public override void draw(Rectangle bounds, SpriteBatch sb)
    {
        base.draw(bounds, sb);

        var center = os.display.bounds.Center;
        Hacknet.Gui.TextItem.doLabel(new Vector2(center.X, center.Y), DisplayString, Color.Aquamarine);
    }
}
```

## Registration

```CSharp
Pathfinder.Daemon.DaemonManager.RegisterDaemon<CustomDaemon>();
```

## Adding a custom daemon in a computer

```XML
<CustomDaemon />
```
