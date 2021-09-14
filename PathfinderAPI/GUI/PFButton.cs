using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.GUI
{
    public class PFButton : IDisposable
    {
        private static int _idCounter = int.MinValue;
        private static readonly List<int> returnedIds = new List<int>();
        public static int GetNextID()
        {
            if (returnedIds.Count > 0)
            {
                var ret = returnedIds[0];
                returnedIds.RemoveAt(0);
                return ret;
            }

            return _idCounter++;
        }
        public static void ReturnID(int id)
        {
            returnedIds.Add(id);
        }

        public readonly int ID = GetNextID();

        public int X;
        public int Y;
        public int Height;
        public int Width;
        public string Text;
        public Color? Color;
        public Texture2D Texture;

        private bool invalid = false;
        
        public PFButton(int x, int y, int width, int height, string text, Color? color = null, Texture2D texture = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Text = text ?? throw new ArgumentNullException(nameof(text), "Button text cannot be null!");
            Color = color;
            Texture = texture;
        }

        public bool Do()
        {
            if (invalid)
                throw new ObjectDisposedException(nameof(PFButton), "This Button has been disposed, and is no longer valid");
            if (Texture == null)
                return Button.doButton(ID, X, Y, Width, Height, Text, Color);
            return Button.doButton(ID, X, Y, Width, Height, Text, Color, Texture);
        }

        public void Dispose()
        {
            if (invalid)
                return;
            returnedIds.Add(ID);
            invalid = true;
        }
    }
}
