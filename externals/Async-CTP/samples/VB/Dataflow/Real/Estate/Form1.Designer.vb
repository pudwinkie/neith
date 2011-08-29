<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RealEstateForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
        Me.splitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.splitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.txtActiveBuyers = New System.Windows.Forms.TextBox()
        Me.txtActiveListings = New System.Windows.Forms.TextBox()
        Me.lbActivityLog = New System.Windows.Forms.ListBox()
        Me.lblBuyer = New System.Windows.Forms.Label()
        Me.lblActivityLog = New System.Windows.Forms.Label()
        Me.lblForSale = New System.Windows.Forms.Label()
        CType(Me.splitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitContainer1.Panel1.SuspendLayout()
        Me.splitContainer1.Panel2.SuspendLayout()
        Me.splitContainer1.SuspendLayout()
        CType(Me.splitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitContainer2.Panel1.SuspendLayout()
        Me.splitContainer2.Panel2.SuspendLayout()
        Me.splitContainer2.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitContainer1
        '
        Me.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.splitContainer1.Name = "splitContainer1"
        '
        'splitContainer1.Panel1
        '
        Me.splitContainer1.Panel1.Controls.Add(Me.splitContainer2)
        '
        'splitContainer1.Panel2
        '
        Me.splitContainer1.Panel2.Controls.Add(Me.lblActivityLog)
        Me.splitContainer1.Panel2.Controls.Add(Me.lbActivityLog)
        Me.splitContainer1.Size = New System.Drawing.Size(905, 568)
        Me.splitContainer1.SplitterDistance = 330
        Me.splitContainer1.TabIndex = 1
        '
        'splitContainer2
        '
        Me.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.splitContainer2.Name = "splitContainer2"
        Me.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'splitContainer2.Panel1
        '
        Me.splitContainer2.Panel1.Controls.Add(Me.lblBuyer)
        Me.splitContainer2.Panel1.Controls.Add(Me.txtActiveBuyers)
        '
        'splitContainer2.Panel2
        '
        Me.splitContainer2.Panel2.Controls.Add(Me.lblForSale)
        Me.splitContainer2.Panel2.Controls.Add(Me.txtActiveListings)
        Me.splitContainer2.Size = New System.Drawing.Size(330, 568)
        Me.splitContainer2.SplitterDistance = 267
        Me.splitContainer2.TabIndex = 0
        '
        'txtActiveBuyers
        '
        Me.txtActiveBuyers.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtActiveBuyers.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtActiveBuyers.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtActiveBuyers.Location = New System.Drawing.Point(0, 29)
        Me.txtActiveBuyers.Multiline = True
        Me.txtActiveBuyers.Name = "txtActiveBuyers"
        Me.txtActiveBuyers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtActiveBuyers.Size = New System.Drawing.Size(330, 235)
        Me.txtActiveBuyers.TabIndex = 0
        '
        'txtActiveListings
        '
        Me.txtActiveListings.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtActiveListings.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtActiveListings.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtActiveListings.Location = New System.Drawing.Point(0, 29)
        Me.txtActiveListings.Multiline = True
        Me.txtActiveListings.Name = "txtActiveListings"
        Me.txtActiveListings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtActiveListings.Size = New System.Drawing.Size(330, 262)
        Me.txtActiveListings.TabIndex = 0
        '
        'lbActivityLog
        '
        Me.lbActivityLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lbActivityLog.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.lbActivityLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbActivityLog.FormattingEnabled = True
        Me.lbActivityLog.ItemHeight = 20
        Me.lbActivityLog.Location = New System.Drawing.Point(3, 30)
        Me.lbActivityLog.Name = "lbActivityLog"
        Me.lbActivityLog.Size = New System.Drawing.Size(565, 524)
        Me.lbActivityLog.TabIndex = 4
        '
        'lblBuyer
        '
        Me.lblBuyer.AutoSize = True
        Me.lblBuyer.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBuyer.Location = New System.Drawing.Point(3, 0)
        Me.lblBuyer.Name = "lblBuyer"
        Me.lblBuyer.Size = New System.Drawing.Size(69, 26)
        Me.lblBuyer.TabIndex = 1
        Me.lblBuyer.Text = "Buyer"
        '
        'lblActivityLog
        '
        Me.lblActivityLog.AutoSize = True
        Me.lblActivityLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblActivityLog.Location = New System.Drawing.Point(447, 1)
        Me.lblActivityLog.Name = "lblActivityLog"
        Me.lblActivityLog.Size = New System.Drawing.Size(124, 26)
        Me.lblActivityLog.TabIndex = 2
        Me.lblActivityLog.Text = "Activity Log"
        '
        'lblForSale
        '
        Me.lblForSale.AutoSize = True
        Me.lblForSale.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblForSale.Location = New System.Drawing.Point(3, 0)
        Me.lblForSale.Name = "lblForSale"
        Me.lblForSale.Size = New System.Drawing.Size(94, 26)
        Me.lblForSale.TabIndex = 2
        Me.lblForSale.Text = "For Sale"
        '
        'RealEstateForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(905, 568)
        Me.Controls.Add(Me.splitContainer1)
        Me.Name = "RealEstateForm"
        Me.Text = "Real Estate Simulation"
        Me.splitContainer1.Panel1.ResumeLayout(False)
        Me.splitContainer1.Panel2.ResumeLayout(False)
        Me.splitContainer1.Panel2.PerformLayout()
        CType(Me.splitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitContainer1.ResumeLayout(False)
        Me.splitContainer2.Panel1.ResumeLayout(False)
        Me.splitContainer2.Panel1.PerformLayout()
        Me.splitContainer2.Panel2.ResumeLayout(False)
        Me.splitContainer2.Panel2.PerformLayout()
        CType(Me.splitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitContainer2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents splitContainer1 As System.Windows.Forms.SplitContainer
    Private WithEvents splitContainer2 As System.Windows.Forms.SplitContainer
    Private WithEvents txtActiveBuyers As System.Windows.Forms.TextBox
    Private WithEvents txtActiveListings As System.Windows.Forms.TextBox
    Private WithEvents lbActivityLog As System.Windows.Forms.ListBox
    Friend WithEvents lblBuyer As System.Windows.Forms.Label
    Friend WithEvents lblForSale As System.Windows.Forms.Label
    Friend WithEvents lblActivityLog As System.Windows.Forms.Label

End Class
