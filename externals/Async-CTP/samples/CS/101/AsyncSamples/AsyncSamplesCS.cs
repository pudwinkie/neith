//Copyright (C) Microsoft Corporation.  All rights reserved.

using SampleSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Collections.ObjectModel;
using System.Text;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Media;
using Application = System.Windows.Application;

using Console = SampleSupport.Console;

namespace Samples
{
    [Title("101 C# Async Samples")]
    [Prefix("Async")]
    [Before("C# 4.0")]
    [After("await")]
    [Extension("cs")]
    public class AsyncSamplesCS : SampleHarness
    {
        private CancellationTokenSource cts;

        [Category("Introduction to await")]
        [Title("await - Single Network Request")]
        [Description("Performs a web request asynchronously using await.")]
        public async void AsyncIntroSingle()
        {
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov")));
        }

        [Description("Performs a web request asynchronously using a separate continuation method.")]
        [LinkedMethod("AsyncIntroSingleBefore_DownloadStringCompleted")]
        public void AsyncIntroSingleBefore()
        {
            var client = new WebClient();

            client.DownloadStringCompleted += AsyncIntroSingleBefore_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.weather.gov"));
        }

        void AsyncIntroSingleBefore_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);
        }

        [Category("Introduction to await")]
        [Title("await - Serial Network Requests")]
        [Description("Performs a series of web requests in sequence using await.  The next request will not be issued until the previous request completes.")]
        public async void AsyncIntroSerial()
        {
            var client = new WebClient();

            WriteLinePageTitle(await client.DownloadStringTaskAsync(new Uri("http://www.weather.gov")));
            WriteLinePageTitle(await client.DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/")));
            WriteLinePageTitle(await client.DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/")));
        }

        [Description("Performs a series of web requests using separate continuation methods.")]
        [LinkedMethod(
            "AsyncIntroSerialBefore_DownloadStringCompleted_1",
            "AsyncIntroSerialBefore_DownloadStringCompleted_2",
            "AsyncIntroSerialBefore_DownloadStringCompleted_3")]
        public void AsyncIntroSerialBefore()
        {
            var client = new WebClient();

            client.DownloadStringCompleted += AsyncIntroSerialBefore_DownloadStringCompleted_1;
            client.DownloadStringAsync(new Uri("http://www.weather.gov"));
        }

        void AsyncIntroSerialBefore_DownloadStringCompleted_1(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);

            var client = new WebClient();

            client.DownloadStringCompleted += AsyncIntroSerialBefore_DownloadStringCompleted_2;
            client.DownloadStringAsync(new Uri("http://www.weather.gov/climate/"));
        }

        void AsyncIntroSerialBefore_DownloadStringCompleted_2(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);

            var client = new WebClient();

            client.DownloadStringCompleted += AsyncIntroSerialBefore_DownloadStringCompleted_3;
            client.DownloadStringAsync(new Uri("http://www.weather.gov/rss/"));
        }

        void AsyncIntroSerialBefore_DownloadStringCompleted_3(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);
        }

        [Category("Introduction to await")]
        [Title("await - Parallel Network Requests")]
        [Description("Performs a set of web requests in parallel.\r\n\r\nCalling a Task-returning method such as DownloadStringTaskAsync will always kick off the operation immediately, but control flow does not wait for completion until the await keywords later on.  Here, the output will always occur in order as the results are awaited in order.")]
        public async void AsyncIntroParallel()
        {
            Task<string> page1 = new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov"));
            Task<string> page2 = new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/"));
            Task<string> page3 = new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/"));

            WriteLinePageTitle(await page1);
            WriteLinePageTitle(await page2);
            WriteLinePageTitle(await page3);
        }

        public void WriteLinePageTitle(string page)
        {
            Console.WriteLine(GetPageTitle(page));
        }

        public string GetPageTitle(string page)
        {
            Regex titleRegex = new Regex(@"\<title\>(?<title>.*)\<\/title\>", RegexOptions.IgnoreCase);
            var match = titleRegex.Match(page);
            if (match.Success)
            {
                return "Page title: " + match.Groups["title"].Value;
            }
            else
            {
                return "Page has no title";
            }
        }


        [Category("UI Responsiveness")]
        [Title("Responsive UI during network requests")]
        [Description("Performs a web request asynchronously using await.  This example simulates slow network conditions by delaying the requests.\r\n\r\nDrag the window around or scroll the tree to see that the UI is still responsive while the download occurs.")]
        public async void AsyncResponsiveNetwork()
        {
            WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov")));
            WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/climate/")));
            WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/rss/")));
        }

        public async Task<string> DownloadStringTaskSlowNetworkAsync(Uri address)
        {
            await TaskEx.Delay(500);    // Simulate 500ms of network delay
            return await new WebClient().DownloadStringTaskAsync(address);
        }

        public async Task<string> DownloadStringTaskSlowNetworkAsync(Uri address, CancellationToken cancellationToken)
        {
            await TaskEx.Delay(500);    // Simulate 500ms of network delay
            return await new WebClient().DownloadStringTaskAsync(address, cancellationToken);
        }


        [Category("UI Responsiveness")]
        [Title("Responsive UI during CPU-bound tasks")]
        [Description("Processes data on a background thread in the thread pool.\r\n\r\nDrag the window around or scroll the tree to see that the UI is still responsive while the processing occurs.")]
        [LinkedMethod("ProcessDataAsync")]
        public async Task AsyncResponsiveCPU()
        {
            Console.WriteLine("Processing data...  Drag the window around or scroll the tree!");
            Console.WriteLine();
            int[] data = await ProcessDataAsync(GetData(), 16, 16);
            Console.WriteLine();
            Console.WriteLine("Processing complete.");
        }

        public Task<int[]> ProcessDataAsync(byte[] data, int width, int height)
        {
            return TaskEx.Run(() =>
            {
                var result = new int[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Thread.Sleep(10);   // simulate processing cell [x,y]
                    }
                    Console.WriteLine("Processed row {0}", y);
                }

                return result;
            });
        }

        public byte[] GetData()
        {
            return new byte[256];
        }


        [Category("Cancellation")]
        [Title("CancellationToken - Single Request")]
        [Description("Performs a web request using await, passing a CancellationToken to allow cancellation.  This example simulates slow network conditions by delaying the requests.")]
        [LinkedField("cts")]
        public async Task AsyncCancelSingle()
        {
            cts = new CancellationTokenSource();

            try
            {
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov"), cts.Token));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Downloading canceled.");
            }
        }

        [Category("Cancellation")]
        [Title("CancellationToken - Single CPU-bound Request")]
        [Description("Processes data on a background thread in the thread pool.\r\n\r\nThe CancellationToken is polled once per iteration of the y for loop to see if cancellation has been requested, and if so an OperationCanceledException is thrown.")]
        [LinkedMethod("ProcessAsync")]
        [LinkedField("cts")]
        public async Task AsyncCancelSingleCPU()
        {
            cts = new CancellationTokenSource();

            try
            {
                int[] data = await ProcessAsync(GetData(), 16, 16, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Processing canceled.");
            }
        }

        public Task<int[]> ProcessAsync(byte[] data, int width, int height, CancellationToken cancellationToken)
        {
            return TaskEx.Run(() =>
            {
                var result = new int[width * height];

                for (int y = 0; y < height; y++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    for (int x = 0; x < width; x++)
                    {
                        Thread.Sleep(10);   // simulate processing cell [x,y]
                    }
                    Console.WriteLine("Processed row {0}", y);
                }

                return result;
            });
        }

        [Category("Cancellation")]
        [Title("CancellationToken - Serial Requests")]
        [Description("Performs a series of web requests using await in sequence.  This example simulates slow network conditions by delaying the requests.\r\n\r\nThe same CancellationToken is used across multiple requests so that whichever operation happens to be running at the moment can be canceled.")]
        [LinkedField("cts")]
        public async Task AsyncCancelSingleSerial()
        {
            cts = new CancellationTokenSource();

            try
            {
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov"), cts.Token));
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/climate/"), cts.Token));
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/rss/"), cts.Token));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Download canceled.");
            }
        }

        [Category("Cancellation")]
        [Title("CancellationToken - Parallel Requests")]
        [Description("Performs a set of web requests in parallel.  This example simulates slow network conditions by delaying the requests.\r\n\r\nThe same CancellationToken is used across multiple requests so that all outstanding requests will be canceled together.")]
        [LinkedField("cts")]
        public async Task AsyncCancelSingleParallel()
        {
            cts = new CancellationTokenSource();

            try
            {
                Task<string> page1 = DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov"), cts.Token);
                Task<string> page2 = DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/climate/"), cts.Token);
                Task<string> page3 = DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/rss/"), cts.Token);

                WriteLinePageTitle(await page1);
                WriteLinePageTitle(await page2);
                WriteLinePageTitle(await page3);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Download canceled.");
            }
        }

        [Category("Cancellation")]
        [Title("CancelAfter")]
        [Description("Performs a long-running, cancellable operation, automatically cancelling it after 3 seconds if it does not either complete or get canceled by the user.  Because this operation takes between 2 and 4 seconds to complete, cancellation will occur about half of the time.")]
        [LinkedField("cts")]
        [LinkedMethod(
            "TimeoutAfter",
            "LongRunningOperation")]
        public async void AsyncCancelAfter()
        {
            cts = new CancellationTokenSource();

            try
            {
                cts.CancelAfter(3000);
                int result = await CancellableOperation(cts.Token);
                Console.WriteLine("Operation completed successfully.  Result is {0}.", result);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Canceled!");
            }
        }


        [Category("Progress")]
        [Title("Progress<T>")]
        [Description("Gets all directory paths on the C: drive, with an Progress<T> object passed in to receive progres notifications.\r\n\r\nNote that all calls into the ProgressChanged lambda are occurring while AsyncProgressPolling is suspended awaiting GetAllDirsAsync.")]
        [LinkedField("cts")]
        [LinkedMethod("GetAllDirsAsync")]
        [LinkedClass("GetAllDirsPartialResult")]
        public async Task AsyncProgressPolling()
        {
            cts = new CancellationTokenSource();
            var progress = new Progress<GetAllDirsPartialResult>();

            try
            {
                progress.ProgressChanged += (source, e) =>
                {
                    ProgressBar.Value = e.Count % 100;
                };
                foreach (var item in await GetAllDirsAsync(@"c:\", cts.Token, progress))
                {
                    Console.WriteLine(item);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation canceled.");
            }
        }

        public class GetAllDirsPartialResult
        {
            public IList<string> Directories;
            public int Count;
        }

        public async Task<string[]> GetAllDirsAsync(string root, CancellationToken cancel, IProgress<GetAllDirsPartialResult> progress)
        {
            var todo = new Queue<string>(); todo.Enqueue(root);
            var results = new List<string>(1000);
            while (todo.Count > 0)
            {
                cancel.ThrowIfCancellationRequested();
                if (results.Count >= 300) break;
                var dir = todo.Dequeue(); results.Add(dir);
                if (progress != null) progress.Report(new GetAllDirsPartialResult() { Directories = new ReadOnlyCollection<string>(results), Count = results.Count });
                try { foreach (var subdir in await GetDirectoriesAsync(dir)) todo.Enqueue(subdir); }
                catch (UnauthorizedAccessException) { }
            }
            return results.ToArray();
        }

        public static Task<string[]> GetDirectoriesAsync(string dir)
        {
            /* Simulates an async OS API for enumerating directories */


            // * * *
            // If you're seeing exceptions on this line, go to "Debug|Options and Settings" and
            // turn off "Just My Code" to disable display of handled first-chance exceptions.
            return TaskEx.Run(() => System.IO.Directory.GetDirectories(dir));
            // * * *
        }


        [Category("Exceptions")]
        [Title("try-catch")]
        [Description("Exceptions thrown by an awaited method may be naturally caught within a try-catch block.")]
        public async void AsyncTryCatch()
        {
            try
            {
                WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/clmitae/")));
            }
            catch
            {
                Console.WriteLine("Error loading page.");
            }
        }

        [Category("Exceptions")]
        [Title("try-finally")]
        [Description("Exceptions thrown by an awaited method within a try-finally will naturally trigger the relevant finally block.")]
        public async void AsyncTryFinally()
        {
            Console.WriteLine("Download process beginning...");
            try
            {
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov"), cts.Token));
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/clmitae/"), cts.Token));
                WriteLinePageTitle(await DownloadStringTaskSlowNetworkAsync(new Uri("http://www.weather.gov/rss/"), cts.Token));
            }
            catch
            {
                Console.WriteLine("There was an error downloading the pages.");
            }
            finally
            {
                Console.WriteLine("Download process completed.");
            }
            Console.WriteLine("Success!");
        }

        [Category("Exceptions")]
        [Title("throw")]
        [Description("Exceptions thrown by your own Task-returning methods may be caught naturally by a caller awaiting the method.")]
        [LinkedMethod("CheckPageSizes")]
        public async void AsyncThrow()
        {
            var uris = new List<Uri> { new Uri("http://www.weather.gov/climate/"), new Uri("http://www.weather.gov"), new Uri("http://www.weather.gov/rss/") };

            try
            {
                await CheckPageSizes(uris);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task CheckPageSizes(IList<Uri> uris)
        {
            foreach (var uri in uris)
            {
                string page = await new WebClient().DownloadStringTaskAsync(uri);

                if (page.Length < 50000) throw new Exception(String.Format("{0} is too small!", uri));

                Console.WriteLine("{0} contains {1} bytes.", uri, page.Length);
            }
        }


        [Category("Declaring Async Methods")]
        [Title("Returning Task")]
        [Description("Shows how your own TaskReturningMethod can easily return a Task representing control flow reaching the end of the method.\r\n\r\nTaskReturningMethod returns a Task after its first await, which is then awaited by AsyncReturnTask.  This task will complete once control reaches the end of TaskReturningMethod.  Only then does control return to AsyncReturnTask.")]
        [LinkedMethod("TaskReturningMethod")]
        public async void AsyncReturnTask()
        {
            Console.WriteLine("*** BEFORE CALL ***");
            await TaskReturningMethod();
            Console.WriteLine("*** AFTER CALL ***");
        }

        private async Task TaskReturningMethod()
        {
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov")));
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/")));
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/")));
        }

        [Category("Declaring Async Methods")]
        [Title("Returning void")]
        [Description("Shows how your own VoidReturningMethod can still await asynchronous operations, even though it returns void itself.  This becomes a \"fire and forget\" method.\r\n\r\nVoidReturningMethod returns to its caller after its first await.  Control then returns immediately to AsyncReturnVoid.")]
        [LinkedMethod("VoidReturningMethod")]
        public void AsyncReturnVoid()
        {
            Console.WriteLine("*** BEFORE CALL ***");
            VoidReturningMethod();
            Console.WriteLine("*** AFTER CALL ***");
        }

        private async void VoidReturningMethod()
        {
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov")));
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/")));
            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/")));
        }

        [Category("Declaring Async Methods")]
        [Title("Returning Task<T>")]
        [Description("Shows how your own TaskOfStringReturningMethod can easily return a Task<T> representing a result value of type T.\r\n\r\nTaskOfStringReturningMethod returns a Task<string> after its first await, which is then awaited by AsyncReturnTaskOfT.  This task will complete once control reaches the end of TaskOfStringReturningMethod.  Only then does control return to AsyncReturnTaskOfT.")]
        [LinkedMethod("TaskOfStringReturningMethod")]
        public async void AsyncReturnTaskOfT()
        {
            Console.WriteLine("*** BEFORE CALL ***");
            Console.WriteLine(await TaskOfStringReturningMethod());
            Console.WriteLine("*** AFTER CALL ***");
        }

        private async Task<string> TaskOfStringReturningMethod()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetPageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov"))));
            sb.AppendLine(GetPageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/"))));
            sb.AppendLine(GetPageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/"))));
            return sb.ToString();
        }


        public async Task<int> LongRunningOperation()
        {
            Console.WriteLine("Attempting long-running operation...");
            
            return await TaskEx.RunEx<int>(async () =>
            {
                // Simulate a process that takes between 2 and 4 seconds to complete.
                var r = new Random();
                await TaskEx.Delay(r.Next(2000, 4000));
                return 123;
            });
        }

        public async Task<int> CancellableOperation(CancellationToken cancel)
        {
            Console.WriteLine("Attempting long-running, cancellable operation...");

            return await TaskEx.RunEx<int>(async () =>
            {
                // Simulate a process that takes between 2 and 4 seconds to complete.
                var r = new Random();
                await TaskEx.Delay(r.Next(2000, 4000), cancel);
                return 123;
            });
        }


        [Category("Combinators")]
        [Title("Task.WhenAll")]
        [Description("Performs a set of web requests in parallel, awaiting WhenAll to continue when all tasks complete.\r\n\r\nUsing WhenAll can be easier than awaiting each Task individually when deaing with a set of Tasks.")]
        public async void AsyncWhenAll()
        {
            Uri[] uris = { new Uri("http://www.weather.gov"), new Uri("http://www.weather.gov/climate/"), new Uri("http://www.weather.gov/rss/") };

            string[] pages = await TaskEx.WhenAll(from uri in uris select new WebClient().DownloadStringTaskAsync(uri));

            foreach (string page in pages)
            {
                WriteLinePageTitle(page);
            }
        }

        [Category("Combinators")]
        [Title("Task.WhenAny - Redundancy")]
        [Description("Requests a set of buy/sell recommendations from 3 different servers, continuing once one returns a value.\r\n\r\nUsing WhenAny in this way enables redundancy across multiple data sources.")]
        [LinkedMethod(
            "GetBuyRecommendation1Async",
            "GetBuyRecommendation2Async",
            "GetBuyRecommendation3Async")]
        public async void AsyncWhenAnyRedundancy()
        {
            string symbol = "ABCXYZ";

            var recommendations = new List<Task<bool>>() 
            { 
                GetBuyRecommendation1Async(symbol), 
                GetBuyRecommendation2Async(symbol),
                GetBuyRecommendation3Async(symbol)
            };
            Task<bool> recommendation = await TaskEx.WhenAny(recommendations);
            if (await recommendation)
            {
                Console.WriteLine("Buy stock {0}!", symbol);
            }
            else
            {
                Console.WriteLine("Sell stock {0}!", symbol);
            }
        }

        public async Task<bool> GetBuyRecommendation1Async(string symbol)
        {
            await TaskEx.Delay(500);  // Simulate 500ms delay in fetching recommendation 1
            return true;
        }

        public async Task<bool> GetBuyRecommendation2Async(string symbol)
        {
            await TaskEx.Delay(250);   // Simulate 250ms delay in fetching recommendation 2
            return true;
        }

        public async Task<bool> GetBuyRecommendation3Async(string symbol)
        {
            await TaskEx.Delay(1000);   // Simulate 1s delay in fetching recommendation 3
            return true;
        }

        [Category("Combinators")]
        [Title("Task.WhenAny - Interleaving")]
        [Description("Performs a set of web requests in sequence.\r\n\r\nUsing WhenAny in this way enables interleaving the requests one at a time.")]
        public async void AsyncWhenAnyInterleaving()
        {
            Uri[] uris = { new Uri("http://www.weather.gov"), new Uri("http://www.weather.gov/climate/"), new Uri("http://www.weather.gov/rss/") };

            List<Task<string>> downloadTasks = (from uri in uris select new WebClient().DownloadStringTaskAsync(uri)).ToList();

            while (downloadTasks.Count > 0)
            {
                Task<string> downloadTask = await TaskEx.WhenAny(downloadTasks);
                downloadTasks.Remove(downloadTask);

                string page = await downloadTask;
                WriteLinePageTitle(page);
            }
        }

        [Category("Combinators")]
        [Title("Task.WhenAny - Throttling")]
        [Description("Performs a set of web requests in parallel, capping at no more than 4 requests outstanding at a time.\r\n\r\nUsing WhenAny in this way enables throttling sets of parallel requests.")]
        public async void AsyncWhenAnyThrottling()
        {
            const int CONCURRENCY_LEVEL = 4;    // Maximum of 4 requests at a time

            Uri[] uris = { new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h000.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h001.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h002.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h003.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h010.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h011.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h012.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h013.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h020.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h021.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h022.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h023.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h030.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h031.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h032.jpeg?g=400"),
                           new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h033.jpeg?g=400"),
                         };
            int nextIndex = 0;
            var downloadTasks = new List<Task<string>>();
            while (nextIndex < CONCURRENCY_LEVEL && nextIndex < uris.Length)
            {
                Console.WriteLine("Queuing up initial download #{0}.", nextIndex + 1);
                downloadTasks.Add(new WebClient().DownloadStringTaskAsync(uris[nextIndex]));
                nextIndex++;
            }

            while (downloadTasks.Count > 0)
            {
                try
                {
                    Task<string> downloadTask = await TaskEx.WhenAny(downloadTasks);
                    downloadTasks.Remove(downloadTask);

                    string str = await downloadTask;
                    int length = str.Length;

                    Console.WriteLine("* Downloaded {0}-byte image.", length);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }

                if (nextIndex < uris.Length)
                {
                    Console.WriteLine("New download slot available.  Queuing up download #{0}.", nextIndex + 1);
                    downloadTasks.Add(new WebClient().DownloadStringTaskAsync(uris[nextIndex]));
                    nextIndex++;
                }
            }

        }

        [Category("Combinators")]
        [Title("Task.Delay")]
        [Description("Delays for 3 seconds before printing a second message.\r\n\r\nNotice that the UI is still responsive during the delay.")]
        public async void AsyncDelay()
        {
            Console.WriteLine("Before the delay.");
            await TaskEx.Delay(3000);
            Console.WriteLine("After the delay.");
        }


        [Category("Building Combinators")]
        [Title("TimeoutAfter")]
        [Description("Performs a long-running operation with a 3-second timeout.  Because this operation takes between 2 and 4 seconds to complete, the timeout will be hit about half of the time.\r\n\r\nThis shows how you can easily write your own combinators that operate on Tasks to simplify common patterns you encounter.")]
        [LinkedMethod(
            "TimeoutAfter",
            "LongRunningOperation")]
        public async void AsyncTimeoutAfter()
        {
            try
            {
                int result = await TimeoutAfter(LongRunningOperation(), 3000);
                Console.WriteLine("Operation completed successfully.  Result is {0}.", result);
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Timeout - operation took longer than 3 seconds.");
            }
        }

        public static async Task<T> TimeoutAfter<T>(Task<T> task, int delay)
        {
            await TaskEx.WhenAny(task, TaskEx.Delay(delay));

            if (!task.IsCompleted)
                throw new TimeoutException("Timeout hit.");

            return await task;
        }

        [Category("Building Combinators")]
        [Title("RetryOnFault")]
        [Description("Performs a long-running operation with a 3-second timeout, retrying up to 3 times if the operation times out.\r\n\r\nThis combinator builds on the TimeoutAfter combinator defined above, showing how one combinator can leverage another.")]
        [LinkedMethod(
            "RetryOnFault",
            "LongRunningOperation")]
        public async void AsyncRetryOnFault()
        {
            try
            {
                int result = await RetryOnFault(() => TimeoutAfter(LongRunningOperation(), 3000), 3);
                Console.WriteLine("Operation completed successfully.  Result is {0}.", result);
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Operation timed out 3 times.  Giving up.");
            }
        }

        public static async Task<T> RetryOnFault<T>(Func<Task<T>> function, int maxTries)
        {
            for (int i = 0; i < maxTries; i++)
            {
                try { return await function(); }
                catch { if (i == maxTries - 1) throw; }
            }
            return default(T);
        }


        [Category("Migration")]
        [Title("From APM")]
        [Description("Defines a ReadAsync method that wraps an existing APM API, Stream.BeginRead and Stream.EndRead.\r\n\r\nOn the desktop, Task<T>.Factory.FromAsync would handle all of the complexity for you.  On Silverlight, you can wrap such APIs manually, as in ReadAsync.")]
        [LinkedMethod("ReadAsync")]
        public async void AsyncFromAPM()
        {
            var response = await WebRequest.Create("http://www.weather.gov").GetResponseAsync();
            var stream = response.GetResponseStream();
            var buffer = new byte[16];
            int count;
            while ((count = await ReadAsync(stream, buffer, 0, 16)) > 0)
            {
                Console.Write(Encoding.UTF8.GetString(buffer, 0, count));
            }
        }

        public static Task<int> ReadAsync(Stream stream, byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<int>();
            stream.BeginRead(buffer, offset, count, iar =>
            {
                try { tcs.TrySetResult(stream.EndRead(iar)); }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }, null);
            return tcs.Task;
        }


        [Category("Migration")]
        [Title("From EAP")]
        [Description("Defines our own DownloadStringAsync method that wraps an existing EAP API, WebClient.DownloadStringAsync.\r\n\r\nEAP methods are a bit more involved to wrap than APM methods.  This implementation also handles progress notifications and cancellation.")]
        [LinkedMethod("DownloadStringAsync")]
        [LinkedField("cts")]
        public async Task AsyncFromEAP()
        {
            cts = new CancellationTokenSource();
            var progress = new Progress<DownloadProgressChangedEventArgs>();
            progress.ProgressChanged += (sender, e) => { ProgressBar.Value = e.ProgressPercentage; };

            try
            {
                WriteLinePageTitle(await DownloadStringAsync(new Uri("http://www.weather.gov"), cts.Token, progress));
            }
            catch
            {
                Console.WriteLine("Downloading canceled.");
            }
        }

        public static Task<string> DownloadStringAsync(Uri address, CancellationToken cancel, IProgress<DownloadProgressChangedEventArgs> progress)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<string>(address);
            var webClient = new WebClient();

            // Register the cancellation token
            var ctr = cancel.Register(webClient.CancelAsync);

            // Setup the callback event handlers
            webClient.DownloadProgressChanged += (s, e) => progress.Report(e);
            webClient.DownloadStringCompleted += (s, e) =>
            {
                ctr.Dispose();
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };

            // Start the async operation.
            webClient.DownloadStringAsync(address, tcs);

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [Category("Migration")]
        [Title("From synchronous - CopyTo")]
        [Description("This example shows the minor changes needed to migrate an existing synchronous CopyTo method to become CopyToAsync.  Scroll down to see the CopyTo and the translated CopyToAsync implementations.\r\n\r\nThe core transformation just involves calling ReadAsync/WriteAsync instead of Read/Write, and then awaiting each of those calls.  This example then adds progress support by accepting an IProgress<T> parameter and reporting progress to it, and adds cancellation by accepting a CancellationToken parameter and polling it each time through the loop.")]
        [LinkedField("cts")]
        [LinkedMethod("CopyTo", "CopyToAsync")]
        public async Task AsyncCopyTo()
        {
            cts = new CancellationTokenSource();

            // Download a file.
            using (var source = await new WebClient().OpenReadTaskAsync(new Uri("http://www.weather.gov/climate/")))
            {
                // Create the streams.
                MemoryStream destination = new MemoryStream();

                try
                {
                    Console.WriteLine("Source length: {0}", source.Length.ToString());

                    var progress = new Progress<long>();
                    progress.ProgressChanged += (sender, e) => Console.WriteLine("{0} bytes read.", e);

                    // Copy source to destination.
                    await CopyToAsync(source, destination, cts.Token, progress);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Copy canceled.");
                }

                Console.WriteLine("Destination length: {0}", destination.Length.ToString());
            }
        }

        public static async Task CopyToAsync(Stream source, Stream destination,
                                             CancellationToken cancellationToken,
                                             IProgress<long> progress)
        {
            // New asynchronous implementation:

            var buffer = new byte[0x1000];
            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead);
                cancellationToken.ThrowIfCancellationRequested();   // cancellation support
                totalRead += bytesRead;
                progress.Report(totalRead);                         // progress support
            }
        }

        public static void CopyTo(Stream source, Stream destination)
        {
            // Old synchronous implementation:

            var buffer = new byte[0x1000];
            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalRead += bytesRead;
            }
        }


        [Category("Switching Threads")]
        [Title("CPU-intensive work - Task.Run")]
        [Description("Performs CPU-intensive work represented in a lambda expression by dispatching this work to a thread pool thread.")]
        [LinkedMethod("DoCpuIntensiveWork")]
        public async void AsyncRunCPU()
        {
            Console.WriteLine("On the UI thread.");

            int result = await TaskEx.Run(() =>
            {
                Console.WriteLine("Starting CPU-intensive work on background thread...");
                int work = DoCpuIntensiveWork();
                Console.WriteLine("Done with CPU-intensive work!");
                return work;
            });

            Console.WriteLine("Back on the UI thread.  Result is {0}.", result);
        }

        public int DoCpuIntensiveWork()
        {
            // Simulate some CPU-bound work on the background thread:
            Thread.Sleep(5000);
            return 123;
        }

        [Category("Switching Threads")]
        [Title("Avoiding thread switches - Task.ConfigureAwait")]
        [Description("Avoids thread switches back to the UI thread after each network request by calling ConfigureAwait on the download task with a parameter of false.  This can be useful to optimize highly performance-sensitive code.\r\n\r\nOnce the composite result is gathered, control is switched back to the UI thread to output the result.")]
        public async void AsyncConfigureAwait()
        {
            Uri[] uris = { new Uri("http://www.weather.gov"), new Uri("http://www.weather.gov/climate/"), new Uri("http://www.weather.gov/rss/") };

            int totalLength = 0;

            foreach (var uri in uris)
            {
                string s = await new WebClient().DownloadStringTaskAsync(uri).ConfigureAwait(false);    // Use ConfigureAwait to skip switching threads after download
                totalLength += s.Length;
            }

            // Switch back to UI thread to update UI
            Application.Current.Dispatcher.Invoke(new Action(delegate {
                    Console.WriteLine("Back on the UI thread.  Total length of pages is {0}.", totalLength);
                }));
        }


        [Category("Control Flow")]
        [Title("if-else")]
        [Description("Control flows naturally across network requests both within and after an if-else block.")]
        public async void AsyncIfElse()
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/om/marine/home.htm")));
                Console.WriteLine("It's the weekend!  Time for the marine forecast!");
            }
            else
            {
                WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov")));
                Console.WriteLine("Back to work!");
            }

            WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/forecasts/graphical/")));
            Console.WriteLine("Always useful to get a general forecast!");
        }

        [Category("Control Flow")]
        [Title("if-else")]
        [Description("Without await, each branch of an if that performs its own network request must have its own callback method.  To consolidate the logic following the if block, further methods are also needed.")]
        [LinkedMethod(
            "AsyncIfElseBefore_Weekend_DownloadStringCompleted",
            "AsyncIfElseBefore_Weekday_DownloadStringCompleted",
            "AsyncIfElseBefore_GeneralForecast",
            "AsyncIfElseBefore_GeneralForecast_DownloadStringCompleted")]
        public void AsyncIfElseBefore()
        {
            WebClient client = new WebClient();

            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                client.DownloadStringCompleted += AsyncIfElseBefore_Weekend_DownloadStringCompleted;
                client.DownloadStringAsync(new Uri("http://www.weather.gov/om/marine/home.htm"));
            }
            else
            {
                client.DownloadStringCompleted += AsyncIfElseBefore_Weekday_DownloadStringCompleted;
                client.DownloadStringAsync(new Uri("http://www.weather.gov"));
            }
        }

        void AsyncIfElseBefore_Weekend_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);

            Console.WriteLine("It's the weekend!  Time for the marine forecast!");

            AsyncIfElseBefore_GeneralForecast();
        }

        void AsyncIfElseBefore_Weekday_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);

            Console.WriteLine("Back to work!");

            AsyncIfElseBefore_GeneralForecast();
        }

        void AsyncIfElseBefore_GeneralForecast()
        {
            WebClient client = new WebClient();

            client.DownloadStringCompleted += AsyncIfElseBefore_GeneralForecast_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.weather.gov/forecasts/graphical/"));
        }

        void AsyncIfElseBefore_GeneralForecast_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            WriteLinePageTitle(e.Result);

            Console.WriteLine("Always useful to get a general forecast!");
        }

        [Category("Control Flow")]
        [Description("Control flows naturally across network requests both within and after a switch block.")]
        [Title("switch")]
        public async void AsyncSwitch()
        {
            double stockPrice = 123.45;

            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/wa.php?x=1")));
                    stockPrice += 1.25;
                    break;
                case DayOfWeek.Tuesday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/or.php?x=1")));
                    stockPrice *= 1.04;
                    break;
                case DayOfWeek.Wednesday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/ca.php?x=1")));
                    stockPrice -= 0.58;
                    break;
                case DayOfWeek.Thursday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/nv.php?x=1")));
                    stockPrice *= 0.99;
                    break;
                case DayOfWeek.Friday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/az.php?x=1")));
                    stockPrice += 0.79;
                    break;
                case DayOfWeek.Saturday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/ut.php?x=1")));
                    stockPrice += 1.8;
                    break;
                case DayOfWeek.Sunday:
                    WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/alerts-beta/nm.php?x=1")));
                    stockPrice /= 1.2;
                    break;
            }

            Console.WriteLine("Today's stock price: {0}", stockPrice);
        }

        [Category("Control Flow")]
        [Title("do-while")]
        [Description("Control flows naturally across network requests while iterating through a do-while loop.")]
        [LinkedField("cts")]
        [LinkedMethod("GetNextUri")]
        public async Task AsyncDoWhile()
        {
            Console.WriteLine("Fetching movies...");

            cts = new CancellationTokenSource();

            WebClient client = new WebClient();

            string next = "http://odata.netflix.com/Catalog/Titles?$inlinecount=allpages&$filter=ReleaseYear%20le%201942";

            try
            {
                int movieCount = 0;
                do
                {
                    // Fetch next batch of movies:
                    var task = new WebClient().DownloadStringTaskAsync(new Uri(next), cts.Token);
                    string resultString = await task;
                    var result = XDocument.Parse(resultString);

                    // Output movies:
                    var entries = result.Descendants(name("entry"));
                    foreach (var entry in entries)
                    {
                        Console.WriteLine(entry.Element(name("title")).Value);
                        movieCount++;
                    }

                    // Update progress:
                    var countElement = result.Descendants(mName("count")).SingleOrDefault() as XElement;
                    if (countElement != null)
                    {
                        int total = int.Parse(countElement.Value);
                        ProgressBar.Value = (int)((movieCount * 100.0) / total);
                    }

                    next = GetNextUri(result);
                } while (next != null);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Fetch canceled.");
            }
        }

        private string GetNextUri(XDocument xml)
        {
            return (from elem in xml.Element(name("feed")).Elements(name("link"))
                    where elem.Attribute("rel").Value == "next"
                    select elem.Attribute("href").Value).SingleOrDefault();
        }

        public static XName name(string x) { return XName.Get(x, "http://www.w3.org/2005/Atom"); }
        public static XName mName(string x) { return XName.Get(x, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"); }

        [Category("Control Flow")]
        [Title("for")]
        [Description("Control flows naturally across network requests while iterating through a for loop.")]
        public async void AsyncFor()
        {
            var tasks = new Task<string>[] {
                new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov")),
                new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/")),
                new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/"))
            };

            for (int i = 0; i < 3; i++)
            {
                WriteLinePageTitle(await tasks[i]);
            }
        }

        [Category("Control Flow")]
        [Title("foreach")]
        [Description("Control flows naturally across network requests while iterating through a foreach loop.")]
        public async void AsyncForEach()
        {
            var uris = new List<Uri> { new Uri("http://www.weather.gov"), new Uri("http://www.weather.gov/climate/"), new Uri("http://www.weather.gov/rss/") };

            foreach (var uri in uris)
            {
                WriteLinePageTitle(await new WebClient().DownloadStringTaskAsync(uri));
            }
        }

        [Category("Control Flow")]
        [Title("break")]
        [Description("Control flows naturally across network requests while escaping a loop using a break statement.")]
        [LinkedField("cts")]
        public async Task AsyncBreak()
        {
            cts = new CancellationTokenSource();

            Console.WriteLine("Successful searches:");

            try
            {
                for (string queryString = "mad"; ; queryString += ", mad")
                {
                    var uri = new Uri(String.Format("http://odata.netflix.com/Catalog/Titles/$count?$filter=substringof('{0}',Name)", queryString));
                    int results = int.Parse(await new WebClient().DownloadStringTaskAsync(uri, cts.Token));

                    Console.WriteLine("Movies containing '{0}': {1}", queryString, results);

                    if (results == 0) break;
                }

                Console.WriteLine("No more results!");
            }
            catch
            {
                Console.WriteLine("Canceled!");
            }
        }

        [Category("Control Flow")]
        [Title("continue")]
        [Description("Control flows naturally across network requests while skipping the rest of a loop iteration using a continue statement.")]
        public async void AsyncContinue()
        {
            var uris = new List<Uri> { new Uri("http://www.weather.gov"), new Uri("http://www.weather.gov/climate/"), new Uri("http://www.weather.gov/rss/") };

            foreach (var uri in uris)
            {
                string page = await new WebClient().DownloadStringTaskAsync(uri);

                if (page.Length > 50000) continue;

                Console.WriteLine("{0} is a small {1}-character page.", uri, page.Length);
                Console.WriteLine("First character is {0}.", page[0]);
            }
        }

        [Category("Control Flow")]
        [Title("using")]
        [Description("When control flows out of a using block that awaits expressions, either naturally or due to an exception, that using block's resource object is always properly disposed.")]
        public async void AsyncUsing()
        {
            using (var response = await WebRequest.Create("http://www.weather.gov").GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                WriteLinePageTitle(await reader.ReadToEndAsync());
            }
        }


        [Category("Anonymous Functions")]
        [Title("async delegate")]
        [Description("A method may be awaited from within an anonymous delegate with the async modifier.  Click the button to advance to the next year.\r\n\r\nThis is especially useful for signing up event handlers.")]
        public void AsyncAnonymousDelegate()
        {
            cts = new CancellationTokenSource();

            var form = new Form();
            var button = new Button();
            button.Dock = DockStyle.Fill;
            form.Controls.Add(button);
            button.Text = "Click me!";
            form.Show();

            var client = new WebClient();

            var year = 1950;

            button.Click += async delegate
            {
                var url = String.Format("http://netflix.cloudapp.net/Catalog/Titles/$count?$filter=ReleaseYear%20eq%20{0}", year);
                var count = int.Parse(await client.DownloadStringTaskAsync(new Uri(url), cts.Token));
                button.Text = String.Format("Netflix has {0} movies from the year {1}.", count, year);

                year++;
            };
        }

        [Category("Anonymous Functions")]
        [Title("async lambda expression")]
        [Description("A method may be awaited from within a lambda expression with the async modifier.  Click the button to advance to the next year.\r\n\r\nThis is especially useful for signing up event handlers.")]
        public void AsyncAnonymousLambda()
        {
            cts = new CancellationTokenSource();

            var form = new Form();
            var button = new Button();
            button.Dock = DockStyle.Fill;
            form.Controls.Add(button);
            button.Text = "Click me!";
            form.Show();

            var client = new WebClient();

            Func<int, Task> countTask = null;
            countTask = async year =>
                {
                    var url = String.Format("http://netflix.cloudapp.net/Catalog/Titles/$count?$filter=ReleaseYear%20eq%20{0}", year);
                    var count = int.Parse(await client.DownloadStringTaskAsync(new Uri(url), cts.Token));

                    button.Text = String.Format("Netflix has {0} movies from the year {1}.", count, year);

                    if (year < 1960)
                        await countTask(year + 1);
                };

            countTask(1950);
        }


        [Category("Expressions (within await)")]
        [Title("Member access")]
        [Description("An await expression may await a member of an object.")]
        [LinkedClass("StringTaskPair")]
        public async void AsyncMemberAccessWithin()
        {
            var pair = new StringTaskPair();
            pair.Task1 = new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov"));
            pair.Task2 = new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/"));

            WriteLinePageTitle(await pair.Task2);
        }

        public class StringTaskPair
        {
            public Task<string> Task1 { get; set; }
            public Task<string> Task2 { get; set; }
        }

        [Category("Expressions (within await)")]
        [Title("Invocation")]
        [Description("An await expression may await the result of invoking a delegate.")]
        public async void AsyncInvocationWithin()
        {
            var topic = "monkeys";

            Func<string, Task<string>> searchTaskGenerator =
                s => new WebClient().DownloadStringTaskAsync(new Uri(String.Format("http://odata.netflix.com/Catalog/Titles/$count?$filter=substringof('{0}',Name)", s)));

            Console.WriteLine("{0} movies about {1}.", await searchTaskGenerator(topic), topic);
        }

        [Category("Expressions (within await)")]
        [Title("Indexing")]
        [Description("An await expression may await the result of indexing a collection.")]
        public async void AsyncIndexingWithin()
        {
            var tasks = new Task<string>[] { new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov")),
                                             new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/climate/")),
                                             new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov/rss/")) };

            WriteLinePageTitle(await tasks[2]);
        }

        [Category("Expressions (within await)")]
        [Title("Explicit conversion")]
        [Description("An await expression may await an expression that has just been converted to an awaitable type such as Task<string>.")]
        [LinkedClass("StringCalculation")]
        public async void AsyncExplicitConversionWithin()
        {
            var sr = new StringCalculation();

            Console.WriteLine(await (Task<string>)sr);
        }

        public class StringCalculation
        {
            public static explicit operator Task<string>(StringCalculation sr)
            {
                return TaskEx.Run(() =>
                {
                    Thread.Sleep(1000);     // Simulate calculation to produce a string.
                    return "FooBar";
                });
            }
        }

        [Category("Expressions (within await)")]
        [Title("as operator")]
        [Description("An await expression may await an expression that has just been converted using the as operator.")]
        [LinkedMethod("LoadTaskObject")]
        public async void AsyncAsWithin()
        {
            object o = LoadTaskObject();

            WriteLinePageTitle(await (o as Task<string>));
        }

        public object LoadTaskObject()
        {
            return new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov"));
        }

        public class RemoteInteger
        {
            private int value;  // Hold the value that we're simulating remote storage of.

            public static Task<RemoteInteger> CreateAsync(int i)
            {
                // Simulate sending i to a remote server:
                var creation = new Task<RemoteInteger>(() => new RemoteInteger(i));
                creation.Start();
                return creation;
            }
            private RemoteInteger(int i) { value = i; }

            public Task<int> GetValueAsync()
            {
                // Simulate fetching the value back from a remote server:
                var getValue = new Task<int>(() => value);
                getValue.Start();
                return getValue;
            }

            public static Task<RemoteInteger> operator +(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote addition of i1 and i2:
                var addition = new Task<RemoteInteger>(() => new RemoteInteger(i1.value + i2.value));
                addition.Start();
                return addition;
            }

            public static Task<RemoteInteger> operator -(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote subtraction of i1 and i2:
                var subtraction = new Task<RemoteInteger>(() => new RemoteInteger(i1.value - i2.value));
                subtraction.Start();
                return subtraction;
            }

            public static Task<RemoteInteger> operator *(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote multiplication of i1 and i2:
                var multiplication = new Task<RemoteInteger>(() => new RemoteInteger(i1.value * i2.value));
                multiplication.Start();
                return multiplication;
            }

            public static Task<RemoteInteger> operator /(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote division of i1 and i2:
                var division = new Task<RemoteInteger>(() => new RemoteInteger(i1.value / i2.value));
                division.Start();
                return division;
            }

            public static Task<RemoteInteger> operator %(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote modulus of i1 and i2:
                var modulus = new Task<RemoteInteger>(() => new RemoteInteger(i1.value % i2.value));
                modulus.Start();
                return modulus;
            }

            public static Task<RemoteInteger> operator -(RemoteInteger i)
            {
                // Simulate remote negation of i:
                var negation = new Task<RemoteInteger>(() => new RemoteInteger(-i.value));
                negation.Start();
                return negation;
            }

            public static Task<bool> operator ==(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote equality comparison of i1 and i2:
                var equality = new Task<bool>(() => (i1.value == i2.value));
                equality.Start();
                return equality;
            }

            public static Task<bool> operator !=(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote inequality comparison of i1 and i2:
                var inequality = new Task<bool>(() => (i1.value != i2.value));
                inequality.Start();
                return inequality;
            }

            public static Task<bool> operator <(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote less-than comparison of i1 and i2:
                var lessThan = new Task<bool>(() => (i1.value < i2.value));
                lessThan.Start();
                return lessThan;
            }

            public static Task<bool> operator >(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote greater-than comparison of i1 and i2:
                var greaterThan = new Task<bool>(() => (i1.value > i2.value));
                greaterThan.Start();
                return greaterThan;
            }

            public static Task<bool> operator <=(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote less-than-or-equal comparison of i1 and i2:
                var lessThanEqual = new Task<bool>(() => (i1.value <= i2.value));
                lessThanEqual.Start();
                return lessThanEqual;
            }

            public static Task<bool> operator >=(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote greater-than-or-equal comparison of i1 and i2:
                var greaterThanEqual = new Task<bool>(() => (i1.value >= i2.value));
                greaterThanEqual.Start();
                return greaterThanEqual;
            }

            public static Task<RemoteInteger> operator <<(RemoteInteger i1, int i2)
            {
                // Simulate remote left-shift of i1 by i2 bits:
                var leftShift = new Task<RemoteInteger>(() => new RemoteInteger(i1.value << i2));
                leftShift.Start();
                return leftShift;
            }

            public static Task<RemoteInteger> operator >>(RemoteInteger i1, int i2)
            {
                // Simulate remote left-shift of i1 by i2 bits:
                var rightShift = new Task<RemoteInteger>(() => new RemoteInteger(i1.value >> i2));
                rightShift.Start();
                return rightShift;
            }

            public static Task<RemoteInteger> operator &(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote bitwise AND of i1 and i2:
                var bitwiseAnd = new Task<RemoteInteger>(() => new RemoteInteger(i1.value & i2.value));
                bitwiseAnd.Start();
                return bitwiseAnd;
            }

            public static Task<RemoteInteger> operator ~(RemoteInteger i)
            {
                // Simulate remote bitwise NOT of i:
                var bitwiseNot = new Task<RemoteInteger>(() => new RemoteInteger(~i.value));
                bitwiseNot.Start();
                return bitwiseNot;
            }

            public static Task<RemoteInteger> operator |(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote bitwise OR of i1 and i2:
                var bitwiseOr = new Task<RemoteInteger>(() => new RemoteInteger(i1.value | i2.value));
                bitwiseOr.Start();
                return bitwiseOr;
            }

            public static Task<RemoteInteger> operator ^(RemoteInteger i1, RemoteInteger i2)
            {
                // Simulate remote bitwise XOR of i1 and i2:
                var bitwiseXor = new Task<RemoteInteger>(() => new RemoteInteger(i1.value ^ i2.value));
                bitwiseXor.Start();
                return bitwiseXor;
            }

            public override bool Equals(object obj) { throw new NotSupportedException(); }
            public override int GetHashCode() { throw new NotSupportedException(); }
        }

        [Category("Expressions (within await)")]
        [Title("Binary + operator")]
        [Description("An await expression may await the result of an addition operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncBinaryPlusWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            RemoteInteger result = await (remoteInt1 + remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("Binary - operator")]
        [Description("An await expression may await the result of a subtraction operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncBinaryMinusWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            RemoteInteger result = await (remoteInt1 - remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("* operator")]
        [Description("An await expression may await the result of a multiplication operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncMultiplyWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(12);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(23);

            RemoteInteger result = await (remoteInt1 * remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("/ operator")]
        [Description("An await expression may await the result of a division operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncDivideWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(360);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(15);

            RemoteInteger result = await (remoteInt1 / remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("% operator")]
        [Description("An await expression may await the result of a remainder operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncModulusWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            RemoteInteger result = await (remoteInt1 % remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("Unary - operator")]
        [Description("An await expression may await the result of a negation operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncUnaryMinusWithin()
        {
            RemoteInteger remoteInt = await RemoteInteger.CreateAsync(100);

            RemoteInteger result = await -remoteInt;

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("== operator")]
        [Description("An await expression may await the result of an equality test.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncEqualsWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            Console.WriteLine(await (remoteInt1 == remoteInt2));
        }

        [Category("Expressions (within await)")]
        [Title("!= operator")]
        [Description("An await expression may await the result of an inequality test.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncNotEqualsWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            Console.WriteLine(await (remoteInt1 != remoteInt2));
        }

        [Category("Expressions (within await)")]
        [Title("< operator")]
        [Description("An await expression may await the result of a less-than test.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncLessThanWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            Console.WriteLine(await (remoteInt1 < remoteInt2));
        }

        [Category("Expressions (within await)")]
        [Title("> operator")]
        [Description("An await expression may await the result of a greater-than test.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncGreaterThanWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            Console.WriteLine(await (remoteInt1 > remoteInt2));
        }

        [Category("Expressions (within await)")]
        [Title("<= operator")]
        [Description("An await expression may await the result of a less-than-or-equal test.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncLessThanEqualWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            Console.WriteLine(await (remoteInt1 <= remoteInt2));
        }

        [Category("Expressions (within await)")]
        [Title(">= operator")]
        [Description("An await expression may await the result of a greater-than-or-equal test.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncGreaterThanEqualWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            Console.WriteLine(await (remoteInt1 >= remoteInt2));
        }

        [Category("Expressions (within await)")]
        [Title("<< operator")]
        [Description("An await expression may await the result of a left-shift operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncLeftShift()
        {
            RemoteInteger remoteInt = await RemoteInteger.CreateAsync(200);

            RemoteInteger result = await (remoteInt << 3);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title(">> operator")]
        [Description("An await expression may await the result of a right-shift operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncRightShift()
        {
            RemoteInteger remoteInt = await RemoteInteger.CreateAsync(200);

            RemoteInteger result = await (remoteInt >> 4);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("& operator")]
        [Description("An await expression may await the result of a bitwise AND operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncBitwiseAndWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            RemoteInteger result = await (remoteInt1 & remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("| operator")]
        [Description("An await expression may await the result of a bitwise OR operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncBitwiseOrWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            RemoteInteger result = await (remoteInt1 | remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("^ operator")]
        [Description("An await expression may await the result of a bitwise XOR operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncBitwiseXorWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(200);
            RemoteInteger remoteInt2 = await RemoteInteger.CreateAsync(34);

            RemoteInteger result = await (remoteInt1 ^ remoteInt2);

            Console.WriteLine(await result.GetValueAsync());
        }

        [Category("Expressions (within await)")]
        [Title("~ operator")]
        [Description("An await expression may await the result of a bitwise NOT operation.\r\n\r\nThis example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")]
        [LinkedClass("RemoteInteger")]
        public async void AsyncBitwiseNotWithin()
        {
            RemoteInteger remoteInt1 = await RemoteInteger.CreateAsync(100);

            RemoteInteger result = await ~remoteInt1;

            Console.WriteLine(await result.GetValueAsync());
        }


        [Category("Expressions (around await)")]
        [Title("Member access")]
        [Description("The result of an await expression may have its members directly evaluated.")]
        public async void AsyncMemberAccessAround()
        {
            Console.WriteLine("Content length:");
            Console.WriteLine((await new WebClient().DownloadStringTaskAsync(new Uri("http://www.weather.gov"))).Length);
        }


        [Category("Expressions (around await)")]
        [Title("Invocation")]
        [Description("The result of an await expression may be directly invoked.")]
        [LinkedMethod("PrecalculateCosineTableAsync")]
        public async void AsyncInvocationAround()
        {
            Console.WriteLine((await PrecalculateCosineTableAsync())(0.0));
        }

        public async Task<Func<double, double>> PrecalculateCosineTableAsync()
        {
            return await TaskEx.Run<Func<double, double>>(() =>
                {
                    Thread.Sleep(1000);    // Simulate precalculating a table of cosine values

                    return d =>
                    {
                        if (d == 0.0) return 1.0;
                        else throw new NotSupportedException();
                    };
                });
        }

        [Category("Expressions (around await)")]
        [Title("Indexing")]
        [Description("The result of an await expression may be directly indexed.")]
        public async void AsyncIndexingAround()
        {
            Console.WriteLine((await GetDigitsOfPi())[50]);
        }

        public async Task<string> GetDigitsOfPi()
        {
            return await TaskEx.Run(() =>
                {
                    Thread.Sleep(1000);     // Simulate calculating digits of pi

                    return "3141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067";
                });
        }

        [Category("Expressions (around await)")]
        [Title("Explicit conversion")]
        [Description("The result of an await expression may be directly cast to another type.")]
        [LinkedMethod("PrintByte", "LongRunningOperation")]
        public async void AsyncExplicitConversionAround()
        {
            PrintByte((byte)await LongRunningOperation());
        }

        public void PrintByte(byte b)
        {
            Console.WriteLine(b);
        }

        [Category("Expressions (around await)")]
        [Title("Implicit conversion")]
        [Description("The result of an await expression may be directly used in a context where an implicit cast will be required.")]
        [LinkedMethod("PrintLong", "LongRunningOperation")]
        public async void AsyncImplicitConversionAround()
        {
            PrintLong(await LongRunningOperation());
        }

        public void PrintLong(long l)
        {
            Console.WriteLine(l);
        }

        [Category("Expressions (around await)")]
        [Title("as operator")]
        [Description("The result of an await expression may be directly converted to another type using the as operator.")]
        [LinkedMethod("DeserializeAsync")]
        public async void AsyncAsAround()
        {
            string str = await DeserializeAsync() as string;
            if (str != null)
            {
                Console.WriteLine(str);
            }
            else
            {
                Console.WriteLine("Deserialization failed.");
            }
        }

        public async Task<object> DeserializeAsync()
        {
            await TaskEx.Delay(200);     // Simulate loading an object from disk and deserializing it
            return "serialized string";
        }

        [Category("Expressions (around await)")]
        [Title("is operator")]
        [Description("The result of an await expression may be directly type-tested.")]
        [LinkedMethod("DeserializeAsync")]
        public async void AsyncIsAround()
        {
            if (await DeserializeAsync() is string)
            {
                Console.WriteLine("Value has type string.");
            }
            else
            {
                Console.WriteLine("Value has another type.");
            }
        }

        [Category("Expressions (around await)")]
        [Title("Binary + operator")]
        [Description("The result of an await expression may be directly added to another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncBinaryPlusAround()
        {
            Console.WriteLine(await LongRunningOperation() + 1);
        }

        [Category("Expressions (around await)")]
        [Title("Binary - operator")]
        [Description("The result of an await expression may have another value directly subtracted from it.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncBinaryMinusAround()
        {
            Console.WriteLine(await LongRunningOperation() - 5);
        }

        [Category("Expressions (around await)")]
        [Title("* operator")]
        [Description("The result of an await expression may be directly multiplied by another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncMultiplyAround()
        {
            Console.WriteLine(await LongRunningOperation() * 3);
        }

        [Category("Expressions (around await)")]
        [Title("/ operator")]
        [Description("The result of an await expression may be directly divided by another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncDivideAround()
        {
            Console.WriteLine(await LongRunningOperation() / 2);
        }

        [Category("Expressions (around await)")]
        [Title("% operator")]
        [Description("The result of an await expression may directly participate in a remainder calculation.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncModulusAround()
        {
            Console.WriteLine(await LongRunningOperation() % 16);
        }

        [Category("Expressions (around await)")]
        [Title("Unary - operator")]
        [Description("The result of an await expression may be directly negated.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncUnaryMinusAround()
        {
            Console.WriteLine(-await LongRunningOperation());
        }

        [Category("Expressions (around await)")]
        [Title("== operator")]
        [Description("The result of an await expression may be directly tested for equality.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncEqualsAround()
        {
            Console.WriteLine(await LongRunningOperation() == 123);
        }

        [Category("Expressions (around await)")]
        [Title("!= operator")]
        [Description("The result of an await expression may be directly tested for inequality.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncNotEqualsAround()
        {
            Console.WriteLine(await LongRunningOperation() != 123);
        }

        [Category("Expressions (around await)")]
        [Title("< operator")]
        [Description("The result of an await expression may be directly tested for being less than another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncLessThanAround()
        {
            Console.WriteLine(await LongRunningOperation() < 123);
        }

        [Category("Expressions (around await)")]
        [Title("> operator")]
        [Description("The result of an await expression may be directly tested for being greater than another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncGreaterThanAround()
        {
            Console.WriteLine(await LongRunningOperation() > 123);
        }

        [Category("Expressions (around await)")]
        [Title("<= operator")]
        [Description("The result of an await expression may be directly tested for being less than or equal to another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncLessThanEqualAround()
        {
            Console.WriteLine(await LongRunningOperation() <= 123);
        }

        [Category("Expressions (around await)")]
        [Title(">= operator")]
        [Description("The result of an await expression may be directly tested for being greater than or equal to another value.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncGreaterThanEqualAround()
        {
            Console.WriteLine(await LongRunningOperation() >= 123);
        }

        [Category("Expressions (around await)")]
        [Title("<< operator")]
        [Description("The result of an await expression may directly participate in a left-shift calculation.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncLeftShiftAround()
        {
            Console.WriteLine(await LongRunningOperation() << 3);
        }


        [Category("Expressions (around await)")]
        [Title(">> operator")]
        [Description("The result of an await expression may directly participate in a right-shift calculation.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncRightShiftAround()
        {
            Console.WriteLine(await LongRunningOperation() >> 4);
        }

        [Category("Expressions (around await)")]
        [Title("& operator")]
        [Description("The result of an await expression may directly participate in a bitwise AND.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncBitwiseAndAround()
        {
            Console.WriteLine(await LongRunningOperation() & 0x04);
        }

        [Category("Expressions (around await)")]
        [Title("| operator")]
        [Description("The result of an await expression may directly participate in a bitwise OR.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncBitwiseOrAround()
        {
            Console.WriteLine(await LongRunningOperation() | 0x04);
        }

        [Category("Expressions (around await)")]
        [Title("^ operator")]
        [Description("The result of an await expression may directly participate in a bitwise XOR.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncBitwiseXorAround()
        {
            Console.WriteLine(await LongRunningOperation() ^ 0x04);
        }

        [Category("Expressions (around await)")]
        [Title("~ operator")]
        [Description("The result of an await expression may directly participate in a bitwise NOT.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncBitwiseNotAround()
        {
            Console.WriteLine(~await LongRunningOperation());
        }

        [Category("Expressions (around await)")]
        [Title("?? operator")]
        [Description("The result of an await expression may be coalesced, in case it's null.")]
        [LinkedMethod("FetchByKeyAsync")]
        public async void AsyncCoalesceAround()
        {
            Console.WriteLine(await FetchByKeyAsync("baz") ?? "Key not found.");
        }

        [Category("Expressions (around await)")]
        [Title("checked")]
        [Description("Operations on the result of an await expression may be controlled by a checked scope.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncCheckedAround()
        {
            Console.WriteLine(checked(await LongRunningOperation() + int.MaxValue));
        }

        [Category("Expressions (around await)")]
        [Title("unchecked")]
        [Description("Operations on the result of an await expression may be controlled by a unchecked scope.")]
        [LinkedMethod("LongRunningOperation")]
        public async void AsyncUncheckedAround()
        {
            Console.WriteLine(checked(123 + unchecked(await LongRunningOperation() + int.MaxValue)));
        }

        public async Task<string> FetchByKeyAsync(string key)
        {
            await TaskEx.Delay(500);     // Simulate fetching a remote value by key
            if (key == "foo")
            {
                return "bar";
            }
            else
            {
                return null;
            }
        }


        [Category("APIs: System.IO")]
        [Title("Stream.CopyToAsync")]
        [Description("Copies one stream to another asynchronously.")]
        public async void AsyncStreamCopyTo()
        {
            // Download a file.
            await new WebClient().DownloadFileTaskAsync(new Uri("http://www.weather.gov/climate/"), "climate.html");

            // Create the streams.
            MemoryStream destination = new MemoryStream();

            using (FileStream source = File.Open("climate.html", FileMode.Open))
            {
                Console.WriteLine("Source length: {0}", source.Length.ToString());

                // Copy source to destination.
                await source.CopyToAsync(destination);
            }

            Console.WriteLine("Destination length: {0}", destination.Length.ToString());

            // NOTE: If Just My Code is on, VS will currently show a first-chance exception here.
            // You may hit F5 to continue running after seeing the exception.
        }

        [Category("APIs: System.IO")]
        [Title("Step 1: TextWriter.WriteAsync")]
        [Description("Writes data to a file asynchronously.")]
        public async void AsyncTextWriterWrite()
        {
            try
            {
                using (TextWriter writer = new StreamWriter("data.txt"))
                {
                    string data = "1234 abcd";

                    await writer.WriteAsync(data);
                }

                Console.WriteLine("Data written to file.");
            }
            catch
            {
                Console.WriteLine("Error writing to file!");
            }
        }

        [Category("APIs: System.IO")]
        [Title("Step 2: TextWriter.ReadLineAsync")]
        [Description("Reads the first line of data from a file asynchronously.\r\n\r\nBe sure to run the TextWriter.WriteAsync sample first to create the data file.")]
        public async void AsyncTextWriterReadLine()
        {
            try
            {
                using (TextReader reader = new StreamReader("data.txt"))
                {
                    var data = await reader.ReadLineAsync();

                    Console.WriteLine("Data: {0}", data);
                }
            }
            catch
            {
                Console.WriteLine("Error reading from file!  Have you run the TextWriter.WriteAsync sample yet?");
            }
        }


        [Category("APIs: System.Net")]
        [Title("Dns.GetHostAddressesAsync")]
        [Description("Resolves a domain name to a set of IP addresses asynchronously.")]
        public async void AsyncDnsGetHostAddresses()
        {
            try
            {
                var addresses = await DnsEx.GetHostAddressesAsync("http://www.weather.gov");

                foreach (var address in addresses)
                {
                    Console.WriteLine("IP address: {0}", address.ToString());
                }
            }
            catch
            {
                Console.WriteLine("The DNS server was unable to resolve the domain name.");
            }
        }

        [Category("APIs: System.Net")]
        [Title("Dns.GetHostEntryAsync")]
        [Description("Determines the host name of a known IP address asynchronously.")]
        public async void AsyncDnsGetHostEntry()
        {
            try
            {
                var entry = await DnsEx.GetHostEntryAsync("127.0.0.1");

                Console.WriteLine(entry.HostName);
            }
            catch
            {
                Console.WriteLine("The DNS server was unable to resolve the IP address.");
            }
        }


        [Category("APIs: System.Net")]
        [Title("WebClient.DownloadStringTaskAsync - Customized headers")]
        [Description("Performs a web request asynchronously using await.  This example shows the reuse of a WebClient object that has been customized to use a specific base address for all requests.")]
        public async void AsyncWebRequestDownloadStringAsyncCustomized()
        {
            WebClient client = new WebClient();

            client.BaseAddress = "http://www.weather.gov";

            Console.WriteLine("Base address set.");

            WriteLinePageTitle(await client.DownloadStringTaskAsync(new Uri("/", UriKind.Relative)));
            WriteLinePageTitle(await client.DownloadStringTaskAsync(new Uri("/climate/", UriKind.Relative)));
            WriteLinePageTitle(await client.DownloadStringTaskAsync(new Uri("/rss/", UriKind.Relative)));
        }

        [Category("APIs: System.Net")]
        [Title("WebClient.DownloadDataTaskAsync")]
        [Description("Performs a web request to fill a byte array asynchronously.")]
        public async void AsyncWebRequestDownloadData()
        {
            byte[] buffer = await new WebClient().DownloadDataTaskAsync(new Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h003.jpeg?g=400"));

            Console.WriteLine("First 16 bytes:");
            for (int i = 0; i < 16; i++)
            {
                Console.Write("{0} ", buffer[i].ToString("X2"));
            }
        }

        [Category("APIs: System.Net")]
        [Title("WebClient.DownloadFileTaskAsync")]
        [Description("Performs a web request to download a file to disk asynchronously.")]
        public async void AsyncWebRequestDownloadFile()
        {
            Console.WriteLine("Downloading wallpaper...");
            await new WebClient().DownloadFileTaskAsync(new Uri("http://www.microsoft.com/games/mw4mercs/img/desktop_2_800.jpg"), "mech.jpg");
            Console.WriteLine("Downloaded as mech.jpg.");

            Console.WriteLine("Previewing mech.jpg...");
            Process.Start("mech.jpg");
        }

        [Category("APIs: System.Net")]
        [Title("WebRequest.GetResponseAsync")]
        [Description("Performs a web request to return a network stream asynchronously.")]
        public async void AsyncWebRequestGetResponse()
        {
            var response = await WebRequest.Create("http://www.weather.gov").GetResponseAsync();
            var stream = response.GetResponseStream();
            Console.WriteLine("First byte: {0}", stream.ReadByte().ToString("X2"));
        }

        [Category("APIs: System.Net.NetworkInformation")]
        [Title("Ping.SendTaskAsync")]
        [Description("Sends a ping and asynchronously awaits the response.\r\n\r\nIn this case, since microsoft.com does not reply to ping messages, control will enter the Else branch.")]
        public async void AsyncPingSend()
        {
            var ping = new Ping();
            var reply = await ping.SendTaskAsync("www.microsoft.com");
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("www.microsoft.com is responding to pings.");
            }
            else
            {
                Console.WriteLine("www.microsoft.com is not responding to pings.");
            }
        }


        #region Samples Infrastructure
        public override CancellationTokenSource CancellationTokenSource { get { return cts; } set { cts = value; } }
        #endregion
    }
}