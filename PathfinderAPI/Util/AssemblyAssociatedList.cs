using System.Collections.ObjectModel;
using System.Reflection;

namespace Pathfinder.Util;

public class AssemblyAssociatedList<T>
{
    private Dictionary<Assembly, List<T>> asmDictionary = new Dictionary<Assembly, List<T>>();
    public ReadOnlyCollection<T> AllItems { get; private set; } = new ReadOnlyCollection<T>(new List<T>());

    public ReadOnlyCollection<T> this[Assembly assembly]
    {
        get
        {
            if(!asmDictionary.TryGetValue(assembly, out var list))
                return new ReadOnlyCollection<T>(new List<T>());
            return new ReadOnlyCollection<T>(list);
        }
    }

    public void Add(T val, Assembly owner)
    {
        if (!asmDictionary.ContainsKey(owner))
            asmDictionary[owner] = new List<T>();
        asmDictionary[owner].Add(val);

        RefreshCache();
    }

    public void Remove(T val, Assembly owner)
    {
        if (asmDictionary.TryGetValue(owner, out var list))
        {
            list.Remove(val);
            RefreshCache();
        }
    }

    public void RemoveAll(Predicate<T> predicate, Assembly owner)
    {
        if (asmDictionary.TryGetValue(owner, out var list))
        {
            list.RemoveAll(predicate);
            RefreshCache();
        }
    }

    public bool RemoveAssembly(Assembly asm, out List<T> removed)
    {
        asmDictionary.TryGetValue(asm, out removed);
        if (asmDictionary.Remove(asm))
        {
            RefreshCache();
            return true;
        }
        return false;
    }

    private void RefreshCache()
    {
        var allItems = new List<T>();
        foreach (var list in asmDictionary.Values)
        {
            allItems.AddRange(list);
        }
        AllItems = new ReadOnlyCollection<T>(allItems);
    }
}