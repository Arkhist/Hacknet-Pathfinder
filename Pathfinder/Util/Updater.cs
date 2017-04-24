using System;
using System.Net;
using System.Security.Policy;

namespace Pathfinder.Util
{
    internal static class Updater
    {
        public static string GetString(string url, string tag, string authKey = "")
        {
            Logger.Verbose("Getting version from {0} at tag {1}", url, tag);
            string str = "";
            using (var client = new CustomWebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + authKey;
                client.DownloadString(url);
            }
            var obj = new JsonObject(str);
            return obj[tag].AsString;
        }

        public static bool VerifyVersions(Version latestVersion, string currentVersion)
        {
            Version currentVer;
            if (Version.TryParse(currentVersion, out currentVer))
                return latestVersion <= currentVer;
            return false;
        }

        public static bool VerifyVersionByUrl(string url, string versionTag, string currentVer)
        {
            Logger.Verbose("Verifiying {1} equals version {2} at url {0}", url, versionTag, currentVer);
            var v = GetString(url, versionTag);
            if (v != currentVer)
            {
                Version latest;
                if (Version.TryParse(v, out latest))
                {
                    return VerifyVersions(latest, currentVer);
                }
                return false;
            }
            return true;
        }
    }

    class CustomWebClient : WebClient
    {
        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).KeepAlive = false;
            }
            return request;
        }
    }
}
