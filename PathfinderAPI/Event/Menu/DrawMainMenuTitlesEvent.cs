using BepInEx.Hacknet;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Hacknet;
using Hacknet.Gui;
using Hacknet.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Event.Menu;

[HarmonyPatch]
public class DrawMainMenuTitlesEvent : MainMenuEvent
{
    public DrawMainMenuTitlesEvent(MainMenu menu, TitleData main, TitleData sub) : base(menu) { Main = main; Sub = sub; }

    public TitleData Main { get; private set; }
    public TitleData Sub { get; private set; }

    static Color defaultTitleColor = new Color(190, 190, 190, 0);
    static SpriteFont defaultTitleFont;

    delegate void EmitDelegate(MainMenu menu, ref Rectangle rect);

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.DrawBackgroundAndTitle))]
    private static void onDrawMainMenuTitlesIL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchCall(AccessTools.Constructor(typeof(Rectangle), [typeof(int), typeof(int), typeof(int), typeof(int)]))
        );

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca, 0);
        c.EmitDelegate<EmitDelegate>((MainMenu self, ref Rectangle dest) =>
        {
            if (defaultTitleFont == null) defaultTitleFont = self.ScreenManager.Game.Content.Load<SpriteFont>("Kremlin");

            var version = HacknetChainloader.VERSION;
            var mainTitle = "HACKNET";
            var subtitle = "OS"
                + (DLC1SessionUpgrader.HasDLC1Installed ? "+Labyrinths " : " ")
                + MainMenu.OSVersion + " Pathfinder " + version;

            var main = new TitleData(mainTitle,
                defaultTitleColor,
                defaultTitleFont,
                dest
            );
            var sub = new TitleData(subtitle,
                main.Color * 0.5f,
                GuiData.smallfont,
                new Rectangle(520, 178, 0, 0)
            );

            var drawMainMenuTitles = new DrawMainMenuTitlesEvent(self, main, sub);
            EventManager<DrawMainMenuTitlesEvent>.InvokeAll(drawMainMenuTitles);

            main = drawMainMenuTitles.Main;
            sub = drawMainMenuTitles.Sub;
            FlickeringTextEffect.DrawLinedFlickeringText(
                dest = main.Destination,
                main.Title,
                7f,
                0.55f,
                main.Font,
                null,
                main.Color
            );
            TextItem.doFontLabel(new Vector2(sub.Destination.Location.X, sub.Destination.Location.Y), sub.Title, sub.Font, sub.Color, 600f, 26f);
        });

        var firstLabel = c.MarkLabel();
        c.GotoNext(MoveType.Before,
            x => x.MatchCall(AccessTools.Method(
                    typeof(TextItem),
                    nameof(TextItem.doFontLabel),
                    [typeof(Vector2), typeof(string), typeof(SpriteFont), typeof(Color?), typeof(float), typeof(float), typeof(bool)]
                )
            )
        );
        var endInst = c.Index;

        c.GotoLabel(firstLabel, MoveType.Before);
        c.RemoveRange((endInst - c.Index) + 1);
    }

    public class TitleData
    {
        public TitleData(string title, Color color, SpriteFont font, Rectangle dest)
        {
            Title = title;
            Color = color;
            Font = font;
            Destination = dest;
        }
        public string Title { get; set; }
        public Color Color { get; set; }
        public SpriteFont Font { get; set; }
        public Rectangle Destination { get; set; }
    }
}