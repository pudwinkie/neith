namespace Smdn.Windows.Forms {
  partial class SelectWallpaperDialog {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.buttonChangeFile = new System.Windows.Forms.Button();
      this.textBoxFileName = new System.Windows.Forms.TextBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOK = new System.Windows.Forms.Button();
      this.labelFile = new System.Windows.Forms.Label();
      this.screenWallpaperView = new Smdn.Windows.Forms.SelectWallpaperDialog.ScreenWallpaperView();
      this.labelDescription = new System.Windows.Forms.Label();
      this.buttonApply = new System.Windows.Forms.Button();
      this.tabControlWallpaper = new System.Windows.Forms.TabControl();
      this.tabPageFileWallpaper = new System.Windows.Forms.TabPage();
      this.panelCommonConfiguration = new System.Windows.Forms.Panel();
      this.label5 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.imageFillStyleDropDownList = new Smdn.Windows.Forms.SelectWallpaperDialog.ImageFillStyleDropDownList();
      this.numericUpDownGradientDirection = new Smdn.Windows.Forms.CircularNumericUpDown();
      this.labelBackgroundColorNear = new System.Windows.Forms.Label();
      this.labelBackgroundColorFar = new System.Windows.Forms.Label();
      this.tabPageDirectoryWallpaper = new System.Windows.Forms.TabPage();
      this.label4 = new System.Windows.Forms.Label();
      this.fileSelectionOrderDropDownList = new Smdn.Windows.Forms.SelectWallpaperDialog.DirectoryWallapaperSelectionOrderDropDownList();
      this.buttonChangeDirectory = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.textBoxDirectoryName = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.timeSpanUpdateChangeInterval = new Smdn.Windows.Forms.TimeSpanUpDown();
      this.checkBoxDisableChangeInterval = new System.Windows.Forms.CheckBox();
      this.tabPageWallpaper = new System.Windows.Forms.TabPage();
      this.labelScreenWallpaper = new System.Windows.Forms.Label();
      this.tabControlWallpaper.SuspendLayout();
      this.tabPageFileWallpaper.SuspendLayout();
      this.panelCommonConfiguration.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGradientDirection)).BeginInit();
      this.tabPageDirectoryWallpaper.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.timeSpanUpdateChangeInterval)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonChangeFile
      // 
      this.buttonChangeFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChangeFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonChangeFile.Location = new System.Drawing.Point(359, 12);
      this.buttonChangeFile.Name = "buttonChangeFile";
      this.buttonChangeFile.Size = new System.Drawing.Size(32, 20);
      this.buttonChangeFile.TabIndex = 2;
      this.buttonChangeFile.Text = "...";
      // 
      // textBoxFileName
      // 
      this.textBoxFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxFileName.Location = new System.Drawing.Point(65, 16);
      this.textBoxFileName.Name = "textBoxFileName";
      this.textBoxFileName.ReadOnly = true;
      this.textBoxFileName.Size = new System.Drawing.Size(288, 12);
      this.textBoxFileName.TabIndex = 1;
      this.textBoxFileName.Text = "...";
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonCancel.Location = new System.Drawing.Point(366, 528);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(72, 24);
      this.buttonCancel.TabIndex = 5;
      this.buttonCancel.Text = "キャンセル";
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonOK.Location = new System.Drawing.Point(288, 528);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(72, 24);
      this.buttonOK.TabIndex = 4;
      this.buttonOK.Text = "OK";
      // 
      // labelFile
      // 
      this.labelFile.Location = new System.Drawing.Point(3, 16);
      this.labelFile.Name = "labelFile";
      this.labelFile.Size = new System.Drawing.Size(56, 16);
      this.labelFile.TabIndex = 0;
      this.labelFile.Text = "ファイル: ";
      // 
      // screenWallpaperView
      // 
      this.screenWallpaperView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.screenWallpaperView.Location = new System.Drawing.Point(12, 28);
      this.screenWallpaperView.Name = "screenWallpaperView";
      this.screenWallpaperView.SelectedScreen = null;
      this.screenWallpaperView.SelectedScreenIndex = -1;
      this.screenWallpaperView.SelectedScreenWallpaper = null;
      this.screenWallpaperView.Size = new System.Drawing.Size(503, 244);
      this.screenWallpaperView.TabIndex = 1;
      // 
      // labelDescription
      // 
      this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelDescription.Location = new System.Drawing.Point(10, 9);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(505, 16);
      this.labelDescription.TabIndex = 0;
      this.labelDescription.Text = "...";
      // 
      // buttonApply
      // 
      this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonApply.Location = new System.Drawing.Point(444, 528);
      this.buttonApply.Name = "buttonApply";
      this.buttonApply.Size = new System.Drawing.Size(72, 24);
      this.buttonApply.TabIndex = 6;
      this.buttonApply.Text = "適用";
      // 
      // tabControlWallpaper
      // 
      this.tabControlWallpaper.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlWallpaper.Controls.Add(this.tabPageFileWallpaper);
      this.tabControlWallpaper.Controls.Add(this.tabPageDirectoryWallpaper);
      this.tabControlWallpaper.Controls.Add(this.tabPageWallpaper);
      this.tabControlWallpaper.Location = new System.Drawing.Point(13, 290);
      this.tabControlWallpaper.Name = "tabControlWallpaper";
      this.tabControlWallpaper.SelectedIndex = 0;
      this.tabControlWallpaper.Size = new System.Drawing.Size(503, 232);
      this.tabControlWallpaper.TabIndex = 3;
      // 
      // tabPageFileWallpaper
      // 
      this.tabPageFileWallpaper.Controls.Add(this.panelCommonConfiguration);
      this.tabPageFileWallpaper.Controls.Add(this.buttonChangeFile);
      this.tabPageFileWallpaper.Controls.Add(this.labelFile);
      this.tabPageFileWallpaper.Controls.Add(this.textBoxFileName);
      this.tabPageFileWallpaper.Location = new System.Drawing.Point(4, 22);
      this.tabPageFileWallpaper.Name = "tabPageFileWallpaper";
      this.tabPageFileWallpaper.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageFileWallpaper.Size = new System.Drawing.Size(495, 206);
      this.tabPageFileWallpaper.TabIndex = 0;
      this.tabPageFileWallpaper.Text = "ファイル";
      this.tabPageFileWallpaper.UseVisualStyleBackColor = true;
      // 
      // panelCommonConfiguration
      // 
      this.panelCommonConfiguration.Controls.Add(this.label5);
      this.panelCommonConfiguration.Controls.Add(this.label2);
      this.panelCommonConfiguration.Controls.Add(this.label1);
      this.panelCommonConfiguration.Controls.Add(this.imageFillStyleDropDownList);
      this.panelCommonConfiguration.Controls.Add(this.numericUpDownGradientDirection);
      this.panelCommonConfiguration.Controls.Add(this.labelBackgroundColorNear);
      this.panelCommonConfiguration.Controls.Add(this.labelBackgroundColorFar);
      this.panelCommonConfiguration.Location = new System.Drawing.Point(5, 54);
      this.panelCommonConfiguration.Name = "panelCommonConfiguration";
      this.panelCommonConfiguration.Size = new System.Drawing.Size(406, 115);
      this.panelCommonConfiguration.TabIndex = 3;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(-2, 69);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 12);
      this.label5.TabIndex = 5;
      this.label5.Text = "画像の配置";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(161, 11);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(120, 12);
      this.label2.TabIndex = 3;
      this.label2.Text = "グラデーションの角度";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(-2, 11);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 12);
      this.label1.TabIndex = 0;
      this.label1.Text = "背景色";
      // 
      // imageFillStyleDropDownList
      // 
      this.imageFillStyleDropDownList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.imageFillStyleDropDownList.FormattingEnabled = true;
      this.imageFillStyleDropDownList.Location = new System.Drawing.Point(0, 84);
      this.imageFillStyleDropDownList.Name = "imageFillStyleDropDownList";
      this.imageFillStyleDropDownList.SelectedItem = Smdn.Imaging.ImageFillStyle.Center;
      this.imageFillStyleDropDownList.Size = new System.Drawing.Size(160, 20);
      this.imageFillStyleDropDownList.TabIndex = 6;
      // 
      // numericUpDownGradientDirection
      // 
      this.numericUpDownGradientDirection.Location = new System.Drawing.Point(163, 29);
      this.numericUpDownGradientDirection.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
      this.numericUpDownGradientDirection.Name = "numericUpDownGradientDirection";
      this.numericUpDownGradientDirection.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownGradientDirection.TabIndex = 4;
      // 
      // labelBackgroundColorNear
      // 
      this.labelBackgroundColorNear.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.labelBackgroundColorNear.Location = new System.Drawing.Point(0, 29);
      this.labelBackgroundColorNear.Name = "labelBackgroundColorNear";
      this.labelBackgroundColorNear.Size = new System.Drawing.Size(66, 28);
      this.labelBackgroundColorNear.TabIndex = 1;
      this.labelBackgroundColorNear.Text = "1";
      this.labelBackgroundColorNear.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelBackgroundColorFar
      // 
      this.labelBackgroundColorFar.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.labelBackgroundColorFar.Location = new System.Drawing.Point(80, 29);
      this.labelBackgroundColorFar.Name = "labelBackgroundColorFar";
      this.labelBackgroundColorFar.Size = new System.Drawing.Size(66, 28);
      this.labelBackgroundColorFar.TabIndex = 2;
      this.labelBackgroundColorFar.Text = "2";
      this.labelBackgroundColorFar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tabPageDirectoryWallpaper
      // 
      this.tabPageDirectoryWallpaper.Controls.Add(this.label4);
      this.tabPageDirectoryWallpaper.Controls.Add(this.fileSelectionOrderDropDownList);
      this.tabPageDirectoryWallpaper.Controls.Add(this.buttonChangeDirectory);
      this.tabPageDirectoryWallpaper.Controls.Add(this.label3);
      this.tabPageDirectoryWallpaper.Controls.Add(this.textBoxDirectoryName);
      this.tabPageDirectoryWallpaper.Controls.Add(this.label6);
      this.tabPageDirectoryWallpaper.Controls.Add(this.timeSpanUpdateChangeInterval);
      this.tabPageDirectoryWallpaper.Controls.Add(this.checkBoxDisableChangeInterval);
      this.tabPageDirectoryWallpaper.Location = new System.Drawing.Point(4, 22);
      this.tabPageDirectoryWallpaper.Name = "tabPageDirectoryWallpaper";
      this.tabPageDirectoryWallpaper.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageDirectoryWallpaper.Size = new System.Drawing.Size(495, 206);
      this.tabPageDirectoryWallpaper.TabIndex = 1;
      this.tabPageDirectoryWallpaper.Text = "フォルダから選択";
      this.tabPageDirectoryWallpaper.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(3, 37);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(56, 16);
      this.label4.TabIndex = 3;
      this.label4.Text = "選択順序: ";
      // 
      // fileSelectionOrderDropDownList
      // 
      this.fileSelectionOrderDropDownList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.fileSelectionOrderDropDownList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.fileSelectionOrderDropDownList.FormattingEnabled = true;
      this.fileSelectionOrderDropDownList.Location = new System.Drawing.Point(65, 34);
      this.fileSelectionOrderDropDownList.Name = "fileSelectionOrderDropDownList";
      this.fileSelectionOrderDropDownList.SelectedItem = Smdn.Windows.UserInterfaces.DirectoryWallpaper.SelectionOrder.ByFileName;
      this.fileSelectionOrderDropDownList.Size = new System.Drawing.Size(140, 20);
      this.fileSelectionOrderDropDownList.TabIndex = 4;
      // 
      // buttonChangeDirectory
      // 
      this.buttonChangeDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChangeDirectory.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonChangeDirectory.Location = new System.Drawing.Point(359, 12);
      this.buttonChangeDirectory.Name = "buttonChangeDirectory";
      this.buttonChangeDirectory.Size = new System.Drawing.Size(32, 20);
      this.buttonChangeDirectory.TabIndex = 2;
      this.buttonChangeDirectory.Text = "...";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(3, 16);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(56, 16);
      this.label3.TabIndex = 0;
      this.label3.Text = "フォルダ: ";
      // 
      // textBoxDirectoryName
      // 
      this.textBoxDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDirectoryName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxDirectoryName.Location = new System.Drawing.Point(65, 16);
      this.textBoxDirectoryName.Name = "textBoxDirectoryName";
      this.textBoxDirectoryName.ReadOnly = true;
      this.textBoxDirectoryName.Size = new System.Drawing.Size(288, 12);
      this.textBoxDirectoryName.TabIndex = 1;
      this.textBoxDirectoryName.Text = "...";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(3, 63);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(56, 16);
      this.label6.TabIndex = 5;
      this.label6.Text = "変更間隔: ";
      // 
      // timeSpanUpdateChangeInterval
      // 
      this.timeSpanUpdateChangeInterval.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.timeSpanUpdateChangeInterval.Increment = System.TimeSpan.Parse("00:10:00");
      this.timeSpanUpdateChangeInterval.Location = new System.Drawing.Point(65, 61);
      this.timeSpanUpdateChangeInterval.Maximum = System.TimeSpan.Parse("10.00:00:00");
      this.timeSpanUpdateChangeInterval.Minimum = System.TimeSpan.Parse("00:00:00");
      this.timeSpanUpdateChangeInterval.Name = "timeSpanUpdateChangeInterval";
      this.timeSpanUpdateChangeInterval.Size = new System.Drawing.Size(140, 19);
      this.timeSpanUpdateChangeInterval.TabIndex = 6;
      this.timeSpanUpdateChangeInterval.Text = "0:00:00.000";
      this.timeSpanUpdateChangeInterval.Value = System.TimeSpan.Parse("00:00:00");
      // 
      // checkBoxDisableChangeInterval
      // 
      this.checkBoxDisableChangeInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxDisableChangeInterval.Location = new System.Drawing.Point(211, 62);
      this.checkBoxDisableChangeInterval.Name = "checkBoxDisableChangeInterval";
      this.checkBoxDisableChangeInterval.Size = new System.Drawing.Size(86, 17);
      this.checkBoxDisableChangeInterval.TabIndex = 7;
      this.checkBoxDisableChangeInterval.Text = "変更しない";
      // 
      // tabPageWallpaper
      // 
      this.tabPageWallpaper.Location = new System.Drawing.Point(4, 22);
      this.tabPageWallpaper.Name = "tabPageWallpaper";
      this.tabPageWallpaper.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageWallpaper.Size = new System.Drawing.Size(495, 206);
      this.tabPageWallpaper.TabIndex = 2;
      this.tabPageWallpaper.Text = "なし";
      this.tabPageWallpaper.UseVisualStyleBackColor = true;
      // 
      // labelScreenWallpaper
      // 
      this.labelScreenWallpaper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelScreenWallpaper.AutoSize = true;
      this.labelScreenWallpaper.Location = new System.Drawing.Point(12, 275);
      this.labelScreenWallpaper.Name = "labelScreenWallpaper";
      this.labelScreenWallpaper.Size = new System.Drawing.Size(35, 12);
      this.labelScreenWallpaper.TabIndex = 2;
      this.labelScreenWallpaper.Text = "label4";
      // 
      // SelectWallpaperDialog
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(527, 564);
      this.Controls.Add(this.labelScreenWallpaper);
      this.Controls.Add(this.tabControlWallpaper);
      this.Controls.Add(this.buttonApply);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.screenWallpaperView);
      this.Controls.Add(this.labelDescription);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SelectWallpaperDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "SelectWallpaperDialog";
      this.tabControlWallpaper.ResumeLayout(false);
      this.tabPageFileWallpaper.ResumeLayout(false);
      this.tabPageFileWallpaper.PerformLayout();
      this.panelCommonConfiguration.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGradientDirection)).EndInit();
      this.tabPageDirectoryWallpaper.ResumeLayout(false);
      this.tabPageDirectoryWallpaper.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.timeSpanUpdateChangeInterval)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonChangeFile;
    private System.Windows.Forms.TextBox textBoxFileName;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Label labelFile;
    private Smdn.Windows.Forms.SelectWallpaperDialog.ScreenWallpaperView screenWallpaperView;
    private System.Windows.Forms.Label labelDescription;
    private System.Windows.Forms.Button buttonApply;
    private System.Windows.Forms.TabControl tabControlWallpaper;
    private System.Windows.Forms.TabPage tabPageFileWallpaper;
    private System.Windows.Forms.TabPage tabPageDirectoryWallpaper;
    private System.Windows.Forms.Label labelBackgroundColorFar;
    private System.Windows.Forms.Label labelBackgroundColorNear;
    private System.Windows.Forms.TabPage tabPageWallpaper;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private Smdn.Windows.Forms.CircularNumericUpDown numericUpDownGradientDirection;
    private System.Windows.Forms.Button buttonChangeDirectory;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBoxDirectoryName;
    private Smdn.Windows.Forms.SelectWallpaperDialog.DirectoryWallapaperSelectionOrderDropDownList fileSelectionOrderDropDownList;
    private System.Windows.Forms.Label labelScreenWallpaper;
    private Smdn.Windows.Forms.SelectWallpaperDialog.ImageFillStyleDropDownList imageFillStyleDropDownList;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Panel panelCommonConfiguration;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private Smdn.Windows.Forms.TimeSpanUpDown timeSpanUpdateChangeInterval;
    private System.Windows.Forms.CheckBox checkBoxDisableChangeInterval;
  }
}