namespace Pathfinder.Util.Attribute
{
    /// <summary>
    /// Allow order unload attribute for allowing load order to be responsible for unloading the mod.
    /// </summary>
    public class AllowOrderUnloadAttribute : System.Attribute
    {
        public bool Allowed { get; }
        public AllowOrderUnloadAttribute(bool allowed = true) { Allowed = allowed; }
    }
}
