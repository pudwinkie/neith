using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using Neith.Signpost.Services;

namespace Neith.Signpost
{
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }

        private void BeginInvoke(Action act)
        {
            Dispatcher.BeginInvoke(act);
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs ev)
        {
            Dispatcher.BeginInvoke(() => TileGridSizeChange(ev));

        }


        private int lastColumnCount = -1;
        private int lastRowCount = -1;

        private const double BASE_SIZE = 120;
        private const double SPLIT_SIZE = 12;
        private const double SPAN_SIZE = BASE_SIZE + SPLIT_SIZE;
        private static readonly GridLength GL_ZERO = new GridLength(0);
        private static readonly GridLength GL_SPAN = new GridLength(SPAN_SIZE);

        private void TileGridSizeChange(SizeChangedEventArgs ev)
        {
            var size = ev.NewSize;
            var columnCount = (int)Math.Floor(size.Width / SPAN_SIZE);
            var rowCount = (int)Math.Floor(size.Height / SPAN_SIZE);
            if (columnCount > 16) columnCount = 16;
            if (rowCount > 16) rowCount = 16;
            if (lastColumnCount == columnCount && lastRowCount == rowCount) return;

            lastColumnCount = columnCount;
            lastRowCount = rowCount;
            tbSize.Text = string.Format("size: (row,col)=({0},{1})", rowCount, columnCount);


            TileGrid.Width = SPAN_SIZE * columnCount;
            foreach (var item in TileGrid.ColumnDefinitions) {
                item.Width = columnCount > 0 ? GL_SPAN : GL_ZERO;
                columnCount--;
            }

            TileGrid.Height = SPAN_SIZE * rowCount;
            foreach (var item in TileGrid.RowDefinitions) {
                item.Height = rowCount > 0 ? GL_SPAN : GL_ZERO;
                rowCount--;
            }

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var command = tbCommand.Text;
            SendKeyTestAsync(command).Start();
        }

        private async Task SendKeyTestAsync(string command)
        {
            var ch = await Channels.GetSignpostChannelAsync();
            var time = await ch.SendKeysAsync(command);
            BeginInvoke(() =>
            {
                lbResult.Text = string.Format("[SendKeysAsync]time = {0}", time);
            });
        }

    }
}