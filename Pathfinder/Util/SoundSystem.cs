using System;
using Hacknet;

namespace Pathfinder.Util
{
    public static class SoundSystem
    {
        public static bool SoundMuted { get; set; }

        private static float soundVolume;

        public static float SoundVolume
        {
            get { return soundVolume; }
            set { soundVolume = Math.Min(2, value); }
        }

        public static bool MusicMuted
        {
            get
            {
                return MusicManager.isMuted;
            }
            set
            {
                MusicManager.setIsMuted(value);
            }
        }

        public static float MusicVolume
        {
            get
            {
                return MusicManager.getVolume();
            }
            set
            {
                MusicManager.setVolume(value);
            }
        }
    }
}
