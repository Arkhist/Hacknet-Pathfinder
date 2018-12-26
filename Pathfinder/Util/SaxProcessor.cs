using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Sax.Net;
using Sax.Net.Helpers;

namespace Pathfinder.Util
{
    public class SaxProcessor : DefaultHandler
    {
        /// <summary>
        /// Gets the IXMLReader.
        /// </summary>
        /// <value>The reader.</value>
        public IXmlReader Parser { get; protected set; }

        public class ElementInfo
        {
            public ElementInfo(string name, int depth, string value = "", IAttributes a = null)
            {
                Name = name;
                Depth = depth;
                Value = value;
                Attributes = a;
            }
            public string Name { get; }
            public string Value { get; internal set; }
            public int StartPosition { get; internal set; }
            public IAttributes Attributes { get; }
            public ReadOnlyCollection<ElementInfo> Elements => new ReadOnlyCollection<ElementInfo>(elements);
            internal int Depth { get; set; }
            internal Collection<ElementInfo> elements = new Collection<ElementInfo>();
        }

        private Dictionary<string, List<Action<ElementInfo>>> elementActions = new Dictionary<string, List<Action<ElementInfo>>>();
        private ElementInfo currentElement;
        private int currentDepth;
        private int lastPosition;

        public SaxProcessor()
        {
            Parser = XmlReaderFactory.Current.CreateXmlReader();
            Parser.ContentHandler = this;
            Parser.ErrorHandler = this;
        }

        /// <summary>
        /// Processes the specified input.
        /// </summary>
        /// <returns>The process.</returns>
        /// <param name="input">Input.</param>
        public void Process(string input)
        {
            Parser.Parse(new InputSource(new StringReader(input)));
        }

        public void Process(Stream stream)
        {
            Parser.Parse(new InputSource(stream));
        }

        /// <summary>
        /// Adds an action for a tag name.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <param name="action">Action.</param>
        public void AddActionForTag(string tag, Action<ElementInfo> action)
        {
            if (elementActions.ContainsKey(tag) && elementActions[tag] != null)
                elementActions[tag].Add(action);
            else
                elementActions[tag] = new List<Action<ElementInfo>>(new Action<ElementInfo>[] { action });
        }

        /// <summary>
        /// Removes an action for a tag name.
        /// </summary>
        /// <returns><c>true</c>, if action for tag was removed, <c>false</c> otherwise.</returns>
        /// <param name="tag">Tag.</param>
        /// <param name="action">Action.</param>
        public bool RemoveActionForTag(string tag, Action<ElementInfo> action)
        {
            if (!elementActions.ContainsKey(tag) || elementActions[tag] == null)
                return true;
            return elementActions[tag].Remove(action);
        }

        public override void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            if (currentDepth == 0)
                currentElement = null;

            if (string.IsNullOrEmpty(uri))
            {
                if (currentElement != null)
                    currentElement.elements.Add(new ElementInfo(qName, ++currentDepth, a: atts));
                else if (elementActions.ContainsKey(qName) && elementActions[qName] != null && elementActions[qName].Count > 0)
                    currentElement = new ElementInfo(qName, ++currentDepth, a: atts.Clone());
            }
        }

        public override void Characters(char[] ch, int start, int length)
        {
            if (currentElement == null) return;
            var value = "";
            for (int i = start; i < start + length; i++)
                value += ch[i];
            if (currentDepth != currentElement.Depth)
            {
                var info = currentElement.elements[currentElement.elements.Count - 1];
                info.Value = value;
                info.StartPosition = lastPosition;
            }
            else
                currentElement.Value = value;
            lastPosition = length;
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            if (string.IsNullOrEmpty(uri))
            {
                if (qName == currentElement.Name
                    && currentDepth == currentElement.Depth
                    && elementActions.ContainsKey(qName)
                    && elementActions[qName] != null
                    && elementActions[qName].Count > 0)
                    foreach (var action in elementActions[qName])
                        action(currentElement);
                currentDepth--;
            }
        }

        public override void EndDocument()
        {
            if (currentDepth > currentElement?.Depth && currentDepth > 0)
                throw new SAXParseException("Unexpected end of file", null);
            if (currentDepth < currentElement?.Depth && currentDepth < 0)
                throw new SAXParseException("Invalid XML formatting", null);
        }
    }
}
