using System;
using System.Xml;
using System.Xml.Schema;

namespace Pathfinder.Util.XML
{
    public sealed class ReadOnlyXmlReader : XmlReader
    {
        internal XmlReader Reader { get; set; }

        public ReadOnlyXmlReader() { }

        public ReadOnlyXmlReader(XmlReader reader)
        {
            Reader = reader;
        }

        public override int AttributeCount => Reader.AttributeCount;
        public override string BaseURI => Reader.BaseURI;
        public override int Depth => Reader.Depth;
        public override bool EOF => Reader.EOF;
        public override bool IsEmptyElement => Reader.IsEmptyElement;
        public override string LocalName => Reader.LocalName;
        public override string NamespaceURI => Reader.NamespaceURI;
        public override XmlNameTable NameTable => Reader.NameTable;
        public override XmlNodeType NodeType => Reader.NodeType;
        public override string Prefix => Reader.Prefix;
        public override ReadState ReadState => Reader.ReadState;
        public override string Value => Reader.Value;
        public override bool HasAttributes => Reader.HasAttributes;
        public override bool HasValue => Reader.HasValue;
        public override bool IsDefault => Reader.IsDefault;
        public override string this[int i] => Reader[i];
        public override string this[string name] => Reader[name];
        public override string this[string name, string namespaceURI] => Reader[name, namespaceURI];
        public override string Name => Reader.Name;
        public override char QuoteChar => Reader.QuoteChar;
        public override IXmlSchemaInfo SchemaInfo => Reader.SchemaInfo;
        public override XmlReaderSettings Settings => Reader.Settings;
        public override Type ValueType => Reader.ValueType;
        public override string XmlLang => Reader.XmlLang;
        public override XmlSpace XmlSpace => Reader.XmlSpace;

        public override void Close() { }

        public override string GetAttribute(int i) => Reader.GetAttribute(i);
        public override string GetAttribute(string name) => Reader.GetAttribute(name);
        public override string GetAttribute(string name, string namespaceURI) => Reader.GetAttribute(name, namespaceURI);
        public override string LookupNamespace(string prefix) => Reader.LookupNamespace(prefix);

        public override bool MoveToAttribute(string name) => false;
        public override bool MoveToAttribute(string name, string ns) => false;
        public override void MoveToAttribute(int i) { }
        public override XmlNodeType MoveToContent() => NodeType;
        public override bool MoveToElement() => false;
        public override bool MoveToFirstAttribute() => false;
        public override bool MoveToNextAttribute() => false;
        public override bool Read() => false;
        public override bool ReadAttributeValue() => false;
        public override void ResolveEntity() { }
        public override void Skip() { }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
            => null;

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
            => 0;

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
            => 0;

        public override bool ReadContentAsBoolean() => false;
        public override DateTime ReadContentAsDateTime() => default;
        public override decimal ReadContentAsDecimal() => 0;
        public override double ReadContentAsDouble() => 0;
        public override float ReadContentAsFloat() => 0;
        public override int ReadContentAsInt() => 0;
        public override long ReadContentAsLong() => 0;
        public override object ReadContentAsObject() => null;
        public override string ReadContentAsString() => null;

        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
            => null;

        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
            => null;

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
            => 0;

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
            => 0;

        public override bool ReadElementContentAsBoolean()
            => false;

        public override bool ReadElementContentAsBoolean(string localName, string namespaceURI)
            => false;

        public override DateTime ReadElementContentAsDateTime()
            => default;

        public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
            => default;

        public override decimal ReadElementContentAsDecimal()
            => 0;

        public override decimal ReadElementContentAsDecimal(string localName, string namespaceURI)
            => 0;

        public override double ReadElementContentAsDouble()
            => 0;

        public override double ReadElementContentAsDouble(string localName, string namespaceURI)
            => 0;

        public override float ReadElementContentAsFloat()
            => 0;

        public override float ReadElementContentAsFloat(string localName, string namespaceURI)
            => 0;

        public override int ReadElementContentAsInt()
            => 0;

        public override int ReadElementContentAsInt(string localName, string namespaceURI)
            => 0;

        public override long ReadElementContentAsLong()
            => 0;

        public override long ReadElementContentAsLong(string localName, string namespaceURI)
            => 0;

        public override object ReadElementContentAsObject()
            => null;

        public override object ReadElementContentAsObject(string localName, string namespaceURI)
            => null;

        public override string ReadElementContentAsString()
            => null;

        public override string ReadElementContentAsString(string localName, string namespaceURI)
            => null;

        public override string ReadElementString()
            => null;

        public override string ReadElementString(string name)
            => null;

        public override string ReadElementString(string localname, string ns)
            => null;

        public override void ReadEndElement() { }

        public override string ReadInnerXml() => null;
        public override string ReadOuterXml() => null;

        public override void ReadStartElement() { }
        public override void ReadStartElement(string name) { }
        public override void ReadStartElement(string localname, string ns) { }

        public override string ReadString() => null;
        public override XmlReader ReadSubtree() => this;

        public override bool ReadToDescendant(string name) => false;
        public override bool ReadToDescendant(string localName, string namespaceURI) => false;
        public override bool ReadToFollowing(string name) => false;
        public override bool ReadToFollowing(string localName, string namespaceURI) => false;
        public override bool ReadToNextSibling(string name) => false;
        public override bool ReadToNextSibling(string localName, string namespaceURI) => false;

        public override int ReadValueChunk(char[] buffer, int index, int count)
            => 0;
    }
}
