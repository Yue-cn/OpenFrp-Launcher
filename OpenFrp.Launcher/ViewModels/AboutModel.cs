using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class AboutModel : ObservableObject
    {

        public AboutModel()
        {
            aboutDatas.Add(new()
            {
                Title = "作者",
                Content = "Yue"
            });
            aboutDatas.Add(new()
            {
                Title = "版本",
                Content = Utils.ApplicationVersions
            });
            aboutDatas.Add(new()
            {
                Title = "注明",
                Content = "本软件为 GPL3 协议，您可以修改且分发，但不得删除原作品的标记，且同样需要把修改过的源代码放出来。"
            });
            aboutDatas.Add(new()
            {
                Title = "版权所有",
                Content = "ZGIT Network, 2023。"
            });
        }

        [ObservableProperty]
        public ObservableCollection<AboutDataItem> aboutDatas = new();

        public class AboutDataItem
        {
            public string Title { get; set; } = "Unknown";

            public string Content { get; set; } = "Content";
        }

        [RelayCommand]
        private void CheckUpdate() => (App.Current.MainWindow as WpfSurface)?.CheckUpdate();

    }
}
