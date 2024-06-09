using System.Collections.ObjectModel;
using System.Reflection;

namespace Pathfinder.Util;

public class AssemblyAssociatedList<T>
{
    private Dictionary<Assembly, List<T>> asmDictionary = new Dictionary<Assembly, List<T>>();
    private ReadOnlyCollection<T> _allItems = null;
    public ReadOnlyCollection<T> AllItems {
        get {
             if(_allItems == null)
                  RefreshCache();
             return _allItems;
        }
    }

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
            asmDictionary[owner] = [];
        asmDictionary[owner].Add(val);

        InvalidateCache();
    }

    public void Remove(T val, Assembly owner)
    {
        if (asmDictionary.TryGetValue(owner, out var list))
        {
            list.Remove(val);
            InvalidateCache();
        }
    }

    public void RemoveAll(Predicate<T> predicate, Assembly owner)
    {
        if (asmDictionary.TryGetValue(owner, out var list))
        {
            list.RemoveAll(predicate);
            InvalidateCache();
        }
    }

    public bool RemoveAssembly(Assembly asm, out List<T> removed)
    {
        asmDictionary.TryGetValue(asm, out removed);
        if (asmDictionary.Remove(asm))
        {
            InvalidateCache();
            return true;
        }
        return false;
    }

    private void InvalidateCache()
    {
        _allItems = null;
    }

    private void RefreshCache()
    {
        var allItems = new List<T>();
        foreach (var list in asmDictionary.Values)
        {
            allItems.AddRange(list);
        }
        _allItems = new ReadOnlyCollection<T>(allItems);
    }
}
