using OpenFrp.Core.Api;
using OpenFrp.Core.App;
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

        /// <summary>
        /// 取消令牌
        /// </summary>
        private CancellationTokenSource _sourec { get; set; } = new();

        /// <summary>
        /// 点击了"确定"按钮后开始登录。
        /// </summary>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
            args.Cancel = true;
            IsPrimaryButtonEnabled = Of_Ld_ErrorInfo.IsEnabled = false;
            Of_Ld_ELoader1.ShowLoader();
            // 由于在内网模式下 延迟≈0 所以延迟一下。
            await Task.Delay(500);
            // 登录且获得信息
            var res = await OfAppHelper.LoginAndUserInfo(Of_Ld_Username.Text, Of_Ld_Password.Password, _sourec.Token);
            if (res.Flag)
            {
                // PUSH 给服务端
                OfSettings.Instance.Account = new(Of_Ld_Username.Text, Of_Ld_Password.Password);
                Of_Ld_Username.Text = Of_Ld_Password.Password = string.Empty;
                Hide();
                return;
            }
            Of_Ld_ErrorInfo.Message = res.Message;
            Of_Ld_ELoader1.ShowContent();
            IsPrimaryButtonEnabled = Of_Ld_ErrorInfo.IsEnabled = true;
        }

        /// <summary>
        /// 点击"取消"按钮后取消令牌。
        /// </summary>
        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => _sourec.Cancel();
    }
}
