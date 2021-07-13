# Creating custom commands

## New command

```CSharp
public static void TestCommand(OS os, string[] args)
{
    os.write("Arguments passed in: " + string.Join(" ", args));
}
```

## Registration

```CSharp
Pathfinder.Command.CommandManager.RegisterCommand("CommandName", TestCommand);
```

Typing `CommandName` in the in-game terminal will now print "Arguments passed in...".
