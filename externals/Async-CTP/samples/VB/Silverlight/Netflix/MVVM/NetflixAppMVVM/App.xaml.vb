 Partial Public Class App
    Inherits Application

    public Sub New()
        InitializeComponent()
    End Sub
    
    Private Sub Application_Startup(o As Object, e As StartupEventArgs) Handles Me.Startup
        Me.RootVisual = New MainPage()
    End Sub

    Private Sub Application_Exit(o As Object, e As EventArgs) Handles Me.Exit

    End Sub

    Private Sub Application_UnhandledException(sender As Object, e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException

        ' If the app is running outside of the debugger then report the exception using
        ' the browser's exception mechanism. On IE this will display it a yellow alert 
        ' icon in the status bar and Firefox will display a script error.
        If Not System.Diagnostics.Debugger.IsAttached Then

            ' NOTE: This will allow the application to continue running after an exception has been thrown
            ' but not handled. 
            ' For production applications this error handling should be replaced with something that will 
            ' report the error to the website and stop the application.
            e.Handled = True
            Deployment.Current.Dispatcher.BeginInvoke(New Action(Of ApplicationUnhandledExceptionEventArgs)(AddressOf ReportErrorToDOM), e)
        End If
    End Sub

    Private Sub ReportErrorToDOM(e As ApplicationUnhandledExceptionEventArgs)

        Try
            Dim errorMsg As String = e.ExceptionObject.Message + e.ExceptionObject.StackTrace
            errorMsg = errorMsg.Replace(""""c, "'"c).Replace(ChrW(13) & ChrW(10), "\n")

            System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(""Unhandled Error in Silverlight Application " + errorMsg + """);")
        Catch

        End Try
    End Sub
    
End Class
