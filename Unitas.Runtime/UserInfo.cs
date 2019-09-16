using System;

namespace Unitas.Runtime {
    public class UserInfo {
        public static string Username { get; set; } = "Unitas by xsilent007";
        public static string Email { get; set; } = "null@gmail.com";
        public static string IP { get; set; } = "0.0.0.0";
        public static int Level { get; set; } = 0;
        public static DateTime Expires { get; set; } = DateTime.Now.AddYears(1);
    }
}