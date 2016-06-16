using System;
using System.Threading;
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
                var modPath = "..\\..\\GameData\\ProfileGraph\\";
                var cfgfilename = modPath + "PluginData\\ProfileGraph\\profile.cfg";

                var infilename = "Assembly-CSharp.orig.dll";
                var outfilename = "Assembly-CSharp.dll";

                if (!File.Exists(infilename))
                {
                    File.Move(outfilename, infilename);
                }
                var asm = AssemblyDefinition.ReadAssembly(infilename);

                var asmLib = AssemblyDefinition.ReadAssembly(modPath + "ProfileGraph.dll");
                TypeDefinition tUtils = asmLib.MainModule.GetType("ProfileGraph.Utils");
                MethodDefinition startFunc = tUtils.Methods.First(x => x.Name == "StartTimed");
                MethodDefinition stopFunc = tUtils.Methods.First(x => x.Name == "StopTimed");

                // Read in the profile config
                int channel = 0;

                // Loop through the lines

                // Must be an odd number of strings
                // Ignore the first (name) and process the rest in pairs
                Print("Loading config from " + cfgfilename);
                if (File.Exists(cfgfilename))
                {
                    String[] lines = File.ReadAllLines(cfgfilename);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        String[] line = lines[i].Split(',');
                        if (line.Length % 2 == 1)
                        {
                            for (int index = 1; index < line.Length; index += 2)
                            {
                                PatchFunc(asm, line[index].Trim(), line[index+1].Trim(), startFunc, stopFunc, channel);
                            }
                            channel++;
                        }
                        else
                        {
                            Print("Ignoring invalid line in settings: '{0}'", lines[i]);
                        }
                    }
                }
                else
                    Print("Can't find profile.cfg");

                Print("Writing file {0}", outfilename);
                asm.Write(outfilename);

                Print("Patched file created.");
            }
            catch (Exception e)
            {
                Print("Exception while trying to patch assembly: {0}", e.Message);
            }
        }

        private void PatchFunc(AssemblyDefinition asm, String TypeName, String FuncName, MethodDefinition startFunc, MethodDefinition stopFunc, int channel)
        {
            Print("Patching {0}.{1} for channel {2}", TypeName, FuncName, channel);

            TypeDefinition type = asm.MainModule.GetType(TypeName);

            foreach (MethodDefinition func in type.Methods.Where(x => x.Name == FuncName))
            {
                //DumpWholeFunction(func, "Before");
                var insList = func.Body.Instructions;
                ILProcessor proc = func.Body.GetILProcessor();

                var loadChannel = proc.Create(OpCodes.Ldc_I4, channel);
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
                proc.InsertBefore(insList[0], loadChannel);

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

            Print("Press any key to continue");
            while (!Console.KeyAvailable)
                Thread.Sleep(100);
        }
    }
}
