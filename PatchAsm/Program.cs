using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;

namespace PatchAsm
{
    class Program
    {
        public void Run()
        {
            try
            {
                var infilename = "Assembly-CSharp.orig.dll";
                var outfilename = "Assembly-CSharp.dll";
                if (!File.Exists(infilename))
                {
                    Print("File {0} not found! Please run me in KSP_DATA\\Managed", infilename);
                    return;
                }
                var asm = AssemblyDefinition.ReadAssembly(infilename);

                var asmLib = AssemblyDefinition.ReadAssembly("TimerLib.dll");
                TypeDefinition tUtils = asmLib.MainModule.GetType("TimerLib.Utils");
                MethodDefinition startFunc0 = tUtils.Methods.First(x => x.Name == "StartTimed0");
                MethodDefinition startFunc1 = tUtils.Methods.First(x => x.Name == "StartTimed1");
                MethodDefinition startFunc2 = tUtils.Methods.First(x => x.Name == "StartTimed2");
                MethodDefinition startFunc3 = tUtils.Methods.First(x => x.Name == "StartTimed3");
                MethodDefinition stopFunc = tUtils.Methods.First(x => x.Name == "StopTimed");

                PatchFunc(asm, "Part", "requestResource", startFunc0, stopFunc);
                PatchFunc(asm, "Part", "TransferResource", startFunc0, stopFunc);
                PatchFunc(asm, "Part", "GetConnectedResources", startFunc0, stopFunc);

                PatchFunc(asm, "ModuleDockingNode", "FixedUpdate", startFunc1, stopFunc);

                PatchFunc(asm, "ResourceConverter", "ProcessRecipe", startFunc2, stopFunc);

                PatchFunc(asm, "FlightIntegrator", "FixedUpdate", startFunc3, stopFunc);

                Print("Writing file {0}", outfilename);
                asm.Write(outfilename);

                Print("Patched file created.");
            }
            catch (Exception e)
            {
                Print("Exception while trying to patch assembly: {0}", e.Message);
            }
        }

        private void PatchFunc(AssemblyDefinition asm, String TypeName, String FuncName, MethodDefinition startFunc, MethodDefinition stopFunc)
        {
            Print("Patching {0}.{1}", TypeName, FuncName);

            TypeDefinition type = asm.MainModule.GetType(TypeName);

            foreach (MethodDefinition func in type.Methods.Where(x => x.Name == FuncName))
            {
                //DumpWholeFunction(func, "Before");
                var insList = func.Body.Instructions;
                ILProcessor proc = func.Body.GetILProcessor();

                var callStart = proc.Create(OpCodes.Call, func.Module.Import(startFunc));
                var methodStop = func.Module.Import(stopFunc);
                var ret = proc.Create(OpCodes.Ret);

                // Insert call to stop before every return
                for (int i = insList.Count - 1; i >= 0; i--)
                {
                    if (insList[i].OpCode == OpCodes.Ret)
                    {
                        proc.InsertAfter(insList[i], ret);
                        insList[i].OpCode = OpCodes.Call;
                        insList[i].Operand = methodStop;
                    }
                }

                // Insert call to start at the beginning
                proc.InsertBefore(insList[0], callStart);

                //DumpWholeFunction(func, "After");
                Print("{0}.{1} patched ok", TypeName, FuncName);
            }
        }

        private void DumpWholeFunction(MethodDefinition func, string message)
        {
            Print(message);
            foreach (var instr in func.Body.Instructions)
                Print("Offset {0}: {1}", instr.Offset, EncodeNonAsciiCharacters(instr.ToString()));
        }

        // http://stackoverflow.com/a/1615860
        private static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c < 32 || c > 127)
                {
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static void Print(string message, params object[] values)
        {
            Console.WriteLine(String.Format(message, values));
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
