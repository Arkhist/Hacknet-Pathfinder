using System.Collections.Generic;
using Pathfinder.Computer;

namespace TemplateMod
{
    static class Commands
    {
        public static bool TemplateModVersion(Hacknet.OS os, List<string> args)
        {
            os.write("Template Mod version 1 !");
            if (os.thisComputer.AddModPort("tempPort"))
                os.write("tempPort added");
            return false;
        }
    }
}
