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

    public override Vector2 MinSize
    {
        get
        {
            var minsize = base.MinSize;
            minsize.X = Math.Max(HeaderTextSize.X, DescriptionTextSize.X + 32);
            return minsize;
        }
    }

    public override void OnDraw(GameTime gameTime)
    {
        DrawString(Position, HeaderText, HeaderColor, HeaderFont);
        Value = CheckBox.doCheckBox(HacknetGuiId, (int)Position.X, (int)Position.Y + 34, Value, SelectedColor);
        DrawString(Position + new Vector2(32, 30), DescriptionText, DescriptionColor, DescriptionFont);
    }
}
