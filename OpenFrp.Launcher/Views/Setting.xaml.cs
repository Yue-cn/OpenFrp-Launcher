using OpenFrp.Core;
using OpenFrp.Core.App;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace OpenFrp.Launcher.Views
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Page
    {
        public SettingModel Model
        {
            get => (SettingModel)DataContext;
            set => DataContext = value;
        }

        public Setting()
        {
            InitializeComponent();
            this.DataContext = OfAppHelper.SettingViewModel;
            this.Unloaded += async (sender, args) =>
            {
                var sb = new OfSettings();
                sb = OfSettings.Instance;
                sb.AutoRunTunnel = new()!;
                await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Push_Config,
                    Config = sb
                });
            };
        }

        private void AutoLaunch_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch tg)
            {
                string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{Utils.PipeRouteName}.lnk");
                if (tg.IsOn)
                {
                    var shortcut = (IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShell().CreateShortcut(file);
                    shortcut.TargetPath = Utils.ExcutableName;
                    shortcut.Arguments = "--minimize";
                    shortcut.Description = "OpenFrp Launcher 开机自启动";
                    shortcut.Save();
                }
                else
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }

            }
        }
        /// <summary>
        /// 字体列表
        /// </summary>
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var list = Fonts.SystemFontFamilies.ToArray();
            if (sender is ComboBox box)
            {
                if (list != null)
                {
                    var items = new List<ComboBoxItem>();
                    var lang = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                    int count = 0;
                    for (int i = 0; i < list.Length; i++)
                    {
                        string fontName = list[i].FamilyNames.ContainsKey(lang) ? list[i].FamilyNames[lang] : list[i].ToString();
                        if (fontName == Model.ConsoleSettng.FontName) count = i;
                        items.Add(new ComboBoxItem()
                        {
                            Content = fontName,
                            FontFamily = list[i],
                            IsSelected = (fontName == Model.ConsoleSettng.FontName)
                        });
                    }
                    box.ItemsSource = items;
                    box.SelectedIndex = count;
                }
                box.SelectionChanged += (sender, args) =>
                {
                    if (box.SelectedValue is ComboBoxItem item)
                    {
                        Model.ConsoleSettng.FontName = item.Content.ToString();
                    }
                };
            }

        }
    }
}

