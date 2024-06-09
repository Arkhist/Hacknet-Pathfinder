using System.Xml;

namespace Pathfinder.Util.XML;

public delegate void ReadExecution(EventExecutor exec, ElementInfo info);

[Flags]
public enum ParseOption
{
    None = 0,
    ParseInterior = 0b1,
    FireOnEnd = 0b10,
    DontAllowOthers = 0b100
}

public class EventExecutor : EventReader
{
    private struct ExecutorHolder
    {
        public ReadExecution Executor;
        public ParseOption Options;
    }

    private struct ExecutorState
    {
        public XmlReader Reader;
        public string Text;
        public Dictionary<string, List<ExecutorHolder>> Temps;
        public Dictionary<string, List<ExecutorHolder>> AllExecs;
        public List<ReadExecution> CurrentExecs;
        public Stack<ElementInfo> ElementStack;
        public List<string> ParentNames;
    }

    private readonly Stack<ExecutorState> stateStack = new Stack<ExecutorState>();

    private readonly Dictionary<string, List<ExecutorHolder>> Executors = new Dictionary<string, List<ExecutorHolder>>();

    private Dictionary<string, List<ExecutorHolder>> TemporaryExecutors =
        new Dictionary<string, List<ExecutorHolder>>();

    private Dictionary<string, List<ExecutorHolder>> _allExecs = null;
        
    private Dictionary<string, List<ExecutorHolder>> AllExecutors
    {
        get
        {
            if (_allExecs != null)
                return _allExecs;
                
            var ret = new Dictionary<string, List<ExecutorHolder>>();
            foreach (var exec in Executors)
            {
                ret[exec.Key] = [..exec.Value];
            }

            foreach (var temp in TemporaryExecutors)
            {
                if (ret.ContainsKey(temp.Key))
                    ret[temp.Key].AddRange(temp.Value);
                else
                    ret[temp.Key] = [..temp.Value];
            }

            _allExecs = ret;
            return ret;
        }
    }

    private Stack<ElementInfo> currentElementStack = new Stack<ElementInfo>();
    private List<ReadExecution> currentExecutors;
        
    public EventExecutor() {}

    public EventExecutor(string text, bool isPath) : base(text, isPath)
    {
    }

    public EventExecutor(XmlReader rdr) : base(rdr)
    {
    }

    public void SaveState()
    {
        stateStack.Push(new ExecutorState
        {
            Reader = Reader,
            Text = Text,
            Temps = TemporaryExecutors,
            AllExecs = _allExecs,
            CurrentExecs = currentExecutors,
            ElementStack = currentElementStack,
            ParentNames = ParentNames
        });

        Reader = null;
        Text = null;
        TemporaryExecutors = new Dictionary<string, List<ExecutorHolder>>();
        _allExecs = null;
        currentExecutors = [];
        currentElementStack = new Stack<ElementInfo>();
        ParentNames = [];
    }

    public void PopState()
    {
        var state = stateStack.Pop();
        Reader = state.Reader;
        Text = state.Text;
        TemporaryExecutors = state.Temps;
        _allExecs = state.AllExecs;
        currentExecutors = state.CurrentExecs;
        currentElementStack = state.ElementStack;
        ParentNames = state.ParentNames;
    }

    public void RegisterExecutor(string element, ReadExecution executor, ParseOption options = ParseOption.None)
    {
        if (!Executors.ContainsKey(element))
            Executors.Add(element, []);
        if (!TemporaryExecutors.ContainsKey(element))
            TemporaryExecutors.Add(element, []);
            
        if (Executors[element].Any(x => (x.Options & ParseOption.DontAllowOthers) != 0) ||
            TemporaryExecutors[element].Any(x => (x.Options & ParseOption.DontAllowOthers) != 0))
        {
            return;
        }

        if ((options & ParseOption.DontAllowOthers) != 0)
        {
            Executors[element].Clear();
            TemporaryExecutors[element].Clear();
        }

        Executors[element].Add(new ExecutorHolder {Executor = executor, Options = options});
    }

    public void RegisterTempExecutor(string element, ReadExecution executor, ParseOption options = ParseOption.None)
    {
        if (!TemporaryExecutors.ContainsKey(element))
            TemporaryExecutors.Add(element, []);
        if (!Executors.ContainsKey(element))
            Executors.Add(element, []);
            
        if (TemporaryExecutors[element].Any(x => (x.Options & ParseOption.DontAllowOthers) != 0) ||
            Executors[element].Any(x => (x.Options & ParseOption.DontAllowOthers) != 0))
        {
            return;
        }

        if ((options & ParseOption.DontAllowOthers) != 0)
        {
            TemporaryExecutors[element].Clear();
        }

        TemporaryExecutors[element].Add(new ExecutorHolder {Executor = executor, Options = options});
    }

#pragma warning disable 618
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

                var interiors = executors.Where(x => (x.Options & ParseOption.ParseInterior) != 0)
                    .Select(x => x.Executor).ToList();

                if (interiors.Count > 0)
                {
                    element.Content = null;
                    currentElementStack.Push(element);
                    currentExecutors = interiors;
                }

                foreach (var executor in executors)
                {
                    if ((executor.Options & ParseOption.ParseInterior) == 0 &&
                        (executor.Options & ParseOption.FireOnEnd) == 0)
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
                Parent = topElement,
                Content = null
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
            if (!Reader.IsEmptyElement && top.Children.Count == 0 && top.Content == null)
                top.Content = "";
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

    protected override void EndRead()
    {
        _allExecs = null;
        TemporaryExecutors.Clear();
    }

    private bool FindExecutors(string name, out List<ExecutorHolder> executors)
    {
        while (true)
        {
            if (AllExecutors.TryGetValue(name, out executors))
            {
                if (executors.Any(x => (x.Options & ParseOption.DontAllowOthers) != 0))
                    executors = executors.Where(x => (x.Options & ParseOption.ParseInterior) != 0).Take(1).ToList();
                return true;
            }

            if (!name.Contains("."))
            {
                if (AllExecutors.TryGetValue("*", out executors))
                {
                    if (executors.Any(x => (x.Options & ParseOption.DontAllowOthers) != 0))
                        executors = executors.Where(x => (x.Options & ParseOption.ParseInterior) != 0).Take(1).ToList();
                    return true;
                }

                return false;
            }

            name = name.Substring(name.IndexOf('.') + 1);
        }
    }
}