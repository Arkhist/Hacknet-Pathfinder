using System.Reflection;
using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load;

public abstract class BaseAttribute : Attribute
{
    internal protected abstract void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo);

    internal void ThrowOnInvalidOperation(bool evaluation, string message)
    {
        if(evaluation)
            throw new InvalidOperationException(message);
    }
}