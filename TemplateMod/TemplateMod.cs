using System;
using Command = Pathfinder.Command;
using Executable = Pathfinder.Executable;

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
            Command.Handler.AddCommand("templateModVersion", Commands.TemplateModVersion, true);
            Executable.Handler.AddExecutable("TempExe", new TempExe());
        }

        public override void Unload()
        {
            Console.WriteLine("Unloading Template Mod");
        }

        class TempExe : Executable.Interface
        {
            public override string GetIdentifer(Executable.Instance instance)
            {
                return "TempExe";
            }

            public override bool? Update(Executable.Instance instance, float time)
            {
                Console.WriteLine("TempExe");
                return true;
            }
        }
    }
}
