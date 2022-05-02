using BepInEx.Configuration;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.GUI;
using Pathfinder.Options;

namespace PathfinderUpdater;

internal class RestartPopupScreen : GameScreen
{
    private PFButton AcceptVersion = new PFButton(450, 330, 120, 30, "Yes", new Color(102,255,127));
    private PFButton DenyVersion = new PFButton(1030, 330, 120, 30, "No", new Color(255,92,87));
    internal PluginCheckbox NoRestartPrompt = new PluginCheckbox("", "Do not prompt for restart", id: MainMenuOverride.NoRestartPrompt.Id);
    private ConfigFile _config;

    public RestartPopupScreen(ConfigFile config)
    {
        _config = config;
        IsPopup = true;
        NoRestartPrompt.Tab = MainMenuOverride.UpdaterTab;
        NoRestartPrompt.TrySetOffset(new Vector2(595, 300));
    }

    public void SaveUiData()
    {
        NoRestartPrompt.ConfigEntry.ConfigFile.Save();
    }

    public override void HandleInput(InputState input)
    {
        base.HandleInput(input);
        GuiData.doInput(input);
    }

    public override void Draw(GameTime gameTime)
    {
        ScreenManager.SpriteBatch.Begin();
        ScreenManager.SpriteBatch.Draw(Utils.white, new Rectangle(0, 0, ScreenManager.SpriteBatch.GraphicsDevice.Viewport.Width, GuiData.spriteBatch.GraphicsDevice.Viewport.Height), new Color(0, 0, 0, 0.65f));
        ScreenManager.SpriteBatch.Draw(Utils.white, new Rectangle(400, 250, 800, 150), Color.Black);
        TextItem.doLabel(new Vector2(550, 260), $"Do you want to restart the game?", Color.White);
        var prevRestartValue = NoRestartPrompt.Value;
        NoRestartPrompt.OnDraw(gameTime);
        if(NoRestartPrompt.Value != prevRestartValue)
            SaveUiData();
        if(AcceptVersion.Do())
        {
            PathfinderUpdaterPlugin.RestartForUpdate();
            ExitScreen();
        }
        else if(DenyVersion.Do())
            ExitScreen();
        ScreenManager.SpriteBatch.End();
    }
}