using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

static class Paddl2Game2csAsync
{
   static Application app = new Application();
   static Border frame = new Border {BorderThickness=new Thickness(15.0), BorderBrush=new SolidColorBrush(Color.FromRgb(0,0,0)), Width=432.0, Height=507.0};
   static Window window = new Window {Title="Super Breakaway C# Async!", SizeToContent=SizeToContent.WidthAndHeight, Content=frame};

    [STAThread]
    static void Main()
    {
        app.Startup += MainAsync;
        app.Run(window);
    }

    static async void MainAsync(object sender, StartupEventArgs e)
    {
        // Introduction screen...
        var cb = GameLogic.MakeIntroScreen();  // returns a tuple (content, button)
        frame.Child = cb.Item1;
        await ButtonClick(cb.Item2);

        while (true)
        {
            // Gameplay screen...
            var dat = GameLogic.InitializeGameData();
            frame.Child = GameLogic.MakeGameScreen(dat);
            while (dat.remaining > 0 && dat.active > 0)
            {
                await TaskEx.Delay(10);
                GameLogic.MovePaddle(dat);
                GameLogic.UpdateBalls(dat);
            }

            // Victory screen...
            var cs = GameLogic.MakeVictoryScreen(dat.remaining); // returns a tuple (content,sound)
            frame.Child = cs.Item1;
            await TaskEx.Delay(3000);
            cs.Item2.Stop();
        }
    }

    // A helper method to make it easier to await for a button-click
    static Task ButtonClick(Button b)
    {
        var tcs = new TaskCompletionSource<object>();
        RoutedEventHandler handler = null;
        handler = (sender, e) => { b.Click -= handler; tcs.TrySetResult(null); };
        b.Click += handler;
        return tcs.Task;
    }
}