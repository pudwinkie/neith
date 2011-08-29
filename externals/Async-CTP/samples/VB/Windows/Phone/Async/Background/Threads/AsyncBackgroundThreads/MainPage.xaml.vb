Imports Microsoft.Phone.Controls
Imports System.Threading
Imports System.IO
Imports System.IO.IsolatedStorage
Imports System.Threading.Tasks
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Xml.Linq

Partial Public Class MainPage
    Inherits PhoneApplicationPage
    ' boolean used to track if the page is new, and therefore state needs to be restored.
    Private newPageInstance As Boolean = False

    ' string used to represent the data used by the page. A real application will likely use a more
    ' complex data structure, such as an XML document. The only requirement is that the object be serializable.
    Private pageDataObject As String


    ''' <summary>
    ''' Main page constructor.
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        ' The constructor is not called if the page is already in memory and therefore page and application
        ' state are still valid. Set newPageInstance to true so that in OnNavigatedTo, we know we need to 
        ' recreate page and application state.
        newPageInstance = True
    End Sub

    ''' <summary>
    ''' Override of Page's OnNavigatedFrom method. Use helper class to preserve the visual state of the page's
    ''' controls. Preserving application state takes place, if necessary, in the tombstoning event handlers
    ''' in App.xaml.cs.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnNavigatedFrom(e As System.Windows.Navigation.NavigationEventArgs)
        MyBase.OnNavigatedFrom(e)
        newPageInstance = False
    End Sub

    ''' <summary>
    ''' Override of Page's OnNavigatedTo method. The page's visual state is restored. Then, the application
    ''' state data is either set from the Application state dictionary or it is retrieved asynchronously.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Async Sub OnNavigatedTo(e As System.Windows.Navigation.NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        If newPageInstance Then
            ' if the application member variable is empty, call the method that loads data.
            If (TryCast(Application.Current, AsyncBackgroundThreads.App)).AppDataObject Is Nothing Then
                ' Let the user know that data is being loaded in the background.
                statusTextBlock.Text = "loading data..."

                'Asynchronously wait for the result of the background task
                Dim TextToSet As Result = Await TaskEx.RunEx(Function() GetData())

                SetData(TextToSet.Data, TextToSet.Source)
            Else
                ' Otherwise set the page's data object from the application member variable
                pageDataObject = (TryCast(Application.Current, AsyncBackgroundThreads.App)).AppDataObject

                ' Show the data to the user
                SetData(pageDataObject, "preserved state")
            End If

        End If
        ' Set the new page instance to false. It will not be set to true 
        ' unless the page constructor is called.
        newPageInstance = False
    End Sub

    ''' <summary>
    ''' GetData is called on a background thread. If data is present in Isolated Storage, and its save
    ''' date is recent, load the data from Isolated Storage. Otherwise, start an asynchronous http
    ''' request to obtain fresh data.
    ''' </summary>
    Public Async Function GetData() As Task(Of Result)
        ' Check the time elapsed since data was last saved to Isolated Storage
        Dim TimeSinceLastSave As TimeSpan = TimeSpan.FromSeconds(0)
        If IsolatedStorageSettings.ApplicationSettings.Contains("DataLastSave") Then
            Dim dataLastSave As Date = CDate(IsolatedStorageSettings.ApplicationSettings("DataLastSave"))
            TimeSinceLastSave = Date.Now.Subtract(dataLastSave)
        End If

        ' Check to see if data exists in Isolated Storage and see if the data is fresh.
        ' This example uses 30 seconds as the valid time window to make it easy to test. 
        ' Real apps will use a larger window.
        Dim isoStore As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication()
        If isoStore.FileExists("myDataFile.txt") AndAlso TimeSinceLastSave.TotalSeconds < 30 Then
            ' This method loads the data from Isolated Storage, if it is available.
            Dim sr As New StreamReader(isoStore.OpenFile("myDataFile.txt", FileMode.Open))
            Dim data As String = Await sr.ReadToEndAsync()
            Await PhoneWorkaround.TemporaryAsyncPhoneWorkaround
            sr.Close()

            Return New Result() With {.Data = data, .Source = "Isolated storage"}
        Else
            Try
                '' Otherwise it gets the data from the Web. 
                Dim client As New WebClient()
                Dim dataResult As String = Await client.DownloadStringTaskAsync("http://windowsteamblog.com/windows_phone/b/windowsphone/rss.aspx")
                Await PhoneWorkaround.TemporaryAsyncPhoneWorkaround

                Return New Result() With {.Data = dataResult, .Source = "Web"}


            Catch ex As Exception
                Return New Result() With {.Data = Nothing, .Source = "An unexpected error occured"}
            End Try
        End If

    End Function

    ''' <summary>
    ''' If data was obtained asynchronously from the Web or from Isolated Storage, this method
    ''' is invoked on the UI thread to update the page to show the data.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="source"></param>
    Public Async Sub SetData(data As String, source As String)
        pageDataObject = data
        Dim itemsCollection = New ObservableCollection(Of RSSItem)()
        Dim items = Await TaskEx.Run(Function() From item In XElement.Parse(data).<channel>.<item>
                                                Select New RSSItem With {.Title = item.<title>.Value,
                                                                         .Url = item.<link>.Value,
                                                                         .PubDate = item.<pubDate>.Value}
                                    )
        For Each item In items
            itemsCollection.Add(item)
        Next
        itemsListBox.ItemsSource = itemsCollection



        ' Set the Application class member variable to the data so that the
        ' Application class can store it when the application is deactivated or closed.
        TryCast(Application.Current, AsyncBackgroundThreads.App).AppDataObject = pageDataObject

        statusTextBlock.Text = "data retrieved from " & source & "."
    End Sub

    Public Sub DeferredLoadListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim listbox = CType(sender, ListBox)
        If listbox Is Nothing Then Return

        Dim item = CType(listbox.SelectedItem, RSSItem)
        If item Is Nothing Then Return

        webBrowser.Visibility = Visibility.Visible
        webBrowser.Navigate(New Uri(item.Url))
    End Sub

    Protected Overrides Sub OnBackKeyPress(e As CancelEventArgs)
        If webBrowser.Visibility = Visibility.Visible Then
            webBrowser.Visibility = Visibility.Collapsed
        End If

        e.Cancel = True
    End Sub

    Public Class Result
        Public Property Data As String
        Public Property Source As String
    End Class

    Public Class RSSItem
        Public Property Title As String
        Public Property Url As String
        Public Property PubDate As String
    End Class
End Class



Public Structure PhoneWorkaround
    Public Shared ReadOnly Property TemporaryAsyncPhoneWorkaround As PhoneWorkaround
        Get
            Return New PhoneWorkaround
        End Get
    End Property

    Public Function GetAwaiter() As PhoneWorkaround
        Return Me
    End Function
    Public ReadOnly Property IsCompleted As Boolean
        Get
            Return False
        End Get
    End Property
    Public Sub OnCompleted(continuation As Action)
        TaskEx.Run(continuation)
    End Sub
    Public Sub GetResult()
    End Sub
End Structure

