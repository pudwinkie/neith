Imports System.Windows
Imports System.Windows.Shapes
Imports System.Windows.Media
Imports System.Windows.Controls
Imports System.Windows.Input

Public Class GameData
    Public xs As Single(,)  ' x speed
    Public ys As Single(,)  ' y speed
    Public remaining, active As Integer
    Public canvas As Canvas
    Public tb As TextBlock
    Public lines As Line(,)
    Public paddle As Rectangle
    Public blocks As Ellipse(,)
    Public beeps As MediaElement()
    Public blockBeeps As MediaElement()
    Public curBeep As Integer = 0
    Public lastTick As DateTime
End Class

Public Module GameLogic
    ReadOnly RNG As New System.Random
    Const E = 0.0001F
    ReadOnly WHITE As New SolidColorBrush(Colors.White)
    ReadOnly GRAY As New SolidColorBrush(Colors.Gray)
    ReadOnly BLACK As New SolidColorBrush(Colors.Black)

    ' Pixel size of a ball/brick
    Const SIZE = 5.0F
    Const HALFSIZE = SIZE / 2.05
    ' Initial grid size of blocks
    Const WIDTH = 80
    Const HEIGHT = 20
    ' Pixel location of bottom of bricks
    Const BOTBRICKS = HEIGHT * SIZE
    ' Canvas size
    Const CANWIDTH = SIZE * WIDTH
    Const CANHEIGHT = SIZE * 80.0F
    ' Paddle size
    Const PADHEIGHT = 10.0F
    Const PADWIDTH = 8.0F * SIZE
    Const HALFPADWIDTH = PADWIDTH / 2.0F
    Const MAXWIDTH = CANWIDTH - PADWIDTH
    ' Pixel location of top of paddle
    Const TOPPAD = CANHEIGHT - 60.0F
    ' Pixel speed per second
    Const PADVELOCITY = 250
    Const LINEVELOCITY = 50
    ' How many beeps will we load for the game
    Const NUMBEEP = 20

    Public Function MakeIntroScreen() As Tuple(Of FrameworkElement, Button)
        Dim content As New StackPanel
        Dim title As New TextBlock With {.Height = 45.0, .Text = "Super BreakAway!", .FontSize = 30.0, .HorizontalAlignment = HorizontalAlignment.Center}
        Dim subTitle As New TextBlock With {.Height = 30.0, .HorizontalAlignment = HorizontalAlignment.Center, .Text = "Use CURSOR KEYS to move the paddle"}
        Dim button As New Button With {.Content = "Play"}
        content.Children.Add(title)
        content.Children.Add(subTitle)
        content.Children.Add(button)
        Return Tuple.Create(Of FrameworkElement, Button)(content, button)
    End Function

    Public Function MakeVictoryScreen(remaining As Integer) As Tuple(Of FrameworkElement, MediaElement)
        Dim content As New StackPanel
        Dim title As New TextBox() With {.Text = If(remaining > 0, String.Format("THE END" & vbCrLf & "left {0} bricks", remaining), "VICTORY!!!" & vbCrLf & "All bricks cleared!!!")}
        Dim sound As New MediaElement() With {.LoadedBehavior = MediaState.Manual, .Source = New System.Uri(If(remaining > 0, "boooo.wav", "happykids.wav"), System.UriKind.Relative)}
        content.Children.Add(title)
        content.Children.Add(sound)
        sound.Play()
        Return Tuple.Create(Of FrameworkElement, MediaElement)(content, sound)
    End Function

    Public Function MakeGameScreen(dat As GameData) As FrameworkElement
        Dim title As New TextBlock With {.Height = 45.0, .Width = CANWIDTH, .Text = "Super BreakAway!", .FontSize = 30.0, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center}
        Dim subTitle As New TextBlock With {.Height = 30.0, .Width = CANWIDTH, .Text = "written in F#, by Brian McNamara, translated to VB by Avner and Hillel and Lucian", .FontSize = 10.0, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center}
        Dim game As New Border With {.BorderThickness = New Thickness(1.0F), .BorderBrush = BLACK, .Child = dat.canvas}

        Dim grid As New Grid
        grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1.0, GridUnitType.Auto)})
        For i = 1 To 3 : grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1.0, GridUnitType.Auto)}) : Next
        grid.Children.Add(title) : grid.SetColumn(title, 0) : grid.SetRow(title, 0)
        grid.Children.Add(subTitle) : grid.SetColumn(subTitle, 0) : grid.SetRow(subTitle, 1)
        grid.Children.Add(game) : grid.SetColumn(game, 0) : grid.SetRow(game, 2)

        Return grid
    End Function


    Public Function InitializeGameData() As GameData
        Dim dat As New GameData
        dat.remaining = WIDTH * HEIGHT - 1
        dat.active = 1
        dat.canvas = New Canvas With {.Width = CANWIDTH, .Height = CANHEIGHT, .Background = WHITE}

        ' Textbox...
        dat.tb = New TextBlock With {.Height = 25.0, .Width = CANWIDTH, .FontSize = 20.0, .Text = String.Format("{0} bricks remain, {1} balls active", dat.remaining, dat.active)}
        dat.canvas.AddAt(TOPPAD + PADHEIGHT + 5.0F, 10.0F, dat.tb)

        ' Paddle...
        dat.paddle = New Rectangle With {.Width = PADWIDTH, .Height = PADHEIGHT, .Fill = BLACK}
        dat.canvas.AddAt(TOPPAD, CANWIDTH / 2.0F, dat.paddle)

        ' Lines and Blocks...
        ReDim dat.lines(HEIGHT, WIDTH) ' trailer lines
        ReDim dat.blocks(HEIGHT, WIDTH)
        For y = 0 To HEIGHT - 1
            For x = 0 To WIDTH - 1
                Dim f = CSng(x) / CSng(WIDTH)
                Dim fill As Color
                If f < 0.25 Then
                    fill = Color.FromArgb(255, 255, CByte(f * 1020), 0)
                ElseIf f < 0.5 Then
                    fill = Color.FromArgb(255, CByte((0.75 - f) * 1020 Mod 256), 255, 0)
                ElseIf f < 0.75 Then
                    fill = Color.FromArgb(255, 0, CByte((1.5 - f) * 1020 Mod 256), CByte((f - 0.5) * 1020 Mod 256))
                Else
                    fill = Color.FromArgb(255, CByte((f - 0.75) * 1020 Mod 256), 0, 255)
                End If

                Dim e As New Ellipse With {.Width = SIZE, .Height = SIZE, .Fill = New SolidColorBrush(fill)}
                dat.canvas.AddAt(SIZE * y, SIZE * x, e)
                dat.blocks(y, x) = e
            Next
        Next

        ' Beeps...
        ReDim dat.beeps(NUMBEEP)
        ReDim dat.blockBeeps(NUMBEEP)
        For i = 0 To NUMBEEP - 1
            dat.beeps(i) = New MediaElement With {.LoadedBehavior = MediaState.Manual, .Source = New System.Uri("BEEPPURE.wav", System.UriKind.Relative)}
            dat.canvas.Children.Add(dat.beeps(i))
            dat.blockBeeps(i) = New MediaElement With {.LoadedBehavior = MediaState.Manual, .Source = New System.Uri("BEEPDOUB.wav", System.UriKind.Relative)}
            dat.canvas.Children.Add(dat.blockBeeps(i))
        Next

        ' Speeds...
        ReDim dat.xs(HEIGHT, WIDTH)
        ReDim dat.ys(HEIGHT, WIDTH)
        Dim mid = WIDTH \ 2
        dat.ys(HEIGHT - 1, mid) = 4.0F
        dat.xs(HEIGHT - 1, mid) = 0.4F

        ' Start the first block dropping...
        Dim initBlock = dat.blocks(HEIGHT - 1, mid)
        Canvas.SetTop(initBlock, Canvas.GetTop(initBlock) + SIZE)
        dat.lines(HEIGHT - 1, mid) = New Line With {.X1 = Canvas.GetLeft(initBlock), .X2 = Canvas.GetLeft(initBlock), .Y1 = Canvas.GetTop(initBlock), .Y2 = Canvas.GetTop(initBlock), .StrokeThickness = SIZE / 3.0, .Stroke = GRAY}
        dat.canvas.Children.Add(dat.lines(HEIGHT - 1, mid))
        dat.lastTick = DateTime.Now

        Return dat
    End Function


    Public Sub UpdateBalls(dat As GameData)
        Dim now = DateTime.Now
        Dim interval = now.Subtract(dat.lastTick).TotalSeconds
        dat.lastTick = now

        Dim wantPaddleBeep = False
        Dim wantBlockBeep = False

        Dim leftPad = Canvas.GetLeft(dat.paddle)
        For y = 0 To HEIGHT - 1
            For x = 0 To WIDTH - 1
                If dat.ys(y, x) = 0.0 Then Continue For
                Dim origCenteredX = Canvas.GetLeft(dat.blocks(y, x)) + HALFSIZE
                Dim origCenteredY = Canvas.GetTop(dat.blocks(y, x)) + HALFSIZE
                ' compute new X
                Dim newX = dat.xs(y, x) * LINEVELOCITY * interval + CSng(Canvas.GetLeft(dat.blocks(y, x)))
                If newX < 0.0 Then newX = 0 : dat.xs(y, x) *= -1
                If newX > CANWIDTH - E Then newX = CANWIDTH - E : dat.xs(y, x) *= -1
                Dim newY = dat.ys(y, x) * LINEVELOCITY * interval + CSng(Canvas.GetTop(dat.blocks(y, x)))
                If newY < 0.0 Then newY = 0 : dat.ys(y, x) *= -1

                ' update position
                Canvas.SetTop(dat.blocks(y, x), newY)
                Canvas.SetLeft(dat.blocks(y, x), newX)
                ' update trailer line
                Dim newCenteredX = Canvas.GetLeft(dat.blocks(y, x)) + HALFSIZE
                Dim newCenteredY = Canvas.GetTop(dat.blocks(y, x)) + HALFSIZE
                Dim dx = origCenteredX - newCenteredX
                Dim dy = origCenteredY - newCenteredY
                Dim dd = Math.Sqrt(dx * dx + dy * dy)
                dat.lines(y, x).X2 = newCenteredX
                dat.lines(y, x).Y2 = newCenteredY
                dat.lines(y, x).X1 = 20.0 * dx / dd + newCenteredX
                dat.lines(y, x).Y1 = 20.0 * dy / dd + newCenteredY

                Dim top = Canvas.GetTop(dat.blocks(y, x))

                If top >= TOPPAD AndAlso top < TOPPAD + PADHEIGHT Then
                    ' see if hit paddle
                    Dim left = Canvas.GetLeft(dat.blocks(y, x))
                    If left < leftPad OrElse left >= leftPad + PADWIDTH Then Continue For
                    dat.ys(y, x) = -Math.Abs(dat.ys(y, x))
                    dat.xs(y, x) = dat.xs(y, x) + (CSng(left) - CSng(leftPad) - HALFPADWIDTH) / HALFPADWIDTH
                    If dat.xs(y, x) = 0 Then dat.xs(y, x) = E
                    wantPaddleBeep = True

                ElseIf top < BOTBRICKS Then
                    ' see if hit a brick
                    Dim left = Canvas.GetLeft(dat.blocks(y, x))
                    Dim brickX = CInt(Math.Floor(left / SIZE))
                    Dim brickY = CInt(Math.Floor(top / SIZE))
                    Dim thereIsStillABrickHere = (dat.ys(brickY, brickX) = 0.0)
                    If Not thereIsStillABrickHere Then Continue For

                    Dim brick = dat.blocks(brickY, brickX)
                    Dim t = Canvas.GetTop(brick)
                    Dim l = Canvas.GetLeft(brick)
                    Dim intersect = left >= l AndAlso left < l + SIZE AndAlso top >= t AndAlso top < t + SIZE
                    If Not intersect Then Continue For

                    dat.remaining -= 1
                    dat.active += 1
                    dat.tb.Text = String.Format("{0} bricks remain, {1} balls active", dat.remaining, dat.active)
                    Dim side = hitSide(CSng(l - left), CSng(t - top), dat.xs(y, x), dat.ys(y, x))
                    If side Then dat.xs(y, x) *= -1 Else dat.ys(y, x) *= -1

                    dat.ys(brickY, brickX) = CSng(SIZE * (RNG.NextDouble() + 1.0) / 2.1)
                    dat.xs(brickY, brickX) = CSng(SIZE * (RNG.NextDouble() - 0.5))
                    If dat.xs(brickY, brickX) = 0 Then dat.xs(brickY, brickX) = E
                    Canvas.SetTop(brick, t + SIZE * 1.5)
                    Dim initBlock = dat.blocks(brickY, brickX)
                    dat.lines(brickY, brickX) = New Line With {.X1 = Canvas.GetLeft(initBlock), .X2 = Canvas.GetLeft(initBlock), .Y1 = Canvas.GetTop(initBlock), .Y2 = Canvas.GetTop(initBlock), .StrokeThickness = SIZE / 3.0, .Stroke = GRAY}
                    dat.canvas.Children.Add(dat.lines(brickY, brickX))
                    wantBlockBeep = True

                ElseIf top > CANHEIGHT Then
                    dat.xs(y, x) = 0.0F
                    dat.ys(y, x) = 0.0F
                    dat.canvas.Children.Remove(dat.blocks(y, x))
                    dat.canvas.Children.Remove(dat.lines(y, x))
                    dat.active -= 1
                    dat.tb.Text = String.Format("{0} bricks remain, {1} balls active", dat.remaining, dat.active)
                End If
            Next
        Next

        dat.curBeep = (dat.curBeep + 1) Mod NUMBEEP
        If wantPaddleBeep Then dat.beeps(dat.curBeep).Stop() : dat.beeps(dat.curBeep).Play()
        If wantBlockBeep Then dat.blockBeeps(dat.curBeep).Stop() : dat.blockBeeps(dat.curBeep).Play()
    End Sub




    Public Sub MovePaddle(dat As GameData)
        Dim interval = DateTime.Now.Subtract(dat.lastTick).TotalSeconds

        Dim pos = Canvas.GetLeft(dat.paddle)
        If Keyboard.IsKeyDown(Key.Left) AndAlso pos > 0 Then Canvas.SetLeft(dat.paddle, pos - PADVELOCITY * interval)
        If Keyboard.IsKeyDown(Key.Right) AndAlso pos < MAXWIDTH Then Canvas.SetLeft(dat.paddle, pos + PADVELOCITY * interval)
    End Sub





    Function hitSide(x As Single, y As Single, dx As Single, dy As Single) As Boolean
        ' screen coordinates, a ball hit a block(0-SIZE,0-size) at point
        ' (x,y) with velocity (dx,dy) - did it hit the side of the brick?
        Dim ballSlope = -dy / dx
        If dy > 0.0 Then
            If dx < 0.0 Then Return ballSlope < y / (SIZE - x) ' it's going 'down-left'
            Return ballSlope > -y / x  'it's going 'down-right'
        Else
            If dx > 0.0 Then Return ballSlope < (SIZE - y) / x ' it's going 'up-right'
            Return ballSlope > -(SIZE - y) / (SIZE - x) ' it's going 'up-left'
        End If
    End Function



    <Runtime.CompilerServices.Extension()>
    Friend Sub AddAt(canvas As Canvas, top As Single, left As Single, element As UIElement)
        canvas.SetTop(element, top)
        canvas.SetLeft(element, left)
        canvas.Children.Add(element)
    End Sub

End Module



