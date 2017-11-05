using System;
using System.IO;
using System.Xml;

namespace Pathfinder.Event
{
    public class OSEvent : PathfinderEvent
    {
        public Hacknet.OS OS { get; private set; }
        [Obsolete("Use OS instead")]
        public Hacknet.OS OsInstance => OS;
        public OSEvent(Hacknet.OS os) { OS = os; }
    }

    public class OSLoadContentEvent : OSEvent
    {
        public OSLoadContentEvent(Hacknet.OS os) : base(os) { }
    }

    public class OSPostLoadContentEvent : OSEvent
    {
        public OSPostLoadContentEvent(Hacknet.OS os) : base(os) { }
    }

    public class OSUnloadContentEvent : OSEvent
    {
        public OSUnloadContentEvent(Hacknet.OS os) : base(os) { }
    }

    public class OSLoadSaveFileEvent : OSEvent
    {
        public XmlReader Reader { get; private set; }
        [Obsolete("Use Reader")]
        public XmlReader XmlReader => Reader;
        public Stream Stream { get; private set; }
        public OSLoadSaveFileEvent(Hacknet.OS os, XmlReader xmlReader, Stream stream) : base(os)
        {
            Reader = xmlReader;
            Stream = stream;
        }
    }

    public class OSSaveFileEvent : OSEvent
    {
        public string Filename { get; private set; }
        public OSSaveFileEvent(Hacknet.OS os, string fnam) : base(os) { Filename = fnam; }
    }

    public class OSSaveWriteEvent : OSSaveFileEvent
    {
        public string SaveString { get; set; }
        public OSSaveWriteEvent(Hacknet.OS os, string fnam, string saveStr) : base(os, fnam) { SaveString = saveStr; }
    }
}
