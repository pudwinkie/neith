using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;

namespace Neith.Signpost.Proxy
{
    /// <summary>
    /// SettingPage.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingPage : UserControl
    {
        /// <summary>設定オブジェクトへの参照</summary>
        public static object Setting { get { return _Setting.Value; } set { return; } }
        private static readonly Lazy<object> _Setting = new Lazy<object>(() =>
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return new DummySetting();
            return Properties.Settings.Default;
        });

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SettingPage()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) DataContext = new DummySetting();
            else DataContext = Properties.Settings.Default;
            InitializeComponent();
        }

        /// <summary>
        /// ダミーの設定オブジェクト。
        /// </summary>
        public class DummySetting : ReactiveObject
        {
            public string ChannelName { get { return _ChannelName; } set { this.RaiseAndSetIfChanged(a => a.ChannelName, value); } }
            private string _ChannelName = "DUMMY_ChannelName";
        }
    }
}
