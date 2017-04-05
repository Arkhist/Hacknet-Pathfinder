using System;

namespace TemplateMod
{
    public class TemplateMod : Pathfinder.PathfinderMod
    {
        public override string GetIdentifier()
        {
            return "Template Mod";
        }

        public override void Load()
        {
            Console.WriteLine("Loading Template Mod");
        }

        public override void LoadContent()
        {
            Pathfinder.CommandHandler.AddCommand("templateModVersion", Commands.TemplateModVersion, true);
        }

        public override void Unload()
        {
            Console.WriteLine("Unloading Template Mod");
        }
    }
}
