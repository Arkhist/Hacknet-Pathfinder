using System;
using System.IO;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Mission
{
    public static class MissionManager
    {
        public static MissionListingServer CreateMissionListingDaemon(this Hacknet.Computer c,
                                                                      string serviceName,
                                                                      string logoPath,
                                                                      bool isPublic = false,
                                                                      bool isAssigner = false,
                                                                      Hacknet.OS os = null)
        {
            var s = new MissionListingServer(c, serviceName, serviceName, os ?? Hacknet.OS.currentInstance, isPublic, isAssigner);
            try
            {
                using (var fs = File.OpenRead(logoPath))
                    s.logo = Texture2D.FromStream(GuiData.spriteBatch.GraphicsDevice, fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Loading file " + logoPath + " failed: " + e);
                s.logo = os.content.Load<Texture2D>("Sprites/Academic_Logo");
            }
            c.daemons.Add(s);
            return s;
        }

        public static MissionListingServer SetThemeColor(this MissionListingServer s, Color color)
        {
            s.themeColor = color;
            return s;
        }

        public static MissionListingServer SetGroupName(this MissionListingServer s, string name)
        {
            s.groupName = name;
            return s;
        }

        public static MissionListingServer SetListingTitle(this MissionListingServer s, string title)
        {
            s.listingTitle = title;
            return s;
        }

        public static MissionListingServer SetState(this MissionListingServer s, int state)
        {
            s.state = state;
            return s;
        }

        public static MissionListingServer AddMission(this MissionListingServer s, ActiveMission m, bool injectTop = false)
        {
            s.addMisison(m, injectTop);
            var inst = m as Instance;
            if (inst != null)
                inst.MissionComputer = s.comp;
            return s;
        }

        public static MissionHubServer CreateMissionHubDaemon(this Hacknet.Computer c, string serviceName, Hacknet.OS os = null)
        {
            var s = new MissionHubServer(c, serviceName, serviceName, os ?? Hacknet.OS.currentInstance);
            c.daemons.Add(s);
            return s;
        }

        public static MissionHubServer SetThemeColor(this MissionHubServer s, Color color)
        {
            s.themeColor = color;
            return s;
        }

        public static MissionHubServer SetGroupName(this MissionHubServer s, string name)
        {
            s.groupName = name;
            return s;
        }

        public static MissionHubServer AddMission(this MissionHubServer s, ActiveMission m, bool insertTop = false, bool preventRegistryChange = false, int desiredIndex = -1)
        {
            if (insertTop && desiredIndex <= -1)
            {
                desiredIndex = 0;
            }
            s.contractRegistryNumber += Utils.getRandomByte() + 1;
            s.listingMissions.Add(string.Concat(s.contractRegistryNumber), m);
            var item = new FileEntry(MissionSerializer.generateMissionFile(m, s.contractRegistryNumber, s.groupName), "Contract#" + s.contractRegistryNumber);
            if (insertTop || desiredIndex >= 0)
                s.listingsFolder.files.Insert(desiredIndex, item);
            else
                s.listingsFolder.files.Add(item);
            var inst = m as Instance;
            if (inst != null)
                inst.MissionComputer = s.comp;
            return s;
        }
    }
}
