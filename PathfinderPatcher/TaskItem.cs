using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Inject;
using static PathfinderPatcher.AccessLevel;

namespace PathfinderPatcher
{
    public abstract class GenericTypeTaskItem
    {
        protected GenericTypeTaskItem(string typeName, AccessLevel? targetLevel = null)
        {
            this.typeName = typeName;
            this.targetLevel = targetLevel;
        }

        public string typeName { get; }
        protected AccessLevel? targetLevel;

        public void addFieldModification(string name, AccessLevel newLevel)
        {
            fieldModifications[name] = newLevel;
        }

        public void addMethodModification(string name, AccessLevel newLevel)
        {
            methodModifications[name] = newLevel;
        }

        public void addTypeModification(GenericTypeTaskItem modification)
        {
            typeModifications.Add(modification.typeName, modification);
        }

        public AccessLevel? newDefaultFieldAccess = null;
        public AccessLevel? newDefaultMethodAccess = null;
        public AccessLevel? newDefaultTypeAccess = null;

        private readonly Dictionary<string, AccessLevel> fieldModifications = new Dictionary<string, AccessLevel>();
        private readonly Dictionary<string, AccessLevel> methodModifications = new Dictionary<string, AccessLevel>();

        private readonly Dictionary<string, GenericTypeTaskItem> typeModifications =
            new Dictionary<string, GenericTypeTaskItem>();


        protected void execute(TypeDefinition target)
        {
            if (targetLevel.HasValue)
                applyTypeAccess(target);
            if (newDefaultFieldAccess.HasValue)
            {
                foreach (FieldDefinition field in target.Fields)
                {
                    if (!fieldModifications.TryGetValue(field.Name, out AccessLevel newLevel))
                        newLevel = newDefaultFieldAccess.Value;
                    field.MakeFieldAccess(newLevel);
                }
            } else
            {
                foreach ((string fieldName, AccessLevel newLevel) in fieldModifications)
                {
                    target.GetField(fieldName).MakeFieldAccess(newLevel);
                }
            }

            if (newDefaultMethodAccess.HasValue)
            {
                foreach (MethodDefinition method in target.Methods)
                {
                    if (!methodModifications.TryGetValue(method.Name, out AccessLevel newLevel))
                        newLevel = newDefaultMethodAccess.Value;
                    method.MakeMethodAccess(newLevel);
                }
            } else
            {
                foreach ((string methodName, AccessLevel newLevel) in methodModifications)
                {
                    target.GetMethod(methodName).MakeMethodAccess(newLevel);
                }
            }

            if (!newDefaultTypeAccess.HasValue && typeModifications.Count <= 0) return;
            foreach (TypeDefinition type in target.NestedTypes)
            {
                if (typeModifications.TryGetValue(type.Name, out GenericTypeTaskItem nestedTask))
                {
                    if (!nestedTask.targetLevel.HasValue && newDefaultTypeAccess.HasValue)
                        type.MakeNestedAccess(newDefaultTypeAccess.Value);
                    nestedTask.execute(type);
                } else if (newDefaultTypeAccess.HasValue)
                    type.MakeNestedAccess(newDefaultTypeAccess.Value);
            }
        }

        protected abstract void applyTypeAccess(TypeDefinition target);

        protected virtual string getToStringPrefix()
        {
            return "Task: At Type ";
        }

        public override string ToString()
        {
            bool Sub1<T>(StringBuilder dst, T? nullable, string prefix) where T : struct
            {
                if (!nullable.HasValue) return false;
                dst.Append(prefix);
                dst.Append(nullable.Value);
                return true;
            }

            StringBuilder taskStr = new StringBuilder(getToStringPrefix())
                .Append(typeName);
            bool written = false;
            written |= Sub1(taskStr, newDefaultFieldAccess, ",\n\tset all fields to ");
            written |= Sub1(taskStr, newDefaultMethodAccess, ",\n\tset all methods to ");
            written |= Sub1(taskStr, newDefaultTypeAccess, ",\n\tset all nested types to ");
            if (fieldModifications.Count + methodModifications.Count > 0)
            {
                taskStr.AppendLine(written ? "\nand set" : ", set");
                written = true;
                foreach ((string fieldName, AccessLevel newLevel) in fieldModifications)
                {
                    taskStr.Append("\tthe field `").Append(fieldName).Append("` to ").AppendLine(newLevel.ToString());
                }

                foreach ((string methodName, AccessLevel newLevel) in methodModifications)
                {
                    taskStr.Append("\tthe method `").Append(methodName).Append("` to ").AppendLine(newLevel.ToString());
                }
            }

            if (typeModifications.Count > 0)
            {
                if (written)
                    taskStr.Append(".\nAdditionally, ");
                taskStr.Append("modify nested types as follows: ");
                foreach ((string _, GenericTypeTaskItem nestedTask) in typeModifications)
                {
                    taskStr.Append(nestedTask);
                }
            }

            return taskStr.ToString();
        }
    }

    public class TypeTaskItem : GenericTypeTaskItem
    {
        public TypeTaskItem(string typeName, AccessLevel? targetLevel = null) : base(typeName, targetLevel)
        {
            if (!targetLevel.HasValue) return;
            if (targetLevel.Value != Public && targetLevel.Value != Internal)
            {
                throw new ArgumentOutOfRangeException(nameof(targetLevel), targetLevel,
                    "For a non-nested type, the only valid access levels are \"public\" and \"internal\"."
                );
            }
        }

        protected override void applyTypeAccess(TypeDefinition target)
        {
            Debug.Assert(targetLevel.HasValue);
            switch (targetLevel.Value)
            {
                case Public:
                    target.IsPublic = true;
                    break;
                case Internal:
                    target.IsNotPublic = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void execute(ModuleDefinition target)
        {
            var targetDef = target.GetType(typeName);
            if(targetDef == null)
                throw new Exception($"Could not execute task: No such type: {typeName}");
            execute(targetDef);
        }
    }


    public class NestedTypeTaskItem : GenericTypeTaskItem
    {
        public NestedTypeTaskItem(string typeName, AccessLevel? targetLevel = null) : base(typeName, targetLevel) { }

        protected override void applyTypeAccess(TypeDefinition target)
        {
            Debug.Assert(targetLevel.HasValue);
            target.MakeNestedAccess(targetLevel.Value);
        }

        protected override string getToStringPrefix()
        {
            return "For the nested type ";
        }
    }
}
