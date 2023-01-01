using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenFrp.Launcher.Controls
{
    public partial class BaseView : UserControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(BaseView));



        public object SafeBlockTop
        {
            get { return (object)GetValue(SafeBlockTopProperty); }
            set { SetValue(SafeBlockTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SafeBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SafeBlockTopProperty =
            DependencyProperty.Register("SafeBlockTop", typeof(object), typeof(BaseView));



        public void ExcuteScroll(MouseWheelEventArgs e) => ((ScrollViewerEx)GetTemplateChild("_ScrollViewer"))?.ExcuteScroll(e);
    }
}
