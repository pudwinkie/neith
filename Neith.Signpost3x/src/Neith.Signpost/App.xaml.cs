using System.Reactive.Disposables;
using System.Windows;
using Neith.Signpost.Services;

namespace Neith.Signpost
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private readonly CompositeDisposable Tasks = new CompositeDisposable();

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Tasks.Dispose();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Tasks.Add(new WebServer());
        }

    }
}
