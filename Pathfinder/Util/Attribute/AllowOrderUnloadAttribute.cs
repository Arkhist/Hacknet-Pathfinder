namespace Pathfinder.Util.Attribute
{
    public class AllowOrderUnloadAttribute : System.Attribute
    {
        public bool Allowed { get; }
        public AllowOrderUnloadAttribute(bool allowed = true) { Allowed = allowed; }
    }
}
