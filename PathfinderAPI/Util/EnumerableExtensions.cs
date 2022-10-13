
namespace Pathfinder.Util;

public static class EnumerableExtensions
{
    public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct {
        using IEnumerator<T> iter = source.GetEnumerator();
        if(iter.MoveNext())
            return iter.Current;
        return null;
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct {
        foreach(T item in source) {
            if(predicate(item))
                return item;
        }
        return null;
    }
}
