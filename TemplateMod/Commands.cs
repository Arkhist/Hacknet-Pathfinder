using System;
using System.Collections.Generic;
using Pathfinder.Computer;
using Pathfinder.GameFilesystem;
using Pathfinder.Util;

namespace TemplateMod
{
    static class Commands
    {
        public static bool TemplateModVersion(Hacknet.OS os, List<string> args)
        {
            os.write("Template Mod version 1 !");
            if (os.thisComputer.AddModPort("Template Mod.tempPort"))
                os.write("tempPort added");
            var bin = os.thisComputer.GetFilesystem().Directory.FindDirectory("bin");
            if (Pathfinder.Executable.Handler.GetStandardFileDataBy("Template Mod.TempExe") != null)
                Logger.Info("working");
            if(!bin.ContainsFile("derpy"))
                bin.CreateFile("derpy", "derpyderp");
            if (!bin.ContainsFile("derp.exe") && bin.CreateExecutableFile("derp.exe", "Template Mod.TempExe") != null)
                os.write("TempExe added");
            if(new Random().Next(1,100) > 50)
                throw new System.Exception("throwing stuff");
            return false;
        }
    }
}
