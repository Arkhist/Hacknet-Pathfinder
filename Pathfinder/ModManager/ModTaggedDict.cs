using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Pathfinder.Util;
using Pathfinder.Exceptions;

namespace Pathfinder.ModManager
{
    public class ModTaggedDict<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private static void RemoveUnloaded(List<ModTaggedValue<TValue>> list)
        {
            list.RemoveAll(e => Manager.LoadedMods.ContainsKey(e.ModId));
        }

        private class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {

            private readonly IEnumerator<KeyValuePair<TKey, List<ModTaggedValue<TValue>>>> Backend;
            internal Enumerator(ModTaggedDict<TKey, TValue> dict)
            {
                Backend = dict.Backend.GetEnumerator();
            }

            public bool MoveNext()
            {
                List<ModTaggedValue<TValue>> list;
                do
                {
                    if (!Backend.MoveNext())
                        return false;
                    list = Backend.Current.Value;
                    RemoveUnloaded(list);
                } while (list.IsEmpty());
                return true;
            }

            public void Reset()
             => Backend.Reset();

            public void Dispose()
             => Backend.Dispose();

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(Backend.Current.Key, Backend.Current.Value.Last().Value);

            object IEnumerator.Current => Current;
        }

        private Dictionary<TKey, List<ModTaggedValue<TValue>>> Backend = new Dictionary<TKey, List<ModTaggedValue<TValue>>>();

        private List<ModTaggedValue<TValue>> GetNormalized(TKey key)
        {
            if (Backend.TryGetValue(key, out var list))
                RemoveUnloaded(list);
            else
            {
                list = new List<ModTaggedValue<TValue>>();
                Backend.Add(key, list);
            }
            return list;
        }

        private Exception MakeConflictEx(TKey key, string op)
        {
            return new ModConflictException(GetNormalized(key).Last().ModId, $"Cannot {op}: Key '{key}' is already in use.");
        }

        private bool TryAdd(TKey key, TValue value, bool forOverride = false)
        {
            var list = GetNormalized(key);
            if (list.Any(v => v.AddedByCurrent))
                return false;
            var existing = list.LastOrNull();
            if (existing != null)
            {
                if (!(existing.AddedByCurrent || existing.IsOverrideable))
                    return false;
            }
            list.Add(new ModTaggedValue<TValue>(value, forOverride));
            return true;
        }

        public void Add(TKey key, TValue value)
        {
            if(!TryAdd(key, value))
                throw MakeConflictEx(key, "add");
        }

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            if(!TryAdd(pair.Key, pair.Value))
                throw MakeConflictEx(pair.Key, "add");
        }

        public void AddOverrideable(TKey key, TValue value)
        {
            TryAdd(key, value, true);
        }

        public void AddOverrideable(KeyValuePair<TKey, TValue> pair)
        {
            TryAdd(pair.Key, pair.Value, true);
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            var existing = GetNormalized(pair.Key);
            return existing.Any(v => EqualityComparer<TValue>.Default.Equals(v.Value, pair.Value));
        }

        public bool ContainsKey(TKey key)
        {
            return GetNormalized(key).LastOrNull() != null;
        }

        public void Clear()
        {
            throw new NotSupportedException("This collection cannot fulfill both contracts on this method.");
        }

        public int Count => Backend.Count(e => e.Value.Count > 0);

        public void CopyTo(KeyValuePair<TKey, TValue>[] target, int startIdx)
        {
            throw new NotSupportedException("Please tell me *why* you would need this.");
        }

        public bool IsReadOnly => false;

        public bool TryGetValue(TKey key, out TValue value)
        {
            var existing = GetNormalized(key).LastOrNull();
            value = existing != null ? existing.Value : default;
            return existing != null;
        }

        public ICollection<TKey> Keys => Backend.Keys.Where(v => !Backend[v].IsEmpty()).ToArray();

        public ICollection<TValue> Values =>
            Backend.Values.Where(v => !v.IsEmpty())
                .Select(v => v.Last().Value).ToList();

        public bool Remove(TKey key)
        {
            var list = GetNormalized(key);
            var existing = list.Find(v => v.AddedByCurrent);
            return existing != null && list.Remove(existing);
        }

        public bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            var list = GetNormalized(pair.Key);
            var existing = list.Find(v =>
                v.AddedByCurrent && EqualityComparer<TValue>.Default.Equals(v.Value, pair.Value));
            return existing != null && list.Remove(existing);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var enumerator = new Enumerator(this);
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get
            {
                var rawValue = Backend[key].LastOrNull();
                if (rawValue == null) throw new KeyNotFoundException();
                return rawValue.Value;
            }
            set
            {
                var list = GetNormalized(key);
                var index = list.FindIndex(v => v.AddedByCurrent);
                if (index != -1)
                {
                    list[index] = new ModTaggedValue<TValue>(value);
                }
                else if (TryAdd(key, value))
                    throw MakeConflictEx(key, "set value");
            }
        }
    }
}
