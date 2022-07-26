# Ports

## Extension development

Pathfinder includes a new syntax for registering ports on a computer, which looks like this:
```XML
<PFPorts replace="true">ssh ftp:50 random_protocol:78:display_name_underscores_are_spaces</PFPorts>
```
Using `replace="true"` makes it so that the Pathfinder ports list will replace the vanilla ports list when read, otherwise it will append to the port list. Putting the vanilla ports element after PFPorts will always append, no matter what is set here.

In order, the parameters are:

- The protocol name, all lowercase as a convention
- The port number if you don't want the default
- The display name of the port when shown to the user, eg. `FTP Server` is the default for `ftp`

### Protocol list

Here's a list of all the protocol names and their default ports included with Pathfinder:

- ssh 22
- ftp 21
- smtp 25
- web 80
- sql 1433
- medical 104
- torrent 6881
- ssl 443
- pacific 192
- rtsp 554
- transfer 211
- version 9418
- blizzard 3724
- eos 3659
- sigscramble 32

## Plugin development

### Adding new port defaults
You can add a new default with `Pathfinder.Port.PortManager.RegisterPort(string protocol, string displayName, int defaultPort)`. The last parameter is optional, and if you omit it you will always have to specify a port number in XML.

Ports can also be added by using the Port attribute on a field or property.

```CSharp
[Pathfinder.Meta.Load.Port]
public Pathfinder.Port.PortRecord customPort = new Pathfinder.Port.PortRecord("protocol", "displayName", 50);
```

### Reading port data

Ports are handled differently in Pathfinder than in the base game, which means you should try to ignore most of what you might see while decompiling.

**DO NOT USE `Computer.ports`, `Computer.portsOpen`, or `Computer.portRemapping`! None of these are populated any more, and will not give you any information!**

If you want to interface with ports, please add `using Pathfinder.Port;` to the top of your `.cs` files, and use the extension methods like so:
```CSharp
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Port;

[BepInPlugin("PortsExampleGuid", "plugin name", "0.0.1")]
public class SomePlugin {
    public override bool Load() {
        Pathfinder.Command.CommandManager.RegisterCommand("openallifssh", (os, args) => {
            if (os.connectedComp?.isPortOpen("ssh") ?? false) {
                foreach (var port in os.connectedComp.GetAllPorts()) {
                    os.connectedComp.openPort(port.Protocol, os.thisComputer.ip);
                }
            }
        });
    }
}
```
This command would open all ports on the connected computer if there's a port open that uses the  `ssh` protocol.

It's preferred to *always* use the protocol names and not the port numbers, and it is required for some methods except for the base game ports.
