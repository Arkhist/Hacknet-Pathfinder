using Hacknet;

namespace Pathfinder.Event.Menu
{
    public abstract class MainMenuEvent : PathfinderEvent
    {
        public MainMenu MainMenu { get; private set; }
        public MainMenuEvent(MainMenu mainMenu) { MainMenu = mainMenu; }
    }
}
