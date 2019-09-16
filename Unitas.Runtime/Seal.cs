using System.Collections.Generic;
using System.Windows.Forms;

namespace Unitas.Runtime {
    public class Seal {
        static readonly Dictionary<string, string> _vars = new Dictionary<string, string>();

        
        public static string Secret { get; set; }
        public static bool GrabVariables(string username, string password, string token, string idontremember) => true;
        public static bool Initialize(string version) => true;
        public static bool Login(string username, string password, bool message = true) => true;
        public static string Var(string name) {
            var ret = _vars.TryGetValue(name, out var val);
            return ret ? val : $"<SERVER SIDED VARIABLE: '{name}'>";
        }

        public static void InitializeForm(string programid, string version, string variablekey, Form AuthForm, Form MainForm, SealColours Colour) {
            AuthForm.Hide();
            MainForm.Opacity = 1;
            MainForm.Show();
        }
    }
}