using System;
using Pathfinder.Util;
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
            Logger.Verbose("Loading Template Mod");
        }

        public override void LoadContent()
        {
            Command.Handler.AddCommand("templateModVersion", Commands.TemplateModVersion, autoComplete: true);
            Executable.Handler.AddExecutable("TempExe", new TempExe());
        }

        public override void Unload()
        {
            Logger.Verbose("Unloading Template Mod");
        }

        class TempExe : Executable.Interface
        {
            public override string Identifier => "TempExe";

            public override bool? Update(Executable.Instance instance, float time)
            {
                Logger.Verbose("TempExe");
                return true;
            }
        }
    }
}
