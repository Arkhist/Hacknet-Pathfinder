namespace Pathfinder.Event
{
    public class ModuleEvent : PathfinderEvent
    {
        public Hacknet.Module Module { get; private set; }
        public ModuleEvent(Hacknet.Module module) { Module = module; }
    }

    public class DisplayModuleEvent : ModuleEvent
    {
        public Hacknet.DisplayModule DisplayModule { get; private set; }
        public new Hacknet.Module Module => DisplayModule;
        public DisplayModuleEvent(Hacknet.DisplayModule module) : base(module) { DisplayModule = module; }
    }

    public class DisplayModuleUpdateEvent : DisplayModuleEvent
    {
        public float Time { get; private set; }
        public DisplayModuleUpdateEvent(Hacknet.DisplayModule module, float time) : base(module) { Time = time; }
    }

    public class DisplayModuleDrawEvent : ModuleEvent
    {
        public float Time { get; private set; }
        public DisplayModuleDrawEvent(Hacknet.DisplayModule module, float time) : base(module) { Time = time; }
    }
}
