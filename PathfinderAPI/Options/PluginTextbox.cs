using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Options;

public class PluginTextbox : BasePluginOption<string>
{
    public int TextBoxWidth { get; set; } = 20;
    public int TextBoxLines { get; set; } = 1;
    public SpriteFont TextBoxFont { get; set; }

    public PluginTextbox(string headerText, string descriptionText = null, string defaultValue = null, string configDesc = null, string id = null)
        : base(headerText, descriptionText, defaultValue ?? "", configDesc, id)
    {
        TextBoxWidth = 20;
    }

    public override void LoadContent()
    {
        TextBoxFont ??= GuiData.smallfont;
        base.LoadContent();
    }

    public Vector2 TextBoxSize =>
        TextBoxWidth > 0
            ? TextBoxFont.MeasureString(new string('o', TextBoxWidth+1))
            : (TextBoxWidth == 0
                ? new Vector2(2, TextBoxFont.MeasureString("o").Y)
                : TextBoxFont.MeasureString(Value) + new Vector2(5, 0)
            );

    public override Vector2 MinSize
    {
        get
        {
            var textBoxSize = TextBoxSize;
            var headerSize = HeaderTextSize;
            var descSize = DescriptionTextSize;
            return new Vector2(
                Math.Max(headerSize.X, descSize.X + textBoxSize.X + 10),
                headerSize.Y + Math.Max(descSize.Y, textBoxSize.Y)// + (Math.Max(0, TextBoxLines - 1)) * 25)
            );
        }
    }

    public override void OnDraw(GameTime gameTime)
    {
        var textBoxSize = TextBoxSize;
        DrawString(Position, HeaderText, HeaderColor, HeaderFont);
        // Textbox doesn't support cliping the value to the width and doesn't take into account held down keys or special cases, like ctrl+backspace
        Value = TextBox.doTextBox(
            HacknetGuiId,
            (int)Position.X,
            (int)Position.Y + 34,
            (int)textBoxSize.X,
            TextBoxLines,
            Value,
            TextBoxFont
        );

        DrawString(Position + new Vector2(textBoxSize.X + 10, 32), DescriptionText, DescriptionColor, DescriptionFont);
    }
}