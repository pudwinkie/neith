'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: MainWindow.xaml.vb
'
'--------------------------------------------------------------------------
Imports System
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Shapes
Imports System.Threading.Tasks.Dataflow

Namespace DiningPhilosophers

    Partial Public Class MainWindow
        Inherits Window
        Private Const NUM_PHILOSOPHERS As Integer = 25
        Private Const TIMESCALE As Integer = 100
        Private ReadOnly m_rand As New Random()

        ''' <summary>Initializes the MainWindow.</summary>
        Public Sub New()
            ' Initialize the component's layout.
            InitializeComponent()

            ' Initialize the philosophers and forks
            Dim philosophers(NUM_PHILOSOPHERS - 1) As Ellipse
            Dim forks(NUM_PHILOSOPHERS - 1) As BufferBlock(Of Boolean)
            For i As Integer = 0 To philosophers.Length - 1
                philosophers(i) = New Ellipse With {.Height = 75, .Width = 75, .Fill = Brushes.Red, .Stroke = Brushes.Black}
                diningTable.Children.Add(philosophers(i))
                forks(i) = New BufferBlock(Of Boolean)()
                forks(i).Post(True)
            Next

            ' Run each philosopher
            For i As Integer = 0 To philosophers.Length - 1
                ' Pass the forks to each philosopher in an ordered (lock-leveled) manner
                RunPhilosopherAsync(philosophers(i),
                    If(i < philosophers.Length - 1, forks(i), forks(1)),
                    If(i < philosophers.Length - 1, forks(i + 1), forks(i)))
            Next
        End Sub

        ''' <summary>Runs a philosopher asynchronously.</summary>
        Private Async Sub RunPhilosopherAsync(philosopher As Ellipse, fork1 As BufferBlock(Of Boolean), fork2 As BufferBlock(Of Boolean))
            ' Think, Wait, and Eat, ad infinitum
            While (True)
                ' Think (Yellow)
                philosopher.Fill = Brushes.Yellow
                Await TaskEx.Delay(m_rand.Next(10) * TIMESCALE)

                ' Wait for forks (Red)
                philosopher.Fill = Brushes.Red
                Await fork1.ReceiveAsync()
                Await fork2.ReceiveAsync()

                ' Eat (Green)
                philosopher.Fill = Brushes.Green
                Await TaskEx.Delay(m_rand.Next(10) * TIMESCALE)

                ' Done with forks; put them back
                fork1.Post(True)
                fork2.Post(True)
            End While
        End Sub

    End Class
End Namespace