using Hacknet;
using Microsoft.Xna.Framework.Audio;

namespace Pathfinder.Util
{
    public static class SoundSystem
    {
        private static float? lastSound;
        public static bool SoundMuted
        {
            get { return lastSound.HasValue; }
            set
            {
                if (value)
                {
                    lastSound = SoundVolume;
                    SoundVolume = 0;
                }
                else
                {
                    SoundVolume = lastSound ?? 0;
                    lastSound = null;
                }
            }
        }

        public static float SoundVolume
        {
            get { return SoundEffect.MasterVolume; }
            set { SoundEffect.MasterVolume = value; }
        }

        public static bool MusicMuted
        {
            get { return MusicManager.isMuted; }
            set { MusicManager.setIsMuted(value); }
        }

        public static float MusicVolume
        {
            get { return MusicManager.getVolume(); }
            set { MusicManager.setVolume(value); }
        }
    }
}
