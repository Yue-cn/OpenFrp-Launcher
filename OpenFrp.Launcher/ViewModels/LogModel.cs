using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class LogModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<ConsoleWrapper> consoleWrappers = new();

        [ObservableProperty]
        public int selectedIndex;

        [RelayCommand]
        internal async void RefreshList(Views.Logs page)
        {
            var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
            {
                Action = Core.Pipe.PipeModel.OfAction.Get_Logs,
            });
            consoleWrappers = new ObservableCollection<ConsoleWrapper>(resp.Logs?.ConsoleWrappers ?? new ConsoleWrapper[] { });
            
            //page.Items.GetBindingExpression(ItemsControl.ItemsSourceProperty)?.UpdateTarget();

            //var item = ((ConsoleWrapper)page.selectBox.SelectedValue);

            page.selectBox.GetBindingExpression(ComboBox.SelectedIndexProperty)?.UpdateTarget();
            int num = int.Parse(SelectedIndex.ToString());
            page.selectBox.GetBindingExpression(ComboBox.ItemsSourceProperty)?.UpdateTarget();

            /*SelectedIndex =*/ page.selectBox.SelectedIndex = num;
        }

        [RelayCommand]
        async void RemoveAllLogs(Views.Logs page)
        {
            await OfAppHelper.PipeClient.PushMessageAsync(new()
            {
                Action = Core.Pipe.PipeModel.OfAction.Clear_Logs,
            });
            ConsoleWrappers?.ToList().ForEach(item => item.Content.Clear());
            RefreshList(page);
        }

        [RelayCommand]
        async void RemoveSelectedLogs(Views.Logs page)
        {
            try
            {
                if (SelectedIndex != -1 &&
                    ConsoleWrappers?.Count > SelectedIndex &&
                    ConsoleWrappers?.Count != 0)
                {
                    await OfAppHelper.PipeClient.PushMessageAsync(new()
                    {
                        Action = Core.Pipe.PipeModel.OfAction.Clear_Logs,
                        FrpMessage = new()
                        {
                            Tunnel = ConsoleWrappers?[SelectedIndex].UserTunnelModel
                        }
                    });
                    ConsoleWrappers?[SelectedIndex].Content.Clear();
                }
            }
            catch { }
            RefreshList(page);
        }
        [RelayCommand]
        async void SaveSelectLogs(Views.Logs page)
        {
            try
            {
                if (SelectedIndex != -1 &&
                    ConsoleWrappers?.Count > SelectedIndex &&
                    ConsoleWrappers?.Count != 0)
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "日志文件|*.log"
                    };

                    if (dialog.ShowDialog() is true)
                    {
                        var stream = new StreamWriter(dialog.FileName,false,Encoding.UTF8,2048);
                        ConsoleWrappers?[SelectedIndex].Content.ForEach(async content => await stream.WriteLineAsync(content.Content));
                        await stream.FlushAsync();
                        stream.Close();
                    }
                    if (!OfAppHelper.HasDialog)
                    {
                        OfAppHelper.HasDialog = true;
                        await new ContentDialog()
                        {
                            Title = "日志保存成功",
                            CloseButtonText = "确定",
                            DefaultButton = ContentDialogButton.Close
                        }.ShowAsync();
                        OfAppHelper.HasDialog = false;
                    }
                }
            }
            catch { }
        }

        public List<LogContent> WrapperValue
        {
            get
            {
                if (SelectedIndex != -1 && ConsoleWrappers?.Count >= SelectedIndex && ConsoleWrappers?.Count != 0)
                {
                    return ConsoleWrappers?[SelectedIndex].Content ?? new();
                }
                return new();
            }
        }

        public OfSettings.ConsoleModel ConsoleModel
        {
            get => OfSettings.Instance.Console;
            set => OfSettings.Instance.Console = value;
        }


    }

    public class FontFamilyConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return new FontFamily(str);
            }
            return new FontFamily("微软雅黑");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontFamily fam)
            {
                return fam.ToString();
            }
            return "微软雅黑";
        }
    }
}
