using Pathfinder.Util.File;

namespace Pathfinder
{
    public abstract class PathfinderMod
    {
        private ModContent modContent;

        /// <summary>
        /// This function returns the mod identifier of your mod.
        /// </summary>
        /// <returns>String - The mod identifier of your mod</returns>
        public abstract string GetIdentifier();

        public virtual string TexturePath
        {
            get
            {
                return "Texture";
            }
        }

        public virtual string SoundPath
        {
            get
            {
                return "Sound";
            }
        }

        public virtual string MusicPath
        {
            get
            {
                return "Music";
            }
        }

        public ModContent ModContent
        {
            get
            {
                if(modContent == null)
                    modContent = new ModContent(GetIdentifier(), TexturePath, SoundPath, MusicPath);
                return modContent;
            }
        }

        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        public abstract void Load();


        public abstract void LoadContent();

        /// <summary>
        /// Called when the mod is unloaded
        /// </summary>
        public abstract void Unload();
    }
}
