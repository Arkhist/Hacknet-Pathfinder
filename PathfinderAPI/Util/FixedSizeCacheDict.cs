using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Util
{
    public class FixedSizeCacheDict<K, V>
    {
        public readonly int Max;
        private readonly LinkedList<V> BackingList = new LinkedList<V>();
        private readonly Dictionary<K, LinkedListNode<V>> FastAccessDict = new Dictionary<K, LinkedListNode<V>>();
        
        public FixedSizeCacheDict(int maxSize)
        {
            Max = maxSize;
        }

        public void Register(K key, V item)
        {
            if (BackingList.Count == Max)
            {
                var old = BackingList.Last;
                FastAccessDict.Remove(FastAccessDict.First(x => object.ReferenceEquals(old, x.Value)).Key);
                BackingList.RemoveLast();
                
                if (old.Value is IDisposable dispose)
                    dispose.Dispose();
            }

            FastAccessDict.Add(key, BackingList.AddFirst(item));
        }

        public bool TryGetCached(K key, out V val)
        {
            if (FastAccessDict.TryGetValue(key, out LinkedListNode<V> node))
            {
                BackingList.Remove(node);
                BackingList.AddFirst(node);

                val = node.Value;
                return true;
            }

            val = default;
            return false;
        }
    }
}
