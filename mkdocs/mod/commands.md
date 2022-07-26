# Creating custom commands

## New command

```CSharp
public static void TestCommand(OS os, string[] args)
{
    os.write("Arguments passed in: " + string.Join(" ", args));
}
```

## Registration

Commands can be registered manually or with the Command attribute.

```CSharp
Pathfinder.Command.CommandManager.RegisterCommand("CommandName", TestCommand);
```

```CSharp
[Pathfinder.Meta.Load.Command("CommandName")]
public static void TestCommand(OS os, string[] args)
```

Typing `CommandName` in the in-game terminal will now print "Arguments passed in...".
