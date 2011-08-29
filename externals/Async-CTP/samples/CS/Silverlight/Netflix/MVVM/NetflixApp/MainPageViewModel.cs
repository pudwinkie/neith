using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NetflixApp.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace NetflixApp
{
    public class MainPageViewModel : DependencyObject
    {
        private const string defaultQuery = "http://odata.netflix.com/Catalog/Titles?$inlinecount=allpages&$filter=ReleaseYear%20le%201942";


        #region Dependency Properties

        public string Query
        {
            get { return (string)GetValue(QueryProperty); }
            set { SetValue(QueryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Query.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QueryProperty =
            DependencyProperty.Register("Query", typeof(string), typeof(MainPageViewModel), new PropertyMetadata(defaultQuery));



        public ObservableCollection<Movie> Movies
        {
            get { return (ObservableCollection<Movie>)GetValue(MoviesProperty); }
            set { SetValue(MoviesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Movies.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoviesProperty =
            DependencyProperty.Register("Movies", typeof(ObservableCollection<Movie>), typeof(MainPageViewModel), new PropertyMetadata(null));


        public double? Progress
        {
            get { return (double?)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Progress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double?), typeof(MainPageViewModel), new PropertyMetadata(0.0));


        public bool Fetching
        {
            get { return (bool)GetValue(FetchingProperty); }
            set { SetValue(FetchingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fetching.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FetchingProperty =
            DependencyProperty.Register("Fetching", typeof(bool), typeof(MainPageViewModel), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var vm = (MainPageViewModel)d;
                    vm.Fetch.Invalidate();
                    vm.Cancel.Invalidate();
                })));


        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set { SetValue(ProgressTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(MainPageViewModel), new PropertyMetadata(""));

        #endregion


        #region Commands

        private FetchCommand fetch = null;
        public FetchCommand Fetch { get { return fetch ?? (fetch = new FetchCommand(this)); } }

        public class FetchCommand : ICommand
        {
            MainPageViewModel vm;

            internal FetchCommand(MainPageViewModel vm)
            {
                this.vm = vm;
            }

            internal void Invalidate()
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }

            public bool CanExecute(object parameter)
            {
                return !vm.Fetching;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                vm.DoFetch();
            }
        }


        private CancelCommand cancel = null;
        public CancelCommand Cancel { get { return cancel ?? (cancel = new CancelCommand(this)); } }

        public class CancelCommand : ICommand
        {
            MainPageViewModel vm;

            internal CancelCommand(MainPageViewModel vm)
            {
                this.vm = vm;
            }

            internal void Invalidate()
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }

            public bool CanExecute(object parameter)
            {
                return vm.Fetching;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                vm.DoCancel();
            }
        }
        #endregion Commands


        NetflixQuery<Movie> query;
        CancellationTokenSource cancelSource;

        private async void DoFetch()
        {
            Fetching = true;

            Progress = null;
            ProgressText = "Loading...";

            cancelSource = new CancellationTokenSource();
            
            query = new NetflixQuery<Movie>(Query);
            Movies = query.Entities;
            Movies.CollectionChanged += delegate
            {
                if (query.EntitiesExpected != null)
                {
                    Progress = (double)Movies.Count / query.EntitiesExpected;
                    ProgressText = String.Format("Loaded {0} of {1} movies so far...", Movies.Count, query.EntitiesExpected);
                }
                else
                {
                    Progress = null;
                    ProgressText = String.Format("Loaded {0} movies so far...", Movies.Count, query.EntitiesExpected);
                }
            };

            try
            {
                await query.FetchEntitiesAsync(cancelSource.Token);

                ProgressText = String.Format("Loaded {0} movies.", Movies.Count);
            }
            catch (OperationCanceledException)
            {
                ProgressText = String.Format("Cancelled after {0} movies.", Movies.Count);
            }
            catch (Exception e)
            {
                ProgressText = "Error!";
                MessageBox.Show(e.ToString());
            }

            Progress = 1.0;

            Fetching = false;
        }

        private void DoCancel()
        {
            cancelSource.Cancel();
        }
    }
}
