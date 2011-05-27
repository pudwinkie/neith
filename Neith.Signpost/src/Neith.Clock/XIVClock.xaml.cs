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

namespace Neith.Clock
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class XIVClock : UserControl
    {
        public XIVClock()
        {
            InitializeComponent();
        }


        #region 依存関係プロパティ
        // 日時パラメータ
        public static readonly DependencyProperty YearProperty;
        public static readonly DependencyProperty MonthProperty;
        public static readonly DependencyProperty DayProperty;
        public static readonly DependencyProperty HourProperty;
        public static readonly DependencyProperty MinuteProperty;
        public static readonly DependencyProperty SecondProperty;

        static XIVClock()
        {
            try {
                // 日時パラメータ
                YearProperty = DependencyProperty.Register("Year", typeof(int), typeof(XIVClock), new FrameworkPropertyMetadata(0, (d, ev) => { ((XIVClock)d).UpdateYear(); }));
                MonthProperty = DependencyProperty.Register("Month", typeof(int), typeof(XIVClock), new FrameworkPropertyMetadata(0, (d, ev) => { ((XIVClock)d).UpdateMonth(); }));
                DayProperty = DependencyProperty.Register("Day", typeof(int), typeof(XIVClock), new FrameworkPropertyMetadata(0, (d, ev) => { ((XIVClock)d).UpdateDay(); }));
                HourProperty = DependencyProperty.Register("Hour", typeof(int), typeof(XIVClock), new FrameworkPropertyMetadata(0, (d, ev) => { ((XIVClock)d).UpdateHour(); }));
                MinuteProperty = DependencyProperty.Register("Minute", typeof(int), typeof(XIVClock), new FrameworkPropertyMetadata(0, (d, ev) => { ((XIVClock)d).UpdateMinute(); }));
                SecondProperty = DependencyProperty.Register("Second", typeof(int), typeof(XIVClock), new FrameworkPropertyMetadata(0, (d, ev) => { ((XIVClock)d).UpdateSecond(); }));
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>年を設定・取得します。</summary>
        public int Year { get { return (int)GetValue(YearProperty); } set { SetValue(YearProperty, value); } }
        /// <summary>月を設定・取得します。</summary>
        public int Month { get { return (int)GetValue(MonthProperty); } set { SetValue(MonthProperty, value); } }
        /// <summary>日を設定・取得します。</summary>
        public int Day { get { return (int)GetValue(DayProperty); } set { SetValue(DayProperty, value); } }
        /// <summary>時を設定・取得します。</summary>
        public int Hour { get { return (int)GetValue(HourProperty); } set { SetValue(HourProperty, value); } }
        /// <summary>分を設定・取得します。</summary>
        public int Minute { get { return (int)GetValue(MinuteProperty); } set { SetValue(MinuteProperty, value); } }
        /// <summary>秒を設定・取得します。</summary>
        public int Second { get { return (int)GetValue(SecondProperty); } set { SetValue(SecondProperty, value); } }

        private void UpdateYear()
        {
        }

        private void UpdateMonth()
        {
        }

        private void UpdateDay()
        {
        }

        private void UpdateHour()
        {
        }

        private void UpdateMinute()
        {
        }

        private void UpdateSecond()
        {
        }


        #endregion

    }
}