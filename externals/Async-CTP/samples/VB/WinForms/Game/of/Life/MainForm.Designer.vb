Namespace GameOfLife
	Partial Public Class MainForm
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.IContainer = Nothing

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing AndAlso (components IsNot Nothing) Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

		#Region "Windows Form Designer generated code"

		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
            Me.pbLifeDisplay = New System.Windows.Forms.PictureBox()
            Me.lblFramesPerSecond = New System.Windows.Forms.Label()
            CType(Me.pbLifeDisplay, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'pbLifeDisplay
            '
            Me.pbLifeDisplay.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                        Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.pbLifeDisplay.BackColor = System.Drawing.Color.White
            Me.pbLifeDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.pbLifeDisplay.Location = New System.Drawing.Point(12, 12)
            Me.pbLifeDisplay.Name = "pbLifeDisplay"
            Me.pbLifeDisplay.Size = New System.Drawing.Size(487, 426)
            Me.pbLifeDisplay.TabIndex = 0
            Me.pbLifeDisplay.TabStop = False
            '
            'lblFramesPerSecond
            '
            Me.lblFramesPerSecond.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.lblFramesPerSecond.AutoSize = True
            Me.lblFramesPerSecond.Location = New System.Drawing.Point(12, 441)
            Me.lblFramesPerSecond.Name = "lblFramesPerSecond"
            Me.lblFramesPerSecond.Size = New System.Drawing.Size(77, 13)
            Me.lblFramesPerSecond.TabIndex = 5
            Me.lblFramesPerSecond.Text = "Frames / Sec: "
            '
            'MainForm
            '
            Me.ClientSize = New System.Drawing.Size(511, 463)
            Me.Controls.Add(Me.lblFramesPerSecond)
            Me.Controls.Add(Me.pbLifeDisplay)
            Me.Name = "MainForm"
            Me.Text = "Conway's Game Of Life"
            CType(Me.pbLifeDisplay, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

#End Region

        Private pbLifeDisplay As PictureBox
        Private lblFramesPerSecond As Label
	End Class
End Namespace

