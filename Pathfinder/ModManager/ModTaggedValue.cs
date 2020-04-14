using System;
using Pathfinder.Util;

namespace Pathfinder.ModManager
{
    public class ModTaggedValue<T>
    {

        public ModTaggedValue(T value, bool overrideable = false)
        {
            ModId = Utility.ActiveModId;
            Value = value;
            IsOverrideable = overrideable;
        }

        public bool AddedByCurrent => ModId == Utility.ActiveModId;

        public string ModId { get; private set; }
        public T Value { get; private set; }
        public bool IsOverrideable { get; private set; }

        public static explicit operator T(ModTaggedValue<T> v) => v.Value;
    }
}
