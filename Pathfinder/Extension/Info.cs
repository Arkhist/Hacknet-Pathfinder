using System.IO;

namespace Pathfinder.Extension
{
    public abstract class Info
    {
        public string Id { get; internal set; }
        public bool IsActive => this == Handler.ActiveInfo;
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string LogoPath { get; }
        public abstract bool AllowSaves { get; }
        public abstract void OnConstruct(Hacknet.OS os);
        public virtual void OnLoad(Hacknet.OS os, Stream loadingStream) {}
    }
}
