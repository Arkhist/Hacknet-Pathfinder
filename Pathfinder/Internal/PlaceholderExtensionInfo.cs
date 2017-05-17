using System.IO;
using Hacknet.Extensions;

namespace Pathfinder.Internal
{
    class PlaceholderExtensionInfo : ExtensionInfo
    {
        public PlaceholderExtensionInfo(Extension.Info info)
        {
            Name = info.Name;
            Description = info.Description;
            AllowSave = info.AllowSaves;
            FolderPath = Path.GetDirectoryName(info.GetType().Assembly.Location)
                             + Path.PathSeparator + info.Id.Remove(info.Id.IndexOf('.'));
            StartsWithTutorial = false;
            HasIntroStartup = false;
        }
    }
}
