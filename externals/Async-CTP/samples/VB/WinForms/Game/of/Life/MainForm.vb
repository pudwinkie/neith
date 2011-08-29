'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: MainForm.vb
'
'--------------------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Threading
Imports System.Threading.Tasks

Namespace GameOfLife
	Partial Public Class MainForm
		Inherits Form
		Public Sub New()
			InitializeComponent()
        End Sub

        Private Async Sub MainForm_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
            pbLifeDisplay.Image = Nothing
            Dim width As Integer = pbLifeDisplay.Width
            Dim height As Integer = pbLifeDisplay.Height

            ' Initialize the object pool and the game board
            Dim pool = New ObjectPool(Of Bitmap)(Function() New Bitmap(width, height))
            Dim game = New GameBoard(width, height, 0.1, pool)

            ' Run until cancellation is requested
            Dim sw = New Stopwatch()
            While (True)
                ' Move to the next board, timing how long it takes
                sw.Restart()
                Dim bmp As Bitmap = Await TaskEx.Run(Function() game.MoveNext())
                Dim framesPerSecond As Double = 1 / sw.Elapsed.TotalSeconds

                lblFramesPerSecond.Text = String.Format("Frames / Sec: {0:F2}", framesPerSecond)
                Dim old As Bitmap = CType(pbLifeDisplay.Image, Bitmap)
                pbLifeDisplay.Image = bmp
                If (old IsNot Nothing) Then pool.PutObject(old)
            End While
        End Sub
    End Class
End Namespace
