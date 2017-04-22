using System;
using Pathfinder.Util.File;

namespace Pathfinder
{
    public interface IPathfinderMod
    {
        string Identifier { get; }

        void Load();
        void LoadContent();
        void Unload();
    }

    public abstract class PathfinderMod : IPathfinderMod
    {
        private ModContent modContent;

        /// <summary>
        /// Retrieves the Mod's identifier.
        /// </summary>
        /// <value>The Mod's identifier</value>
#pragma warning disable CS0618 // Type or member is obsolete
        public string Identifier => GetIdentifier();
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Retrieves the Mod's identifier.
        /// </summary>
        /// <returns>String - The Mod's identifier</returns>
        /// <remarks>Obsolete: Use Identifier instead</remarks>
        [Obsolete("Use Identifier")]
        public abstract string GetIdentifier();

        public virtual string TexturePath => "Texture";
        public virtual string SoundPath => "Sound";
        public virtual string MusicPath => "Music";

        public ModContent ModContent
        {
            get
            {
                if(modContent == null)
                    modContent = new ModContent(Identifier, TexturePath, SoundPath, MusicPath);
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
