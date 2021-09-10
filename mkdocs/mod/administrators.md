## New administrators

```CSharp
public class CustomAdministrator : BaseAdministrator
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

### Registration

```CSharp
Pathfinder.Administrator.AdministratorManager.RegisterAdministrator<CustomAdministrator>();
```

### Adding the administrator to a computer

```XML
<admin type="CustomAdministrator" />
```
