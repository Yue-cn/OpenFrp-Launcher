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
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e); 
            if (isSupportDarkMode2)
            {
                //Launcher.Properties.UxTheme.SetPreferredAppMode(Launcher.Properties.UxTheme.PreferredAppMode.Default);
            }
        }
        private bool isSupportDarkMode2 = Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build >= 18362;


}
}
