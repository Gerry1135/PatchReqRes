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
        private readonly Log log = new Log("PatchAsm");

        public void Run()
        {
            try
            {
                var infilename = "Assembly-CSharp.orig.dll";
                var outfilename = "Assembly-CSharp.dll";
                if (!File.Exists(infilename))
                {
                    log.Error("File {0} not found! Please run me in KSP_DATA\\Managed", infilename);
                    return;
                }
                var asm = AssemblyDefinition.ReadAssembly(infilename);

                var asmLib = AssemblyDefinition.ReadAssembly("TimerLib.dll");
                TypeDefinition tUtils = asmLib.MainModule.GetType("TimerLib.Utils");
                MethodDefinition startFunc = tUtils.Methods.First(x => x.Name == "StartTimed");
                MethodDefinition stopFunc = tUtils.Methods.First(x => x.Name == "StopTimed");

                //DumpWholeFunction(asm, "Part", "requestResource");

                PatchFunc(asm, "Part", "requestResource", startFunc, stopFunc);

                PatchFunc(asm, "ResourceBroker", "RequestResource", startFunc, stopFunc);

                PatchFunc(asm, "ResourceBroker", "StoreResource", startFunc, stopFunc);

                //DumpWholeFunction(asm, "Part", "requestResource");

                log.Debug("Writing file {0}", outfilename);
                asm.Write(outfilename);

                log.Info("Patched file created.");
            }
            catch (Exception e)
            {
                log.Error("Exception while trying to patch assembly: {0}", e.Message);
            }
        }

        private void PatchFunc(AssemblyDefinition asm, String TypeName, String FuncName, MethodDefinition startFunc, MethodDefinition stopFunc)
        {
            TypeDefinition type = asm.MainModule.GetType(TypeName);

            MethodDefinition func = type.Methods.First(x => x.Name == FuncName);

            var insList = func.Body.Instructions;
            ILProcessor proc = func.Body.GetILProcessor();

            var callStart = proc.Create(OpCodes.Call, func.Module.Import(startFunc));
            var callStop = proc.Create(OpCodes.Call, func.Module.Import(stopFunc));

            // Insert call to stop before every return
            for (int i = insList.Count - 1; i >= 0; i--)
            {
                if (insList[i].OpCode == OpCodes.Ret)
                    proc.InsertBefore(insList[i], callStop);
            }

            // Insert call to start at the beginning
            proc.InsertBefore(insList[0], callStart);
        }

        private void DumpWholeFunction(AssemblyDefinition asmdef, string typeName, string functionName)
        {
            var type = asmdef.MainModule.GetType(typeName);
            var func = type.Methods.First(x => x.Name == functionName);

            var instrList = func.Body.Instructions;

            foreach (var instr in instrList)
            {
                Console.WriteLine("Offset {0}: {1}", instr.Offset, EncodeNonAsciiCharacters(instr.ToString()));
            }
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

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }


    public class Log
    {
        private static readonly string ns = typeof(Log).Namespace;
        private readonly string id = String.Format("{0:X8}", Guid.NewGuid().GetHashCode());
        private readonly string name;

        public Log(string name)
        {
            this.name = name;
        }

        private void Print(string level, string message, params object[] values)
        {
            Console.WriteLine("[" + name + ":" + level + ":" + id + "]  " + String.Format(message, values));
        }

        public void Debug(string message, params object[] values)
        {
            Print("DEBUG", message, values);
        }

        public void Info(string message, params object[] values)
        {
            Print("INFO", message, values);
        }

        public void Error(string message, params object[] values)
        {
            Print("ERROR", message, values);
        }
    }
}
