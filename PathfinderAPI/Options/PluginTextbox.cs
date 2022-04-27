using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Options;

public class PluginTextbox : BasePluginOption<string>
{
    private int _textBoxWidthInPixels;
    private int _textBoxWidth;
    public int TextBoxWidth
    {
        get => _textBoxWidth;
        set
        {
            _textBoxWidth = value;
            if(TextBoxFont != null)
                _textBoxWidthInPixels = ((int)TextBoxFont.MeasureString("o").X) * (_textBoxWidth - 1);
        }
    }
    public int TextBoxLines { get; set; } = 1;
    private SpriteFont _textBoxFont;
    public SpriteFont TextBoxFont
    {
        get => _textBoxFont;
        set
        {
            _textBoxFont = value;
            _textBoxWidthInPixels = ((int)TextBoxFont.MeasureString("o").X) * (_textBoxWidth - 1);
        }
    }

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

    public override void SetSize()
    {
        var characterSize = TextBoxFont.MeasureString("O");
        var headerSize = HeaderTextSize;
        var descSize = DescriptionTextSize;
        if(DrawData.Width == null)
            DrawData = DrawData.Set(width: (int)(Math.Max(headerSize.X, descSize.X + _textBoxWidthInPixels + 10)));
        if(DrawData.Height == null)
            DrawData = DrawData.Set(height: (int)(headerSize.Y + Math.Max(descSize.Y, (TextBoxLines * characterSize.Y) + (Math.Max(0, TextBoxLines - 1)) * 25)));
    }

    public override void OnDraw(GameTime gameTime)
    {
        DrawString(DrawData, HeaderText, HeaderColor, HeaderFont);
        // Textbox doesn't support cliping the value to the width and doesn't take into account held down keys or special cases, like ctrl+backspace
        Value = TextBox.doTextBox(
            HacknetGuiId,
            DrawData.X.Value,
            DrawData.Y.Value + 34,
            _textBoxWidthInPixels,
            TextBoxLines,
            Value,
            TextBoxFont
        );

        DrawString(DrawData.QuickAdd(_textBoxWidthInPixels + 10, 32), DescriptionText, DescriptionColor, DescriptionFont);
        RenderedRectangle.doRectangleOutline(DrawData.X.Value, DrawData.Y.Value, DrawData.Width.Value, DrawData.Height.Value, 2, null);
    }
}