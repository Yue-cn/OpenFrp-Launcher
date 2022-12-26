using OpenFrp.Core.App;
using System.Configuration.Install;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Windows;

namespace OpenFrp.Core
{
    internal class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleExitEvent handler, bool add);
        private delegate bool ConsoleExitEvent(CtrlType sig);

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        internal async static Task Main(string[] args)
        {
            /*
                特此谢明: SakuraFrp
             */
            if (Utils.ExcutableName != Utils.CorePath)
            {

                MessageBox.Show("请不要重命名 OpenFrp.Core.exe", "Core Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (args.Length is 1)
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                Console.WriteLine("AdminMode: " + principal.IsInRole(WindowsBuiltInRole.Administrator));
                switch (args[0])
                {
                    case "--install":
                        {
                            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo(Utils.CorePath, "--install")
                                    {
                                        CreateNoWindow = false,
                                        Verb = "runas"
                                    });
                                }
                                catch { }
                            }
                            else
                            {
                                var dir = new DirectoryInfo(Utils.ApplicationPath);
                                var acl = dir.GetAccessControl(System.Security.AccessControl.AccessControlSections.Access);
                                acl.SetAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                                    new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null),
                                    System.Security.AccessControl.FileSystemRights.FullControl,
                                    System.Security.AccessControl.InheritanceFlags.ObjectInherit,
                                    System.Security.AccessControl.PropagationFlags.None,
                                    System.Security.AccessControl.AccessControlType.Allow));
                                dir.SetAccessControl(acl);

                                ManagedInstallerClass.InstallHelper(new string[] { Utils.CorePath });
                                
                            }
                            return;

                        }
                    case "--uninstall":
                        {
                            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo(Utils.CorePath, "--uninstall")
                                    {
                                        CreateNoWindow = false,
                                        Verb = "runas"
                                    });
                                }
                                catch { }
                            }
                            else
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { "-u", Utils.CorePath });
                            }
                            return;
                        };
                    case "--ws":
                        {
                            // 服务端
                            var appServer = new Pipe.PipeServer();
                            appServer.Start();
                            // 连接启动器用的。
                            var appClient = new Pipe.PipeClient();
                            await appClient.Start(true);

                            if (!Utils.ServicesMode)
                            {
                                SetConsoleCtrlHandler((ctrl) =>
                                {
                                    // 如果有客户端连接，那么这里需要推送。
                                    if (appClient.State)
                                    {
                                        appClient.PushMessageAsync(new Pipe.PipeModel.RequestModel()
                                        {
                                            Action = Pipe.PipeModel.OfAction.Server_Closed
                                        }).GetAwaiter().GetResult();
                                    }
                                    Environment.Exit(0);
                                    return true;
                                }, true);
                            }
                            while (true)
                            {
                                await Task.Delay(3000);
                            }

                        }
                }
                Console.ReadKey();
            }
            else if (Utils.ServicesMode)
            {
                await OfSettings.ReadConfig();
                if (OfSettings.Instance.WorkMode == WorkMode.DeamonService)
                {
                    ServiceBase.Run(new WindowService());
                }

                
                
            }
            
            
        }
    }
}