// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Smdn.Windows.Forms {
  partial class SelectIconDialog {
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
      this.listBoxIcons = new System.Windows.Forms.ListBox();
      this.labelDescription = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // buttonChangeFile
      // 
      this.buttonChangeFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChangeFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonChangeFile.Location = new System.Drawing.Point(382, 21);
      this.buttonChangeFile.Name = "buttonChangeFile";
      this.buttonChangeFile.Size = new System.Drawing.Size(32, 20);
      this.buttonChangeFile.TabIndex = 10;
      this.buttonChangeFile.Text = "...";
      this.buttonChangeFile.Click += new System.EventHandler(this.buttonChangeFile_Click);
      // 
      // textBoxFileName
      // 
      this.textBoxFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxFileName.Location = new System.Drawing.Point(74, 25);
      this.textBoxFileName.Name = "textBoxFileName";
      this.textBoxFileName.ReadOnly = true;
      this.textBoxFileName.Size = new System.Drawing.Size(296, 12);
      this.textBoxFileName.TabIndex = 9;
      this.textBoxFileName.Text = "...";
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonCancel.Location = new System.Drawing.Point(342, 259);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(72, 24);
      this.buttonCancel.TabIndex = 13;
      this.buttonCancel.Text = "キャンセル";
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonOK.Location = new System.Drawing.Point(264, 259);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(72, 24);
      this.buttonOK.TabIndex = 12;
      this.buttonOK.Text = "OK";
      // 
      // labelFile
      // 
      this.labelFile.Location = new System.Drawing.Point(12, 25);
      this.labelFile.Name = "labelFile";
      this.labelFile.Size = new System.Drawing.Size(56, 16);
      this.labelFile.TabIndex = 8;
      this.labelFile.Text = "ファイル: ";
      // 
      // listBoxIcons
      // 
      this.listBoxIcons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxIcons.ItemHeight = 12;
      this.listBoxIcons.Location = new System.Drawing.Point(12, 45);
      this.listBoxIcons.Name = "listBoxIcons";
      this.listBoxIcons.Size = new System.Drawing.Size(402, 196);
      this.listBoxIcons.TabIndex = 11;
      // 
      // labelDescription
      // 
      this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelDescription.Location = new System.Drawing.Point(12, 9);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(402, 16);
      this.labelDescription.TabIndex = 7;
      this.labelDescription.Text = "...";
      // 
      // SelectIconDialog
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(426, 295);
      this.Controls.Add(this.buttonChangeFile);
      this.Controls.Add(this.textBoxFileName);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.labelFile);
      this.Controls.Add(this.listBoxIcons);
      this.Controls.Add(this.labelDescription);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SelectIconDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "SelectIconDialog";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonChangeFile;
    private System.Windows.Forms.TextBox textBoxFileName;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Label labelFile;
    private System.Windows.Forms.ListBox listBoxIcons;
    private System.Windows.Forms.Label labelDescription;
  }
}