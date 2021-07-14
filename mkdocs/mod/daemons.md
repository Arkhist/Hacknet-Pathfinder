# Creating custom daemons

## Base daemon

```CSharp
public class CustomDaemon : Pathfinder.Daemon.BaseDaemon
{
    public CustomDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

    public override string Identifier => "Custom Daemon";

    [XMLStorage]
    public string DisplayString = "Default text";

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

## XML Storage

XMLStorage variables are __string__ variables that can be specified in the Daemon's XML.

In the code:

```CSharp
[XMLStorage]
public string DisplayString;
```

## Adding a custom daemon in a computer

```XML
<CustomDaemon DisplayString="XML Edited test" />
```
