Imports System.IO.IsolatedStorage
Imports System.IO

Partial Public Class App
    Inherits Application

    ''' <summary>
    ''' Provides easy access to the root frame of the Phone Application.
    ''' </summary>
    ''' <returns>The root frame of the Phone Application.</returns>
    Public Property RootFrame As PhoneApplicationFrame

    ' Declare the dataObject variable as a public member of the Application class.
    ' Real applications will use a more complex data structure, such as an XML
    ' document. The only requirement is that the object be serializable.
    Public AppDataObject As String

    ''' <summary>
    ''' Constructor for the Application object.
    ''' </summary>
    Public Sub New()
        ' Show graphics profiling information while debugging.
        If Diagnostics.Debugger.IsAttached Then
            ' Display the current frame rate counters.
            Application.Current.Host.Settings.EnableFrameRateCounter = True

            ' Show the areas of the app that are being redrawn in each frame.
            'Application.Current.Host.Settings.EnableRedrawRegions = True

            ' Enable non-production analysis visualization mode, 
            ' which shows areas of a page that are being GPU accelerated with a colored overlay.
            'Application.Current.Host.Settings.EnableCacheVisualization = True
        End If

        ' Standard Silverlight initialization
        InitializeComponent()

        ' Phone-specific initialization
        InitializePhoneApplication()
    End Sub

    ' Code to execute when the application is launching (eg, from Start)
    ' This code will not execute when the application is reactivated
    Private Sub Application_Launching(sender As Object, e As LaunchingEventArgs)
    End Sub

    ' Code to execute when the application is activated (brought to foreground)
    ' This code will not execute when the application is first launched
    Private Sub Application_Activated(sender As Object, e As ActivatedEventArgs)
        ' Check to see if the key for the application state data is in the State dictionary.
        If PhoneApplicationService.Current.State.ContainsKey("dataObject") Then
            ' If it exists, assign the data to the application member variable.
            AppDataObject = TryCast(PhoneApplicationService.Current.State("dataObject"), String)
        End If
    End Sub

    ' Code to execute when the application is deactivated (sent to background)
    ' This code will not execute when the application is closing
    Private Sub Application_Deactivated(sender As Object, e As DeactivatedEventArgs)
        ' If there is data in the application member variable...
        If Not String.IsNullOrEmpty(AppDataObject) Then
            ' Store it in the State dictionary.
            PhoneApplicationService.Current.State("dataObject") = AppDataObject

            ' Also store it in Isolated Storage, in case the application is never reactivated.
            SaveDataToIsolatedStorage("myDataFile.txt", AppDataObject)
        End If
    End Sub

    ' Code to execute when the application is closing (eg, user hit Back)
    ' This code will not execute when the application is deactivated
    Private Sub Application_Closing(sender As Object, e As ClosingEventArgs)
        ' The application will not be tombstoned, so only save to Isolated Storage
        If Not String.IsNullOrEmpty(AppDataObject) Then
            SaveDataToIsolatedStorage("myDataFile.txt", AppDataObject)
        End If
    End Sub

    ' Code to execute if a navigation fails
    Private Sub RootFrame_NavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        If Diagnostics.Debugger.IsAttached Then
            ' A navigation has failed; break into the debugger
            Diagnostics.Debugger.Break()
        End If
    End Sub

    Public Sub Application_UnhandledException(sender As Object, e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException

        ' Show graphics profiling information while debugging.
        If Diagnostics.Debugger.IsAttached Then
            Diagnostics.Debugger.Break()
        Else
            e.Handled = True
            MessageBox.Show(e.ExceptionObject.Message & Environment.NewLine & e.ExceptionObject.StackTrace,
                            "Error", MessageBoxButton.OK)
        End If
    End Sub

    Private Sub SaveDataToIsolatedStorage(isoFileName As String, value As String)
        Dim isoStore = IsolatedStorageFile.GetUserStoreForApplication()
        Dim sw As New StreamWriter(isoStore.OpenFile(isoFileName, FileMode.OpenOrCreate))
        sw.Write(value)
        sw.Close()
        IsolatedStorageSettings.ApplicationSettings("DataLastSave") = Date.Now
    End Sub

#Region "Phone application initialization"
    ' Avoid double-initialization
    Private phoneApplicationInitialized As Boolean = False

    ' Do not add any additional code to this method
    Private Sub InitializePhoneApplication()
        If phoneApplicationInitialized Then
            Return
        End If

        ' Create the frame but don't set it as RootVisual yet; this allows the splash
        ' screen to remain active until the application is ready to render.
        RootFrame = New PhoneApplicationFrame()
        AddHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication

        ' Handle navigation failures
        AddHandler RootFrame.NavigationFailed, AddressOf RootFrame_NavigationFailed

        ' Ensure we don't initialize again
        phoneApplicationInitialized = True
    End Sub

    ' Do not add any additional code to this method
    Private Sub CompleteInitializePhoneApplication(sender As Object, e As NavigationEventArgs)
        ' Set the root visual to allow the application to render
        If RootVisual IsNot RootFrame Then
            RootVisual = RootFrame
        End If

        ' Remove this handler since it is no longer needed
        RemoveHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication
    End Sub
#End Region

End Class