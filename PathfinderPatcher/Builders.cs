using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace PathfinderPatcher
{
    public class FieldBuilder
    {
        public TypeBuilder Parent { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public FieldAttributes Attributes { get; set; } = FieldAttributes.Public;

        public FieldDefinition Build(ModuleDefinition module) => new FieldDefinition(Name, Attributes, module.ImportReference(Type));
    }

    public class TypeBuilder
    {
        public ModuleDefinition Module { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public TypeAttributes Attributes { get; set; } = TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
        public Type BaseType { get; set; }
        public List<FieldBuilder> Fields { get; set; } = new List<FieldBuilder>();

        public TypeDefinition Build(ModuleDefinition module = null)
        {
            Module = Module ?? module;
            var res = new TypeDefinition(Namespace, Name, Attributes, Module.ImportReference(BaseType));
            foreach (var f in Fields) res.Fields.Add(f.Build(Module));
            return res;
        }
    }
}
