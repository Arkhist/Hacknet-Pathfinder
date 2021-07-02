using System.Collections.Generic;

namespace Pathfinder.Util.XML
{
    public class EventExecutor : EventReader, IExecutor
    {
        private struct ExecutorHolder
        {
            public ReadExecution Executor;
            public ParseOption Options;
        }

        private Dictionary<string, ExecutorHolder> Executors = new Dictionary<string, ExecutorHolder>();

        private Stack<ElementInfo> currentElementStack = new Stack<ElementInfo>();
        private ReadExecution currentExecutor;

        public EventExecutor(string text, bool isPath) : base(text, isPath) {}
        
        public void RegisterExecutor(string element, ReadExecution executor, ParseOption options)
        {
            Executors.Add(element, new ExecutorHolder { Executor = executor, Options = options});
        }

        public void UnregisterExecutor(string element)
        {
            Executors.Remove(element);
        }

        protected override void ReadElement(Dictionary<string, string> attributes)
        {
            base.ReadElement(attributes);

            var namespaceString = string.Join(".", ParentNames);

            if (currentElementStack.Count == 0)
            {
                if (FindExecutor(namespaceString, out var executor))
                {
                    var element = new ElementInfo()
                    {
                        Name = Reader.Name,
                        Attributes = attributes
                    };

                    if ((executor.Options & ParseOption.ParseInterior) != 0)
                    {
                        currentElementStack.Push(element);
                        currentExecutor = executor.Executor;
                    }
                    else
                    {
                        executor.Executor(this, element);
                    }
                }
            }
            else
            {
                var topElement = currentElementStack.Peek();
                var element = new ElementInfo()
                {
                    Name = Reader.Name,
                    Attributes = attributes,
                    Parent = topElement
                };
                topElement.Children.Add(element);
                currentElementStack.Push(element);
            }
        }

        protected override void ReadEndElement()
        {
            base.ReadEndElement();

            if (currentElementStack.Count != 0)
            {
                var top = currentElementStack.Pop();
                if (currentElementStack.Count == 0)
                    currentExecutor(this, top);
            }
        }

        protected override void ReadText()
        {
            base.ReadText();

            if (currentElementStack.Count != 0)
            {
                var topElement = currentElementStack.Peek();
                if (topElement.Content == null)
                    topElement.Content = Reader.Value;
                else
                    topElement.Content += "\n" + Reader.Value;
            }
        }

        private bool FindExecutor(string name, out ExecutorHolder executor)
        {
            while (true)
            {
                if (Executors.TryGetValue(name, out executor))
                {
                    return true;
                }

                if (!name.Contains("."))
                {
                    executor = default;
                    return false;
                }

                name = name.Substring(name.IndexOf('.') + 1);
            }
        }
    }
}
