using System;
using System.Collections.Generic;
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

            OfAppHelper.PipeClient = new OpenFrp.Core.Pipe.PipeClient();
            await OfAppHelper.PipeClient.Start();


        }
    }
}
