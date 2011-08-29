'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: MainWindow.xaml.vb
'
'--------------------------------------------------------------------------

Imports System.Windows.Media.Media3D
Imports System.Threading.Tasks
Imports System.Media

Class MainWindow
    ''' <summary>The number of boids to simulate.</summary>
    Private Const NUM_BOIDS As Integer = 500
    ''' <summary>The size of a neighborhood that affects a boid.</summary>
    Private Const NEIGHBORHOOD_SIZE As Integer = 100
    ''' <summary>The minimum distance one boid tries to maintain from another.</summary>
    Private Const MIN_DISTANCE_FROM_NEIGHBOR As Integer = 20
    ''' <summary>The angle that a boid can see around it.</summary>
    Private Const DEGREES_OF_SIGHT As Double = 180

    ''' <summary>The duration in milliseconds of a scatter event.</summary>
    Private Const SCATTER_TIME As Integer = 2000
    ''' <summary>The maximum speed of a boid.</summary>
    Private m_speedLimit As Double = 9.0

    ''' <summary>Multiplicative factor used when determining how much to move a boid towards the average position of boids in its neighborhood.</summary>
    Private Const PERCENTAGE_TO_MOVE_TOWARDS_AVERAGE_POSITION As Double = 0.01 * GLOBAL_MODIFICATION_RATE
    ''' <summary>Multiplicative factor used when determining how much to move a boid towards the average velocity of boids in its neighborhood.</summary>
    Private Const PERCENTAGE_TO_MOVE_TOWARDS_AVERAGE_VELOCITY As Double = 0.01 * GLOBAL_MODIFICATION_RATE
    ''' <summary>Multiplicative factor used when determining how much to move a boid towards staying in bounds if it's currently out of bounds.</summary>
    Private Const PERCENTAGE_TO_MOVE_TOWARDS_INBOUNDS As Double = 0.2 * GLOBAL_MODIFICATION_RATE
    ''' <summary>Multiplicative factor used when determining how much to move a boid towards its "home" position.</summary>
    Private Const PERCENTAGE_TO_MOVE_TOWARDS_HOME As Double = 0.01 * GLOBAL_MODIFICATION_RATE
    ''' <summary>Multiplicative factor included in all velocity-modifying rates.</summary>
    Private Const GLOBAL_MODIFICATION_RATE As Double = 1.0

    ''' <summary>Base weight to use for rule #1: flying towards the center of the neighborhood.</summary>
    Private m_rule1Weight As Double = 1.0
    ''' <summary>Base weight to use for rule #2: staying away from neighbors too close to it.</summary>
    Private m_rule2Weight As Double = 1.1
    ''' <summary>Base weight to use for rule #3: maintaining a similar velocity to its neighbors.</summary>
    Private m_rule3Weight As Double = 1.0
    ''' <summary>Base weight to use for rule #4: staying within the aviary.</summary>
    Private m_rule4Weight As Double = 0.9
    ''' <summary>Base weight to use for rule #5: staying close to home.</summary>
    Private m_rule5Weight As Double = 0.8

    ''' <summary>The boids.</summary>
    Private m_boidModels As Boid()
    ''' <summary>The bounds of the aviary in which the boids fly.</summary>
    Private m_aviary As Rect3D
    ''' <summary>The "home" position boids tend towards.</summary>
    Private m_home As Vector3D
    ''' <summary>Whether to move the camera automatically to keep boids in view.</summary>
    Private m_autoPanCamera As Boolean = True
    ''' <summary>The last mouse position while it was down.</summary>
    Private m_lastMousePosition As Point

    ''' <summary>Initializes the window.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Initialize the scene.</summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The eventargs</param>
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Me.Cursor = Cursors.Hand

        ' Set up the aviary.  This should match the size of the grass geometry configured in the XAML.
        m_aviary = New Rect3D(-300, 0, -400, 600, 180, 800)
        m_home = New Vector3D(0, 15, 0) ' home is close to 0,0,0, just a bit off the ground

        ' The color combinations to use for boids.  At least one combination is necessary,
        ' but more can be added to get more variations.
        Dim colorCombinations = New Tuple(Of Color, Color)() {
                Tuple.Create(Colors.SeaGreen, Colors.Silver),
                Tuple.Create(Colors.Pink, Colors.Purple),
        Tuple.Create(Colors.Yellow, Colors.Gold)
            }

        ' Generate all of the boids, with random color, position, and velocity assignments.  Then add them to the scene.
        Dim rand As New Random()
        m_boidModels = Enumerable.Range(0, NUM_BOIDS).Select(Function(i) New Boid(colorCombinations(rand.Next(0, colorCombinations.Length)))).ToArray()
        RandomizeBoidPositionsAndVelocities(rand)
        For Each boidModel In m_boidModels
            viewport3D.Children.Add(boidModel)
        Next

        ' Start the rendering loop
        RenderUpdateLoopAsync()
    End Sub

    ''' <summary>Move the boids to random positions and velocities.</summary>
    ''' <param name="rand">The random number generator to use.</param>
    Private Sub RandomizeBoidPositionsAndVelocities(Optional rand As Random = Nothing)
        If (rand Is Nothing) Then rand = New Random()
        For Each boid In m_boidModels
            boid.Position = New Vector3D(
                       rand.Next(m_aviary.X, (m_aviary.X + m_aviary.SizeX)),
                       rand.Next(m_aviary.Y, (m_aviary.Y + m_aviary.SizeY)),
                       rand.Next(m_aviary.Z, (m_aviary.Z + m_aviary.SizeZ)))
            boid.Velocity = New Vector3D(
                       rand.NextDouble() * 2 - 1,
                       rand.NextDouble() * 2 - 1,
                       rand.NextDouble() * 2 - 1)
        Next
    End Sub

    ''' <summary>Handle keydown events.</summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The eventargs.</param>
    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        ' If escape is pressed, normalize the window size
        If (e.Key = Key.Escape) Then
            Topmost = False
            WindowStyle = WindowStyle.ThreeDBorderWindow
            WindowState = WindowState.Normal

            ' If 't' is pressed, toggle translucency to enable following just one boid
        ElseIf (e.Key = Key.T) Then
            For i As Integer = 1 To m_boidModels.Length - 1
                m_boidModels(i).ToggleTranslucency()
            Next

            ' If 'a' is pressed, turn auto-panning/zoom of the camera on/off
        ElseIf (e.Key = Key.A) Then
            m_autoPanCamera = Not m_autoPanCamera

            ' If up or down is pressed and we're in auto-pan mode, 
        ElseIf ((e.Key = Key.Up OrElse e.Key = Key.Down) AndAlso Not m_autoPanCamera) Then
            Zoom(If(e.Key = Key.Up, 1, -1))

            ' If 'r' is pressed, reset all of the boids to random positions and velocities
        ElseIf (e.Key = Key.R) Then
            RandomizeBoidPositionsAndVelocities()

            ' If 'h', display usage instructions to the user
        ElseIf (e.Key = Key.H) Then
            Dim instructions As String =
                    "** Window Controls **" & Environment.NewLine &
                    "Auto-Camera Positioning: 'a'" & Environment.NewLine &
                    "Pan: Click Left And Drag" & Environment.NewLine &
                    "Zoom In / Out: Mousewheel (or) Up/Down Keys (or) Ctrl+Middle Mouse Move" & Environment.NewLine &
                    "Full Screen: Right Double-Click" & Environment.NewLine &
                    "Restore to Normal Window Size: Right Double-Click (or) Escape Key" & Environment.NewLine &
                    "Translucency: 't'" & Environment.NewLine &
                    Environment.NewLine &
                    "** Boid Controls **" & Environment.NewLine &
                    "Scatter: Left Double-Click" & Environment.NewLine &
                    "Change Max Speed: Ctrl + Mousewheel" & Environment.NewLine &
                    "Randomize: 'r'" & Environment.NewLine
            MessageBox.Show(Me, instructions, "Instructions", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    ''' <summary>Handle mousedown events.</summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The eventargs.</param>
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        ' Log the last mouse position
        m_lastMousePosition = e.GetPosition(Me)
    End Sub

    ''' <summary>Handle mousewheel events.</summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The eventargs.</param>
    Private Sub Window_MouseWheel(sender As Object, e As MouseWheelEventArgs)
        ' If ctrl is pressed, change the max bird speed based on the number of mousewheel turns
        If ((Keyboard.Modifiers And ModifierKeys.Control) <> 0) Then
            Const MIN_SPEED As Integer = 2
            Const MAX_SPEED As Integer = 10
            If (e.Delta > 0 AndAlso m_speedLimit < MAX_SPEED) Then
                m_speedLimit = m_speedLimit + 1
            ElseIf (e.Delta < 0 AndAlso m_speedLimit > MIN_SPEED) Then
                m_speedLimit = m_speedLimit - 1
            End If
        ElseIf (Not m_autoPanCamera) Then
            Zoom(e.Delta / Mouse.MouseWheelDeltaForOneLine)
        End If
    End Sub

    ''' <summary>Handle mousedoubleclick events.</summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The eventargs.</param>
    Private Sub Window_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        ' If the right mouse button is double clicked, alternate between maximized and normal view
        If (e.ChangedButton = MouseButton.Right) Then
            If (WindowState = WindowState.Maximized) Then
                Topmost = False
                WindowStyle = WindowStyle.ThreeDBorderWindow
                WindowState = WindowState.Normal
            Else
                Topmost = True
                WindowStyle = WindowStyle.None
                WindowState = WindowState.Maximized
            End If
            ' If the left mouse button is double clicked, scatter the boids
        ElseIf (e.ChangedButton = MouseButton.Left) Then
            ScatterAsync()
        End If
    End Sub

    Private m_scattering As Boolean = False

    ''' <summary>Scatter the boids.</summary>
    Private Async Sub ScatterAsync()
        If (Not m_scattering) Then
            m_scattering = True

            ' Ka'boom.  Something scares the boids.
            SystemSounds.Hand.Play()

            ' Store original settings
            Dim origRule1Weight = m_rule1Weight
            Dim origRule2Weight = m_rule2Weight
            Dim origRule4Weight = m_rule4Weight
            Dim origRule5Weight = m_rule5Weight

            ' Create new scatter settings
            m_rule1Weight = origRule1Weight * 5
            m_rule2Weight = origRule2Weight * 2
            m_rule4Weight = 0
            m_rule5Weight = origRule5Weight * -5

            ' Scatter for a period of time
            Await TaskEx.Delay(SCATTER_TIME)

            ' Stop scattering
            m_rule1Weight = origRule1Weight
            m_rule2Weight = origRule2Weight
            m_rule4Weight = origRule4Weight
            m_rule5Weight = origRule5Weight

            m_scattering = False
        End If
    End Sub

    ''' <summary>Handle mousemove events.</summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The eventargs.</param>
    Private Sub Window_MouseMove(sender As Object, e As MouseEventArgs)
        ' If we're not auto-panning
        If (Not m_autoPanCamera) Then
            ' Get the new mouse position and compute the difference from the previous
            Dim newPosition = e.GetPosition(Me)
            Dim diff = m_lastMousePosition - newPosition

            ' If the left mouse position was pressed, pan based on the x/y differences
            If (e.LeftButton = MouseButtonState.Pressed) Then
                camMain.Position = New Point3D(camMain.Position.X + diff.X * 0.4, camMain.Position.Y - diff.Y * 0.4, camMain.Position.Z)
                ' If the middle button was pressed, zoom based on the y difference
            ElseIf (e.MiddleButton = MouseButtonState.Pressed) Then
                Zoom(diff.Y)
            End If

            ' Store the new position
            m_lastMousePosition = newPosition
        End If
    End Sub

    ''' <summary>Zoom in or out based on the specified degree of zoom.</summary>
    ''' <param name="amountOfChange">Amount to zoom in (positive) or out (negative).</param>
    Private Sub Zoom(amountOfChange As Integer)
        Const ZOOM_FACTOR As Integer = 3
        camMain.Position = Point3D.Add(camMain.Position, camMain.LookDirection * amountOfChange * ZOOM_FACTOR)
    End Sub

    ''' <summary>Runs the rendering loop.</summary>
    Private Async Sub RenderUpdateLoopAsync()
        While (True)
            ' Update the positions and velocities of all of the boids.
            Await TaskEx.Run(AddressOf StepBoids)

            ' Render the boids to the UI
            RenderBoids()
        End While
    End Sub

    ''' <summary>Step the boids one step, updating their velocities and positions.</summary>
    Private Sub StepBoids()
        ' Store the current velocities and positions so that we can operate on an immutable copy
        For Each boid In m_boidModels
            boid.StorePositionAndVelocityIntoPrevious()
        Next

        ' For each boid, analyze how the various boid rules influence its velocity and position,
        ' then store this new information.  After this step, we'll be able to render the boids.
        Parallel.ForEach(m_boidModels,
            Sub(boid)
                Dim v1_2_3 = PrimaryRules_1_2_3(boid) ' weights factored in already in PrimaryRules
                Dim v4 = m_rule4Weight * Rule4_EncourageStayingWithinAviary(boid)
                Dim v5 = m_rule5Weight * Rule5_TendendcyTowardsHome(boid)

                boid.Velocity = BoundVelocity(boid.PreviousVelocity + v1_2_3 + v4 + v5, m_speedLimit)
                boid.Velocity.Normalize()

                Dim tmpPosition = boid.PreviousPosition + boid.Velocity
                If (tmpPosition.Y >= 0) Then boid.Position = tmpPosition
            End Sub)
    End Sub

    ''' <summary>Render the boids.</summary>
    Private Sub RenderBoids()
        ' Make sure the boids are all positioned and pointing correctly
        For Each boid In m_boidModels
            boid.TransformByPositionAndVelocity()
        Next

        ' If we're in auto-panning mode, move the camera appropriately
        If (m_autoPanCamera) Then
            ' Compute the maximum Z value of all of the boids and the center of their mass
            Dim maxZ = Double.MinValue
            Dim totalPos As New Vector3D()

            For Each boid In m_boidModels
                If (boid.Position.Z > maxZ) Then maxZ = boid.Position.Z
                totalPos += boid.Position
            Next

            Dim newCameraPos = totalPos / m_boidModels.Length

            ' Move the camera to point at the center of the boids, a ways back from the max boid's Z
            Const CAMERA_DISTANCE_FROM_MAXZ As Integer = 100
            Const CAMERA_SPEED_LIMIT As Double = 0.01
            Dim newPos As New Point3D(newCameraPos.X, newCameraPos.Y, maxZ + CAMERA_DISTANCE_FROM_MAXZ)
            Dim cameraVelocity = newPos - camMain.Position
            camMain.Position = camMain.Position + (cameraVelocity * CAMERA_SPEED_LIMIT)
        End If
    End Sub

    ''' <summary>Run the three primary rules of boidom.</summary>
    ''' <param name="boid">The boid to process.</param>
    ''' <returns>The velocity vector resulting from the three primary rules and their associated weights.</returns>
    Private Function PrimaryRules_1_2_3(boid As Boid) As Vector3D
        Dim numNearby As Integer = 0
        Dim summedPosition As Vector3D = New Vector3D()
        Dim summedVelocity As New Vector3D()
        Dim summedSeparation As New Vector3D()

        ' For rule #1, we want the boid to fly towards the center of all of those in its neighborhood.
        ' We find all of those boids, average their positions, and create a velocity vector to move the
        ' boid a bit of the way there.

        ' For rule #2, we want the boid to move away from each other boid it's a bit too close to.
        ' Find all of those boids in its immediate vicinity, and push it away.

        ' For rule #3, we want the boid to match velocities with those boids in its neighborhood.
        ' Sum their velocities, find the average, and move this boid's velocity a bit in that direction.

        For Each other In m_boidModels
            If (other IsNot boid AndAlso
                (other.PreviousPosition - boid.PreviousPosition).Length <= NEIGHBORHOOD_SIZE AndAlso
                boid.ComputeAngle(other) <= 135) Then

                summedPosition = summedPosition + other.PreviousPosition
                summedVelocity = summedVelocity + other.PreviousVelocity
                numNearby = numNearby + 1

                If ((other.PreviousPosition - boid.PreviousPosition).Length < MIN_DISTANCE_FROM_NEIGHBOR) Then
                    summedSeparation -= (other.PreviousPosition - boid.PreviousPosition)
                End If
            End If
        Next

        Dim rule1_flyTowardsCenter = If(numNearby > 0, (summedPosition - boid.PreviousPosition) / numNearby, New Vector3D()) * PERCENTAGE_TO_MOVE_TOWARDS_AVERAGE_POSITION
        Dim rule2_separateFromNearby = summedSeparation
        Dim rule3_matchVelocities = If(numNearby > 0, (summedVelocity - boid.PreviousVelocity) / numNearby, New Vector3D()) * PERCENTAGE_TO_MOVE_TOWARDS_AVERAGE_VELOCITY

        Return (m_rule1Weight * rule1_flyTowardsCenter) +
                (m_rule2Weight * rule2_separateFromNearby) +
                (m_rule3Weight * rule3_matchVelocities)
    End Function

    ''' <summary>Encourage a boid to stay within its aviary.</summary>
    ''' <param name="boid">The boid.</param>
    ''' <returns>The velocity vector encouraging a boid to stay within its bounds.</returns>
    Private Function Rule4_EncourageStayingWithinAviary(boid As Boid) As Vector3D
        Dim v As New Vector3D()

        ' X
        If (boid.PreviousPosition.X < m_aviary.X) Then
            v.X = m_aviary.X - boid.PreviousPosition.X
        ElseIf (boid.PreviousPosition.X > m_aviary.X + m_aviary.SizeX) Then
            v.X = (m_aviary.X + m_aviary.SizeX) - boid.PreviousPosition.X
        End If

        ' Y
        If (boid.PreviousPosition.Y < m_aviary.Y) Then
            v.Y = m_speedLimit
        ElseIf (boid.PreviousPosition.Y > m_aviary.Y + m_aviary.SizeY) Then
            v.Y = (m_aviary.Y + m_aviary.SizeY) - boid.PreviousPosition.Y
        End If

        ' Z
        If (boid.PreviousPosition.Z < m_aviary.Z) Then
            v.Z = m_aviary.Z - boid.PreviousPosition.Z
        ElseIf (boid.PreviousPosition.Z > m_aviary.Z + m_aviary.SizeZ) Then
            v.Z = (m_aviary.Z + m_aviary.SizeZ) - boid.PreviousPosition.Z
        End If

        Return v * PERCENTAGE_TO_MOVE_TOWARDS_INBOUNDS
    End Function

    ''' <summary>Encourage a boid to stay close to its home position.</summary>
    ''' <param name="boid">The boid.</param>
    ''' <returns>The velocity vector encouraging a boid to stay at home.</returns>
    Private Function Rule5_TendendcyTowardsHome(boid As Boid) As Vector3D
        Return (m_home - boid.PreviousPosition) * PERCENTAGE_TO_MOVE_TOWARDS_HOME
    End Function

    ''' <summary>Bound a velocity to the max speed limit.</summary>
    ''' <param name="velocity">The velocity to bound.</param>
    ''' <returns>The bounded velocity.</returns>
    Private Shared Function BoundVelocity(velocity As Vector3D, speedLimit As Double) As Vector3D
        Return If(velocity.Length > speedLimit, (velocity / velocity.Length) * speedLimit, velocity)
    End Function
End Class
