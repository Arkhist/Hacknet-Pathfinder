using System;

namespace Pathfinder.Util.Network
{
    static class Updater
    {
        public static string GetString(string url, string tag, string authKey = "")
        {
            Logger.Verbose("Getting version from {0} at tag {1}", url, tag);
            return new JsonObject(new Network.RestManager().Fetch(url, apiKey: authKey))[tag].AsString;
        }

        public static bool VerifyVersions(Version latestVersion, string currentVersion)
        {
            Version currentVer;
            if (Version.TryParse(currentVersion, out currentVer))
                return latestVersion <= currentVer;
            return false;
        }

        public static bool VerifyVersionByUrl(string url, string versionTag, string currentVer, string authKey = "")
        {
            Logger.Verbose("Verifiying {1} equals version {2} at url {0}", url, versionTag, currentVer);
            var v = GetString(url, versionTag, authKey);
            if (v != currentVer)
            {
                Version latest;
                if (Version.TryParse(v, out latest))
                    return VerifyVersions(latest, currentVer);
                return false;
            }
            return true;
        }
    }
}
