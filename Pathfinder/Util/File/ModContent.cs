using System;
using System.Collections.Generic;
using IO = System.IO;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Hacknet;

namespace Pathfinder.Util.File
{
    public class ModContent
    {
        private string modId;
        private string contentPath;
        private string texturePath;
        private string soundPath;
        private string musicPath;
        private Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        private Dictionary<string, SoundEffect> loadedSounds = new Dictionary<string, SoundEffect>();
        private Dictionary<string, string> songNameToPath = new Dictionary<string, string>();

        public ModContent(string modId, string textureDirName, string soundDirName, string musicDirName)
        {
            var sep = Path.DirectorySeparatorChar;
            this.modId = modId;
            this.contentPath = Pathfinder.ModFolderPath + sep + this.modId;
            this.texturePath = this.contentPath + sep + textureDirName;
            this.soundPath = this.contentPath + sep + soundDirName;
            this.musicPath = this.contentPath + sep + musicDirName;
            if (!Directory.Exists(this.contentPath))
               Directory.CreateDirectory(this.contentPath);
            if (!Directory.Exists(this.texturePath))
                Directory.CreateDirectory(this.texturePath);
            if (!Directory.Exists(this.soundPath))
                Directory.CreateDirectory(this.soundPath);
            if (!Directory.Exists(this.musicPath))
                Directory.CreateDirectory(this.musicPath);

            LoadTextures();
            LoadSounds();
            LoadMusicList();
        }

        public Texture2D GetTexture(string texName)
        {
            if (this.loadedTextures.ContainsKey(texName))
                return this.loadedTextures[texName];
            return null;
        }

        public SoundEffect GetSound(string soundName)
        {
            if (this.loadedSounds.ContainsKey(soundName))
                return this.loadedSounds[soundName];
            return null;
        }

        public string GetSongPath(string songName)
        {
            if (this.songNameToPath.ContainsKey(songName))
                return this.songNameToPath[songName];
            return null;
        }

        public bool PlaySound(string soundName, float volume = 1, float pitch = 0, float pan = 0)
        {
            if (Settings.soundDisabled || SoundSystem.SoundMuted) return false;
            bool? b = GetSound(soundName)?.Play(Math.Min(SoundSystem.SoundVolume, volume*SoundSystem.SoundVolume), pitch, pan);
            return b.HasValue && b.Value;
        }

        public void LoadAsCurrentSong(string songName)
        {
            if(songNameToPath.ContainsKey(songName))
                MusicManager.loadAsCurrentSong(songNameToPath[songName]);
        }

        public void TransitionToSong(string songName)
        {
            if (songNameToPath.ContainsKey(songName))
                MusicManager.transitionToSong(songNameToPath[songName]);
        }

        public void PlaySongImmediately(string songName)
        {
            if (songNameToPath.ContainsKey(songName))
                MusicManager.playSongImmediatley(songNameToPath[songName]);
        }

        public void LoadTextures()
        {
            loadedTextures.Clear();
            foreach (var tex in Directory.GetFiles(this.texturePath))
            {
                try
                {
                    using (var stream = IO.File.OpenRead(tex))
                        loadedTextures.Add(Path.GetFileNameWithoutExtension(tex),
                                           Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, stream));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed loading texture " + tex + " : " + e.Message);
                }
            }
        }

        public void LoadSounds()
        {
            loadedSounds.Clear();
            foreach (var sou in Directory.GetFiles(this.soundPath))
			{
                try
                {
                    using (var stream = IO.File.OpenRead(sou))
                        loadedSounds.Add(Path.GetFileNameWithoutExtension(sou), SoundEffect.FromStream(stream));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed loading sound " + sou + " : " + e.Message);
                }
            }
        }

        public void LoadMusicList()
        {
            songNameToPath.Clear();
            foreach (var mus in Directory.GetFiles(this.musicPath))
                songNameToPath.Add(Path.GetFileNameWithoutExtension(mus), mus);
        }
    }
}
