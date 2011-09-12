using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Neith.Signpost.Proxy
{
    /// <summary>
    /// SettingPage.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingPage : UserControl
    {
        public static object Setting { get { return _Setting.Value; } set { return; } }
        private static readonly Lazy<object> _Setting = new Lazy<object>(() =>
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return new DummySetting();
            return Properties.Settings.Default;
        });

        public SettingPage()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) DataContext = new DummySetting();
            else DataContext = Properties.Settings.Default;
            InitializeComponent();
        }

        public class DummySetting : INotifyPropertyChanged
        {
            public string ChannelName { get { return _ChannelName; } set { _ChannelName = value; } }
            private string _ChannelName = "DUMMY_ChannelName";

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
