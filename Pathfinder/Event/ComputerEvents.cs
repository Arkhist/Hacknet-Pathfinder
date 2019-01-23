using System;
using System.IO;
using System.Xml;
using Hacknet;
using Pathfinder.Internal;

namespace Pathfinder.Event
{
    public class ComputerEvent : PathfinderEvent
    {
        public Hacknet.Computer Computer { get; set; }
        public ComputerEvent(Hacknet.Computer com) { Computer = com; }
    }

    [Obsolete("LoadComputerXmlReadEvent being removed in next update, use a derived LoadSavedComputerEvent/LoadContentComputerEvent")]
    public class LoadComputerXmlReadEvent : ComputerEvent
    {
        public XmlReader Reader { get; private set; }
        public bool FromSave { get; private set; }
        public string Filename { get; private set; }
        public bool PreventNetmapAdd { get; private set; }
        public bool PreventDaemonInit { get; private set; }
        public LoadComputerXmlReadEvent(Hacknet.Computer com, XmlReader red, string fnam, bool prevNetmap, bool prevDaeInit, bool fromSave) : base(com)
        {
            FromSave = fromSave;
            Reader = red;
            Filename = fnam;
            PreventNetmapAdd = prevNetmap;
            PreventDaemonInit = prevDaeInit;
        }
    }

    public class LoadComputerEvent : ComputerEvent
    {
        public Stream Stream { get; set; }
        public LoadComputerEvent(Computer c, Stream s) : base(c) { Stream = s; }
    }

    public class LoadSavedComputerEvent : LoadComputerEvent
    {
        public OS OS { get; set; }
        public XmlReader Reader { get; set; }
        public LoadSavedComputerEvent(Computer c, XmlReader r, OS os) : base(c, r.ToStream()) { Reader = r; OS = os; }
    }

    public class LoadSavedComputerStartEvent : LoadSavedComputerEvent
    {
        public LoadSavedComputerStartEvent(Computer c, XmlReader r, OS os) : base(c, r, os) { }
    }

    public class LoadSavedComputerEndEvent : LoadSavedComputerEvent
    {
        public LoadSavedComputerEndEvent(Computer c, XmlReader r, OS os) : base(c, r, os) { }
    }

    public class LoadContentComputerEvent : LoadComputerEvent
    {
        public string Filename { get; set; }
        public bool PreventNetmapAdd { get; set; }
        public bool PreventDaemonInit { get; set; }
        public LoadContentComputerEvent(Computer c, string f, bool pna, bool pdi, Stream s = null) : base(c, s)
        {
            Filename = f;
            PreventNetmapAdd = pna;
            PreventDaemonInit = pdi;
        }
    }

    public class LoadContentComputerStartEvent : LoadContentComputerEvent
    {
        public string LocalizedFilename => LocalizedFileLoader.GetLocalizedFilepath(Filename);
        public LoadContentComputerStartEvent(Computer c, string f, bool pna, bool pdi, Stream s = null) : base(c, f, pna, pdi, s) { }
    }

    public class LoadContentComputerEndEvent : LoadContentComputerEvent
    {
        public LoadContentComputerEndEvent(Computer c, string f, bool pna, bool pdi, Stream s) : base(c, f, pna, pdi, s) { }
    }
}
