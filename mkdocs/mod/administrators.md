# Custom administrators

## New administrators

```CSharp
public class CustomAdministrator : Pathfinder.Administrator.BaseAdministrator
{
    public CustomAdministrator(Computer computer, OS opSystem) : base(computer, opSystem)
    {
    }
    public override void disconnectionDetected(Computer c, OS os)
    {
    }

    public override void traceEjectionDetected(Computer c, OS os)
    {
    }
}
```

## Registration

Administrator can be registered manually or with the Administrator attribute.

```CSharp
Pathfinder.Administrator.AdministratorManager.RegisterAdministrator<CustomAdministrator>();
```

```CSharp
[Pathfinder.Meta.Load.Administrator]
public class CustomAdministrator : BaseAdministrator
```

## Adding a custom administrator to a computer

```XML
<admin type="CustomAdministrator" />
```
