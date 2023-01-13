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
using System.Windows.Markup;
using System.Xml;
using OpenFrp.Core.App;

namespace OpenFrp.Launcher.Views
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Page
    {
        /// <summary>
        /// 首页模型
        /// </summary>
        private HomeModel HomeModel
        {
            get => (HomeModel)DataContext;
            set => DataContext = value;
        }

        public Home() => InitializeComponent();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            // 开始相关操作
            RefreshUserInfo();
            RefreshLauncherPreview();
            RefreshBroadCast();
        }

        /// <summary>
        /// 自适应大小
        /// </summary>
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
            Of_Home_UserInfoLoader.ShowLoader();
            while (OfAppHelper.isLoading && OfSettings.Instance.Account.HasAccount)
            {
                await Task.Delay(500);
            }
            // 如果为系统服 那么这里等待
            if (OfSettings.Instance.WorkMode is WorkMode.DeamonService) await Task.Delay(1000);
            // 如果没有登录
            if (!OfApi.LoginState)
            {
                // 如果没有账户
                if (!OfSettings.Instance.Account.HasAccount)
                {
                    Of_Home_UserInfoLoader.ShowError();
                    Of_Home_UserInfoLoader.PushMessage(() =>
                    {
                        (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.Setting));
                        var item = ((App.Current.MainWindow as WpfSurface)?.OfApp_NavigationView.SettingsItem as NavigationViewItem);
                        if (item is not null) item.IsSelected = true;
                    }, "登录后即可查看个人信息。", "登录");
                }
                else
                {
                    var resq =  await OfAppHelper.LoginAndUserInfo(OfSettings.Instance.Account.User, OfSettings.Instance.Account.Password);
                    if (resq.Flag)
                    {
                        HomeModel.UserInfoViewModels = new()
                        {
                            new()
                            {
                                IconElement = "\xe715",
                                Title = "邮箱",
                                Content = HomeModel.UserInfoData.Email,
                            },
                            new()
                            {
                                IconElement = $"\xe77b",
                                Title = "昵称",
                                Content = HomeModel.UserInfoData.UserName,
                            },
                            new()
                            {
                                IconElement = "\xe8a4",
                                Title = "隧道数",
                                Content = $"{HomeModel.UserInfoData.UsedProxies} / {HomeModel.UserInfoData.MaxProxies}",
                            },
                            new()
                            {
                                IconElement = "\xeafc",
                                Title = "可用流量",
                                Content = $"{Math.Round(HomeModel.UserInfoData.Traffic / (double)1024,2)} Gib",
                            },
                            new()
                            {
                                IconElement = "\xe780",
                                Title = "实名状态",
                                Content = $"{(HomeModel.UserInfoData.isRealname ? "已": "未")}实名"
                            },
                            new()
                            {
                                IconElement = "\xe902",
                                Title = "所在组",
                                Content = HomeModel.UserInfoData.GroupCName,
                            },
                            new()
                            {
                                IconElement = "\xec05",
                                Title = "带宽速率 (Mbps)",
                                Content = $"{(HomeModel.UserInfoData.InputLimit / 1024) * 8} / {(HomeModel.UserInfoData.OutputLimit / 1024) * 8}"
                            },
                        };

                        Of_Home_UserInfoLoader.ShowContent();
                    }
                    else
                    {
                        Of_Home_UserInfoLoader.ShowError();
                        Of_Home_UserInfoLoader.PushMessage(async () =>
                        {
                            await OfAppHelper.RequestLogin();
                            RefreshUserInfo();
                        }, $"登录请求失败: {resq.Message}", "重试");
                    }
                }
            }
            else
            {
                while(HomeModel.UserInfoData is null) 
                { 
                    await Task.Delay(250); 
                }

                HomeModel.UserInfoViewModels = new()
                {
                    new()
                    {
                        IconElement = "\xe715",
                        Title = "邮箱",
                        Content = HomeModel.UserInfoData.Email,
                    },
                    new()
                    {
                        IconElement = $"\xe77b",
                        Title = "昵称",
                        Content = HomeModel.UserInfoData.UserName,
                    },
                    new()
                    {
                        IconElement = "\xe8a4",
                        Title = "隧道数",
                        Content = $"{HomeModel.UserInfoData.UsedProxies} / {HomeModel.UserInfoData.MaxProxies}",
                    },
                    new()
                    {
                        IconElement = "\xeafc",
                        Title = "可用流量",
                        Content = $"{Math.Round(HomeModel.UserInfoData.Traffic / (double)1024,2)} Gib",
                    },
                    new()
                    {
                        IconElement = "\xe780",
                        Title = "实名状态",
                        Content = $"{(HomeModel.UserInfoData.isRealname ? "已": "未")}实名"
                    },
                    new()
                    {
                        IconElement = "\xe902",
                        Title = "所在组",
                        Content = HomeModel.UserInfoData.GroupCName,
                    },
                    new()
                    {
                        IconElement = "\xec05",
                        Title = "带宽速率 (Mbps)",
                        Content = $"{(HomeModel.UserInfoData.InputLimit / 1024) * 8} / {(HomeModel.UserInfoData.OutputLimit / 1024) * 8}"
                    },
                };

                Of_Home_UserInfoLoader.ShowContent();
            }
            
        }
        /// <summary>
        /// 显示启动器大图
        /// </summary>
        internal async void RefreshLauncherPreview()
        {
            Of_Home_PreviewCardLoader.ShowLoader();
            var lp = await OfApi.GetLauncherPreview();
            
            ((ImageBrush)Of_Home_PreviewBackground.Background).ImageSource = new BitmapImage(new Uri("pack://application:,,,/OpenFrp.Launcher;component/Resourecs/wallhaven-m9o2vk_1920x1080.png"));
            if (lp.Flag)
            {
                if (lp.Data.ImageUrl is not null)
                {
                    Of_Home_PreviewCardLoader.ShowContent();
                    string url = lp.Data.ImageUrl.ToString();
                    string fileName = Path.Combine(Utils.AppTempleFilesPath,"static", $"{lp.Data.ImageUrl.ToString().GetMD5()}.png");
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
                            return;
                        };
                    }
                    
                    ((ImageBrush)Of_Home_PreviewBackground.Background).ImageSource = new BitmapImage(new Uri(fileName));

                }
                HomeModel.LauncherPreviewData = lp.Data;
            }
            Of_Home_PreviewCardLoader.ShowContent();
        }

        internal async void RefreshBroadCast()
        {
            Of_Home_BroadCastLoader.ShowLoader();
            var lp = await OfApi.GetBroadCast();
            if (lp.Flag)
            {
                Of_Home_BroadCast_Content.Children.Add(XamlReader.Parse(lp.Data) as UIElement);
                await Task.Delay(250);
                Of_Home_BroadCastLoader.ShowContent();
            }
            else
            {
                Of_Home_BroadCastLoader.ShowError();
                Of_Home_BroadCastLoader.PushMessage(RefreshBroadCast,lp.Message,"重试");
            }
            

        }

        #region Element Command
        /// <summary>
        /// "刷新的菜单按钮"被按下
        /// </summary>
        private async void Refresh_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Of_Home_PreviewCardLoader.ShowLoader();
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
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "图片文件|*.png"
                };

                if (dialog.ShowDialog() is true)
                {
                    using var file = new FileStream(dialog.FileName,FileMode.Create);
                    
                    Stream stream = HomeModel.LauncherPreviewData.ImageUrl is null ? 
                        App.GetResourceStream(new Uri("pack://application:,,,/OpenFrp.Launcher;component/Resourecs/wallhaven-m9o2vk_1920x1080.png")).Stream :
                        new FileStream(((BitmapImage)((ImageBrush)Of_Home_PreviewBackground.Background).ImageSource).UriSource.ToString().Replace("file:///",""), FileMode.Open,FileAccess.Read);
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
        /// <summary>
        /// 滚动逻辑修复
        /// </summary>
        private void GridView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) => Of_Home_BaseView.ExcuteScroll(e);
        #endregion
    }
}
