using System.Windows;
using CustomWindow;

namespace CustomWindowDemo
{
    /// <summary>
    /// Interaction logic for MyWindow.xaml
    /// </summary>
    public partial class Window1 : StandardWindow
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void ButtonDemoWindow1_Click(object sender, RoutedEventArgs e)
        {
            DemoWindow1 win = new DemoWindow1();
            win.Show();
        }

        private void ButtonDemoWindow2_Click(object sender, RoutedEventArgs e)
        {
            DemoWindow2 win = new DemoWindow2();
            win.Show();
        }

        private void ButtonDemoWindow3_Click(object sender, RoutedEventArgs e)
        {
            DemoWindow3 win = new DemoWindow3();
            win.Show();
        }

        private void ButtonDemoWindow4_Click(object sender, RoutedEventArgs e)
        {
            DemoWindow4 win = new DemoWindow4();
            win.Show();
        }

        private void ButtonDemoWindow5_Click(object sender, RoutedEventArgs e)
        {
            DemoWindow5 win = new DemoWindow5();
            win.Show();
        }

        private void ButtonDemoEssentialWindow1_Click(object sender, RoutedEventArgs e)
        {
            DemoEssentialWindow win = new DemoEssentialWindow();
            win.Show();
        }
    }
}
