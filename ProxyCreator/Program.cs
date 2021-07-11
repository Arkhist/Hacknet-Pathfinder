using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace ProxyCreator
{
    internal class Program
    {
        public static void Main()
        {
            using (var asm = AssemblyDefinition.ReadAssembly("FNA.dll"))
            {
                asm.MainModule.Resources.Clear();
                
                foreach (var method in asm.MainModule.Types.SelectMany(x => x.Methods))
                {
                    if (!method.HasBody)
                        continue;

                    method.Body = new MethodBody(method);
                    method.Body.GetILProcessor().Emit(OpCodes.Ret);

                    method.NoInlining = true;
                }
                
                var cctor = new MethodDefinition(".cctor",
                    MethodAttributes.RTSpecialName |
                    MethodAttributes.Static |
                    MethodAttributes.Public |
                    MethodAttributes.HideBySig,
                    asm.MainModule.TypeSystem.Void
                );

                using (var thisAsm = AssemblyDefinition.ReadAssembly(typeof(Program).Assembly.Location))
                {
                    cctor.Body = thisAsm.MainModule.GetType("ProxyCreator.Program").Methods.First(x => x.Name == nameof(LoadBepInExCopy)).Body;
                }
                
                foreach (var instruction in cctor.Body.Instructions)
                {
                    if (instruction.Operand is MethodReference reference)
                        instruction.Operand = asm.MainModule.ImportReference(reference);
                }
                foreach (var local in cctor.Body.Variables)
                {
                    local.VariableType = asm.MainModule.ImportReference(local.VariableType);
                }

                asm.MainModule.Types[0].Methods.Add(cctor);

                asm.Write("FNA-Proxy.dll");
            }
        }

        public static void LoadBepInExCopy()
        {
            var setup = new AppDomainSetup
            {
                ConfigurationFile = "invalid.cfg",
                PrivateBinPath = "BepInEx/core"
            };
            var domain = AppDomain.CreateDomain("Pathfinder", new Evidence(), setup);

            domain.CreateInstance("BepInEx.Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "BepInEx.Hacknet.Entrypoint");

            Environment.Exit(0);
        }
    }
}