using System;

namespace Pathfinder.Util.XML
{
    public delegate void ReadExecution(IExecutor executor, ElementInfo info);

    public interface IExecutor
    {
        public void AddExecutor(string element, ReadExecution execution, bool parseChildren = false);
        public void RemoveExecutor(string element);
    }
}
