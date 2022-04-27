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

    public override void SetSize()
    {
        var headerSize = HeaderTextSize;
        var descSize = DescriptionTextSize;
        if(DrawData.Width == null)
            DrawData = DrawData.Set(width: (int)(Math.Max(headerSize.X, descSize.X + SliderRangeWidth + 10)));
        if(DrawData.Height == null)
            DrawData = DrawData.Set(height: (int)(headerSize.Y + Math.Max(descSize.Y, SliderHeight + 9)));
    }

    public override void OnDraw(GameTime gameTime)
    {
        DrawString(DrawData, HeaderText, HeaderColor, HeaderFont);
        Value = SliderBar.doSliderBar(
            HacknetGuiId,
            DrawData.X.Value + 5,
            DrawData.Y.Value + 45,
            SliderRangeWidth,
            SliderHeight,
            MaximumValue,
            MinimumValue,
            Value,
            StepSize
        );

        DrawString(DrawData.QuickAdd(SliderRangeWidth + 10, 45), DescriptionText, DescriptionColor, DescriptionFont);
        RenderedRectangle.doRectangleOutline(DrawData.X.Value, DrawData.Y.Value, DrawData.Width.Value, DrawData.Height.Value, 2, null);
    }
}