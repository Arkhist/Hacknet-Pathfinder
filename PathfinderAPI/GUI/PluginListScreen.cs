using BepInEx.Hacknet;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Event.Menu;
using Pathfinder.Meta.Load;
using Pathfinder.Util;

namespace Pathfinder.GUI;

public class PluginListScreen : GameScreen
{
    private List<PluginInfo> _pluginDataList = new List<PluginInfo>();

    public string SelectedGuid;

    private PFButton BackButton = new PFButton(10, 10, 220, 30, $"<- {LocaleTerms.Loc("Back")}", Color.Gray);

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        PostProcessor.begin();
        ScreenManager.FadeBackBufferToBlack(255);
        GuiData.startDraw();
        PatternDrawer.draw(
            new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height),
            0.5f,
            Color.Black,
            new Color(2, 2, 2),
            GuiData.spriteBatch
        );
        if (BackButton.Do())
            ExitScreen();

        int defX = 10, defY = 50;
        foreach(var pluginData in _pluginDataList)
        {
            if(pluginData.DrawListElement(gameTime, this, new Point(defX, defY), pluginData.Guid == SelectedGuid))
                SelectedGuid = pluginData.Guid;
            defY += pluginData.Height + 10;
        }


        GuiData.endDraw();
        PostProcessor.end();
    }

    public override void HandleInput(InputState input)
    {
        base.HandleInput(input);
        GuiData.doInput(input);
    }

    public override void LoadContent()
    {
        base.LoadContent();
        foreach(var plugin in HacknetChainloader.Instance.Plugins)
            _pluginDataList.Add(new PluginInfo(this, (HacknetPlugin)plugin.Value.Instance));
    }

    private static PFButton _pluginListButton = new PFButton(0,0,0,0, "Plugins");

    [Initialize]
    internal static void Initialize()
    {
        EventManager<DrawMainMenuButtonEvent>.AddHandler(OnMainMenuButtonDraw);
    }

    public static void OnMainMenuButtonDraw(DrawMainMenuButtonEvent evt)
    {
        if(!evt.Data.Is(MainMenuButtonType.Extensions)) return;

        _pluginListButton.Color = MainMenu.buttonColor;
        _pluginListButton.X = evt.Data.X;
        _pluginListButton.Y = evt.Data.Y;
        _pluginListButton.Width = evt.Data.Width;
        _pluginListButton.Height = evt.Data.Height;
        if(_pluginListButton.Do())
            evt.MainMenu.ScreenManager.AddScreen(new PluginListScreen());

        evt.Data.Y += evt.Data.Height + DrawMainMenuButtonEvent.ButtonData.DefaultButtonTopPadding;
        evt.Data.YPositionIndex = evt.Data.Y;

    }
}