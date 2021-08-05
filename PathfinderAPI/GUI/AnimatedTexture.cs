using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.GUI
{
    public class AnimatedTexture : IDisposable
    {
        public Texture2D[] Frames { get; }
        public int Framerate { get; set; }
        private float frameTime;
        public bool ShouldLoop { get; set; }
        public Rectangle DestRect { get; set; }
        public bool Paused { get; set; }
        
        public AnimatedTexture(IEnumerable<Texture2D> frames, Rectangle dest, int framerate, bool loop = false, bool startPaused = false)
        {
            Frames = frames?.ToArray() ?? throw new ArgumentNullException(nameof(frames));
            DestRect = dest;
            Framerate = framerate;
            frameTime = 1f / framerate;
            ShouldLoop = loop;
            Paused = startPaused;
        }

        private float timeSinceLastFrame = 0f;
        private int currentFrame = 0;
        public void Draw(float t, SpriteBatch spriteBatch)
        {
            if (Paused)
                return;
            
            timeSinceLastFrame += t;
            if (timeSinceLastFrame > frameTime)
            {
                currentFrame++;
                timeSinceLastFrame = 0f;
            }

            if (currentFrame < 0 || currentFrame >= Frames.Length)
            {
                currentFrame = 0;
                Paused = !ShouldLoop;
                if (Paused)
                    return;
            }
            
            spriteBatch.Draw(Frames[0], DestRect, Color.White);
        }

        public static AnimatedTexture ReadFromGIF(string filename)
        {
            var stream = new FileStream(filename, FileMode.Open);
            var ret = ReadFromGIF(stream);
            stream.Dispose();
            return ret;
        }

        public static AnimatedTexture ReadFromGIF(Stream stream, int x = 0, int y = 0)
        {
            var frameList = new List<Texture2D>();
            ushort width;
            ushort height;
            using (var r = new BinaryReader(stream))
            {
                var magic = r.ReadBytes(6);
                if (!magic.SequenceEqual(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }))
                    throw new NotSupportedException("GIF magic does not match!");

                width = r.ReadUInt16();
                height = r.ReadUInt16();

                byte tableInfo = r.ReadByte();
                int tableSize = tableInfo >> 4;

                int bitDepth = (tableInfo & 0x08) + 1;

                int backgroundIndex = r.ReadByte();
                r.ReadByte(); // ratio, i dont wanna bother

                Color[] colorTable = new Color[tableSize];

                int offset = 0;
                for (int i = 0; i <= tableSize; i++)
                {
                    
                }
            }

            return null;
        }

        public void Dispose()
        {
            foreach (var frame in Frames)
                frame.Dispose();
        }
    }
}
