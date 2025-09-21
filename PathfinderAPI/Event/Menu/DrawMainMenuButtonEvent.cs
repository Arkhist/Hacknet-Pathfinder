using Hacknet;
using Hacknet.Gui;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Event.Menu;

public enum MainMenuButtonType : int
{
    Undefined = -1,
    Unknown = 0,
    NewSession = 1,
    Continue = 1102,
    Login = 11,
    Settings = 3,
    StartRelayServer = 4,
    Extensions = 5,
    NewLabyrinthSession = 7,
    Exit = 15,
}

[HarmonyPatch]
public class DrawMainMenuButtonEvent : MainMenuEvent
{
    delegate bool ButtonDrawDelegate(int id, int x, int y, int width, int height, string text, Color color, MainMenu self);

    public ButtonData Data { get; }

    public DrawMainMenuButtonEvent(MainMenu mainMenu, ButtonData data) : base(mainMenu)
    {
        Data = data;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.drawMainMenuButtons))]
    private static void AfterMainMenuDraw(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdcI4(450),
            x => x.MatchLdcI4(50),
            x => x.MatchLdstr("New Session"),
            x => x.MatchCall(typeof(LocaleTerms), nameof(LocaleTerms.Loc)),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.buttonColor))
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        // Continue Session Button
        c.GotoNext(MoveType.After,
            x => x.MatchLdfld(typeof(MainMenu), "canLoad"),
            x => x.MatchBrtrue(out var _),
            x => x.MatchCall(typeof(Color), "get_Black"),
            x => x.MatchBr(out var _),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.buttonColor)),
            x => x.MatchNop()
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Login"),
            x => x.MatchCall(typeof(LocaleTerms), nameof(LocaleTerms.Loc)),
            x => x.MatchLdloc(1),
            x => x.MatchBrtrue(out var _),
            x => x.MatchCall(typeof(Color), "get_Black"),
            x => x.MatchBr(out var _),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.buttonColor)),
            x => x.MatchNop()
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Settings"),
            x => x.MatchCall(typeof(LocaleTerms), nameof(LocaleTerms.Loc)),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.buttonColor))
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Start Relay Server"),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.buttonColor))
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Extensions"),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.buttonColor))
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        // New Labyrinth Session Button
        c.GotoNext(MoveType.After,
            x => x.MatchCall(typeof(Utils), nameof(Utils.rand)),
            x => x.MatchSub(),
            x => x.MatchCall(typeof(Color), nameof(Color.Lerp))
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Exit"),
            x => x.MatchCall(typeof(LocaleTerms), nameof(LocaleTerms.Loc)),
            x => x.MatchLdsfld(typeof(MainMenu), nameof(MainMenu.exitButtonColor))
        );
        c.Remove();
        c.Remove();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.Emit(OpCodes.Ldloca_S, (byte)4);
        c.EmitDelegate(ButtonDrawExecution);
    }

    // I have no idea why it was implemented with two Y position indexers and fixing it would kind of be pointless, just gonna keep them equal
    // If I don't do this at least two buttons could overlap
    public static bool ButtonDrawExecution(int id, int x, int y, int width, int height, string text, Color color, MainMenu self, ref int yPosIndexOne, ref int yPostIndexTwo)
    {
        var loadButtonData = new ButtonData(id, x, y, width, height, text, color, Math.Max(yPosIndexOne, yPostIndexTwo));
        var drawMainMenuButton = new DrawMainMenuButtonEvent(self, loadButtonData);
        EventManager<DrawMainMenuButtonEvent>.InvokeAll(drawMainMenuButton);
        yPosIndexOne = yPostIndexTwo = loadButtonData.YPositionIndex;

        if(!drawMainMenuButton.Cancelled)
            return Button.doButton(
                loadButtonData.Id,
                loadButtonData.X,
                loadButtonData.Y,
                loadButtonData.Width,
                loadButtonData.Height,
                loadButtonData.Text,
                loadButtonData.Color
            );
        return false;
    }

    public class ButtonData
    {
        public const int DefaultButtonTopPadding = 15;

        public int Id;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public string Text;
        public Color Color;
        public int YPositionIndex;

        private MainMenuButtonType _buttonType = MainMenuButtonType.Undefined;
        public MainMenuButtonType ButtonType
        {
            get
            {
                if(_buttonType == MainMenuButtonType.Undefined)
                {
                    if(Enum.IsDefined(typeof(MainMenuButtonType), Id))
                        _buttonType = (MainMenuButtonType)Id;
                    else _buttonType = MainMenuButtonType.Unknown;
                }
                return _buttonType;
            }
        }

        public ButtonData(int id, int x, int y, int width, int height, string text, Color color, int yPosIndex)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Text = text;
            Color = color;
            YPositionIndex = yPosIndex;
        }

        public bool Is(MainMenuButtonType button) => ButtonType == button;
    }
}
