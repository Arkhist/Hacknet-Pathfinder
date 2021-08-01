using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Hacknet;
using BepInEx.Logging;
using Hacknet;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Gui;
using Hacknet.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pathfinder.Options
{
    public abstract class Option
    {
        public string Name;
        public string Description = "";
        public bool Enabled = true;

        public virtual int SizeX => 0;
        public virtual int SizeY => 0;

        public Option(string name, string description="")
        {
            this.Name = name;
            this.Description = description;
        }

        public abstract void Draw(int id, int x, int y);
    }

    public class OptionCheckbox : Option
    {
        public bool Value;

        public override int SizeX => 100;
        public override int SizeY => 75;

        public OptionCheckbox(string name, string description="", bool defVal=false) : base(name, description)
        {
            this.Value = defVal;
        }

        public override void Draw(int id, int x, int y)
        {
            TextItem.doLabel(new Vector2(x, y), Name, null, 200);
			Value = CheckBox.doCheckBox(id, x, y + 34, Value, null);

            TextItem.doSmallLabel(new Vector2(x+32, y+30), Description, null);
        }
    }
}
