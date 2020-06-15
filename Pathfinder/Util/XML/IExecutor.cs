using System;

namespace Pathfinder.Util.XML
{
    public delegate void ReadExecution(IExecutor executor, ElementInfo info);

    public interface IExecutor
    {
        void AddExecutor(string element, ReadExecution execution, bool parseChildren = false);
        void RemoveExecutor(string element);
    }
}
