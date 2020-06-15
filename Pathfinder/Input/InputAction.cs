using System;
using System.Linq;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Input;

namespace Pathfinder.Input
{
    public class InputAction
    {
        public InputAction(int inputVal, float strength = 1, bool echo = false, int device = 0)
        {
            IsEcho = echo;
            Strength = strength;
            InputValue = inputVal;
            Device = device;
        }

        public int Device { get; set; }

        public bool IsPressed
        {
            get => Strength > Tolerance;
            set => Strength = value ? 1 : 0;
        }
        public bool IsEcho { get; set; }
        public float Strength { get; set; }
        public int InputValue { get; set; }
        public float Tolerance { get; set; } = 0.5f;

        public bool HasAltMod { get; set; }
        public bool HasCommandMod { get; set; }
        public bool HasControlMod { get; set; }
        public bool HasMetaMod { get; set; }
        public bool HasShiftMod { get; set; }

        public bool IsMouse => this is InputActionMouse;
        public bool IsKey => this is InputActionKey;
    }

    public class InputActionKey : InputAction
    {
        public struct KeyData
        {
            public int Unicode { get; }
            public int Scancode => (int)Key;
            public Keys Key { get; }
            public bool Echo { get; }
            public KeyData(Keys val = Keys.None, bool echo = false)
            {
                Key = val;
                Echo = echo;
                var kbs = GuiData.getKeyboadState();
                Unicode = TextBox.ConvertKeyToChar(val, kbs.IsKeyDown(Keys.LeftShift) || kbs.IsKeyDown(Keys.CapsLock))[0];
            }
        }

        public InputActionKey(KeyData[] inputVal, int device = 0) : base(0, 1, false, device)
        {
        }

        public int Unicode { get; set; }

        public KeyData[] PressedKeys { get; set; }

        public KeyData GetKeyDataFor(Keys key) => PressedKeys.FirstOrDefault(k => k.Key == key);
        public bool IsKeyPressed(Keys key) => GetKeyDataFor(key).Key == key;
        public bool IsKeyReleased(Keys key) => !IsKeyPressed(key);
        public bool IsKeyJustPressed(Keys key)
        {
            var data = GetKeyDataFor(key);
            if (data.Key != key) return false;
            return !data.Echo;
        }
    }

    public class InputActionMouse : InputAction
    {
        public enum MouseButtons
        {
            Left = 1,
            Middle,
            Right,
            XOne,
            XTwo
        }

        public InputActionMouse(MouseState inputVal, float strength = 1, bool echo = false, int device = 0) : base(0, strength, echo, device)
        {
            SetMouseButtonValue(GetMouseButtonIndicies(inputVal));
            X = inputVal.X;
            Y = inputVal.Y;
            Scroll = inputVal.ScrollWheelValue;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public int Scroll { get; set; }

        public bool LeftPressed
        {
            get => IsMouseButtonPressed(MouseButtons.Left);
            set => SetMouseButtonPressed(MouseButtons.Left, value);
        }

        public bool MiddlePressed
        {
            get => IsMouseButtonPressed(MouseButtons.Middle);
            set => SetMouseButtonPressed(MouseButtons.Middle, value);
        }

        public bool RightPressed
        {
            get => IsMouseButtonPressed(MouseButtons.Right);
            set => SetMouseButtonPressed(MouseButtons.Right, value);
        }

        public bool XOnePressed
        {
            get => IsMouseButtonPressed(MouseButtons.XOne);
            set => SetMouseButtonPressed(MouseButtons.XOne, value);
        }

        public bool XTwoPressed
        {
            get => IsMouseButtonPressed(MouseButtons.XTwo);
            set => SetMouseButtonPressed(MouseButtons.XTwo, value);
        }

        private static int GetMouseButtonIndicies(MouseState state)
        {
            int ind = 0;
            if (state.LeftButton == ButtonState.Pressed)
                ind |= 1 << (int)MouseButtons.Left;
            if (state.MiddleButton == ButtonState.Pressed)
                ind |= 1 << (int)MouseButtons.Middle;
            if (state.RightButton == ButtonState.Pressed)
                ind |= 1 << (int)MouseButtons.Right;
            if (state.XButton1== ButtonState.Pressed)
                ind |= 1 << (int)MouseButtons.XOne;
            if (state.XButton2 == ButtonState.Pressed)
                ind |= 1 << (int)MouseButtons.XTwo;
            return ind;
        }

        public bool IsMouseButtonPressed(MouseButtons button)
        {
            var flag = 1 << (int)button;
            return (InputValue & flag) == flag;
        }

        public void SetMouseButtonPressed(MouseButtons button, bool pressed)
        {
            if (pressed)
                InputValue |= 1 << (int)button;
            else
                InputValue &= ~(1 << (int)button);
            if (pressed && !IsPressed) IsPressed = true;
            else IsPressed |= InputValue != 0;
        }

        private void SetMouseButtonValue(int flag)
        {
            InputValue |= flag;
            IsPressed |= InputValue != 0;
        }
    }
}
