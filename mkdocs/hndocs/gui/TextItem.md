# TextItem
`TextItem` is the main way to make "labels" in Hacknet, which allow you to show text. This documentation will show how to make a simple label, and then list other ways.
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

TextItem.doLabel(Vector2 position, string text, Color? color);
```
This will create a simple label that uses the default font to display text.
```csharp
using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;

// ...

Vector2 pos = new Vector2(100, 100);
TextItem.doLabel(pos, "Some text", Color.White);
```
Hacknet also has built-in functions for creating labels with small font and tiny font:
```csharp
TextItem.doSmallLabel(Vector2 position, string text, Color? color);
TextItem.doTinyLabel(Vector2 position, string text, Color? color);
```

For more advanced text methods, please refer to the [Advanced Text Guide](../../guides/text/AdvancedText.md).