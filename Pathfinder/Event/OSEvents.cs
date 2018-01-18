using System.IO;
using System.Xml;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Event
{
    public class OSEvent : PathfinderEvent
    {
        public OS OS { get; private set; }
        public OSEvent(OS os) { OS = os; }
    }

    public class OSLoadContentEvent : OSEvent
    {
        public OSLoadContentEvent(OS os) : base(os) { }
    }

    public class OSPostLoadContentEvent : OSEvent
    {
        public OSPostLoadContentEvent(OS os) : base(os) { }
    }

    public class OSUnloadContentEvent : OSEvent
    {
        public OSUnloadContentEvent(OS os) : base(os) { }
    }

    public class OSLoadSaveFileEvent : OSEvent
    {
        public XmlReader Reader { get; private set; }
        public Stream Stream { get; private set; }
        public OSLoadSaveFileEvent(OS os, XmlReader xmlReader, Stream stream) : base(os)
        {
            Reader = xmlReader;
            Stream = stream;
        }
    }

    public class OSSaveFileEvent : OSEvent
    {
        public string Filename { get; private set; }
        public OSSaveFileEvent(OS os, string fnam) : base(os) { Filename = fnam; }
    }

    public class OSSaveWriteEvent : OSSaveFileEvent
    {
        public string SaveString { get; set; }
        public OSSaveWriteEvent(OS os, string fnam, string saveStr) : base(os, fnam) { SaveString = saveStr; }
    }

    public class OSDrawEvent : OSEvent
    {
        public enum Type
        {
            Standard,
            EndingSequence,
            BootAssistance,
            BootingSequence,
            Loading,
            Error,
            Custom
        }
        private static bool doCustom;
        public static void SetCustomDraw(bool shouldDrawCustomOs) => doCustom = shouldDrawCustomOs;
        public static Type GetType(OS os)
        {
            if (doCustom) return Type.Custom;
            if (os.canRunContent && os.isLoaded) return Type.Standard;
            if (os.endingSequence.IsActive) return Type.EndingSequence;
            if ((os.BootAssitanceModule?.IsActive).GetValueOrDefault()) return Type.BootAssistance;
            if (os.bootingUp) return Type.BootingSequence;
            return Type.Loading;
        }
        public bool IgnorePostFXDraw { get; set; }
        public bool IgnoreScanlines { get; set; }
        public GameTime GameTime { get; private set; }
        public Type DrawType { get; set; }
        public OSDrawEvent(OS os, GameTime time) : this(os, time, GetType(os)) { }
        public OSDrawEvent(OS os, GameTime time, Type type) : base(os) { GameTime = time; DrawType = type; }
    }

    public class OSStartDrawEvent : OSDrawEvent
    {
        public OSStartDrawEvent(OS os, GameTime time) : base(os, time) { }
        public OSStartDrawEvent(OS os, GameTime time, Type type) : base(os, time, type) { }
    }

    public class OSEndDrawEvent : OSDrawEvent
    {
        public OSEndDrawEvent(OS os, GameTime time) : base(os, time) { }
        public OSEndDrawEvent(OS os, GameTime time, Type type) : base(os, time, type) { }
    }
}
