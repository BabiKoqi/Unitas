using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Newtonsoft.Json;
using Unitas.Runtime;

namespace Unitas {
    static class Program {
        static ModuleDefMD _mod;
        static void Main(string[] args) {
            Setup();

            if (args.Length < 1)
                PrintHelp();

            if (!File.Exists(args[0]))
                Error($"{args[0]} is not a file or it does not exist");

            try {
                _mod = ModuleDefMD.Load(args[0]);
            } catch (Exception ex) {
                Error(ex.ToString());
            }

            var vars = args.Length > 1 ? args[1] : null;
            ProcessAssembly(vars);
            Console.ReadLine();
        }

        //Here we replace the AssemblyRef entry for TrinitySeal with Unitas.Runtime
        //With this, we not only hijack all library calls
        //But we also bypass the hash check
        static void ProcessAssembly(string vars) {
            Console.WriteLine("Processing assembly...");
            var opts = new ModuleWriterOptions(_mod) { Logger = DummyLogger.NoThrowInstance };
            var writer = new ModuleWriter(_mod, opts);

            foreach (var asmref in _mod.GetAssemblyRefs()) {
                if (asmref.Name != "TrinitySeal")
                    continue;
                
                Console.WriteLine("Replacing reference");
                asmref.Name = "Unitas.Runtime";
            }

            Console.WriteLine("Fixing namespaces...");
            foreach (var typeref in _mod.GetTypeRefs()) {
                if (typeref.Namespace == "TrinitySeal" && typeref.DefinitionAssembly.Name == "Unitas.Runtime")
                    typeref.Namespace = "Unitas.Runtime";
            }
            
            //Preserve EVERYTHING
            opts.MetadataOptions.PreserveHeapOrder(_mod, true);
            opts.MetadataOptions.Flags |= MetadataFlags.PreserveRids | MetadataFlags.KeepOldMaxStack;

            var runtime = Path.Combine(Path.GetDirectoryName(_mod.Location), "Unitas.Runtime.dll");
            File.Copy(typeof(Seal).Assembly.Location, runtime, true);
            if (vars != null)
                AddServerVariables(vars, runtime);
            
            writer.Write(GetNewName());
            Console.WriteLine($"Bypassed assembly saved at: {GetNewName()}");
        }

        static void AddServerVariables(string path, string runtimepath) {
            if (!File.Exists(path))
                Error("Couldn't find server variables file at " + path);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
            var mod = ModuleDefMD.Load(runtimepath);
            
            var seal = mod.Types.Single(t => t.Name == "Seal");
            var vars = seal.Fields.Single(f => f.Name == "_vars");
            var body = seal.FindOrCreateStaticConstructor().Body;
            foreach (var pair in dict) {
                body.Instructions.Insert(2, new Instruction(OpCodes.Ldsfld, vars));
                body.Instructions.Insert(3, new Instruction(OpCodes.Ldstr, pair.Key));
                body.Instructions.Insert(4, new Instruction(OpCodes.Ldstr, pair.Value));
                body.Instructions.Insert(5, new Instruction(OpCodes.Callvirt, _mod.Import(typeof(Dictionary<string, string>).GetMethod("set_Item", (BindingFlags)(-1)))));
            }

            using (var ms = new MemoryStream()) {
                mod.Write(ms);
                mod.Dispose();
                File.WriteAllBytes(runtimepath, ms.ToArray());
            }
        }

        static string GetNewName() {
            var curr = _mod.Location;
            var noext = Path.Combine(Path.GetDirectoryName(curr), Path.GetFileNameWithoutExtension(curr));
            noext += "_unitas";
            noext += Path.GetExtension(curr);
            return noext;
        } 

        static void Error(string msg) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Something went wrong:");
            Console.Write(msg);
            Console.ReadLine();
            Environment.Exit(-1);
        }

        static void PrintHelp() {
            Console.WriteLine($"Usage: Unitas.exe <path to trinityseal protected file>");
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void Setup() {
            Console.Title = "Unitas by xsilent007";
            Console.WriteLine("Unitas | PUBLIC EDITION");
            Console.WriteLine("(Private version has more features)\n");
        }
    }
}