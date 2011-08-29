<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.panel1 = New System.Windows.Forms.Panel()
        Me.richTextBox1 = New System.Windows.Forms.RichTextBox()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnLatest = New System.Windows.Forms.Button()
        Me.btnQueued = New System.Windows.Forms.Button()
        Me.pictureBox1 = New System.Windows.Forms.PictureBox()
        Me.panel1.SuspendLayout()
        CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'panel1
        '
        Me.panel1.Controls.Add(Me.richTextBox1)
        Me.panel1.Controls.Add(Me.btnCancel)
        Me.panel1.Controls.Add(Me.btnLatest)
        Me.panel1.Controls.Add(Me.btnQueued)
        Me.panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.panel1.Location = New System.Drawing.Point(0, 0)
        Me.panel1.Name = "panel1"
        Me.panel1.Size = New System.Drawing.Size(724, 166)
        Me.panel1.TabIndex = 1
        '
        'richTextBox1
        '
        Me.richTextBox1.Location = New System.Drawing.Point(12, 12)
        Me.richTextBox1.Name = "richTextBox1"
        Me.richTextBox1.Size = New System.Drawing.Size(575, 148)
        Me.richTextBox1.TabIndex = 5
        Me.richTextBox1.Text = resources.GetString("richTextBox1.Text")
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCancel.Enabled = False
        Me.btnCancel.Location = New System.Drawing.Point(597, 85)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(115, 23)
        Me.btnCancel.TabIndex = 4
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnLatest
        '
        Me.btnLatest.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnLatest.Location = New System.Drawing.Point(597, 12)
        Me.btnLatest.Name = "btnLatest"
        Me.btnLatest.Size = New System.Drawing.Size(115, 23)
        Me.btnLatest.TabIndex = 3
        Me.btnLatest.Text = "Count Light Pixels"
        Me.btnLatest.UseVisualStyleBackColor = True
        '
        'btnQueued
        '
        Me.btnQueued.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnQueued.Location = New System.Drawing.Point(597, 41)
        Me.btnQueued.Name = "btnQueued"
        Me.btnQueued.Size = New System.Drawing.Size(115, 23)
        Me.btnQueued.TabIndex = 2
        Me.btnQueued.Text = "Spread Light"
        Me.btnQueued.UseVisualStyleBackColor = True
        '
        'pictureBox1
        '
        Me.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pictureBox1.Image = Global.ProgressAndCancellationVB.My.Resources.Resources.progress
        Me.pictureBox1.Location = New System.Drawing.Point(0, 166)
        Me.pictureBox1.Name = "pictureBox1"
        Me.pictureBox1.Size = New System.Drawing.Size(724, 443)
        Me.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pictureBox1.TabIndex = 2
        Me.pictureBox1.TabStop = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(724, 609)
        Me.Controls.Add(Me.pictureBox1)
        Me.Controls.Add(Me.panel1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.panel1.ResumeLayout(False)
        CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents panel1 As System.Windows.Forms.Panel
    Private WithEvents richTextBox1 As System.Windows.Forms.RichTextBox
    Private WithEvents btnCancel As System.Windows.Forms.Button
    Private WithEvents btnLatest As System.Windows.Forms.Button
    Private WithEvents btnQueued As System.Windows.Forms.Button
    Private WithEvents pictureBox1 As System.Windows.Forms.PictureBox

End Class
