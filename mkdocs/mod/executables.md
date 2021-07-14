# Creating custom executables

## The base executable

```CSharp
public class BasicExecutable : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "ExecName";

    public BasicExecutable(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args) {
         this.ramCost = 761;
    }

    public override void LoadContent()
    {
        base.LoadContent();
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
    }

    private float lifetime = 0f;
    public override void Update(float t)
    {
        base.Update(t);
        lifetime += t;
    }
}
```

## Registration

```CSharp
Pathfinder.Executable.ExecutableManager.RegisterExecutable<BasicExecutable>("#PF_BASIC_EXE#");
```

The provided argument corresponds to the replacement tag for extension files.

## Executable features

### Exiting

Setting `isExiting` to `true` will close the executable slowly.

Setting `needsRemoval` to `true` will close the executable instantly.

Example:

```CSharp
private float lifetime = 0f;
public override void Update(float t)
{
    base.Update(t);
    lifetime += t;
    if (lifetime > 2.5f)
        isExiting = true;
}
```

### Opening a port, starting a trace

The executable has access to the OS object `os` (interface with the game), `thisComputer` (the player's computer) and `targetIP`, the IP the player was connected to on executable launch.

```CSharp
Programs.getComputer(os, targetIP).openPort(PORT_NUMBER_HERE, os.thisComputer.ip);
```

To start a trace, if the computer has security:

```CSharp
Programs.getComputer(os, targetIP).hostileActionTaken();
```

### Drawing in the executable's area

The `Hacknet.Gui` namespace contains most of the drawing functions.

```CSharp
public override void Draw(float t)
{
    base.Draw(t);
    drawTarget();
    drawOutline();
    Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X, Bounds.Center.Y), "blue text", new Color(255, 0, 0));
}
```
