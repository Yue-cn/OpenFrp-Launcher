using OpenFrp.Core;
using OpenFrp.Core.Api;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
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


namespace OpenFrp.Launcher.Views
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Page
    {

        private HomeModel HomeModel
        {
            get => (HomeModel)DataContext;
            set => DataContext = value;
        }

        public Home() => InitializeComponent();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RefreshUserInfo();
            RefreshLauncherPreview();
        }


        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var grid = (Grid)sender;
            if (HomeModel.AdaptiveSmall = !(grid.ActualWidth > 735))
            {
                grid.RowDefinitions[0].Height = new GridLength(grid.Height = grid.ActualWidth / 16 * 9);
                grid.ClearValue(HeightProperty);
            }
            else
            {
                grid.RowDefinitions[0].Height = new GridLength(1,GridUnitType.Star);
                grid.Height = grid.ColumnDefinitions[0].ActualWidth / 16 * 9;
            }
        }
        /// <summary>
        /// 显示用户个人信息
        /// </summary>
        internal async void RefreshUserInfo()
        {
            Of_UserInfo_Loader.ShowLoader();
            while (OfAppHelper.isLoading)
            {
                await Task.Delay(500);
            }
            if (!OfApi.LoginState)
            {
                Of_UserInfo_Loader.ShowError();
                Of_UserInfo_Loader.PushMessage(() =>
                {
                    (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.Setting));
                    var item = ((App.Current.MainWindow as WpfSurface)?.OfApp_NavigationView.SettingsItem as NavigationViewItem);
                    if (item is not null) item.IsSelected = true;
                }, "登录后即可查看个人信息。", "登录");
            }
            else
            {
                
                HomeModel.UserInfoViewModels = new()
                {
                    new()
                    {
                        IconElement = new SymbolIcon(Symbol.Mail),
                        Title = "邮箱",
                        Content = HomeModel.UserInfoData.Email,
                    },
                    new()
                    {
                        IconElement = new SymbolIcon(Symbol.ContactInfo),
                        Title = "昵称",
                        Content = HomeModel.UserInfoData.UserName,
                    },
                    new()
                    {
                        IconElement = new FontIcon(){Glyph = "\xe8a4" },
                        Title = "隧道数",
                        Content = $"{HomeModel.UserInfoData.UsedProxies} / {HomeModel.UserInfoData.MaxProxies}",
                    },
                    new()
                    {
                        IconElement = new FontIcon(){Glyph = "\xeafc" },
                        Title = "可用流量",
                        Content = $"{Math.Round(HomeModel.UserInfoData.Traffic / (double)1024,2)} Gib",
                    },
                    new(),
                    new(),
                    new(),
                    new(),
                };
                Of_UserInfo_Loader.ShowContent();
            }
            
        }
        /// <summary>
        /// 显示启动器大图
        /// </summary>
        internal async void RefreshLauncherPreview()
        {
            var lp = await OfApi.GetLauncherPreview();
            ((ImageBrush)Of_Preview_Background.Background).ImageSource = new BitmapImage(new Uri("pack://application:,,,/OpenFrp.Launcher;component/wallhaven-m9o2vk_1920x1080.png"));
            if (lp.Flag)
            {
                if (lp.Data.Info.ImageUrl is not null)
                {
                    string url = lp.Data.Info.ImageUrl.ToString();
                    string fileName = Path.Combine(Utils.AppTempleFilesPath,"static", $"{lp.Data.Info.ImageUrl.ToString().GetMD5()}.png");
                    if (!File.Exists(fileName))
                    {
                        var result = await OfApi.GET(url);

                        if (result is not null)
                        {
                            using var stream = new FileStream(fileName, FileMode.OpenOrCreate);
                            await stream.WriteAsync(result, 0, result.Length);
                            stream.Close();
                        }
                        else
                        {
                            Of_Preview_CardLoader.ShowContent();
                            return;
                        };
                    }
                    
                    ((ImageBrush)Of_Preview_Background.Background).ImageSource = new BitmapImage(new Uri(fileName));

                }
                HomeModel.LauncherPreviewData = lp.Data.Info;
            }
            Of_Preview_CardLoader.ShowContent();
        }
        /// <summary>
        /// "刷新的菜单按钮"被按下
        /// </summary>
        private async void Refresh_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Of_Preview_CardLoader.ShowLoader();
            await Task.Delay(1000);
            RefreshLauncherPreview();
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        private async void Save_MenuItem_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "图片文件|*.png";
                
                if (dialog.ShowDialog() is true)
                {
                    using var file = new FileStream(dialog.FileName,FileMode.Create);
                    
                    Stream stream = HomeModel.LauncherPreviewData.ImageUrl is null ? 
                        App.GetResourceStream(new Uri("pack://application:,,,/OpenFrp.Launcher;component/wallhaven-m9o2vk_1920x1080.png")).Stream :
                        new FileStream(((BitmapImage)((ImageBrush)Of_Preview_Background.Background).ImageSource).UriSource.ToString().Replace("file:///",""), FileMode.Open,FileAccess.Read);
                    byte[] buffer = new byte[stream.Length];
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        await file.WriteAsync(buffer, 0, count);
                    }
                }

            }
            catch { }
            
        }
    }
}
