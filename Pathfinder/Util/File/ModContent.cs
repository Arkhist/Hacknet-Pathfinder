using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using IO = System.IO;

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
            contentPath = Pathfinder.ModFolderPath + sep + this.modId;
            texturePath = contentPath + sep + textureDirName;
            soundPath = contentPath + sep + soundDirName;
            musicPath = contentPath + sep + musicDirName;
            if (!Directory.Exists(contentPath))
               Directory.CreateDirectory(contentPath);
            if (!Directory.Exists(texturePath))
                Directory.CreateDirectory(texturePath);
            if (!Directory.Exists(soundPath))
                Directory.CreateDirectory(soundPath);
            if (!Directory.Exists(musicPath))
                Directory.CreateDirectory(musicPath);

            LoadTextures();
            LoadSounds();
            LoadMusicList();
        }

        public Texture2D GetTexture(string texName)
        {
            if (loadedTextures.ContainsKey(texName))
                return loadedTextures[texName];
            return null;
        }

        public SoundEffect GetSound(string soundName)
        {
            if (loadedSounds.ContainsKey(soundName))
                return loadedSounds[soundName];
            return null;
        }

        public string GetSongPath(string songName)
        {
            if (songNameToPath.ContainsKey(songName))
                return songNameToPath[songName];
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
            foreach (var tex in Directory.GetFiles(texturePath))
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
            foreach (var sou in Directory.GetFiles(soundPath))
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
            foreach (var mus in Directory.GetFiles(musicPath))
                songNameToPath.Add(Path.GetFileNameWithoutExtension(mus), mus);
        }
    }
}
