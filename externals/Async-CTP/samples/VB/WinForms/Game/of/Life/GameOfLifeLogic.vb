'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: GameOfLifeLogic.vb
'
'--------------------------------------------------------------------------

Imports System.Threading.Tasks
Imports System.Collections.Concurrent
Imports System.Runtime.InteropServices

Namespace GameOfLife
    ''' <summary>Represents the game of life board.</summary>
    Friend Class GameBoard
        ''' <summary>Arrays used to store the current and next state of the game.</summary>
        Private m_cells?()(,) As Color
        ''' <summary>Index into the scratch arrays that represents the current stage of the game.</summary>
        Private m_currentIndex As Integer
        ''' <summary>A pool of Bitmaps used for rendering.</summary>
        Private m_pool As ObjectPool(Of Bitmap)
        ''' <summary>Scratch data used to do fast rendering.</summary>
        Private m_bmpDataScratch As Byte()

        ''' <summary>Initializes the game board.</summary>
        ''' <param name="width">The width of the board.</param>
        ''' <param name="height">The height of the board.</param>
        ''' <param name="initialDensity">The initial population density to use to populate the board.</param>
        ''' <param name="pool">The pool of Bitmaps to use.</param>
        Public Sub New(width As Integer, height As Integer, initialDensity As Double, pool As ObjectPool(Of Bitmap))
            ' Validate parameters.
            If width < 1 Then
                Throw New ArgumentOutOfRangeException("width")
            End If
            If height < 1 Then
                Throw New ArgumentOutOfRangeException("height")
            End If
            If pool Is Nothing Then
                Throw New ArgumentNullException("pool")
            End If
            If initialDensity < 0 OrElse initialDensity > 1 Then
                Throw New ArgumentOutOfRangeException("initialDensity")
            End If

            ' Store parameters.
            m_pool = pool
            Me.Width = width
            Me.Height = height

            ' Create the storage arrays.
            m_cells = New Color?(1)(,) {New Color?(width - 1, height - 1) {}, New Color?(width - 1, height - 1) {}}

            ' Populate the board randomly based on the provided initial density.
            Dim rand As New Random()
            For i = 0 To width - 1
                For j = 0 To height - 1
                    m_cells(m_currentIndex)(i, j) = If((rand.NextDouble() < initialDensity), Color.FromArgb(rand.Next()), CType(Nothing, Color?))
                Next j
            Next i
        End Sub

        ''' <summary>Moves to the next stage of the game, returning a Bitmap that represents the state of the board.</summary>
        ''' <returns>A bitmap that represents the state of the board.</returns>
        ''' <remarks>The returned Bitmap should be added back to the pool supplied to the constructor when usage of it is complete.</remarks>
        Public Function MoveNext() As Bitmap
            ' Get the current and next stage board arrays.
            Dim nextIndex = (m_currentIndex + 1) Mod 2
            Dim current?(,) = m_cells(m_currentIndex)
            Dim [next]?(,) = m_cells(nextIndex)
            Dim rand As New Random()

            ' Get a Bitmap from the pool to use.
            Dim bmp = m_pool.GetObject()

            Dim bounds As New Rectangle(0, 0, bmp.Width, bmp.Height)
            Dim bmpData = bmp.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
            If (m_bmpDataScratch Is Nothing) Then m_bmpDataScratch = New Byte(bmpData.Stride * bmpData.Height) {}
            Marshal.Copy(bmpData.Scan0, m_bmpDataScratch, 0, m_bmpDataScratch.Length)

            ' For every row.
            Parallel.For(0, Width,
                Sub(i)
                    ' For every column.
                    For j = 0 To Height - 1
                        Dim count = 0
                        Dim r = 0, g = 0, b = 0

                        ' Count neighbors.
                        For x = i - 1 To i + 1
                            For y = j - 1 To j + 1
                                If (x = i AndAlso j = y) OrElse x < 0 OrElse x >= Width OrElse y < 0 OrElse y >= Height Then
                                    Continue For
                                End If
                                Dim c? = current(x, y)
                                If c.HasValue Then
                                    count += 1
                                    r += c.Value.R
                                    g += c.Value.G
                                    b += c.Value.B
                                End If
                            Next y
                        Next x

                        ' Heuristic for alive or dead based on neighbor count and current state.
                        If count < 1 OrElse count >= 4 Then
                            [next](i, j) = Nothing
                        ElseIf current(i, j).HasValue AndAlso (count = 2 OrElse count = 3) Then
                            [next](i, j) = current(i, j)
                        ElseIf (Not current(i, j).HasValue) AndAlso count = 3 Then
                            [next](i, j) = Color.FromArgb(r \ count, g \ count, b \ count)
                        Else
                            [next](i, j) = Nothing
                        End If

                        ' Render the cell.
                        Dim cellColor = If(current(i, j), Color.White)
                        Dim offset = j * bmpData.Stride + i * 4
                        m_bmpDataScratch(offset + 3) = 255
                        m_bmpDataScratch(offset + 2) = cellColor.R
                        m_bmpDataScratch(offset + 1) = cellColor.G
                        m_bmpDataScratch(offset + 0) = cellColor.B
                    Next j
                End Sub)

            ' Update and return
            m_currentIndex = nextIndex
            Marshal.Copy(m_bmpDataScratch, 0, bmpData.Scan0, m_bmpDataScratch.Length)
            bmp.UnlockBits(bmpData)
            Return bmp
        End Function

        ''' <summary>Gets the width of the board.</summary>
        Private privateWidth As Integer
        Public Property Width() As Integer
            Get
                Return privateWidth
            End Get
            Private Set(value As Integer)
                privateWidth = value
            End Set
        End Property

        ''' <summary>Gets the height of the board.</summary>
        Private privateHeight As Integer
        Public Property Height() As Integer
            Get
                Return privateHeight
            End Get
            Private Set(value As Integer)
                privateHeight = value
            End Set
        End Property
    End Class
End Namespace