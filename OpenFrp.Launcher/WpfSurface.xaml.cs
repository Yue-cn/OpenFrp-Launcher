using OpenFrp.Core;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenFrp.Launcher
{
    /// <summary>
    /// WpfSurface.xaml 的交互逻辑
    /// </summary>
    public partial class WpfSurface : Window
    {
        public WpfSurface()
        {
            InitializeComponent();
        }
        private LauncherModel LauncherModel
        {
            get => (LauncherModel)DataContext;
            set => DataContext = value;
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e); 
            OfApp_NavigationView.ItemInvoked += (s, e) =>
            {
                Type page;
                if (!e.IsSettingsInvoked)
                {
                    
                }
                page = typeof(Views.Setting);
                OfApp_RootFrame.Navigate(page);
            };
            OfApp_RootFrame.Navigating += (s, e) =>
            {
                if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                {
                    e.Cancel = true;
                }
            };
            PipeWorker();
        }

        private async void PipeWorker()
        {
            // 当进程权限过高时，会显示无权获得该进程的信息。
            try
            {
                var process = Process.GetProcessesByName("OpenFrp.Core").FirstOrDefault() ?? Process.Start(new ProcessStartInfo()
                {
                    FileName = System.IO.Path.Combine(Utils.ApplicationPath, "OpenFrp.Core.exe"),
                    CreateNoWindow = false,
                    UseShellExecute = false,
                });
                process.EnableRaisingEvents = true;
                // 进程退出时
                process.Exited += (sender, args) =>
                {
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        LauncherModel.PipeRunningState = false;
                        PipeWorker();
                    });
                };
            }
            catch { }

            OfAppHelper.PipeClient = new OpenFrp.Core.Pipe.PipeClient();
            await OfAppHelper.PipeClient.Start();
            LauncherModel.PipeRunningState = true;

        }
        


    }
}
