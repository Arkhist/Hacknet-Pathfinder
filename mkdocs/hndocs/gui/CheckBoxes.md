# CheckBoxes
Checkboxes in Hacknet work very similarly to [Buttons](./Buttons.md) in the sense that they return a `bool` value relating to whether or not the box has been clicked.
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

CheckBox.doCheckBox(int id, int x, int y, bool isChecked, Color? selectedColor);
```
Since the checkboxes themselves do not track whether or not they've been checked, it would be up to you to determine this. You can do this with, say, a class field.
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

public class SomeClass {
    public bool checkboxIsChecked = false;

    public override void draw(float t) {
        bool checkbox = CheckBox.doCheckBox(182939, 100, 100, checkboxIsChecked);

        if(checkbox) { checkboxIsChecked = !checkboxIsChecked; }
    }
}
```
You can also add text to checkboxes:
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

CheckBox.doCheckBox(182939, 100, 100, false, Color.White, "Some Text Here");
```