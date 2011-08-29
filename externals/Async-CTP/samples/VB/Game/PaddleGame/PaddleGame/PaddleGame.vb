Option Strict On
Imports System.Threading.Tasks

Module PaddleGame
    Dim app As New Application
    Dim frame As New Border With {.BorderThickness = New Thickness(15.0), .BorderBrush = New SolidColorBrush(Color.FromRgb(0, 0, 0)), .Width = 432.0, .Height = 507.0}
    Dim window As New Window With {.Title = "Super Breakaway VB Async!", .SizeToContent = SizeToContent.WidthAndHeight, .Content = frame}

    <STAThread()>
    Sub Main()
        AddHandler app.Startup, AddressOf MainAsync
        app.Run(window)
    End Sub

    Async Sub MainAsync(sender As Object, e As StartupEventArgs)
        ' Introduction screen...
        Dim cb = GameLogic.MakeIntroScreen()  ' returns a tuple (content, button)
        frame.Child = cb.Item1
        'Await ButtonClick(cb.Item2)

        While True
            ' Gameplay screen...
            Dim dat = GameLogic.InitializeGameData()
            frame.Child = GameLogic.MakeGameScreen(dat)
            While dat.remaining > 0 AndAlso dat.active > 0
                Await TaskEx.Delay(10)
                GameLogic.MovePaddle(dat)
                GameLogic.UpdateBalls(dat)
            End While

            ' Victory screen...
            Dim cs = GameLogic.MakeVictoryScreen(dat.remaining) ' returns a tuple (content,sound)
            frame.Child = cs.Item1
            Await TaskEx.Delay(3000)
            cs.Item2.Stop()
        End While
    End Sub



    ' A helper method to make it easier to await for a button-click
    Function ButtonClick(b As Button) As Task
        Dim tcs As New TaskCompletionSource(Of Object)
        Dim handler As RoutedEventHandler = Sub(sender, e)
                                                RemoveHandler b.Click, handler
                                                tcs.TrySetResult(Nothing)
                                            End Sub
        AddHandler b.Click, handler
        Return tcs.Task
    End Function

End Module



