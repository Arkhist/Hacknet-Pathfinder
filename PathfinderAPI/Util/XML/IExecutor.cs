using System;

namespace Pathfinder.Util.XML
{
    public delegate void ReadExecution(IExecutor exec, ElementInfo info);

    [Flags]
    public enum ParseOption
    {
        None = 0,
        ParseInterior = 0b1,
        FireOnEnd = 0b10,
        DontAllowOthers = 0b100
    }
    
    public interface IExecutor
    {
        void RegisterExecutor(string element, ReadExecution executor, ParseOption options);
        void UnregisterExecutors(string element);
    }
}
