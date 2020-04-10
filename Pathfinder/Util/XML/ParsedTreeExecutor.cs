using System;
using System.Linq;
using System.Collections.Generic;

namespace Pathfinder.Util.XML {
    public class ParsedTreeExecutor : IExecutor {

        internal Dictionary<string, Tuple<ReadExecution, bool>> delegateData = new Dictionary<string, Tuple<ReadExecution, bool>>();

        public bool IgnoreCase { get; private set; }
        public bool StopAtRoot { get; private set; }

        public ParsedTreeExecutor(bool ignoreCase = false, bool stopAtRoot = false)
        {
            IgnoreCase = ignoreCase;
            StopAtRoot = stopAtRoot;
        }

        public void AddExecutor(string element, ReadExecution execution, bool parseChildren = false)
            => delegateData.Add(GetElementName(element), new Tuple<ReadExecution, bool>(execution, parseChildren));

        public void RemoveExecutor(string element)
            => delegateData.Remove(GetElementName(element));

        public string GetElementName(string element)
            => IgnoreCase ? element.ToLowerInvariant() : element;

        public bool HasElement(string element)
            => delegateData.ContainsKey(GetElementName(element));

        private bool tryGetExecutor(ElementInfo element, ElementInfo root, out Tuple<ReadExecution, bool> result) {
            var currentElement = element;
            var checkName = currentElement.Name;
            while(true) {
                /* try to find an executor given the current name */
                if(delegateData.TryGetValue(checkName, out result)) {
                    /* found it! */
                    return true;
                }
                if(
                    /* hit root of tree */
                    currentElement.Parent == null ||
                    /* hit root of execution */
                    (StopAtRoot && currentElement == root)
                )
                {
                    return false;
                }
                /* move to parent, try again */
                currentElement = currentElement.Parent;
                checkName = currentElement.Name + "." + checkName;
            }
        }

        private class StackElement
        {
            public StackElement(ElementInfo element)
            {
                Element = element;
                CurrentIndex = 0;
            }
            public ElementInfo Element;
            public int CurrentIndex;
        }

        public void Execute(ElementInfo root)
        {
            var elementStack = new Stack<StackElement>();
            elementStack.Push(new StackElement(root));

            while(true) {
                if(elementStack.Count == 0) {
                    break;
                }

                var top = elementStack.Peek();
                var currentElement = top.Element;

                if(currentElement.Children.Count == top.CurrentIndex)
                {
                    /* Element exhausted, go to parent */
                    elementStack.Pop();
                    continue;
                }

                /* Get current child (and move to next child when we come back here) */
                var child = currentElement.Children[top.CurrentIndex++];
                if(tryGetExecutor(child, root, out var info)) {
                    /* run executor */
                    info.Item1(this, child);
                    if(info.Item2) { /* parseChildren == "run executor, and no children" */
                        continue;
                    }
                }

                /* Add child to stack */
                elementStack.Push(new StackElement(child));
            }
        }
    }
}
