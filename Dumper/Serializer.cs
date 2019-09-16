using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;

namespace Dumper {
    static class Serializer {
        internal static void SaveVars(Dictionary<string, string> vars) {
        	var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "trinityvars.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(vars));
        }
    }
}
