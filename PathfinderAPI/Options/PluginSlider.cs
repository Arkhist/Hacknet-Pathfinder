using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Pathfinder.Options;

public class PluginSlider : BasePluginOption<float>
{
    public int SliderRangeWidth { get; set; } = 50;
    public int SliderHeight { get; set; } = 20;
    public float MinimumValue { get; set; } = 0f;
    public float MaximumValue { get; set; } = 1f;
    public float StepSize { get; set; }

    public PluginSlider(string headerText, string descriptionText = null, float defaultValue = 0f, float stepSize = 0f, string configDesc = null, string id = null)
        : base(headerText, descriptionText, defaultValue, configDesc, id)
    {
        StepSize = stepSize;
    }

    public override Vector2 MinSize
    {
        get
        {
            var headerSize = HeaderTextSize;
            var descSize = DescriptionTextSize;
            return new Vector2(Math.Max(headerSize.X, descSize.X + SliderRangeWidth + 10), headerSize.Y + Math.Max(descSize.Y, SliderHeight + 9));
        }
    }

    public override void OnDraw(GameTime gameTime)
    {
        DrawString(Position, HeaderText, HeaderColor, HeaderFont);
        Value = SliderBar.doSliderBar(
            HacknetGuiId,
            (int)Position.X + 5,
            (int)Position.Y + 45,
            SliderRangeWidth,
            SliderHeight,
            MaximumValue,
            MinimumValue,
            Value,
            StepSize
        );

        DrawString(Position + new Vector2(SliderRangeWidth + 10, 43), DescriptionText, DescriptionColor, DescriptionFont);
    }
}