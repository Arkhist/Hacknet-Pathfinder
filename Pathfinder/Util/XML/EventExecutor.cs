using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Util.XML
{
    public delegate void ReadExecution(EventExecutor executor, ElementInfo info);

    public class EventExecutor : EventReader
    {
        private Dictionary<string, Tuple<ReadExecution, bool>> delegateData = new Dictionary<string, Tuple<ReadExecution, bool>>();

        public bool IgnoreCase { get; private set; }

        public EventExecutor(string str, bool isPath = true, bool ignoreCase = false) : base(str, isPath)
        {
            IgnoreCase = ignoreCase;
        }

        public ReadExecution this[string element]
        {
            get => GetExecutor(element);
            set
            {
                if (value == null)
                {
                    RemoveExecutor(element);
                    return;
                }
                SetExecutionData(element, value);
            }
        }

        public bool CanParseChildrenOf(string element)
            => HasElement(element) && delegateData[GetElementName(element)].Item2;

        public bool TryGetExecutionData(string element, out Tuple<ReadExecution, bool> tuple)
            => delegateData.TryGetValue(GetElementName(element), out tuple);

        public bool TryGetExecutor(string element, out ReadExecution e)
        {
            var r = TryGetExecutionData(element, out var t);
            e = t?.Item1;
            return r;
        }

        public Tuple<ReadExecution, bool> GetExecutionData(string element)
            => TryGetExecutionData(element, out var t) ? t : null;

        public ReadExecution GetExecutor(string element)
            => GetExecutionData(element)?.Item1;

        public void SetExecutionData(string element, ReadExecution execution = null, bool? parseChildren = null)
        {
            if(execution == null)
                execution = delegateData[GetElementName(element)].Item1;
            if (parseChildren == null && delegateData.TryGetValue(GetElementName(element), out var pc))
                parseChildren = pc.Item2;
            delegateData[GetElementName(element)] = new Tuple<ReadExecution, bool>(execution, parseChildren.GetValueOrDefault());
        }

        public void AddExecutor(string element, ReadExecution execution, bool parseChildren = false)
            => delegateData.Add(GetElementName(element), new Tuple<ReadExecution, bool>(execution, parseChildren));

        public void RemoveExecutor(string element)
            => delegateData.Remove(GetElementName(element));

        public string GetElementName(string element)
            => IgnoreCase ? element.ToLowerInvariant() : element;

        public bool HasElement(string element)
            => delegateData.ContainsKey(GetElementName(element));

        private ElementInfo topLevelInfo = new ElementInfo();
        private ElementInfo currentElement;
        private int counter = 0;

        protected override void ReadElement(IDictionary<string, string> attributes)
        {
            base.ReadElement(attributes);
            if (currentElement != null)
            {
                // Not marked to parse children
                if (topLevelInfo.Children == null) return;

                currentElement.Children.Add(new ElementInfo
                {
                    Name = reader.Name,
                    Value = reader.Value,
                    Depth = reader.Depth,
                    Parent = currentElement,
                    LocalNodeId = counter++,
                    Attributes = attributes,
                    Children = new List<ElementInfo>()
                });
                currentElement = currentElement.Children.Last();

                return; // Skip Descendents if executing
            }

            var checkName = reader.Name;
            if (IgnoreCase) checkName = checkName.ToLowerInvariant();

            // If element name isn't contained, check entire parent list
            if (!HasElement(checkName))
            {
                var parentStrTest = new StringBuilder(checkName);
                var parentTestFail = true;
                for (int i = ParentList.Count - 1; i >= 0; i--)
                {
                    parentStrTest.Insert(0, $"{ParentList[i]}.");
                    if (HasElement(parentStrTest.ToString()))
                    {
                        parentTestFail = false;
                        break;
                    }
                }
                if (parentTestFail) return;
            }

            counter = 0;

            topLevelInfo.Name = reader.Name;
            topLevelInfo.Value = reader.Value;
            topLevelInfo.Depth = reader.Depth;
            topLevelInfo.LocalNodeId = counter++;
            topLevelInfo.Parent = new ElementInfo
            {
                Name = string.Join(".", ActiveParents),
                Value = string.Empty,
                Parent = null,
                LocalNodeId = -1,
                Attributes = new Dictionary<string, string>(),
                Children = new List<ElementInfo>() { topLevelInfo }
            };
            topLevelInfo.Attributes = attributes;
            topLevelInfo.Children = CanParseChildrenOf(checkName) ? new List<ElementInfo>() : null;
            currentElement = topLevelInfo;
        }

        protected override void ReadEndElement()
        {
            base.ReadEndElement();

            // If element exists and represents the active node, traverse up the tree.
            // Execute if top executionlevel
            if (currentElement?.RepresentsNode(reader) == true)
            {
                if (currentElement == topLevelInfo && TryGetExecutor(reader.Name, out var exec))
                    exec(this, topLevelInfo);
                currentElement = currentElement.Parent;
            }
        }

        protected override void ReadText()
        {
            base.ReadText();

            // If element is being parsed and contains text, assign text to elements
            if(currentElement != null)
                currentElement.Value += (currentElement.Value == string.Empty ? "" : "\n") + reader.Value;
        }
    }
}
