using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.App
{
    public class AppURLScheme
    {
        public static void RegistryKey()
        {
            var cb = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("oflauncher");
            cb.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", $"\"{Path.Combine(Utils.ApplicationPath, "OpenFrp.Launcher.exe")}\" \"%1\"");
            cb.SetValue("URL protocol", $"\"{Path.Combine(Utils.ApplicationPath, "OpenFrp.Launcher.exe")}\" \"%1\"");
        }
        public static void UnregistryKey()
        {
            Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree("oflauncher");
        }
    }
}
