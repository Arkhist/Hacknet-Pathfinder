using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.GUI;
using Pathfinder.Options;

namespace PathfinderUpdater;

internal sealed class RestartPopupScreen : GameScreen, IDisposable
{
    private readonly PFButton AcceptVersion = new PFButton(500, 330, 120, 30, "Yes", new Color(102,255,127));
    private readonly PFButton DenyVersion = new PFButton(980, 330, 120, 30, "No", new Color(255,92,87));
    internal readonly OptionCheckbox NoRestartPrompt = new OptionCheckbox("", "Do not prompt for restart");

    public RestartPopupScreen()
    {
        IsPopup = true;
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
        NoRestartPrompt.Draw(675, 300);
        if(AcceptVersion.Do())
        {
            PathfinderUpdaterPlugin.RestartForUpdate();
            ExitScreen();
        }
        else if(DenyVersion.Do())
            ExitScreen();
        ScreenManager.SpriteBatch.End();
    }

    public new void ExitScreen()
    {
        base.ExitScreen();
        PathfinderUpdaterPlugin.NoRestartPrompt.Value = NoRestartPrompt.Value;
        MainMenuOverride.NoRestartPrompt.Value = PathfinderUpdaterPlugin.NoRestartPrompt.Value;
    }

    public void Dispose()
    {
        AcceptVersion.Dispose();
        DenyVersion.Dispose();
    }
}