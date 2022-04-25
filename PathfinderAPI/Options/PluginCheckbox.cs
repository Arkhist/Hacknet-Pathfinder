using BepInEx.Configuration;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Pathfinder.Options;

public class PluginCheckbox : BasePluginOption<bool>
{
    public PluginCheckbox(string headerText, string descriptionText = null, bool defaultValue = false, string configDesc = null)
    {
        HeaderText = headerText;
        DescriptionText = descriptionText;
        DefaultValue = defaultValue;
        ConfigDescription = configDesc;
    }

    public override void OnDraw(GameTime gameTime)
    {
        TextItem.doLabel(DrawDataField, HeaderText, null, 200);
        Value = CheckBox.doCheckBox(HacknetGuiId, DrawDataField.X.Value, DrawDataField.Y.Value + 34, Value, null);

        TextItem.doSmallLabel(DrawDataField.QuickAdd(32, 30), DescriptionText, null);
    }
}
