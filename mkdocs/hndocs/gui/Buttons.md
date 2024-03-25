# Buttons
Buttons are essentially just boxes that return a `boolean` value relating to whether or not it has been pressed.

```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

Button.doButton(int id, int x, int y, int width, int height, string text, Color? color);
```
`Button.doButton` will create a simple button (similar to the rest of the UI) that will return a `bool` value of whether or not it has been clicked. Like every other UI element, this should be put in a `draw` function.

Each button must have a unique `id`. If multiple buttons share the same ID, then clicking on one of them will act as clicking on all of them. You can simplify this with Pathfinder via `PFButton` (`Pathfinder.GUI`) by declaring the button ID in a class field:

```csharp
using Hacknet;
using Hacknet.Gui;

using Pathfinder.GUI;

using Microsoft.Xna.Framework;

public class SomeClass {
    public int buttonID = PFButton.GetNextID();

    public override void draw(float t) {
        Button.doButton(buttonID, bounds.X + 25, bounds.Y + 25, 100, 100, "Exit", Color.Red)
    }
}
```

`color` determines the color of the tiny rectangle next to the text in buttons, not the button itself.

An example of how a button might be used is in an executable to exit the executable. For example:
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

bool exitButton = Button.doButton(183738, bounds.X + 25, bounds.Y + 25, 100, 100, "Exit", Color.Red);

if(exitButton) {
    this.isExiting = true;
    return;
}
```
You can obviously compact this code to make it look nicer as;
```csharp
this.isExiting = Button.doButton(183738, bounds.X + 25, bounds.Y + 25, 100, 100, "Exit", Color.Red);
```
...but the above example works for those still fairly new to C#.

# Unused Buttons
These buttons are largely unused in the modding scene, but you may still find use in them:
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

Button.doHoldDownButton(int id, int x, int y, int width, int height, string text, bool hasOutline, Color? outlineColor, Color? selectedColor);
```