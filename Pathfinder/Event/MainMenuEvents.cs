using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Event
{
    public class MainMenuEvent : PathfinderEvent
    {
        public Hacknet.MainMenu MainMenu { get; private set; }
        [Obsolete("Use MainMenu")]
        public Hacknet.MainMenu MainMenuInstance => MainMenu;
        public MainMenuEvent(Hacknet.MainMenu mainMenu)
        {
            MainMenu = mainMenu;
        }
    }

    public class DrawMainMenuEvent : MainMenuEvent
    {
        public GameTime GameTime { get; private set; }
        public DrawMainMenuEvent(Hacknet.MainMenu mainMenu, GameTime gameTime) : base(mainMenu) { GameTime = gameTime; }
    }

    public class DrawMainMenuButtonsEvent : MainMenuEvent
    {
        public DrawMainMenuButtonsEvent(Hacknet.MainMenu mainMenu) : base(mainMenu) {}
    }

    public class DrawMainMenuTitlesEvent : MainMenuEvent
    {
        public class TitleData
        {
            public TitleData(string title, Color color, SpriteFont font, Vector4 dest)
            {
                Title = title;
                Color = color;
                Font = font;
                Destination = dest;
            }
            public string Title { get; set; }
            public Color Color { get; set; }
            public SpriteFont Font { get; set; }
            public Vector4 Destination { get; set; }
            public Rectangle RectangleDestination
            {
                get
                {
                    return new Rectangle((int)Destination.X, (int)Destination.Y, (int)Destination.Z, (int)Destination.W);
                }
            }
        }
        public TitleData Main { get; set; }
        public TitleData Sub { get; set; }
        [Obsolete("Use Main.Title instead")]
        public string MainTitle { get { return Main.Title; } set { Main.Title = value; } }
        [Obsolete("Use Sub.Title instead")]
        public string Subtitle { get { return Sub.Title; } set { Sub.Title = value; } }
        [Obsolete("Use the title data overload instead")]
        public DrawMainMenuTitlesEvent(Hacknet.MainMenu mainMenu, string mainTitle, string subtitle) : base (mainMenu)
        {
            MainTitle = mainTitle;
            Subtitle = subtitle;
        }
        public DrawMainMenuTitlesEvent(Hacknet.MainMenu mainMenu, TitleData main, TitleData sub) : base(mainMenu)
        {
            Main = main;
            Sub = sub;
        }
    }
}
