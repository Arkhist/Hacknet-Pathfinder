using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinder.Util
{
    public class CmdArguments : IEnumerable<KeyValuePair<string, string>>, ICollection<KeyValuePair<string, string>>
    {
        private string[] arguments;
        private readonly KeyValuePair<string, string>[] namedArguments;

        public int Count => namedArguments.Length;
        public bool IsReadOnly => true;

        public KeyValuePair<string, string> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException(nameof(index));
                return namedArguments[index];
            }
        }

        public string this[string key]
        {
            get
            {
                foreach (var pair in this)
                    if (key == pair.Key) return pair.Value;
                return null;
            }
        }

        public string GetArgumentOrNull(int index)
            => index < 0 || index >= arguments.Length ? null : arguments[index];

        public List<KeyValuePair<string, string>> GetArgumentPairs()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string key = null;
            foreach(var arg in arguments)
            {
                if(key != null)
                {
                    result.Add(
                        new KeyValuePair<string, string>(
                            key,
                            arg.StartsWith("-", StringComparison.Ordinal)
                                ? null
                                : arg));
                    key = null;
                }
                if (!arg.StartsWith("-", StringComparison.Ordinal)) continue;
                var a = arg.Remove(0, 1);
                var argsSplit = a.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                switch(argsSplit.Length)
                {
                    case 1:
                        key = argsSplit[0];
                        break;
                    case 2:
                        result.Add(new KeyValuePair<string, string>(argsSplit[0], argsSplit[1]));
                        break;
                }
            }
            return result;
        }

        public List<string> GetAllArgumentsWithName(string key)
        {
            List<string> result = new List<string>();
            foreach (var pair in this)
                if (pair.Key == key) result.Add(pair.Value);
            return result;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Add(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        public bool Contains(KeyValuePair<string, string> item)
        {
            foreach (var pair in this)
                if (pair.Key == item.Key && pair.Value == item.Value) return true;
            return false;
        }

        public bool ContainsKey(string key)
        {
            foreach (var pair in this)
                if (pair.Key == key) return true;
            return false;
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            => namedArguments.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        public void Clear()
            => throw new NotSupportedException();

        public CmdArguments(string[] args)
        {
            arguments = args;
            namedArguments = GetArgumentPairs().ToArray();
        }

        public static implicit operator CmdArguments(string[] args)
            => new CmdArguments(args);
    }
}
