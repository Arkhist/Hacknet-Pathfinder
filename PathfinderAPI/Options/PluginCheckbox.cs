using BepInEx.Configuration;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Options;

public class PluginCheckbox : BasePluginOption<bool>
{
    public Color SelectedColor { get; set; } = Color.White;

    public PluginCheckbox(string headerText, string descriptionText = null, bool defaultValue = false, string configDesc = null, string id = null)
        : base(headerText, descriptionText, defaultValue, configDesc, id)
    {
    }

    public override void OnDraw(GameTime gameTime)
    {
        DrawString(DrawData, HeaderText, HeaderColor, HeaderFont);
        Value = CheckBox.doCheckBox(HacknetGuiId, DrawData.X.Value, DrawData.Y.Value + 34, Value, SelectedColor);
        DrawString(DrawData.QuickAdd(32, 30), DescriptionText, DescriptionColor, DescriptionFont);
        RenderedRectangle.doRectangleOutline(DrawData.X.Value, DrawData.Y.Value, DrawData.Width.Value, DrawData.Height.Value, 2, null);
    }
}
