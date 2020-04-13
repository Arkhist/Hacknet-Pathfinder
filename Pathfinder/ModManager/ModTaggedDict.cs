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

        public class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, ModTaggedValue<TValue>>> Backend;
            private ModTaggedDict<TKey, TValue> Dict;
            internal Enumerator(ModTaggedDict<TKey, TValue> dict)
            {
                Dict = dict;
                Backend = dict.Backend.GetEnumerator();
            }

            public bool MoveNext()
            {
                ModTaggedValue<TValue> newValue;
                do {
                    if(!Backend.MoveNext())
                        return false;
                    newValue = Backend.Current.Value;
                } while(!Manager.LoadedMods.ContainsKey(newValue.ModId));
                return true;
            }

            public void Reset()
            {
                Backend.Reset();
            }

            public void Dispose()
            {
                Backend.Dispose();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get => new KeyValuePair<TKey, TValue>(Backend.Current.Key, Backend.Current.Value.Value);
            }

            object IEnumerator.Current
            {
                get => Current;
            }
        }

        private Dictionary<TKey, ModTaggedValue<TValue>> Backend = new Dictionary<TKey, ModTaggedValue<TValue>>();
        private List<Enumerator> Enumerators = new List<Enumerator>();

        private void VerifyNotUnloaded(TKey key)
        {
            if(!Backend.TryGetValue(key, out var existing)) return;
            if(!Manager.LoadedMods.ContainsKey(existing.ModId))
            {
                Backend.Remove(key);
            }
        }

        private void VerifyPermissible(TKey key, string op)
        {
            VerifyNotUnloaded(key);
            if(!Backend.TryGetValue(key, out var existing)) return;

            if(existing.ModId != Utility.ActiveModId && !existing.IsOverrideable)
                throw new ModConflictException(existing.ModId, $"Cannot {op}: Key '{key}' is already in use.");
        }

        public void Add(TKey key, TValue value)
        {
            VerifyPermissible(key, "add");
            Backend.Add(key, new ModTaggedValue<TValue>(value));
        }

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            VerifyPermissible(pair.Key, "add");
            Backend.Add(pair.Key, new ModTaggedValue<TValue>(pair.Value));
        }

        public void AddOverrideable(TKey key, TValue value)
        {
            VerifyNotUnloaded(key);
            if(!Backend.ContainsKey(key))
            {
                Backend.Add(key, new ModTaggedValue<TValue>(value, true));
            }
        }

        public void AddOverrideable(KeyValuePair<TKey, TValue> pair)
        {
            VerifyNotUnloaded(pair.Key);
            if(!Backend.ContainsKey(pair.Key))
            {
                Backend.Add(pair.Key, new ModTaggedValue<TValue>(pair.Value, true));
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            if(!Backend.TryGetValue(pair.Key, out var existing)) return false;
            return EqualityComparer<TValue>.Default.Equals(existing.Value, pair.Value);
        }

        public bool ContainsKey(TKey key)
        {
            VerifyNotUnloaded(key);
            return Backend.ContainsKey(key);
        }

        public void Clear()
        {
            throw new NotSupportedException("This collection cannot fulfill both contracts on this method.");
        }

        public int Count { get => Backend.Count; }

        public void CopyTo(KeyValuePair<TKey, TValue>[] target, int startIdx)
        {
            throw new NotSupportedException("Please tell me *why* you would need this.");
        }

        public bool IsReadOnly => false;

        public bool TryGetValue(TKey key, out TValue value)
        {
            VerifyNotUnloaded(key);
            if(Backend.TryGetValue(key, out var existing))
            {
                value = existing.Value;
                return true;
            }
            value = default;
            return false;
        }

        public ICollection<TKey> Keys { get => Backend.Keys; }
        public ICollection<TValue> Values { get => Backend.Values.Select(v => v.Value).ToList(); }

        public bool Remove(TKey key)
        {
            if(!Backend.TryGetValue(key, out var existing))
                return false;
            if(existing.ModId != Utility.ActiveModId)
                return false;
            return Backend.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            if(!Backend.TryGetValue(pair.Key, out var existing))
                return false;
            if(existing.ModId != Utility.ActiveModId)
                return false;
            if(!EqualityComparer<TValue>.Default.Equals(existing.Value, pair.Value))
                return false;
            return Backend.Remove(pair.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var enumerator = new Enumerator(this);
            Enumerators.Add(enumerator);
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get => Backend[key].Value;
            set
            {
                VerifyPermissible(key, "set value");
                Backend[key] = new ModTaggedValue<TValue>(value);
            }
        }
    }
}
