# Custom conditions

## New conditions

```CSharp
public class CustomCondition : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string Attribute;

    public override bool Check(object os_obj)
    {
        OS os = (OS)os_obj;

        // return true if actions inside condition should be triggered
        // return false otherwise
    }
}
```

## Registration

Conditions can be registered manually or with the Condition attribute.

```CSharp
Pathfinder.Action.ConditionManager.RegisterCondition<CustomCondition>("CustomConditionXMLTag");
```

```CSharp
[Pathfinder.Meta.Load.Condition]
public class CustomCondition : PathfinderCondition
```

## Adding a custom condition to an actions file

```XML
<CustomConditionXMLTag Attribue="value">
    <!-- Actions go here! -->
</CustomConditionXMLTag>
```
