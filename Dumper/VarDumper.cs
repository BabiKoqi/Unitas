using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dumper {
    class VarDumper {
        static void Postfix() {
            var seal = Assembly.GetCallingAssembly().GetType("TrinitySeal.Seal");
            var vars = seal.GetField("Vars", (BindingFlags) (-1));
            var getvar = seal.GetMethod("Var");

            //Rip
            if (vars == null || getvar == null)
                return;

            var dict = (Dictionary<string, object>)vars.GetValue(null);
            var newDict = new Dictionary<string, string>();
            foreach (var name in dict.Keys)
                newDict[name] = (string)getvar.Invoke(null, new object [] { name });

            Serializer.SaveVars(newDict);
            Environment.Exit(0);
        }
    }
}