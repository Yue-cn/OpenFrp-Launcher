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

namespace OpenFrp.Launcher.Views
{
    /// <summary>
    /// Logs.xaml 的交互逻辑
    /// </summary>
    public partial class Logs : Page
    {
        public Logs()
        {
            InitializeComponent();

            //PreviewContent
        }

        ViewModels.LogModel LogModel
        {
            get => (ViewModels.LogModel)DataContext;
            set => DataContext = value;
        }

        private bool isLinenting { get; set; }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            isLinenting = true;
            this.Unloaded += (s, e) => isLinenting = false;
            while (isLinenting)
            {
                LogModel?.RefreshList(this);
                await Task.Delay(1500);
            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Items.GetBindingExpression(ItemsRepeater.ItemsSourceProperty)?.UpdateTarget();
        }
    }
}
