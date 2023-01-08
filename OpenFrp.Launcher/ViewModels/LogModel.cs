﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class LogModel : ObservableObject
    {








        [ObservableProperty]
        public ConsoleWrapper[] consoleWrappers = new ConsoleWrapper[] {};


        [ObservableProperty]
        public int selectedIndex;

        [RelayCommand]
        internal async void RefreshList(Views.Logs page)
        {
            var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
            {
                Action = Core.Pipe.PipeModel.OfAction.Get_Logs,
            });
            consoleWrappers = resp.Logs?.ConsoleWrappers ?? new ConsoleWrapper[] {};

            page.Items.GetBindingExpression(ItemsControl.ItemsSourceProperty)?.UpdateTarget();

            //var item = ((ConsoleWrapper)page.selectBox.SelectedValue);

            page.selectBox.GetBindingExpression(ComboBox.SelectedIndexProperty)?.UpdateTarget();
            int num = int.Parse(SelectedIndex.ToString());
            page.selectBox.GetBindingExpression(ComboBox.ItemsSourceProperty)?.UpdateTarget();

            SelectedIndex = page.selectBox.SelectedIndex = num;


        }

        public List<LogContent> WrapperValue
        {
            get
            {
                if (SelectedIndex != -1 && ConsoleWrappers?.Length >= SelectedIndex && ConsoleWrappers?.Length != 0)
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
}
