using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace Pathfinder.Util.XML
{
    public delegate bool OpenFile(EventReader reader);
    public delegate bool Read(EventReader reader);
    public delegate void ReadDocument(EventReader reader);
    public delegate void ReadComment(EventReader reader);
    public delegate void ReadElement(EventReader reader, IDictionary<string, string> attributes);
    public delegate void ReadEndElement(EventReader reader);
    public delegate void ReadText(EventReader reader);
    public delegate void ReadProcessingInstructions(EventReader reader);
    public delegate void CloseFile(EventReader reader);

    public class EventReader
    {
        public event OpenFile OnOpenFile;
        public event Read OnRead;
        public event ReadDocument OnReadDocument;
        public event ReadComment OnReadComment;
        public event ReadElement OnReadElement;
        public event ReadEndElement OnReadEndElement;
        public event ReadText OnReadText;
        public event ReadProcessingInstructions OnReadProcessingInstructions;
        public event CloseFile OnCloseFile;

        protected List<string> ParentList { get; set; } = new List<string>();
        protected Exception Exception { get; set; }

        public ReadOnlyCollection<string> ActiveParents => ParentList.AsReadOnly();

        private string path;
        public string Path
        {
            get => path;
            set
            {
                path = new FileInfo(value).FullName;
                Text = null;
            }
        }

        private string text;
        private string queuedText;
        public string Text
        {
            get => text;
            set => queuedText = value;
        }

        protected XmlReader reader;
        protected ReadOnlyXmlReader readonlyReader = new ReadOnlyXmlReader();
        public XmlReader Reader
        {
            get => readonlyReader;
            protected set
            {
                reader = value;
                readonlyReader.Reader = reader;
            }
        }

        public string Name => reader.Name;
        public string Value => reader.Value;
        public int Depth => reader.Depth;
        public bool HasValue => reader.HasValue;
        public bool HasAttributes => reader.HasAttributes;
        public bool IsEmptyElement => reader.IsEmptyElement;
        public XmlNodeType NodeType => reader.NodeType;

        public EventReader(string str, bool isPath = true) { if (isPath) Path = str; else text = str; }

        public void Parse()
        {
            if (!TryParse(out var ex)) throw ex;
        }

        protected virtual bool OpenFile() => OnOpenFile?.Invoke(this) ?? true;
        protected virtual bool Read() => OnRead?.Invoke(this) ?? true;
        protected virtual void ReadDocument() => OnReadDocument?.Invoke(this);
        protected virtual void ReadElement(IDictionary<string, string> attributes) => OnReadElement?.Invoke(this, attributes);
        protected virtual void ReadComment() => OnReadComment?.Invoke(this);
        protected virtual void ReadEndElement() => OnReadEndElement?.Invoke(this);
        protected virtual void ReadProcessingInstructions() => OnReadProcessingInstructions?.Invoke(this);
        protected virtual void ReadText() => OnReadText?.Invoke(this);
        protected virtual void CloseFile() => OnCloseFile?.Invoke(this);


        public bool TryParse(out Exception exception)
        {
            exception = null;

            if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Path))
                text = new StreamReader(Path).ReadToEnd();

            if(Text == null)
            {
                exception = new NullReferenceException($"{nameof(Text)} is null.");
                return false;
            }

            // ensure XML is in a valid form
            using (var r = XmlReader.Create(new StringReader(Text))) while (r.Read());

            // Properly load XML
            using (Reader = XmlReader.Create(new StringReader(Text)))
            {
                if (OpenFile())
                {
                    ParentList.Clear();
                    while (reader.Read())
                    {
                        // Check end of element validity, remove parent if valid
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (ParentList.Count > 0 && reader.Name != ParentList.Last())
                            {
                                exception = new XmlException("Malformed XML Document");
                                break;
                            }
                            ParentList.RemoveAt(ParentList.Count - 1);
                        }
                        // Processor Nodes if Read returns true
                        if (Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Document:
                                    ReadDocument();
                                    break;
                                case XmlNodeType.Element:
                                    // Generate a dictionary of attributes for easier element access
                                    var attribs = new Dictionary<string, string>();
                                    if (reader.HasAttributes)
                                    {
                                        while (reader.MoveToNextAttribute())
                                            attribs.Add(reader.Name, reader.Value);
                                        reader.MoveToElement();
                                    }
                                    ReadElement(attribs);
                                    break;
                                case XmlNodeType.Comment:
                                    ReadComment();
                                    break;
                                case XmlNodeType.EndElement:
                                    ReadEndElement();
                                    break;
                                case XmlNodeType.Text:
                                    ReadText();
                                    break;
                                case XmlNodeType.ProcessingInstruction:
                                    ReadProcessingInstructions();
                                    break;
                            }
                        }
                        // Add element to end of parent list if not a self-contained element
                        if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
                            ParentList.Add(reader.Name);

                        // Kill processing if Exception is set
                        if(Exception != null)
                        {
                            exception = Exception;
                            break;
                        }

                        // Kill processing if queuedStrReader is set, prepare for next Parse()
                        if (queuedText != null)
                        {
                            text = queuedText;
                            queuedText = null;
                            break;
                        }
                    }
                }
                CloseFile();
            }
            return exception == null;
        }
    }
}
