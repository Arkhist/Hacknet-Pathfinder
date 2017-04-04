using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateMod
{
    public class TemplateMod : Pathfinder.PathfinderMod
    {
        public override string GetIdentifier()
        {
            return "Template Mod"
        }

        public override void Load()
        {
            Console.WriteLine("Loading Template Mod");
        }
    }
}
