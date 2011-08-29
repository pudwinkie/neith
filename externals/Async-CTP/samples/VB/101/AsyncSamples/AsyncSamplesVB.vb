Imports System.Threading
Imports System.Threading.Tasks
Imports System.Text.RegularExpressions
Imports System.Windows.Media.Imaging
Imports System.Text
Imports System.IO
Imports System.Xml.Linq
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Collections.ObjectModel
Imports System.Windows.Forms
Imports System.Speech.Synthesis
Imports System.Media

Imports SampleSupport


<Title("101 VB Async Samples")>
<Prefix("Async")>
<Before("VB 10")>
<After("Await")>
<Extension("vb")>
Public Class AsyncSamplesVB
    Inherits SampleHarness

    Private cts As CancellationTokenSource

    <Category("Introduction to await")>
    <Title("await - Single Network Request")>
    <Description("Performs a web request asynchronously using await.")>
    Public Async Sub AsyncIntroSingle()
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov")))
    End Sub

    <Description("Performs a web request asynchronously using a separate continuation method.")>
    <LinkedMethod("AsyncIntroSingleBefore_DownloadStringCompleted")>
    Public Sub AsyncIntroSingleBefore()
        Dim client As New WebClient

        AddHandler client.DownloadStringCompleted, AddressOf AsyncIntroSingleBefore_DownloadStringCompleted
        client.DownloadStringAsync(New Uri("http://www.weather.gov"))
    End Sub

    Sub AsyncIntroSingleBefore_DownloadStringCompleted(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)
    End Sub

    <Category("Introduction to await")>
    <Title("await - Serial Network Requests")>
    <Description("Performs a series of web requests in sequence using await.  The next request will not be issued until the previous request completes.")>
    Public Async Sub AsyncIntroSerial()
        Dim client As New WebClient()

        WriteLinePageTitle(Await client.DownloadStringTaskAsync(New Uri("http://www.weather.gov")))
        WriteLinePageTitle(Await client.DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/")))
        WriteLinePageTitle(Await client.DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/")))
    End Sub

    <Description("Performs a series of web requests using separate continuation methods.")>
    <LinkedMethod(
        "AsyncIntroSerialBefore_DownloadStringCompleted_1",
        "AsyncIntroSerialBefore_DownloadStringCompleted_2",
        "AsyncIntroSerialBefore_DownloadStringCompleted_3")>
    Public Sub AsyncIntroSerialBefore()
        Dim client As New WebClient()

        AddHandler client.DownloadStringCompleted, AddressOf AsyncIntroSerialBefore_DownloadStringCompleted_1
        client.DownloadStringAsync(New Uri("http://www.weather.gov"))
    End Sub

    Sub AsyncIntroSerialBefore_DownloadStringCompleted_1(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)

        Dim client As New WebClient()

        AddHandler client.DownloadStringCompleted, AddressOf AsyncIntroSerialBefore_DownloadStringCompleted_2
        client.DownloadStringAsync(New Uri("http://www.weather.gov/climate/"))
    End Sub

    Sub AsyncIntroSerialBefore_DownloadStringCompleted_2(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)

        Dim client As New WebClient()

        AddHandler client.DownloadStringCompleted, AddressOf AsyncIntroSerialBefore_DownloadStringCompleted_3
        client.DownloadStringAsync(New Uri("http://www.weather.gov/rss/"))
    End Sub

    Sub AsyncIntroSerialBefore_DownloadStringCompleted_3(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)
    End Sub

    <Category("Introduction to await")>
    <Title("await - Parallel Network Requests")>
    <Description("Performs a set of web requests in parallel." + vbCrLf + vbCrLf + "Calling a Task-returning method such as DownloadStringTaskAsync will always kick off the operation immediately, but control flow does not wait for completion until the Await keywords later on.  Here, the output will always occur in order as the results are awaited in order.")>
    Public Async Sub AsyncIntroParallel()
        Dim page1 As Task(Of String) = New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov"))
        Dim page2 As Task(Of String) = New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/"))
        Dim page3 As Task(Of String) = New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/"))

        WriteLinePageTitle(Await page1)
        WriteLinePageTitle(Await page2)
        WriteLinePageTitle(Await page3)
    End Sub

    Public Sub WriteLinePageTitle(ByVal page As String)
        Console.WriteLine(GetPageTitle(page))
    End Sub

    Public Function GetPageTitle(ByVal page As String) As String
        Dim titleRegex As New Regex("\<title\>(?<title>.*)\<\/title\>", RegexOptions.IgnoreCase)
        Dim match = titleRegex.Match(page)
        If (match.Success) Then
            Return "Page title: " + match.Groups("title").Value
        Else
            Return "Page has no title"
        End If
    End Function

    <Category("UI Responsiveness")>
    <Title("Network requests")>
    <Description("Performs a web request asynchronously using await.  This example simulates slow network conditions by delaying the requests." + vbCrLf + vbCrLf + "Drag the window around or scroll the tree to see that the UI is still responsive while the download occurs.")>
    Public Async Sub AsyncResponsiveNetwork()
        WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov")))
        WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/climate/")))
        WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/rss/")))
    End Sub

    Public Async Function DownloadStringTaskSlowNetworkAsync(ByVal address As Uri) As Task(Of String)
        Await TaskEx.Delay(500)     ' Simulate 500ms of network delay
        Return Await New WebClient().DownloadStringTaskAsync(address)
    End Function

    Public Async Function DownloadStringTaskSlowNetworkAsync(ByVal address As Uri, ByVal cancellationToken As CancellationToken) As Task(Of String)
        Await TaskEx.Delay(500)     ' Simulate 500ms of network delay
        Return Await New WebClient().DownloadStringTaskAsync(address, cancellationToken)
    End Function

    <Category("UI Responsiveness")>
    <Title("CPU-bound tasks - Task.Run")>
    <Description("Processes data on a background thread in the thread pool." + vbCrLf + vbCrLf + "Drag the window around or scroll the tree to see that the UI is still responsive while the processing occurs.")>
    <LinkedMethod("ProcessDataAsync")>
    Public Async Function AsyncResponsiveCPURun() As Task
        Console.WriteLine("Processing data...  Drag the window around or scroll the tree!")
        Console.WriteLine()
        Dim data As Integer() = Await ProcessDataAsync(GetData(), 16, 16)
        Console.WriteLine()
        Console.WriteLine("Processing complete.")
    End Function

    Public Function ProcessDataAsync(ByVal data As Byte(), ByVal width As Integer, ByVal height As Integer) As Task(Of Integer())
        Return TaskEx.Run(
            Function()
                Dim result(width * height) As Integer
                For y As Integer = 0 To height - 1
                    For x As Integer = 0 To width - 1
                        Thread.Sleep(10)   ' simulate processing cell [x,y]
                    Next
                    Console.WriteLine("Processed row {0}", y)
                Next
                Return result
            End Function)
    End Function

    Public Function GetData() As Byte()
        Dim bytes(0 To 255) As Byte
        Return bytes
    End Function

    <Category("Cancellation")>
    <Title("CancellationToken - Single Request")>
    <Description("Performs a web request using await, passing a CancellationToken to allow cancellation.  This example simulates slow network conditions by delaying the requests.")>
    <LinkedField("cts")>
    Public Async Function AsyncCancelSingle() As Task
        cts = New CancellationTokenSource()

        Try
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov"), cts.Token))
        Catch ex As OperationCanceledException
            Console.WriteLine("Downloading canceled.")
        End Try
    End Function

    <Category("Cancellation")>
    <Title("CancellationToken - Single CPU-bound Request")>
    <Description("Processes data on a background thread in the thread pool." + vbCrLf + vbCrLf + "The CancellationToken is polled once per iteration of the y for loop to see if cancellation has been requested, and if so an OperationCanceledException is thrown.")>
    <LinkedMethod("ProcessAsync")>
    <LinkedField("cts")>
    Public Async Function AsyncCancelSingleCPU() As Task
        cts = New CancellationTokenSource()

        Try
            Dim data As Integer() = Await ProcessAsync(GetData(), 16, 16, cts.Token)
        Catch ex As OperationCanceledException
            Console.WriteLine("Processing canceled.")
        End Try
    End Function

    Public Function ProcessAsync(ByVal data As Byte(), ByVal width As Integer, ByVal height As Integer, ByVal cancellationToken As CancellationToken) As Task(Of Integer())
        Return TaskEx.Run(
            Function()
                Dim result(width * height) As Integer
                For y As Integer = 0 To height - 1
                    cancellationToken.ThrowIfCancellationRequested()
                    For x As Integer = 0 To width - 1
                        Thread.Sleep(10)   ' simulate processing cell [x,y]
                    Next
                    Console.WriteLine("Processed row {0}", y)
                Next
                Return result
            End Function)
    End Function

    <Category("Cancellation")>
    <Title("CancellationToken - Serial Requests")>
    <Description("Performs a series of web requests using await in sequence.  This example simulates slow network conditions by delaying the requests." + vbCrLf + vbCrLf + "The same CancellationToken is used across multiple requests so that whichever operation happens to be running at the moment can be canceled.")>
    <LinkedField("cts")>
    Public Async Function AsyncCancelSingleSerial() As Task
        cts = New CancellationTokenSource()

        Try
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov"), cts.Token))
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/climate/"), cts.Token))
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/rss/"), cts.Token))
        Catch ex As OperationCanceledException
            Console.WriteLine("Download canceled.")
        End Try
    End Function

    <Category("Cancellation")>
    <Title("CancellationToken - Parallel Requests")>
    <Description("Performs a set of web requests in parallel.  This example simulates slow network conditions by delaying the requests." + vbCrLf + vbCrLf + "The same CancellationToken is used across multiple requests so that all outstanding requests will be canceled together.")>
    <LinkedField("cts")>
    Public Async Function AsyncCancelSingleParallel() As Task
        cts = New CancellationTokenSource()

        Try
            Dim page1 As Task(Of String) = DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov"), cts.Token)
            Dim page2 As Task(Of String) = DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/climate/"), cts.Token)
            Dim page3 As Task(Of String) = DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/rss/"), cts.Token)

            WriteLinePageTitle(Await page1)
            WriteLinePageTitle(Await page2)
            WriteLinePageTitle(Await page3)
        Catch ex As OperationCanceledException
            Console.WriteLine("Download canceled.")
        End Try
    End Function

    <Category("Cancellation")>
    <Title("CancelAfter")>
    <Description("Performs a long-running, cancellable operation, automatically cancelling it after 3 seconds if it does not either complete or get canceled by the user.  Because this operation takes between 2 and 4 seconds to complete, cancellation will occur about half of the time.")>
    <LinkedField("cts")>
    <LinkedMethod(
        "TimeoutAfter",
        "LongRunningOperation")>
    Public Async Sub AsyncCancelAfter()
        cts = New CancellationTokenSource()

        Try
            cts.CancelAfter(3000)
            Dim result As Integer = Await CancellableOperation(cts.Token)
            Console.WriteLine("Operation completed successfully.  Result is {0}.", result)
        Catch ex As OperationCanceledException
            Console.WriteLine("Canceled!")
        End Try
    End Sub


    <Category("Progress")>
    <Title("Progress<T>")>
    <Description("Gets all directory paths on the C: drive, with an Progress<T> object passed in to receive progres notifications." + vbCrLf + vbCrLf + "Note that all calls into the ProgressChanged lambda are occurring while AsyncProgressPolling is suspended awaiting GetAllDirsAsync.")>
    <LinkedField("cts")>
    <LinkedMethod("GetAllDirsAsync")>
    <LinkedClass("GetAllDirsPartialResult")>
    Public Async Function AsyncProgressPolling() As Task
        Dim cts As New CancellationTokenSource
        Dim progress As New Progress(Of GetAllDirsPartialResult)

        Try
            AddHandler progress.ProgressChanged,
                Sub(source, e)
                    ProgressBar.Value = e.Count Mod 100
                End Sub

            For Each i In Await GetAllDirsAsync("c:\", cts.Token, progress)
                Console.WriteLine(i)
            Next
        Catch ex As OperationCanceledException
            Console.WriteLine("Operation canceled.")
        End Try
    End Function

    Public Class GetAllDirsPartialResult
        Public Directories As IList(Of String)
        Public Count As Integer
    End Class

    Public Async Function GetAllDirsAsync(ByVal root As String, ByVal cancel As CancellationToken, ByVal progress As IProgress(Of GetAllDirsPartialResult)) As Task(Of String())
        Dim todo As New Queue(Of String)() : todo.Enqueue(root)
        Dim results As New List(Of String)(1000)
        While todo.Count > 0
            cancel.ThrowIfCancellationRequested()

            If results.Count >= 300 Then Exit While

            Dim dir = todo.Dequeue() : results.Add(dir)
            If progress IsNot Nothing Then
                progress.Report(New GetAllDirsPartialResult With
                                {.Directories = New ReadOnlyCollection(Of String)(results),
                                  .Count = results.Count})
            End If

            Try
                For Each subdir In Await GetDirectoriesAsync(dir)
                    todo.Enqueue(subdir)
                Next
            Catch ex As UnauthorizedAccessException
                ' Skip folders that are inaccessible.
            End Try
        End While

        Return results.ToArray()
    End Function

    Public Shared Function GetDirectoriesAsync(ByVal dir As String) As Task(Of String())
        ' Simulates an async OS API for enumerating directories '


        ''/ * * *
        ' If you're seeing exceptions on this line, go to "Debug|Options and Settings" and
        ' turn off "Just My Code" to disable display of handled first-chance exceptions.
        Return TaskEx.Run(Function() System.IO.Directory.GetDirectories(dir))
        ' * * *
    End Function


    <Category("Exceptions")>
    <Title("Try-Catch")>
    <Description("Exceptions thrown by an awaited method may be naturally caught within a Try-Catch block.")>
    Public Async Sub AsyncTryCatch()
        Try
            WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/clmitae/")))
        Catch
            Console.WriteLine("Error loading page.")
        End Try
    End Sub

    <Category("Exceptions")>
    <Title("Try-Finally")>
    <Description("Exceptions thrown by an awaited method within a Try-Finally will naturally trigger the relevant Finally block.")>
    Public Async Sub AsyncTryFinally()
        Console.WriteLine("Download process beginning...")
        Try
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov"), cts.Token))
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/clmitae/"), cts.Token))
            WriteLinePageTitle(Await DownloadStringTaskSlowNetworkAsync(New Uri("http://www.weather.gov/rss/"), cts.Token))
        Catch
            Console.WriteLine("There was an error downloading the pages.")
        Finally
            Console.WriteLine("Download process completed.")
        End Try
        Console.WriteLine("Success!")
    End Sub

    <Category("Exceptions")>
    <Title("Throw")>
    <Description("Exceptions thrown by your own Task-returning methods may be caught naturally by a caller awaiting the method.")>
    <LinkedMethod("CheckPageSizes")>
    Public Async Sub AsyncThrow()
        Dim uris As New List(Of Uri) From {New Uri("http://www.weather.gov/climate/"), New Uri("http://www.weather.gov"), New Uri("http://www.weather.gov/rss/")}

        Try
            Await CheckPageSizes(uris)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Public Async Function CheckPageSizes(ByVal uris As IList(Of Uri)) As Task
        For Each uri In uris
            Dim page As String = Await New WebClient().DownloadStringTaskAsync(uri)

            If page.Length < 50000 Then Throw New Exception(String.Format("{0} is too small!", uri))

            Console.WriteLine("{0} contains {1} bytes.", uri, page.Length)
        Next
    End Function

    <Category("Declaring Async Methods")>
    <Title("Function that returns Task")>
    <Description("Shows how your own MyTaskReturningFunction method can easily return a Task representing control flow reaching the end of the function." + vbCrLf + vbCrLf + "MyTaskReturningFunction returns a Task after its first await, which is then awaited by AsyncReturnTask.  This task will complete once control reaches the end of MyTaskReturningFunction.  Only then does control return to AsyncFunctionTask.")>
    <LinkedMethod("MyTaskReturningFunction")>
    Public Async Sub AsyncFunctionTask()
        Console.WriteLine("*** BEFORE CALL ***")
        Await MyTaskReturningFunction()
        Console.WriteLine("*** AFTER CALL ***")
    End Sub
    Async Function MyTaskReturningFunction() As Task
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov")))
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/")))
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/")))
    End Function

    <Category("Declaring Async Methods")>
    <Title("Sub")>
    <Description("Shows how your own MySub method can still await asynchronous operations, even though it it's a Sub.  This becomes a ""fire and forget"" method." + vbCrLf + vbCrLf + "MySub returns to its caller after its first await.  Control then returns immediately to AsyncSub.")>
    <LinkedMethod("MySub")>
    Public Sub AsyncSub()
        Console.WriteLine("*** BEFORE CALL ***")
        MySub()
        Console.WriteLine("*** AFTER CALL ***")
    End Sub
    Private Async Sub MySub()
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov")))
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/")))
        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/")))
    End Sub

    <Category("Declaring Async Methods")>
    <Title("Function that returns Task(Of T)")>
    <Description("Shows how your own MyTaskOfTReturningFunction method can easily return a Task(Of T) representing a result value of type T." + vbCrLf + vbCrLf + "MyTaskOfTReturningFunction returns a Task(Of String) after its first await, which is then awaited by AsyncFunctionTaskOfT.  This task will complete once control reaches the end of MyTaskOfTReturningFunction.  Only then does control return to AsyncFunctionTaskOfT.")>
    <LinkedMethod("MyTaskOfTReturningFunction")>
    Public Async Sub AsyncFunctionTaskOfT()
        Console.WriteLine("*** BEFORE CALL ***")
        Console.WriteLine(Await MyTaskOfTReturningFunction())
        Console.WriteLine("*** AFTER CALL ***")
    End Sub
    Private Async Function MyTaskOfTReturningFunction() As Task(Of String)
        Dim sb As New StringBuilder()
        sb.AppendLine(GetPageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov"))))
        sb.AppendLine(GetPageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/"))))
        sb.AppendLine(GetPageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/"))))
        Return sb.ToString()
    End Function

    Public Async Function LongRunningOperation() As Task(Of Integer)
        Console.WriteLine("Attempting long-running operation...")

        Return Await TaskEx.RunEx(Async Function() As Task(Of Integer)
                                      ' Simulate a process that takes between 2 and 4 seconds to complete.
                                      Dim r As New Random
                                      Await TaskEx.Delay(r.Next(2000, 4000))
                                      Return 123
                                  End Function)
    End Function

    Public Async Function CancellableOperation(ByVal cancel As CancellationToken) As Task(Of Integer)
        Console.WriteLine("Attempting long-running, cancellable operation...")

        Return Await TaskEx.RunEx(Async Function() As Task(Of Integer)
                                      ' Simulate a process that takes between 2 and 4 seconds to complete.
                                      Dim r As New Random
                                      Await TaskEx.Delay(r.Next(2000, 4000), cancel)
                                      Return 123
                                  End Function)
    End Function

    <Category("Combinators")>
    <Title("Task.WhenAll")>
    <Description("Performs a set of web requests in parallel, awaiting WhenAll to continue when all tasks complete." + vbCrLf + vbCrLf + "Using WhenAll can be easier than awaiting each Task individually when deaing with a set of Tasks.")>
    Public Async Sub AsyncWhenAll()
        Dim uris = {New Uri("http://www.weather.gov"), New Uri("http://www.weather.gov/climate/"), New Uri("http://www.weather.gov/rss/")}

        Dim pages As String() = Await TaskEx.WhenAll(From uri In uris Select New WebClient().DownloadStringTaskAsync(uri))

        For Each page In pages
            WriteLinePageTitle(page)
        Next
    End Sub


    <Category("Combinators")>
    <Title("Task.WhenAny - Redundancy")>
    <Description("Requests a set of buy/sell recommendations from 3 different servers, continuing once one returns a value." + vbCrLf + vbCrLf + "Using WhenAny in this way enables redundancy across multiple data sources.")>
    <LinkedMethod(
        "GetBuyRecommendation1Async",
        "GetBuyRecommendation2Async",
        "GetBuyRecommendation3Async")>
    Public Async Sub AsyncWhenAnyRedundancy()
        Dim symbol = "ABCXYZ"

        Dim recommendations As New List(Of Task(Of Boolean)) From
            {
                GetBuyRecommendation1Async(symbol),
                GetBuyRecommendation2Async(symbol),
                GetBuyRecommendation3Async(symbol)
            }
        Dim recommendation As Task(Of Boolean) = Await TaskEx.WhenAny(recommendations)
        If Await recommendation Then
            Console.WriteLine("Buy stock {0}!", symbol)
        Else
            Console.WriteLine("Sell stock {0}!", symbol)
        End If
    End Sub

    Public Async Function GetBuyRecommendation1Async(ByVal symbol As String) As Task(Of Boolean)
        Await TaskEx.Delay(500)   ' Simulate 500ms delay in fetching recommendation 1
        Return True
    End Function

    Public Async Function GetBuyRecommendation2Async(ByVal symbol As String) As Task(Of Boolean)
        Await TaskEx.Delay(250)    ' Simulate 250ms delay in fetching recommendation 2
        Return True
    End Function

    Public Async Function GetBuyRecommendation3Async(ByVal symbol As String) As Task(Of Boolean)
        Await TaskEx.Delay(1000)    ' Simulate 1s delay in fetching recommendation 3
        Return True
    End Function

    <Category("Combinators")>
    <Title("Task.WhenAny - Interleaving")>
    <Description("Performs a set of web requests in sequence." + vbCrLf + vbCrLf + "Using WhenAny in this way enables interleaving the requests one at a time.")>
    Public Async Sub AsyncWhenAnyInterleaving()
        Dim uris = {New Uri("http://www.weather.gov"), New Uri("http://www.weather.gov/climate/"), New Uri("http://www.weather.gov/rss/")}

        Dim downloadTasks As List(Of Task(Of String)) = (From uri In uris Select New WebClient().DownloadStringTaskAsync(uri)).ToList()

        While (downloadTasks.Count > 0)
            Dim downloadTask As Task(Of String) = Await TaskEx.WhenAny(downloadTasks)
            downloadTasks.Remove(downloadTask)

            Dim page As String = Await downloadTask
            WriteLinePageTitle(page)
        End While
    End Sub

    <Category("Combinators")>
    <Title("Task.WhenAny - Throttling")>
    <Description("Performs a set of web requests in parallel, capping at no more than 4 requests outstanding at a time." + vbCrLf + vbCrLf + "Using WhenAny in this way enables throttling sets of parallel requests.")>
    Public Async Sub AsyncWhenAnyThrottling()
        Const CONCURRENCY_LEVEL = 4     ' Maximum of 4 requests at a time

        Dim uris = New Uri() {
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h000.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h001.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h002.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h003.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h010.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h011.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h012.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h013.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h020.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h021.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h022.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h023.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h030.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h031.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h032.jpeg?g=400"),
                            New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h033.jpeg?g=400")
                       }

        Dim nextIndex As Integer = 0
        Dim downloadTasks As New List(Of Task(Of String))
        While (nextIndex < CONCURRENCY_LEVEL AndAlso nextIndex < uris.Length)
            Console.WriteLine("Queuing up initial download #{0}.", nextIndex + 1)
            downloadTasks.Add(New WebClient().DownloadStringTaskAsync(uris(nextIndex)))
            nextIndex += 1
        End While
        While (downloadTasks.Count > 0)
            Try
                Dim downloadTask As Task(Of String) = Await TaskEx.WhenAny(downloadTasks)
                downloadTasks.Remove(downloadTask)

                Dim str As String = Await downloadTask
                Dim length As Integer = str.Length

                Console.WriteLine("* Downloaded {0}-byte image.", length)
            Catch ex As Exception
                Console.WriteLine(ex.ToString())
            End Try

            If (nextIndex < uris.Length) Then
                Console.WriteLine("New download slot available.  Queuing up download #{0}.", nextIndex + 1)
                downloadTasks.Add(New WebClient().DownloadStringTaskAsync(uris(nextIndex)))
                nextIndex += 1
            End If
        End While
    End Sub

    <Category("Combinators")>
    <Title("Task.Delay")>
    <Description("Delays for 3 seconds before printing a second message." + vbCrLf + vbCrLf + "Notice that the UI is still responsive during the delay.")>
    Public Async Sub AsyncDelay()
        Console.WriteLine("Before the delay.")
        Await TaskEx.Delay(3000)
        Console.WriteLine("After the delay.")
    End Sub

    <Category("Building Combinators")>
    <Title("TimeoutAfter")>
    <Description("Performs a long-running operation with a 3-second timeout.  Because this operation takes between 2 and 4 seconds to complete, the timeout will be hit about half of the time\r\n\r\nThis shows how you can easily write your own combinators that operate on Tasks to simplify common patterns you encounter.")>
    <LinkedMethod(
        "TimeoutAfter",
        "LongRunningOperation")>
    Public Async Sub AsyncTimeoutAfter()
        Try
            Dim result As Integer = Await TimeoutAfter(LongRunningOperation(), 3000)
            Console.WriteLine("Operation completed successfully.  Result is {0}.", result)
        Catch ex As TimeoutException
            Console.WriteLine("Timeout - operation took longer than 3 seconds.")
        End Try
    End Sub

    Public Shared Async Function TimeoutAfter(Of T)(ByVal task As Task(Of T), ByVal delay As Integer) As Task(Of T)
        Await TaskEx.WhenAny(task, TaskEx.Delay(delay))

        If (Not task.IsCompleted) Then Throw New TimeoutException("Timeout hit.")

        Return Await task
    End Function

    <Category("Building Combinators")>
    <Title("RetryOnFault")>
    <Description("Performs a long-running operation with a 3-second timeout, retrying up to 3 times if the operation times out.\r\n\r\nThis combinator builds on the TimeoutAfter combinator defined above, showing how one combinator can leverage another.")>
    <LinkedMethod(
        "RetryOnFault",
        "LongRunningOperation")>
    Public Async Sub AsyncRetryOnFault()
        Try
            Dim result As Integer = Await RetryOnFault(Function() TimeoutAfter(LongRunningOperation(), 3000), 3)
            Console.WriteLine("Operation completed successfully.  Result is {0}.", result)
        Catch ex As TimeoutException
            Console.WriteLine("Operation timed out 3 times.  Giving up.")
        End Try
    End Sub

    Public Shared Async Function RetryOnFault(Of T)(ByVal f As Func(Of Task(Of T)), ByVal maxTries As Integer) As Task(Of T)
        For i = 0 To maxTries - 1
            Try
                Return Await f()
            Catch ex As Exception When i < maxTries - 1
                ' Swallow exceptions until maxTries tries.
            End Try
        Next

        Return Nothing
    End Function


    <Category("Migration")>
    <Title("From APM")>
    <Description("Defines a ReadAsync method that wraps an existing APM API, Stream.BeginRead and Stream.EndRead." + vbCrLf + vbCrLf + "On the desktop, Task(Of T).Factory.FromAsync would handle all of the complexity for you.  On Silverlight, you can wrap such APIs manually, as in ReadAsync.")>
    <LinkedMethod("ReadAsync")>
    Public Async Sub AsyncFromAPM()
        Dim response = Await WebRequest.Create("http://www.weather.gov").GetResponseAsync()
        Dim stream = response.GetResponseStream()
        Dim buffer(0 To 15) As Byte
        Dim count As Integer

        count = (Await ReadAsync(stream, buffer, 0, 16)) > 0
        While count > 0
            Console.Write(Encoding.UTF8.GetString(buffer, 0, count))
            count = (Await ReadAsync(stream, buffer, 0, 16)) > 0
        End While
    End Sub

    Public Shared Function ReadAsync(ByVal stream As Stream, ByVal buffer As Byte(), ByVal offset As Integer, ByVal count As Integer) As Task(Of Integer)
        Dim tcs As New TaskCompletionSource(Of Integer)
        stream.BeginRead(buffer, offset, count,
                         Sub(iar)
                             Try
                                 tcs.TrySetResult(stream.EndRead(iar))
                             Catch exc As Exception
                                 tcs.TrySetException(exc)
                             End Try
                         End Sub, Nothing)

        Return tcs.Task
    End Function

    <Category("Migration")>
    <Title("From EAP")>
    <Description("Defines our own DownloadStringAsync method that wraps an existing EAP API, WebClient.DownloadStringAsync." + vbCrLf + vbCrLf + "EAP methods are a bit more involved to wrap than APM methods.  This implementation also handles progress notifications and cancellation.")>
    <LinkedMethod("DownloadStringAsync")>
    <LinkedField("cts")>
    Public Async Function AsyncFromEAP() As Task
        cts = New CancellationTokenSource()
        Dim progress As New Progress(Of DownloadProgressChangedEventArgs)
        AddHandler progress.ProgressChanged, Sub(sender, e)
                                                 ProgressBar.Value = e.ProgressPercentage
                                             End Sub

        Try
            WriteLinePageTitle(Await DownloadStringAsync(New Uri("http://www.weather.gov"), cts.Token, progress))
        Catch
            Console.WriteLine("Downloading canceled.")
        End Try
    End Function

    Public Shared Function DownloadStringAsync(ByVal address As Uri, ByVal cancel As CancellationToken, ByVal progress As IProgress(Of DownloadProgressChangedEventArgs)) As Task(Of String)
        ' Create the task to be returned
        Dim tcs As New TaskCompletionSource(Of String)(address)
        Dim webClient As New WebClient()

        ' Register the cancellation token
        Dim ctr = cancel.Register(AddressOf webClient.CancelAsync)

        ' Setup the callback event handlers
        AddHandler webClient.DownloadProgressChanged, Sub(s, e) progress.Report(e)
        AddHandler webClient.DownloadStringCompleted, Sub(s, e)
                                                          ctr.Dispose()
                                                          If e.Error IsNot Nothing Then
                                                              tcs.TrySetException(e.Error)
                                                          ElseIf e.Cancelled Then
                                                              tcs.TrySetCanceled()
                                                          Else
                                                              tcs.TrySetResult(e.Result)
                                                          End If
                                                      End Sub

        ' Start the async operation.
        webClient.DownloadStringAsync(address, tcs)

        ' Return the task that represents the async operation
        Return tcs.Task
    End Function

    <Category("Migration")>
    <Title("From synchronous - CopyTo")>
    <Description("This example shows the minor changes needed to migrate an existing synchronous CopyTo method to become CopyToAsync.  Scroll down to see the CopyTo and the translated CopyToAsync implementations." + vbCrLf + vbCrLf + "The core transformation just involves calling ReadAsync/WriteAsync instead of Read/Write, and then awaiting each of those calls.  This example then adds progress support by accepting an IProgress<T> parameter and reporting progress to it, and adds cancellation by accepting a CancellationToken parameter and polling it each time through the loop.")>
    <LinkedField("cts")>
    <LinkedMethod("CopyTo", "CopyToAsync")>
    Public Async Function AsyncCopyTo() As Task
        cts = New CancellationTokenSource()

        ' Download a file.
        Using source = Await New WebClient().OpenReadTaskAsync(New Uri("http://www.weather.gov/climate/"))
            ' Create the streams.
            Dim destination As MemoryStream = New MemoryStream()

            Try
                Console.WriteLine("Source length: {0}", source.Length.ToString())

                Dim progress As New Progress(Of Long)
                AddHandler progress.ProgressChanged, Sub(sender, e) Console.WriteLine("{0} bytes read.", e)

                ' Copy source to destination.
                Await CopyToAsync(source, destination, cts.Token, progress)
            Catch ex As OperationCanceledException
                Console.WriteLine("Copy canceled.")
            End Try
            Console.WriteLine("Destination length: {0}", destination.Length.ToString())
        End Using
    End Function

    Public Shared Async Function CopyToAsync(ByVal source As Stream, ByVal destination As Stream,
                                             ByVal cancellationToken As CancellationToken,
                                             ByVal progress As IProgress(Of Long)) As Task
        ' New asynchronous implementation:

        Dim buffer(0 To &HFFF) As Byte
        Dim bytesRead As Integer
        Dim totalRead As Long = 0
        While (bytesRead = Await source.ReadAsync(buffer, 0, buffer.Length)) > 0
            Await destination.WriteAsync(buffer, 0, bytesRead)
            cancellationToken.ThrowIfCancellationRequested()    ' cancellation support
            totalRead += bytesRead
            progress.Report(totalRead)                          ' progress support
        End While
    End Function

    Public Shared Sub CopyTo(ByVal source As Stream, ByVal destination As Stream)
        ' Old synchronous implementation:

        Dim buffer(0 To &HFFF) As Byte
        Dim bytesRead As Integer
        Dim totalRead As Long = 0
        While (bytesRead = source.Read(buffer, 0, buffer.Length)) > 0
            destination.Write(buffer, 0, bytesRead)
            totalRead += bytesRead
        End While
    End Sub

    <Category("Switching Threads")>
    <Title("CPU-intensive work - Task.Run")>
    <Description("Performs CPU-intensive work represented in a lambda expression by dispatching this work to a thread pool thread.")>
    <LinkedMethod("DoCpuIntensiveWork")>
    Public Async Sub AsyncRunCPU()
        Console.WriteLine("On the UI thread.")

        Dim result As Integer =
            Await TaskEx.Run(
                Function()
                    Console.WriteLine("Starting CPU-intensive work on background thread...")
                    Dim work As Integer = DoCpuIntensiveWork()
                    Console.WriteLine("Done with CPU-intensive work!")
                    Return work
                End Function)

        Console.WriteLine("Back on the UI thread.  Result is {0}.", result)
    End Sub

    Public Function DoCpuIntensiveWork() As Integer
        ' Simulate some CPU-bound work on the background thread:
        Thread.Sleep(5000)
        Return 123
    End Function

    <Category("Switching Threads")>
    <Title("Avoiding thread switches - Task.ConfigureAwait")>
    <Description("Avoids thread switches back to the UI thread after each network request by calling ConfigureAwait on the download task with a parameter of False.  This can be useful to optimize highly performance-sensitive code." + vbCrLf + vbCrLf + "Once the composite result is gathered, control is switched back to the UI thread to output the result.")>
    Public Async Sub AsyncConfigureAwait()
        Dim uris = New Uri() {New Uri("http://www.weather.gov"), New Uri("http://www.weather.gov/climate/"), New Uri("http://www.weather.gov/rss/")}

        Dim totalLength As Integer = 0

        ' Run on background thread to avoid thread hops after each iteration
        For Each uri In uris
            Dim s As String = Await New WebClient().DownloadStringTaskAsync(uri).ConfigureAwait(False)  ' Avoid hops after each async completion
            totalLength += s.Length
        Next

        ' Switch back to UI thread to update UI
        Application.Current.Dispatcher.Invoke(Sub()
                                                  Console.WriteLine("Back on the UI thread.  Total length of pages is {0}.", totalLength)
                                              End Sub)
    End Sub

    <Category("Control Flow")>
    <Title("If-Then-Else")>
    <Description("Control flows naturally across network requests both within and after an If-Then-Else block.")>
    Public Async Sub AsyncIfElse()
        If DateTime.Now.DayOfWeek = DayOfWeek.Saturday OrElse DateTime.Now.DayOfWeek = DayOfWeek.Sunday Then
            WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/om/marine/home.htm")))
            Console.WriteLine("It's the weekend!  Time for the marine forecast!")
        Else
            WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov")))
            Console.WriteLine("Back to work!")
        End If

        WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/forecasts/graphical/")))
        Console.WriteLine("Always useful to get a general forecast!")
    End Sub

    <Category("Control Flow")>
    <Title("If-Then-Else")>
    <Description("Without await, each branch of an If that performs its own network request must have its own callback method.  To consolidate the logic following the If block, further methods are also needed.")>
    <LinkedMethod(
        "AsyncIfElseBefore_Weekend_DownloadStringCompleted",
        "AsyncIfElseBefore_Weekday_DownloadStringCompleted",
        "AsyncIfElseBefore_GeneralForecast",
        "AsyncIfElseBefore_GeneralForecast_DownloadStringCompleted")>
    Public Sub AsyncIfElseBefore()
        Dim client As New WebClient

        If DateTime.Now.DayOfWeek = DayOfWeek.Saturday OrElse DateTime.Now.DayOfWeek = DayOfWeek.Sunday Then
            AddHandler client.DownloadStringCompleted, AddressOf AsyncIfElseBefore_Weekend_DownloadStringCompleted
            client.DownloadStringAsync(New Uri("http://www.weather.gov/om/marine/home.htm"))
        Else
            AddHandler client.DownloadStringCompleted, AddressOf AsyncIfElseBefore_Weekday_DownloadStringCompleted
            client.DownloadStringAsync(New Uri("http://www.weather.gov"))
        End If
    End Sub

    Sub AsyncIfElseBefore_Weekend_DownloadStringCompleted(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)

        Console.WriteLine("It's the weekend!  Time for the marine forecast!")

        AsyncIfElseBefore_GeneralForecast()
    End Sub

    Sub AsyncIfElseBefore_Weekday_DownloadStringCompleted(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)

        Console.WriteLine("Back to work!")

        AsyncIfElseBefore_GeneralForecast()
    End Sub

    Sub AsyncIfElseBefore_GeneralForecast()
        Dim client As WebClient = New WebClient()

        AddHandler client.DownloadStringCompleted, AddressOf AsyncIfElseBefore_GeneralForecast_DownloadStringCompleted
        client.DownloadStringAsync(New Uri("http://www.weather.gov/forecasts/graphical/"))
    End Sub

    Sub AsyncIfElseBefore_GeneralForecast_DownloadStringCompleted(ByVal sender As Object, ByVal e As DownloadStringCompletedEventArgs)
        WriteLinePageTitle(e.Result)

        Console.WriteLine("Always useful to get a general forecast!")
    End Sub

    <Category("Control Flow")>
    <Description("Control flows naturally across network requests both within and after a Select Case block.")>
    <Title("Select Case")>
    Public Async Sub AsyncSwitch()
        Dim stockPrice As Double = 123.45

        Select Case DateTime.Now.DayOfWeek
            Case DayOfWeek.Monday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/wa.php?x=1")))
                stockPrice += 1.25
            Case DayOfWeek.Tuesday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/or.php?x=1")))
                stockPrice *= 1.04
            Case DayOfWeek.Wednesday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/ca.php?x=1")))
                stockPrice -= 0.58
            Case DayOfWeek.Thursday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/nv.php?x=1")))
                stockPrice *= 0.99
            Case DayOfWeek.Friday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/az.php?x=1")))
                stockPrice += 0.79
            Case DayOfWeek.Saturday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/ut.php?x=1")))
                stockPrice += 1.8
            Case DayOfWeek.Sunday
                WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/alerts-beta/nm.php?x=1")))
                stockPrice /= 1.2
        End Select
        Console.WriteLine("Today's stock price: {0}", stockPrice)
    End Sub

    <Category("Control Flow")>
    <Title("Do-Loop")>
    <Description("Control flows naturally across network requests while iterating through a Do loop.")>
    <LinkedField("cts")>
    <LinkedMethod("GetNextUri")>
    Public Async Function AsyncDoWhile() As Task
        Console.WriteLine("Fetching movies...")

        cts = New CancellationTokenSource()

        Dim client As New WebClient

        Dim nextUri As String = "http://odata.netflix.com/Catalog/Titles?$inlinecount=allpages&$filter=ReleaseYear%20le%201942"

        Try
            Dim movieCount As Integer = 0
            Do
                ' Fetch next batch of movies:
                Dim task = New WebClient().DownloadStringTaskAsync(New Uri(nextUri), cts.Token)
                Dim resultString As String = Await task
                Dim result = XDocument.Parse(resultString)

                ' Output movies:
                Dim entries = result.Descendants(name("entry"))
                For Each entry In entries
                    Console.WriteLine(entry.Element(name("title")).Value)
                    movieCount += 1
                Next
                ' Update progress:
                Dim countElement = CType(result.Descendants(mName("count")).SingleOrDefault(), XElement)
                If countElement IsNot Nothing Then
                    Dim total = Integer.Parse(countElement.Value)
                    ProgressBar.Value = CInt((movieCount * 100.0) / total)
                End If
                nextUri = GetNextUri(result)
            Loop While nextUri IsNot Nothing
        Catch ex As OperationCanceledException
            Console.WriteLine("Fetch canceled.")
        End Try
    End Function
    Private Function GetNextUri(ByVal xml As XDocument) As String
        Return (From elem In xml.Element(name("feed")).Elements(name("link"))
                Where elem.Attribute("rel").Value = "next"
                Select elem.Attribute("href").Value).SingleOrDefault()
    End Function

    Public Shared Function name(ByVal x As String) As XName
        Return XName.Get(x, "http://www.w3.org/2005/Atom")
    End Function

    Public Shared Function mName(ByVal x As String) As XName
        Return XName.Get(x, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")
    End Function

    <Category("Control Flow")>
    <Title("For")>
    <Description("Control flows naturally across network requests while iterating through a For loop.")>
    Public Async Sub AsyncFor()
        Dim tasks = New Task(Of String)() {
                New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov")),
                New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/")),
                New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/"))
            }

        For i = 0 To 2
            WriteLinePageTitle(Await tasks(i))
        Next
    End Sub

    <Category("Control Flow")>
    <Title("For Each")>
    <Description("Control flows naturally across network requests while iterating through a For Each loop.")>
    Public Async Sub AsyncForEach()
        Dim uris = New Uri() {New Uri("http://www.weather.gov"), New Uri("http://www.weather.gov/climate/"), New Uri("http://www.weather.gov/rss/")}

        For Each uri In uris
            WriteLinePageTitle(Await New WebClient().DownloadStringTaskAsync(uri))
        Next
    End Sub

    <Category("Control Flow")>
    <Title("Exit")>
    <Description("Control flows naturally across network requests while escaping a loop using a Exit statement.")>
    <LinkedField("cts")>
    Public Async Function AsyncExit() As Task
        cts = New CancellationTokenSource()

        Console.WriteLine("Successful searches:")

        Try
            Dim queryString = "mad"
            While (True)
                Dim uri As New Uri(String.Format("http://odata.netflix.com/Catalog/Titles/$count?$filter=substringof('{0}',Name)", queryString))
                Dim results = Integer.Parse(Await New WebClient().DownloadStringTaskAsync(uri, cts.Token))

                Console.WriteLine("Movies containing '{0}': {1}", queryString, results)

                If results = 0 Then Exit While

                queryString += ", mad"
            End While

            Console.WriteLine("No more results!")
        Catch
            Console.WriteLine("Canceled!")
        End Try
    End Function

    <Category("Control Flow")>
    <Title("Continue")>
    <Description("Control flows naturally across network requests while skipping the rest of a loop iteration using a Continue statement.")>
    Public Async Sub AsyncContinue()
        Dim uris As New List(Of Uri) From {New Uri("http://www.weather.gov"), New Uri("http://www.weather.gov/climate/"), New Uri("http://www.weather.gov/rss/")}

        For Each uri In uris
            Dim page As String = Await New WebClient().DownloadStringTaskAsync(uri)

            If page.Length > 50000 Then Continue For

            Console.WriteLine("{0} is a small {1}-character page.", uri, page.Length)
            Console.WriteLine("First character is {0}.", page(0))
        Next
    End Sub

    <Category("Control Flow")>
    <Title("Using")>
    <Description("When control flows out of a Using block that awaits expressions, either naturally or due to an exception, that using block's resource object is always properly disposed.")>
    Public Async Sub AsyncUsing()
        Using response = Await WebRequest.Create("http://www.weather.gov").GetResponseAsync()
            Using stream = response.GetResponseStream()
                Using reader As New StreamReader(stream)
                    WriteLinePageTitle(Await reader.ReadToEndAsync())
                End Using
            End Using
        End Using
    End Sub

    <Category("Anonymous Functions")>
    <Title("Async Sub")>
    <Description("A method may be awaited from within a Sub lambda with the Async modifier.  Click the button to advance to the next year." + vbCrLf + vbCrLf + "This is especially useful for signing up event handlers.")>
    <LinkedField("cts")>
    Public Sub AsyncSubLambda()
        cts = New CancellationTokenSource()

        Dim form As New Form
        Dim button As New Button
        button.Dock = DockStyle.Fill
        form.Controls.Add(button)
        button.Text = "Click me!"
        form.Show()

        Dim client As New WebClient

        Dim year As Integer = 1950

        AddHandler button.Click,
            Async Sub()
                Dim url = String.Format("http://netflix.cloudapp.net/Catalog/Titles/$count?$filter=ReleaseYear%20eq%20{0}", year)
                Dim count = Integer.Parse(Await client.DownloadStringTaskAsync(New Uri(url), cts.Token))
                button.Text = String.Format("Netflix has {0} movies from the year {1}.", count, year)

                year += 1
            End Sub
    End Sub

    <Category("Anonymous Functions")>
    <Title("Async Function")>
    <Description("A method may be awaited from within a Function lambda with the Async modifier.  Click the button to advance to the next year.")>
    <LinkedField("cts")>
    Public Sub AsyncFunctionLambda()
        cts = New CancellationTokenSource()

        Dim form As New Form
        Dim button As New Button
        button.Dock = DockStyle.Fill
        form.Controls.Add(button)
        button.Text = "Click me!"
        form.Show()

        Dim client As New WebClient()

        Dim countTask As Func(Of Integer, Task) = Async Function(year)
                                                      Dim url = String.Format("http://netflix.cloudapp.net/Catalog/Titles/$count?$filter=ReleaseYear%20eq%20{0}", year)
                                                      Dim count = Integer.Parse(Await client.DownloadStringTaskAsync(New Uri(url), cts.Token))
                                                      button.Text = String.Format("Netflix has {0} movies from the year {1}.", count, year)

                                                      If year < 1960 Then
                                                          Await countTask(year + 1)
                                                      End If
                                                  End Function

        countTask(1950)
    End Sub


    <Category("Expressions (within await)")>
    <Title("Member access")>
    <Description("An Await expression may await a member of an object.")>
    <LinkedClass("StringTaskPair")>
    Public Async Sub AsyncMemberAccessWithin()
        Dim pair As New StringTaskPair()
        pair.Task1 = New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov"))
        pair.Task2 = New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/"))

        WriteLinePageTitle(Await pair.Task2)
    End Sub

    Public Class StringTaskPair
        Public Property Task1 As Task(Of String)
        Public Property Task2 As Task(Of String)
    End Class

    <Category("Expressions (within await)")>
    <Title("Invocation")>
    <Description("An Await expression may await the result of invoking a delegate.")>
    Public Async Sub AsyncInvocationWithin()
        Dim topic = "monkeys"

        Dim searchTaskGenerator As Func(Of String, Task(Of String)) =
                Function(s) New WebClient().DownloadStringTaskAsync(New Uri(String.Format("http://odata.netflix.com/Catalog/Titles/$count?$filter=substringof('{0}',Name)", s)))

        Console.WriteLine("{0} movies about {1}.", Await searchTaskGenerator(topic), topic)
    End Sub

    <Category("Expressions (within await)")>
    <Title("Indexing")>
    <Description("An Await expression may await the result of indexing a collection.")>
    Public Async Sub AsyncIndexingWithin()
        Dim tasks = New Task(Of String)() {
                                           New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov")),
                                           New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/climate/")),
                                           New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov/rss/"))
                                          }

        WriteLinePageTitle(Await tasks(2))
    End Sub

    <Category("Expressions (within await)")>
    <Title("CType")>
    <Description("An Await expression may await an expression that has just been converted using CType to an awaitable type such As Task(Of String).")>
    <LinkedClass("StringCalculation")>
    Public Async Sub AsyncCTypeWithin()
        Dim sr As New StringCalculation()

        Console.WriteLine(Await CType(sr, Task(Of String)))
    End Sub

    Public Class StringCalculation
        Public Shared Narrowing Operator CType(ByVal sr As StringCalculation) As Task(Of String)
            Return TaskEx.Run(
                Function()
                    Thread.Sleep(1000)      ' Simulate calculation to produce a string.
                    Return "FooBar"
                End Function)
        End Operator
    End Class

    <Category("Expressions (within await)")>
    <Title("TryCast")>
    <Description("An Await expression may await an expression that has just been converted using TryCast.")>
    <LinkedMethod("LoadTaskObject")>
    Public Async Sub AsyncTryCastWithin()
        Dim o As Object = LoadTaskObject()

        WriteLinePageTitle(Await (TryCast(o, Task(Of String))))
    End Sub

    Public Function LoadTaskObject() As Object
        Return New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov"))
    End Function

    Public Class RemoteInteger
        Private value As Integer   ' Hold the value that we're simulating remote storage of.

        Public Shared Function CreateAsync(ByVal i As Integer) As Task(Of RemoteInteger)
            ' Simulate sending i to a remote server:
            Dim creation As New Task(Of RemoteInteger)(Function() New RemoteInteger(i))
            creation.Start()
            Return creation
        End Function

        Private Sub New(ByVal i As Integer)
            value = i
        End Sub

        Public Function GetValueAsync() As Task(Of Integer)
            ' Simulate fetching the value back from a remote server:
            Dim getValue As New Task(Of Integer)(Function() value)
            getValue.Start()
            Return getValue
        End Function
        Public Shared Operator +(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote addition of i1 and i2:
            Dim addition As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value + i2.value))
            addition.Start()
            Return addition
        End Operator
        Public Shared Operator -(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote subtraction of i1 and i2:
            Dim subtraction As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value - i2.value))
            subtraction.Start()
            Return subtraction
        End Operator
        Public Shared Operator *(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote multiplication of i1 and i2:
            Dim multiplication As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value * i2.value))
            multiplication.Start()
            Return multiplication
        End Operator
        Public Shared Operator /(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote division of i1 and i2:
            Dim division As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value / i2.value))
            division.Start()
            Return division
        End Operator
        Public Shared Operator Mod(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote modulus of i1 and i2:
            Dim modulus As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value Mod i2.value))
            modulus.Start()
            Return modulus
        End Operator
        Public Shared Operator -(ByVal i As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote negation of i:
            Dim negation As New Task(Of RemoteInteger)(Function() New RemoteInteger(-i.value))
            negation.Start()
            Return negation
        End Operator
        Public Shared Operator =(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of Boolean)
            ' Simulate remote equality comparison of i1 and i2:
            Dim equality As New Task(Of Boolean)(Function() (i1.value = i2.value))
            equality.Start()
            Return equality
        End Operator
        Public Shared Operator <>(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of Boolean)
            ' Simulate remote inequality comparison of i1 and i2:
            Dim inequality As New Task(Of Boolean)(Function() (i1.value <> i2.value))
            inequality.Start()
            Return inequality
        End Operator
        Public Shared Operator <(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of Boolean)
            ' Simulate remote less-than comparison of i1 and i2:
            Dim lessThan As New Task(Of Boolean)(Function() (i1.value < i2.value))
            lessThan.Start()
            Return lessThan
        End Operator
        Public Shared Operator >(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of Boolean)
            ' Simulate remote greater-than comparison of i1 and i2:
            Dim greaterThan As New Task(Of Boolean)(Function() (i1.value > i2.value))
            greaterThan.Start()
            Return greaterThan
        End Operator
        Public Shared Operator <=(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of Boolean)
            ' Simulate remote less-than-or-equal comparison of i1 and i2:
            Dim lessThanEqual As New Task(Of Boolean)(Function() (i1.value <= i2.value))
            lessThanEqual.Start()
            Return lessThanEqual
        End Operator
        Public Shared Operator >=(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of Boolean)
            ' Simulate remote greater-than-or-equal comparison of i1 and i2:
            Dim greaterThanEqual As New Task(Of Boolean)(Function() (i1.value >= i2.value))
            greaterThanEqual.Start()
            Return greaterThanEqual
        End Operator
        Public Shared Operator <<(ByVal i1 As RemoteInteger, ByVal i2 As Integer) As Task(Of RemoteInteger)
            ' Simulate remote left-shift of i1 by i2 bits:
            Dim leftShift As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value << i2))
            leftShift.Start()
            Return leftShift
        End Operator
        Public Shared Operator >>(ByVal i1 As RemoteInteger, ByVal i2 As Integer) As Task(Of RemoteInteger)
            ' Simulate remote left-shift of i1 by i2 bits:
            Dim rightShift As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value >> i2))
            rightShift.Start()
            Return rightShift
        End Operator
        Public Shared Operator &(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote string concatenation of i1 and i2:
            Dim bitwiseAnd As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value & i2.value))
            bitwiseAnd.Start()
            Return bitwiseAnd
        End Operator
        Public Shared Operator And(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote bitwise AND of i1 and i2:
            Dim bitwiseAnd As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value And i2.value))
            bitwiseAnd.Start()
            Return bitwiseAnd
        End Operator
        Public Shared Operator Not(ByVal i As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote bitwise NOT of i:
            Dim bitwiseNot As New Task(Of RemoteInteger)(Function() New RemoteInteger(Not i.value))
            bitwiseNot.Start()
            Return bitwiseNot
        End Operator
        Public Shared Operator Or(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote bitwise OR of i1 and i2:
            Dim bitwiseOr As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value Or i2.value))
            bitwiseOr.Start()
            Return bitwiseOr
        End Operator
        Public Shared Operator Xor(ByVal i1 As RemoteInteger, ByVal i2 As RemoteInteger) As Task(Of RemoteInteger)
            ' Simulate remote bitwise XOR of i1 and i2:
            Dim bitwiseXor As New Task(Of RemoteInteger)(Function() New RemoteInteger(i1.value Xor i2.value))
            bitwiseXor.Start()
            Return bitwiseXor
        End Operator

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Throw New NotSupportedException()
        End Function

        Public Overrides Function GetHashCode() As Integer
            Throw New NotSupportedException()
        End Function
    End Class

    <Category("Expressions (within await)")>
    <Title("Binary + operator")>
    <Description("An Await expression may await the result of an addition operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncBinaryPlusWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 + remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("Binary - operator")>
    <Description("An Await expression may await the result of a subtraction operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncBinaryMinusWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 - remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("* operator")>
    <Description("An Await expression may await the result of a multiplication operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncMultiplyWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(12)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(23)

        Dim result As RemoteInteger = Await (remoteInt1 * remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("/ operator")>
    <Description("An Await expression may await the result of a division operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncDivideWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(360)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(15)

        Dim result As RemoteInteger = Await (remoteInt1 / remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("Mod operator")>
    <Description("An Await expression may await the result of a remainder operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncModulusWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 Mod remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("Unary - operator")>
    <Description("An Await expression may await the result of a negation operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncUnaryMinusWithin()
        Dim remoteInt As RemoteInteger = Await RemoteInteger.CreateAsync(100)

        Dim result As RemoteInteger = Await -remoteInt

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("= operator")>
    <Description("An Await expression may await the result of an equality test." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncEqualsWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Console.WriteLine(Await (remoteInt1 = remoteInt2))
    End Sub

    <Category("Expressions (within await)")>
    <Title("<> operator")>
    <Description("An Await expression may await the result of an inequality test." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncNotEqualsWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Console.WriteLine(Await (remoteInt1 <> remoteInt2))
    End Sub

    <Category("Expressions (within await)")>
    <Title("< operator")>
    <Description("An Await expression may await the result of a less-than test." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncLessThanWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Console.WriteLine(Await (remoteInt1 < remoteInt2))
    End Sub

    <Category("Expressions (within await)")>
    <Title("> operator")>
    <Description("An Await expression may await the result of a greater-than test." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncGreaterThanWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Console.WriteLine(Await (remoteInt1 > remoteInt2))
    End Sub

    <Category("Expressions (within await)")>
    <Title("<= operator")>
    <Description("An Await expression may await the result of a less-than-or-equal test." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncLessThanEqualWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Console.WriteLine(Await (remoteInt1 <= remoteInt2))
    End Sub

    <Category("Expressions (within await)")>
    <Title(">= operator")>
    <Description("An Await expression may await the result of a greater-than-or-equal test." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncGreaterThanEqualWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Console.WriteLine(Await (remoteInt1 >= remoteInt2))
    End Sub

    <Category("Expressions (within await)")>
    <Title("<< operator")>
    <Description("An Await expression may await the result of a left-shift operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncLeftShift()
        Dim remoteInt As RemoteInteger = Await RemoteInteger.CreateAsync(200)

        Dim result As RemoteInteger = Await (remoteInt << 3)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title(">> operator")>
    <Description("An Await expression may await the result of a right-shift operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncRightShift()
        Dim remoteInt As RemoteInteger = Await RemoteInteger.CreateAsync(200)

        Dim result As RemoteInteger = Await (remoteInt >> 4)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("& operator - concatenation")>
    <Description("An Await expression may await the result of a string concatenation operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncConcatenateWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 & remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("And operator")>
    <Description("An Await expression may await the result of a bitwise AND operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncBitwiseAndWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 And remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("Or operator")>
    <Description("An Await expression may await the result of a bitwise OR operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncBitwiseOrWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 Or remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("Xor operator")>
    <Description("An Await expression may await the result of a bitwise XOR operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncBitwiseXorWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(200)
        Dim remoteInt2 As RemoteInteger = Await RemoteInteger.CreateAsync(34)

        Dim result As RemoteInteger = Await (remoteInt1 Xor remoteInt2)

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (within await)")>
    <Title("Not operator")>
    <Description("An Await expression may await the result of a bitwise NOT operation." + vbCrLf + vbCrLf + "This example simulates a remote server that can perform calculations for you, exposing a RemoteInt class to represent an integer value created on the server.")>
    <LinkedClass("RemoteInteger")>
    Public Async Sub AsyncBitwiseNotWithin()
        Dim remoteInt1 As RemoteInteger = Await RemoteInteger.CreateAsync(100)

        Dim result As RemoteInteger = Await Not remoteInt1

        Console.WriteLine(Await result.GetValueAsync())
    End Sub

    <Category("Expressions (around await)")>
    <Title("Member access")>
    <Description("The result of an Await expression may have its members directly evaluated.")>
    Public Async Sub AsyncMemberAccessAround()
        Console.WriteLine("Content length:")
        Console.WriteLine((Await New WebClient().DownloadStringTaskAsync(New Uri("http://www.weather.gov"))).Length)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Invocation")>
    <Description("The result of an Await expression may be directly invoked.")>
    <LinkedMethod("PrecalculateCosineTableAsync")>
    Public Async Sub AsyncInvocationAround()
        Console.WriteLine((Await PrecalculateCosineTableAsync())(0.0))
    End Sub

    Public Async Function PrecalculateCosineTableAsync() As Task(Of Func(Of Double, Double))
        Return Await TaskEx.Run(Function()
                                    Thread.Sleep(1000)     ' Simulate precalculating a table of cosine values

                                    Return Function(d)
                                               If (d = 0.0) Then
                                                   Return 1.0
                                               Else
                                                   Throw New NotSupportedException()
                                               End If
                                           End Function
                                End Function)
    End Function

    <Category("Expressions (around await)")>
    <Title("Indexing")>
    <Description("The result of an Await expression may be directly indexed.")>
    Public Async Sub AsyncIndexingAround()
        Console.WriteLine((Await GetDigitsOfPi())(50))
    End Sub

    Public Async Function GetDigitsOfPi() As Task(Of String)
        Return Await TaskEx.Run(Function()
                                    Thread.Sleep(1000)     ' Simulate calculating digits of pi

                                    Return "3141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067"
                                End Function)
    End Function

    <Category("Expressions (around await)")>
    <Title("CType")>
    <Description("The result of an Await expression may be directly cast to another type using CType.")>
    <LinkedMethod("PrintByte", "LongRunningOperation")>
    Public Async Sub AsyncCTypeAround()
        PrintByte(CType(Await LongRunningOperation(), Byte))
    End Sub

    Public Sub PrintByte(ByVal b As Byte)
        Console.WriteLine(b)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Type conversion functions")>
    <Description("The result of an Await expression may be directly cast to another type using type conversion functions, such as CShort.")>
    <LinkedMethod("PrintShort", "LongRunningOperation")>
    Public Async Sub AsyncConversionsAround()
        PrintShort(CShort(Await LongRunningOperation()))
    End Sub

    Public Sub PrintShort(ByVal s As Short)
        Console.WriteLine(s)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Widening conversion")>
    <Description("The result of an Await expression may be directly used in a context where a widening conversion will be required.")>
    <LinkedMethod("PrintLong", "LongRunningOperation")>
    Public Async Sub AsyncWideningConversionAround()
        PrintLong(Await LongRunningOperation())
    End Sub

    Public Sub PrintLong(ByVal l As Long)
        Console.WriteLine(l)
    End Sub

    <Category("Expressions (around await)")>
    <Title("TryCast")>
    <Description("The result of an Await expression may be directly converted to another type using TryCast.")>
    <LinkedMethod("DeserializeAsync")>
    Public Async Sub AsyncTryCastAround()
        Dim str As String = TryCast(Await DeserializeAsync(), String)
        If str IsNot Nothing Then
            Console.WriteLine(str)
        Else
            Console.WriteLine("Deserialization failed.")
        End If
    End Sub

    Public Async Function DeserializeAsync() As Task(Of Object)
        Await TaskEx.Delay(200)      ' Simulate loading an object from disk and deserializing it
        Return "serialized string"
    End Function

    <Category("Expressions (around await)")>
    <Title("TypeOf-Is operator")>
    <Description("The result of an Await expression may be directly type-tested.")>
    <LinkedMethod("DeserializeAsync")>
    Public Async Sub AsyncTypeOfIsAround()
        If TypeOf (Await DeserializeAsync()) Is String Then
            Console.WriteLine("Value has type string.")
        Else
            Console.WriteLine("Value has another type.")
        End If
    End Sub

    <Category("Expressions (around await)")>
    <Title("Binary + operator")>
    <Description("The result of an Await expression may be directly added to another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncBinaryPlusAround()
        Console.WriteLine((Await LongRunningOperation()) + 1)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Binary - operator")>
    <Description("The result of an Await expression may have another value directly subtracted from it.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncBinaryMinusAround()
        Console.WriteLine((Await LongRunningOperation()) - 5)
    End Sub

    <Category("Expressions (around await)")>
    <Title("* operator")>
    <Description("The result of an Await expression may be directly multiplied by another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncMultiplyAround()
        Console.WriteLine((Await LongRunningOperation()) * 3)
    End Sub

    <Category("Expressions (around await)")>
    <Title("/ operator")>
    <Description("The result of an Await expression may be directly divided by another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncDivideAround()
        Console.WriteLine((Await LongRunningOperation()) / 7.0)
    End Sub

    <Category("Expressions (around await)")>
    <Title("\ operator")>
    <Description("The result of an Await expression may be directly divided by another value using integer division.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncIntegerDivideAround()
        Console.WriteLine((Await LongRunningOperation()) \ 7)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Mod operator")>
    <Description("The result of an Await expression may directly participate in a remainder calculation.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncModulusAround()
        Console.WriteLine((Await LongRunningOperation()) Mod 16)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Unary - operator")>
    <Description("The result of an Await expression may be directly negated.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncUnaryMinusAround()
        Console.WriteLine(-Await LongRunningOperation())
    End Sub

    <Category("Expressions (around await)")>
    <Title("= operator")>
    <Description("The result of an Await expression may be directly tested for equality.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncEqualsAround()
        Console.WriteLine((Await LongRunningOperation()) = 123)
    End Sub

    <Category("Expressions (around await)")>
    <Title("<> operator")>
    <Description("The result of an Await expression may be directly tested for inequality.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncNotEqualsAround()
        Console.WriteLine((Await LongRunningOperation()) <> 123)
    End Sub

    <Category("Expressions (around await)")>
    <Title("< operator")>
    <Description("The result of an Await expression may be directly tested for being less than another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncLessThanAround()
        Console.WriteLine((Await LongRunningOperation()) < 123)
    End Sub

    <Category("Expressions (around await)")>
    <Title("> operator")>
    <Description("The result of an Await expression may be directly tested for being greater than another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncGreaterThanAround()
        Console.WriteLine((Await LongRunningOperation()) > 123)
    End Sub

    <Category("Expressions (around await)")>
    <Title("<= operator")>
    <Description("The result of an Await expression may be directly tested for being less than or equal to another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncLessThanEqualAround()
        Console.WriteLine((Await LongRunningOperation()) <= 123)
    End Sub

    <Category("Expressions (around await)")>
    <Title(">= operator")>
    <Description("The result of an Await expression may be directly tested for being greater than or equal to another value.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncGreaterThanEqualAround()
        Console.WriteLine((Await LongRunningOperation()) >= 123)
    End Sub

    <Category("Expressions (around await)")>
    <Title("<< operator")>
    <Description("The result of an Await expression may directly participate in a left-shift calculation.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncLeftShiftAround()
        Console.WriteLine((Await LongRunningOperation()) << 3)
    End Sub

    <Category("Expressions (around await)")>
    <Title(">> operator")>
    <Description("The result of an Await expression may directly participate in a right-shift calculation.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncRightShiftAround()
        Console.WriteLine((Await LongRunningOperation()) >> 4)
    End Sub

    <Category("Expressions (around await)")>
    <Title("And operator")>
    <Description("The result of an Await expression may directly participate in a bitwise AND.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncBitwiseAndAround()
        Console.WriteLine((Await LongRunningOperation()) And &H4)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Or operator")>
    <Description("The result of an Await expression may directly participate in a bitwise OR.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncBitwiseOrAround()
        Console.WriteLine((Await LongRunningOperation()) Or &H4)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Xor operator")>
    <Description("The result of an Await expression may directly participate in a bitwise XOR.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncBitwiseXorAround()
        Console.WriteLine((Await LongRunningOperation()) Xor &H4)
    End Sub

    <Category("Expressions (around await)")>
    <Title("Not operator")>
    <Description("The result of an Await expression may directly participate in a bitwise NOT.")>
    <LinkedMethod("LongRunningOperation")>
    Public Async Sub AsyncBitwiseNotAround()
        Console.WriteLine(Not Await LongRunningOperation())
    End Sub

    <Category("Expressions (around await)")>
    <Title("If operator - Nothing coalescing")>
    <Description("The result of an Await expression may be coalesced, in case it's Nothing.")>
    <LinkedMethod("FetchByKeyAsync")>
    Public Async Sub AsyncCoalesceAround()
        Console.WriteLine(If(Await FetchByKeyAsync("baz"), "Key not found."))
    End Sub

    Public Async Function FetchByKeyAsync(ByVal key As String) As Task(Of String)
        Await TaskEx.Delay(500)      ' Simulate fetching a remote value by key
        If key = "foo" Then
            Return "bar"
        Else
            Return Nothing
        End If
    End Function

    <Category("APIs: System.IO")>
    <Title("Stream.CopyToAsync")>
    <Description("Copies one stream to another asynchronously.")>
    Public Async Sub AsyncStreamCopyTo()
        ' Download a file.
        Await New WebClient().DownloadFileTaskAsync(New Uri("http://www.weather.gov/climate/"), "climate.html")

        ' Create the streams.
        Dim destination As New MemoryStream()

        Using source = File.Open("climate.html", FileMode.Open)
            Console.WriteLine("Source length: {0}", source.Length.ToString())

            ' Copy source to destination.
            Await source.CopyToAsync(destination)
        End Using

        Console.WriteLine("Destination length: {0}", destination.Length.ToString())

        ' NOTE: If Just My Code is on, VS will currently show a first-chance exception here.
        ' You may hit F5 to continue running after seeing the exception.
    End Sub

    <Category("APIs: System.IO")>
    <Title("Step 1: TextWriter.WriteAsync")>
    <Description("Writes data to a file asynchronously.")>
    Public Async Sub AsyncTextWriterWrite()
        Try
            Using writer As TextWriter = New StreamWriter("data.txt")
                Dim data = "1234 abcd"

                Await writer.WriteAsync(data)

                Console.WriteLine("Data written to file.")
            End Using
        Catch
            Console.WriteLine("Error writing to file!")
        End Try
    End Sub

    <Category("APIs: System.IO")>
    <Title("Step 2: TextWriter.ReadLineAsync")>
    <Description("Reads the first line of data from a file asynchronously." + vbCrLf + vbCrLf + "Be sure to run the WriteAsync sample first to create the data file.")>
    Public Async Sub AsyncTextWriterReadLine()
        Try
            Using reader As TextReader = New StreamReader("data.txt")
                Dim data = Await reader.ReadLineAsync()

                Console.WriteLine("Data: {0}", data)
            End Using
        Catch
            Console.WriteLine("Error reading from file!  Have you run the TextWriter.WriteAsync sample yet?")
        End Try
    End Sub


    <Category("APIs: System.Net")>
    <Title("Dns.GetHostAddressesAsync")>
    <Description("Resolves a domain name to a set of IP addresses asynchronously.")>
    Public Async Sub AsyncDnsGetHostAddresses()
        Try
            Dim addresses = Await DnsEx.GetHostAddressesAsync("http://www.weather.gov")

            For Each address In addresses
                Console.WriteLine("IP address: {0}", address.ToString())
            Next
        Catch
            Console.WriteLine("The DNS server was unable to resolve the domain name.")
        End Try
    End Sub

    <Category("APIs: System.Net")>
    <Title("Dns.GetHostEntryAsync")>
    <Description("Determines the host name of a known IP address asynchronously.")>
    Public Async Sub AsyncDnsGetHostEntry()
        Try
            Dim entry = Await DnsEx.GetHostEntryAsync("127.0.0.1")

            Console.WriteLine(entry.HostName)
        Catch
            Console.WriteLine("The DNS server was unable to resolve the IP address.")
        End Try
    End Sub

    <Category("APIs: System.Net")>
    <Title("WebClient.DownloadStringTaskAsync - Customized headers")>
    <Description("Performs a web request asynchronously using await.  This example shows the reuse of a WebClient object that has been customized to use a specific base address for all requests.")>
    Public Async Sub AsyncWebRequestDownloadStringAsyncCustomized()
        Dim client As New WebClient

        client.BaseAddress = "http://www.weather.gov"

        Console.WriteLine("Base address set.")

        WriteLinePageTitle(Await client.DownloadStringTaskAsync(New Uri("/", UriKind.Relative)))
        WriteLinePageTitle(Await client.DownloadStringTaskAsync(New Uri("/climate/", UriKind.Relative)))
        WriteLinePageTitle(Await client.DownloadStringTaskAsync(New Uri("/rss/", UriKind.Relative)))
    End Sub

    <Category("APIs: System.Net")>
    <Title("WebClient.DownloadDataTaskAsync")>
    <Description("Performs a web request to fill a byte array asynchronously.")>
    Public Async Sub AsyncWebRequestDownloadData()
        Dim buffer As Byte() = Await New WebClient().DownloadDataTaskAsync(New Uri("http://ecn.t0.tiles.virtualearth.net/tiles/h003.jpeg?g=400"))

        Console.WriteLine("First 16 bytes:")
        For i = 0 To 15
            Console.Write("{0} ", buffer(i).ToString("X2"))
        Next
    End Sub

    <Category("APIs: System.Net")>
    <Title("WebClient.DownloadFileTaskAsync")>
    <Description("Performs a web request to download a file to disk asynchronously.")>
    Public Async Sub AsyncWebRequestDownloadFile()
        Console.WriteLine("Downloading wallpaper...")
        Await New WebClient().DownloadFileTaskAsync(New Uri("http://www.microsoft.com/games/mw4mercs/img/desktop_2_800.jpg"), "mech.jpg")
        Console.WriteLine("Downloaded as mech.jpg.")

        Console.WriteLine("Previewing mech.jpg...")
        Process.Start("mech.jpg")
    End Sub

    <Category("APIs: System.Net")>
    <Title("WebRequest.GetResponseAsync")>
    <Description("Performs a web request to return a network stream asynchronously.")>
    Public Async Sub AsyncWebRequestGetResponse()
        Dim response = Await WebRequest.Create("http://www.weather.gov").GetResponseAsync()
        Dim stream = response.GetResponseStream()
        Console.WriteLine("First byte: {0}", stream.ReadByte().ToString("X2"))
    End Sub

    <Category("APIs: System.Net.NetworkInformation")>
    <Title("Ping.SendTaskAsync")>
    <Description("Sends a ping and asynchronously awaits the response." + vbCrLf + vbCrLf + "In this case, since microsoft.com does not reply to ping messages, control will enter the Else branch.")>
    Public Async Sub AsyncPingSend()
        Dim ping As New Ping

        Dim reply = Await ping.SendTaskAsync("www.microsoft.com")

        If reply.Status = IPStatus.Success Then
            Console.WriteLine("www.microsoft.com is responding.")
        Else
            Console.WriteLine("www.microsoft.com is not responding to pings.")
        End If
    End Sub

#Region "Samples Infrastructure"
    Public Overrides Property CancellationTokenSource As CancellationTokenSource
        Get
            Return cts
        End Get
        Set(ByVal value As CancellationTokenSource)
            cts = value
        End Set
    End Property
#End Region
End Class