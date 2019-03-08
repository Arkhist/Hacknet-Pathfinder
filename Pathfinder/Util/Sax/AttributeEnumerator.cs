using System.Collections;
using System.Collections.Generic;

namespace Sax.Net
{
    public class Attribute
    {
        public Attribute(int i, string u, string l, string q, string t, string v)
        {
            Index = i;
            Uri = u;
            LocalName = l;
            QName = q;
            Type = t;
            Value = v;
        }

        public int Index { get; private set; }
        public string Uri { get; private set; }
        public string LocalName { get; private set; }
        public string QName { get; private set; }
        public string Type { get; private set; }
        public string Value { get; private set; }
    }

    public class AttributeEnumerator : IEnumerator<Attribute>
    {
        IAttributes attributes;

        public AttributeEnumerator(IAttributes attrib)
        {
            attributes = attrib.Clone();
            Reset();
        }

        public Attribute Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (attributes.Length - 1 > Current.Index)
            {
                int newIndex = Current.Index + 1;
                Current = new Attribute(newIndex,
                                        attributes.GetUri(newIndex),
                                        attributes.GetLocalName(newIndex),
                                        attributes.GetQName(newIndex),
                                        attributes.GetType(newIndex),
                                        attributes.GetValue(newIndex)
                                       );
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Current = new Attribute(0,
									attributes.GetUri(0),
                                    attributes.GetLocalName(0),
                                    attributes.GetQName(0),
                                    attributes.GetType(0),
                                    attributes.GetValue(0)
                                   );
        }
    }
}
