using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

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

        public static AnimatedTexture ReadFromFile(string filename)
        {
            var stream = new FileStream(filename, FileMode.Open);
            var ret = ReadFromStream(stream);
            stream.Dispose();
            return ret;
        }
        
        public static AnimatedTexture ReadFromStream(Stream stream)
        {
            var frameList = new List<Texture2D>();

            int width = 0;
            int height = 0;
            using (var image = new Bitmap(stream))
            {
                width = image.Width;
                height = image.Height;
                for (int i = 0; i < image.GetFrameCount(FrameDimension.Time); i++)
                {
                    image.SelectActiveFrame(FrameDimension.Page, i);
                    var data = image.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    Color[] colors = new Color[height * width];
                    unsafe
                    {
                        Color* ptr = (Color*)data.Scan0.ToPointer();
                        for (int arrayIndex = 0; arrayIndex < colors.Length; arrayIndex++)
                        {
                            Color color = *ptr++;
                            colors[arrayIndex] = new Color(color.B, color.G, color.R, color.A);
                        }
                    }
                    image.UnlockBits(data);
                    var frame = new Texture2D(GuiData.spriteBatch.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                    frame.SetData(colors);
                    frameList.Add(frame);
                }
            }

            return new AnimatedTexture(frameList, new Rectangle(0, 0, width, height), 30);
        }

        public void Dispose()
        {
            foreach (var frame in Frames)
                frame.Dispose();
        }
    }
}
