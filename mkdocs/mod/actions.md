# Custom actions

## Base action

```CSharp
public class CustomAction : Pathfinder.Action.PathfinderAction
{
    [XMLStorage]
    public string Attribute1;

    [XMLStorage]
    public string Attribute2 = "Default value!";

    public override void Trigger (object os_obj)
    {
        OS os = (OS)os_obj;
        os.write("Custom action triggered!");
        os.write(Attribute1);
        os.write(Attribute2);
    }
}
```

### Delayable action

DelayablePathfinderAction is a subtype of PathfinderAction that implements the DelayHost and Delay attributes.

```CSharp
public class CustomDelayableAction : Pathfinder.Action.DelayablePathfinderAction
{
    public override void Trigger (OS os)
    {
        if (DelayHost != null)
        {
            os.write("Custom action triggered after " + Delay + " seconds with host " + DelayHost + "!");
        }
    }
}
```

## Registration

Actions can be registered manually or with the Action attribute.

```CSharp
Pathfinder.Action.ActionManager.RegisterAction<CustomAction>("CustomActionXMLTag");
```

```CSharp
[Pathfinder.Meta.Load.Action("CustomActionXMLTag")]
public class CustomAction : PathfinderAction
```

## Adding a custom action to an actions file

```XML
<CustomActionXMLTag Attribute1="Value" />
```
