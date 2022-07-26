# Custom missions goals

## New mission goals

```CSharp
public class CustomMissionGoal : Pathfinder.Mission.PathfinderGoal
{
    [XMLStorage]
    public string Attribute;

    public OS os;

    public CustomMissionGoal()
    {
        os = OS.currentInstance;
    }

    public override bool isComplete(List<string> additionalDetails = null)
    {
        // return true if the mission should be completed
        // return false otherwise
    }
}
```

## Registration

Goals can be registered manually or with the Goal attribute.

```CSharp
Pathfinder.Goal.GoalManager.RegisterGoal<CustomMissionGoal>("CustomGoalName");
```

```CSharp
[Pathfinder.Meta.Load.Goal("CustomGoalName")]
public class CustomMissionGoal : PathfinderGoal
```

## Adding a custom goal to a mission file

```XML
<goal type="CustomGoalName" Attribute="attribute value" />
```