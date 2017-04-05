using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateMod
{
    static class Commands
    {
        public static bool TemplateModVersion(Hacknet.OS os, string[] args)
        {
            os.write("Template Mod version 1 !");
            return false;
        }
    }
}
