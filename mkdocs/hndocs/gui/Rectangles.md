# Rectangles
Basic rectangles. These can be any solid color.

```csharp
using Hacknet;
using Hacknet.Gui;

// ...

RenderedRectangle.doRectangle(int x, int y, int width, int height, Color color = Color.White, bool blocking = false);
```
Creates a simple rectangle to be drawn on the screen. `color` determines what the fill color of the rectangle will be. `blocking` determines whether or not the rectangle should block input, such as mouse clicks.

An example rectangle would be:
```csharp
using Hacknet;
using Hacknet.Gui;

// ...

RenderedRectangle.doRectangle(0, 0, 500, 250, Color.Orange);
```
This will create an orange rectangle at the top left of the screen that is `500 pixels` wide and `250 pixels` tall.

# Rectangle Outlines
Same usage as above, but specifically for outlines.
```csharp
using Hacknet;
using Hacknet.Gui;

// ...

RenderedRectangle.doRectangleOutline(int x, int y, int width, int height, int thickness, Color color = Color.White);
```
`thickness` determines how thick the outline should be, in pixels.