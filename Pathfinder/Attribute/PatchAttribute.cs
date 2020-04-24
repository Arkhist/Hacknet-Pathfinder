using System;
using System.Reflection;
namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PatchAttribute : System.Attribute
    {
        [Flags]
        public enum InjectFlags
        {
            None = 0x0,
            PassTag = 0x1,
            PassInvokingInstance = 0x2,
            ModifyReturn = 0x4,
            PassLocals = 0x8,
            PassFields = 0x10,
            PassParametersVal = 0x20,
            PassParametersRef = 0x40,
            PassStringTag = 0x80,
            All_Val = 0x3E,
            All_Ref = 0x5E
        }

        public string MethodSig;
        public int Offset;
        public object Tag;
        public int Flags;
        public bool After;
        public int[] LocalIds;
        public string DependentSig;

        public PatchAttribute(string sig, int offset = 0, object tag = null, InjectFlags flags = 0, bool before = false, int[] localsID = null, string depSig = null)
        {
            MethodSig = sig;
            Offset = offset;
            Tag = tag;
            Flags = (int)flags;
            After = before;
            LocalIds = localsID;
            DependentSig = depSig;
        }
    }
}
