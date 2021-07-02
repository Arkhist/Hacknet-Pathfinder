using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Util.XML
{
    public class EventExecutor : EventReader, IExecutor
    {
        private struct ExecutorHolder
        {
            public ReadExecution Executor;
            public ParseOption Options;
        }

        private Dictionary<string, List<ExecutorHolder>> Executors = new Dictionary<string, List<ExecutorHolder>>();

        private Stack<ElementInfo> currentElementStack = new Stack<ElementInfo>();
        private List<ReadExecution> currentExecutors;

        public EventExecutor(string text, bool isPath) : base(text, isPath) {}
        
        public void RegisterExecutor(string element, ReadExecution executor, ParseOption options = ParseOption.None)
        {
            if (!Executors.TryGetValue(element, out _))
                Executors.Add(element, new List<ExecutorHolder>());
            Executors[element].Add(new ExecutorHolder { Executor = executor, Options = options });
        }

        public void UnregisterExecutors(string element)
        {
            Executors.Remove(element);
        }

        protected override void ReadElement(Dictionary<string, string> attributes)
        {
            base.ReadElement(attributes);

            if (currentElementStack.Count == 0)
            {
                if (FindExecutors(CurrentNamespace, out var executors))
                {
                    var element = new ElementInfo()
                    {
                        Name = Reader.Name,
                        Attributes = attributes
                    };

                    var interiors = executors.Where(x => (x.Options & ParseOption.ParseInterior) != 0).Select(x => x.Executor).ToList();

                    if (interiors.Count > 0)
                    {
                        currentElementStack.Push(element);
                        currentExecutors = interiors;
                    }

                    foreach (var executor in executors)
                    {
                        if ((executor.Options & ParseOption.ParseInterior) == 0 && (executor.Options & ParseOption.FireOnEnd) == 0)
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
                    foreach (var executor in currentExecutors)
                        executor(this, top);
            }

            if (FindExecutors(CurrentNamespace, out var executors))
            {
                foreach (var executor in executors)
                {
                    if ((executor.Options & ParseOption.FireOnEnd) != 0)
                    {
                        executor.Executor(this, null);
                    }
                }
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

        private bool FindExecutors(string name, out List<ExecutorHolder> executors)
        {
            while (true)
            {
                if (Executors.TryGetValue(name, out executors))
                {
                    return true;
                }

                if (!name.Contains("."))
                {
                    executors = new List<ExecutorHolder>();
                    return false;
                }

                name = name.Substring(name.IndexOf('.') + 1);
            }
        }
    }
}
