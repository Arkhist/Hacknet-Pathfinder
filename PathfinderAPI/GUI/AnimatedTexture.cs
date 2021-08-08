using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;

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

        public static AnimatedTexture ReadFromAPNG(string filename)
        {
            var stream = new FileStream(filename, FileMode.Open);
            var ret = ReadFromAPNG(stream);
            stream.Dispose();
            return ret;
        }
        
        public static AnimatedTexture ReadFromAPNG(Stream stream, int x = 0, int y = 0)
        {
            var frameList = new List<Texture2D>();
            using (var r = new SBAwareBinaryReader(stream, SBAwareBinaryReader.Endianess.BIG))
            {
                var magic = r.ReadBytes(8);
                if (!magic.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                    throw new NotSupportedException("PNG magic mismatch!");
                
                uint width = 0;
                uint height = 0;
                int bitDepth = 0;
                byte colorType = 0;
                Color[] colorTable = null; // required if color type == 3
                Color[,] imageBuffer;
                
                while (true)
                {
                    var length = r.ReadInt32();
                    switch (r.ReadUInt32())
                    {
                        case 0x49484452: // IHDR
                            width = r.ReadUInt32();
                            height = r.ReadUInt32();
                            bitDepth = r.ReadByte();
                            colorType = r.ReadByte();
                            if (r.ReadByte() != 0) // compression method
                                throw new NotSupportedException("PNGs compressed with anything other than Deflate are not supported!");
                            r.ReadByte(); // filter method
                            r.ReadByte(); // interlace method
                            break;
                        case 0x504C5445: // PLTE
                            if (length % 3 != 0)
                                throw new NotSupportedException("PLTE color table length must be divisible by 3");
                            var entries = length / 3;
                            colorTable = new Color[entries];
                            for (int i = 0; i < entries; i++)
                            {
                                colorTable[i] = new Color(r.ReadByte(), r.ReadByte(), r.ReadByte());
                            }
                            break;
                        case 0x49444154: // IDAT
                            if (colorType == 3 && colorTable == null)
                                throw new NotSupportedException("Color type of 3 must have PLTE chunk before first IDAT!");
                            var dataBytes = r.ReadBytes(length);
                            var ms = new MemoryStream(dataBytes);
                            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
                            {
                                using (var dr = new SBAwareBinaryReader(ds, SBAwareBinaryReader.Endianess.BIG))
                                {
                                    bool hasData = true;
                                    switch (colorType)
                                    {
                                        case 2:
                                            while (true)
                                            {
                                                if (bitDepth == 8)
                                                {
                                                    
                                                } 
                                            }
                                            break;
                                        case 3:

                                            break;
                                        case 6:

                                            break;
                                    }
                                }
                            }
                            break;
                        case 0x49454E44: // IEND
                            goto exit;
                    }

                    r.ReadUInt32(); // CRC, dont wanna bother, and its Steam should verify on its end so the only way this could be wrong is a disk read error, and honestly i dont care enough to handle that edge case
                    continue;
                    
                    exit:
                    r.ReadUInt32(); // CRC for IEND
                    break;
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
