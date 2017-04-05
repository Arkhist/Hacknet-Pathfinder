using System;

namespace TemplateMod
{
    public class TemplateMod : Pathfinder.PathfinderMod
    {
        public override string GetIdentifier()
        {
            string s = Hacknet.MainMenu.OSVersion;
            return "Template Mod";
        }

        public override void Load()
        {
            Console.WriteLine("Loading Template Mod");
        }

        public override void Unload()
        {
            Console.WriteLine("Unloading Template Mod");
        }
    }
}
