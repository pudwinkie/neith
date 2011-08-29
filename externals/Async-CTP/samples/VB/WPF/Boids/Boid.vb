'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: Boid.vb
'
'--------------------------------------------------------------------------

Imports System.Windows.Media.Media3D

''' <summary>Represents a Boid.</summary>
Friend Class Boid
    Inherits ModelVisual3D

    ''' <summary>The up vector.</summary>
    Private Shared ReadOnly UNIT_Y As New Vector3D(0, 1, 0)
    ''' <summary>Multiplicative factor to control the size of a boid.</summary>
    Private Const MODEL_SCALE As Integer = 3

    ''' <summary>Rotation of the boid.</summary>
    Private _rotation As AxisAngleRotation3D
    ''' <summary>Translation of the boid.</summary>
    Private _translation As TranslateTransform3D
    ''' <summary>The boid's colors.</summary>
    Private _colors As Tuple(Of Color, Color)
    ''' <summary>The brush for the material.</summary>
    Private _materialBrush As SolidColorBrush
    ''' <summary>The brush for the backmaterial.</summary>
    Private _backmaterialBrush As SolidColorBrush

    ''' <summary>Initializes the boid.</summary>
    ''' <param name="colors">The boids color's, Item1 for Material and Item2 for BackMaterial.</param>
    Public Sub New(colors As Tuple(Of Color, Color))
        If (colors Is Nothing) Then Throw New ArgumentNullException("colors")

        ' Store the colors
        _colors = colors
        _materialBrush = New SolidColorBrush(colors.Item1)
        _backmaterialBrush = New SolidColorBrush(colors.Item2)

        ' Set up the boid's model
        Content = New GeometryModel3D() With {
            .Material = New DiffuseMaterial(_materialBrush),
            .BackMaterial = New DiffuseMaterial(_backmaterialBrush),
            .Geometry = New MeshGeometry3D() With {
                .Positions = Point3DCollection.Parse("0 1 0  1 -1 0  -1 -1 0  0 1 0  0 -1 1  0 -1 -1"),
                .Normals = Vector3DCollection.Parse("0 0 -1  1 0 0"),
                .TriangleIndices = Int32Collection.Parse("0 1 2  3 4 5")}
        }

        ' Initialize its rotation and translation
        _rotation = New AxisAngleRotation3D(UNIT_Y, 0)
        _translation = New TranslateTransform3D(New Vector3D())

        ' Add all of the necessary transforms
        Dim t As New Transform3DGroup()
        t.Children.Add(New ScaleTransform3D(MODEL_SCALE, MODEL_SCALE, MODEL_SCALE))
        t.Children.Add(New RotateTransform3D(_rotation))
        t.Children.Add(_translation)
        Transform = t
    End Sub

    ''' <summary>Gets or sets the boid's velocity.</summary>
    Public Property Velocity As Vector3D
    ''' <summary>Gets or sets the boid's position.</summary>
    Public Property Position As Vector3D

    ''' <summary>Gets the boid's previous velocity.</summary>
    Public Property PreviousVelocity As Vector3D
    ''' <summary>Gets the boid's previous position.</summary>
    Public Property PreviousPosition As Vector3D

    ''' <summary>Stores the current position and velocity into the previous.</summary>
    Public Sub StorePositionAndVelocityIntoPrevious()
        PreviousVelocity = Velocity
        PreviousPosition = Position
    End Sub

    ''' <summary>Sets the boid's rotation and translation for the scene.</summary>
    Public Sub TransformByPositionAndVelocity()
        Dim direction = Velocity
        direction.Normalize()

        _rotation.Axis = Vector3D.CrossProduct(UNIT_Y, direction)
        _rotation.Angle = Math.Acos(Vector3D.DotProduct(UNIT_Y, direction) / (UNIT_Y.Length * direction.Length)) * (180 / Math.PI)

        Dim pos = Position
        _translation.OffsetX = pos.X
        _translation.OffsetY = pos.Y
        _translation.OffsetZ = pos.Z
    End Sub

    Public Sub ToggleTranslucency()
        _materialBrush.Color = Color.FromArgb(
            If(_materialBrush.Color.A < 255, 255, 25),
            _materialBrush.Color.R,
            _materialBrush.Color.G,
            _materialBrush.Color.B)
        _backmaterialBrush.Color = Color.FromArgb(
            If(_backmaterialBrush.Color.A < 255, 255, 25),
            _backmaterialBrush.Color.R,
            _backmaterialBrush.Color.G,
            _backmaterialBrush.Color.B)
    End Sub

    ''' <summary>Computes the angle between two boids based on the current boid's direction.</summary>
    ''' <param name="other">The other boid.</param>
    ''' <returns>The angle.</returns>
    Public Function ComputeAngle(other As Boid) As Double
        If (other Is Nothing) Then Throw New ArgumentNullException("comparisonBoid")
        Return Math.Acos(
            Vector3D.DotProduct(Me.PreviousVelocity, other.PreviousPosition - Me.PreviousPosition) /
                (Me.PreviousVelocity.Length * (other.PreviousPosition - Me.PreviousPosition).Length)) * (180 / Math.PI)
    End Function

    Private Function GenerateSphere(center As Point3D, radius As Double, slices As Integer, stacks As Integer) As MeshGeometry3D
        'Create the MeshGeometry3D.
        Dim mesh As New MeshGeometry3D()

        ' Fill the Position, Normals, and TextureCoordinates collections.
        For stack As Integer = 0 To stacks
            Dim phi As Double = Math.PI / 2 - stack * Math.PI / stacks
            Dim y As Double = radius * Math.Sin(phi)
            Dim scale As Double = -radius * Math.Cos(phi)

            For slice As Integer = 0 To slices
                Dim theta As Double = slice * 2 * Math.PI / slices
                Dim x As Double = scale * Math.Sin(theta)
                Dim z As Double = scale * Math.Cos(theta)

                Dim normal As New Vector3D(x, y, z)
                mesh.Normals.Add(normal)
                mesh.Positions.Add(normal + center)
                mesh.TextureCoordinates.Add(New Point(slice / CType(slices, Double), stack / CType(stacks, Double)))
            Next
        Next

        ' Fill the TriangleIndices collection.
        For Stack As Integer = 0 To stacks - 1
            For slice As Integer = 0 To slices - 1
                Dim n As Integer = slices + 1 ' Keep the line length down.

                If (Stack <> 0) Then
                    mesh.TriangleIndices.Add((Stack + 0) * n + slice)
                    mesh.TriangleIndices.Add((Stack + 1) * n + slice)
                    mesh.TriangleIndices.Add((Stack + 0) * n + slice + 1)
                End If
                If (Stack <> stacks - 1) Then
                    mesh.TriangleIndices.Add((Stack + 0) * n + slice + 1)
                    mesh.TriangleIndices.Add((Stack + 1) * n + slice)
                    mesh.TriangleIndices.Add((Stack + 1) * n + slice + 1)
                End If
            Next
        Next
        Return mesh
    End Function
End Class
