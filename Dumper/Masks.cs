using System.Reflection;
using Harmony;

namespace Dumper {
    [HarmonyPatch(typeof(Assembly))]
    [HarmonyPatch("GetEntryAssembly")]
    class GetEntryAssembly {
        static void Postfix(ref Assembly __result) {
            if (__result == typeof(GetEntryAssembly).Assembly)
                __result = Program.Client;
        }
    }

    [HarmonyPatch(typeof(Assembly))]
    [HarmonyPatch("GetCallingAssembly")]
    class GetCallingAssembly {
        static void Postfix(ref Assembly __result) {
            if (__result == typeof(GetCallingAssembly).Assembly)
                __result = Program.Client;
        }
    }
}