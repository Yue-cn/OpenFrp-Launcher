using OpenFrp.Core.Api;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenFrp.Launcher.Controls
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class LoginDialog : ContentDialog
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
            args.Cancel = true;
            Of_Ld_ErrorInfo.IsEnabled = false;
            Of_Ld_ELoader1.ShowLoader();
            await Task.Delay(2000);
            var res = await OfApi.POST<OpenFrp.Core.Api.OfApiModel.Response.BaseModel>(OfApiUrl.Login, new System.Net.Http.StringContent("w"));
            Of_Ld_ELoader1.ShowContent();
            Of_Ld_ErrorInfo.IsEnabled = true;
        }
    }
}
