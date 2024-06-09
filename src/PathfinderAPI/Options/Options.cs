using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.GUI;

namespace Pathfinder.Options;

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

    public abstract void Draw(int x, int y);
}

public class OptionCheckbox : Option
{
    public bool Value;

    public override int SizeX => 100;
    public override int SizeY => 75;

    private int ButtonID = PFButton.GetNextID();

    public OptionCheckbox(string name, string description="", bool defVal=false) : base(name, description)
    {
        this.Value = defVal;
    }

    public override void Draw(int x, int y)
    {
        TextItem.doLabel(new Vector2(x, y), Name, null, 200);
        Value = CheckBox.doCheckBox(ButtonID, x, y + 34, Value, null);

        TextItem.doSmallLabel(new Vector2(x+32, y+30), Description, null);
    }
}