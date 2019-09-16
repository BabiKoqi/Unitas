using mscoree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Harmony;

namespace Dumper {
    static class Program {
        internal static Assembly Client { get; private set; }

        static HarmonyInstance _inst;
        
        static void Main(string[] args) {
            if (args.Length < 1)
                PrintHelp();

            var file = args[0];
            if (!File.Exists(file))
                Error("Couldn't find " + file);
            
            Console.WriteLine("[+] Initializing");
            MaskHost();
            
            Console.WriteLine("[+] Loading assembly...");
            try {
                Client = Assembly.LoadFile(Path.GetFullPath(file));
            }
            catch { Error("Couldn't load " + file); }
            
            AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            Console.WriteLine("[+] Executing assembly, log in / fuck around until the program quits, a new file should appear with all server variables in it");
            Console.WriteLine("[-] Press enter to continue");
            Console.ReadLine();
            
            //Make sure <Module>.cctor is run...
            RuntimeHelpers.RunModuleConstructor(Client.ManifestModule.ModuleHandle);
            var entry = Client.EntryPoint;
            var arguments = entry.GetParameters().Length < 1 ? null : args.Length > 1 ? args.Skip(1).ToArray() : new string[] { };
            
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(file))); //This is needed because TrinitySeal doesn't use a full path in its HashCheck
            entry.Invoke(null, new object [] { arguments });
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args) {
            var asmpath = Path.Combine(Directory.GetCurrentDirectory(), new AssemblyName(args.Name).Name + ".dll");
            return !File.Exists(asmpath) ? null : Assembly.LoadFrom(asmpath);
        }

        static void AssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
            var asm = args.LoadedAssembly;
            if (asm.GetName().Name != "TrinitySeal")
                return;

            var seal = asm.GetType("TrinitySeal.Seal");
            if (seal == null)
                Error("Something went wrong");

            var grabvars = seal.GetMethod("GrabVariables", (BindingFlags)(-1));
            if (grabvars == null)
                Error("Something went wrong");
            
            Console.WriteLine("[+] Patching Trinity");
            
            //Make sure AntiTamper is done
            RuntimeHelpers.RunModuleConstructor(asm.ManifestModule.ModuleHandle);
            _inst.Patch(grabvars, postfix: new HarmonyMethod(typeof(VarDumper).GetMethod("Postfix", (BindingFlags)(-1))));
        }

        static void Error(string message) {
            Console.WriteLine(message);
            Console.ReadLine();
            Environment.Exit(-1);
        }

        static void PrintHelp() {
            Console.WriteLine("Usage: Dumper.exe <path to assembly>");
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void MaskHost() {
            Console.Title = RandomString();
            _inst = HarmonyInstance.Create(RandomString());
            _inst.PatchAll(typeof(Program).Assembly);
        }
        
        static readonly Random R = new Random();
        static string RandomString() => new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 8).Select(s => s[R.Next(s.Length)]).ToArray());
    }
}