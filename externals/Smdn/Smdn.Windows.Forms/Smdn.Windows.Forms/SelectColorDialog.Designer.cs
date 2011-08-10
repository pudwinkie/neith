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
  partial class SelectColorDialog {
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
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOK = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.numericUpDownHsvSaturation = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      this.numericUpDownHsvValue = new System.Windows.Forms.NumericUpDown();
      this.label3 = new System.Windows.Forms.Label();
      this.numericUpDownRgbB = new System.Windows.Forms.NumericUpDown();
      this.label4 = new System.Windows.Forms.Label();
      this.numericUpDownRgbG = new System.Windows.Forms.NumericUpDown();
      this.label5 = new System.Windows.Forms.Label();
      this.numericUpDownRgbR = new System.Windows.Forms.NumericUpDown();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.textBoxRgb = new System.Windows.Forms.TextBox();
      this.labelHue = new System.Windows.Forms.Label();
      this.labelColor = new System.Windows.Forms.Label();
      this.tableLayoutPanelPreview = new System.Windows.Forms.TableLayoutPanel();
      this.numericUpDownHsvHue = new Smdn.Windows.Forms.CircularNumericUpDown();
      this.hsvColorPicker = new Smdn.Windows.Forms.HsvColorPicker();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHsvSaturation)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHsvValue)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRgbB)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRgbG)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRgbR)).BeginInit();
      this.tableLayoutPanelPreview.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHsvHue)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonCancel.Location = new System.Drawing.Point(387, 176);
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
      this.buttonOK.Location = new System.Drawing.Point(309, 176);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(72, 24);
      this.buttonOK.TabIndex = 12;
      this.buttonOK.Text = "OK";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(209, 12);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(45, 12);
      this.label1.TabIndex = 15;
      this.label1.Text = "色相(&H)";
      // 
      // numericUpDownHsvSaturation
      // 
      this.numericUpDownHsvSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownHsvSaturation.Location = new System.Drawing.Point(260, 35);
      this.numericUpDownHsvSaturation.Name = "numericUpDownHsvSaturation";
      this.numericUpDownHsvSaturation.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownHsvSaturation.TabIndex = 18;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(209, 37);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(44, 12);
      this.label2.TabIndex = 17;
      this.label2.Text = "彩度(&S)";
      // 
      // numericUpDownHsvValue
      // 
      this.numericUpDownHsvValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownHsvValue.Location = new System.Drawing.Point(260, 60);
      this.numericUpDownHsvValue.Name = "numericUpDownHsvValue";
      this.numericUpDownHsvValue.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownHsvValue.TabIndex = 20;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(209, 62);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(45, 12);
      this.label3.TabIndex = 19;
      this.label3.Text = "明度(&V)";
      // 
      // numericUpDownRgbB
      // 
      this.numericUpDownRgbB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownRgbB.Location = new System.Drawing.Point(393, 60);
      this.numericUpDownRgbB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.numericUpDownRgbB.Name = "numericUpDownRgbB";
      this.numericUpDownRgbB.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownRgbB.TabIndex = 26;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(354, 62);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(33, 12);
      this.label4.TabIndex = 25;
      this.label4.Text = "青(&B)";
      // 
      // numericUpDownRgbG
      // 
      this.numericUpDownRgbG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownRgbG.Location = new System.Drawing.Point(393, 35);
      this.numericUpDownRgbG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.numericUpDownRgbG.Name = "numericUpDownRgbG";
      this.numericUpDownRgbG.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownRgbG.TabIndex = 24;
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(354, 37);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(33, 12);
      this.label5.TabIndex = 23;
      this.label5.Text = "緑(&G)";
      // 
      // numericUpDownRgbR
      // 
      this.numericUpDownRgbR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownRgbR.Location = new System.Drawing.Point(393, 10);
      this.numericUpDownRgbR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.numericUpDownRgbR.Name = "numericUpDownRgbR";
      this.numericUpDownRgbR.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownRgbR.TabIndex = 22;
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(354, 12);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(33, 12);
      this.label6.TabIndex = 21;
      this.label6.Text = "赤(&R)";
      // 
      // label7
      // 
      this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(209, 109);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(29, 12);
      this.label7.TabIndex = 27;
      this.label7.Text = "RGB";
      // 
      // textBoxRgb
      // 
      this.textBoxRgb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxRgb.Location = new System.Drawing.Point(260, 106);
      this.textBoxRgb.Name = "textBoxRgb";
      this.textBoxRgb.ReadOnly = true;
      this.textBoxRgb.Size = new System.Drawing.Size(63, 19);
      this.textBoxRgb.TabIndex = 28;
      // 
      // labelHue
      // 
      this.labelHue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.labelHue.Location = new System.Drawing.Point(3, 0);
      this.labelHue.Name = "labelHue";
      this.labelHue.Size = new System.Drawing.Size(48, 28);
      this.labelHue.TabIndex = 29;
      // 
      // labelColor
      // 
      this.labelColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.labelColor.Location = new System.Drawing.Point(57, 0);
      this.labelColor.Name = "labelColor";
      this.labelColor.Size = new System.Drawing.Size(48, 28);
      this.labelColor.TabIndex = 30;
      // 
      // tableLayoutPanelPreview
      // 
      this.tableLayoutPanelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanelPreview.ColumnCount = 2;
      this.tableLayoutPanelPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelPreview.Controls.Add(this.labelHue, 0, 0);
      this.tableLayoutPanelPreview.Controls.Add(this.labelColor, 1, 0);
      this.tableLayoutPanelPreview.Location = new System.Drawing.Point(339, 106);
      this.tableLayoutPanelPreview.Name = "tableLayoutPanelPreview";
      this.tableLayoutPanelPreview.RowCount = 1;
      this.tableLayoutPanelPreview.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelPreview.Size = new System.Drawing.Size(120, 42);
      this.tableLayoutPanelPreview.TabIndex = 33;
      // 
      // numericUpDownHsvHue
      // 
      this.numericUpDownHsvHue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownHsvHue.Location = new System.Drawing.Point(260, 10);
      this.numericUpDownHsvHue.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
      this.numericUpDownHsvHue.Name = "numericUpDownHsvHue";
      this.numericUpDownHsvHue.Size = new System.Drawing.Size(63, 19);
      this.numericUpDownHsvHue.TabIndex = 16;
      // 
      // hsvColorPicker
      // 
      this.hsvColorPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hsvColorPicker.Location = new System.Drawing.Point(12, 12);
      this.hsvColorPicker.Name = "hsvColorPicker";
      this.hsvColorPicker.Size = new System.Drawing.Size(191, 188);
      this.hsvColorPicker.TabIndex = 14;
      // 
      // SelectColorDialog
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(471, 212);
      this.Controls.Add(this.tableLayoutPanelPreview);
      this.Controls.Add(this.textBoxRgb);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.numericUpDownRgbB);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.numericUpDownRgbG);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.numericUpDownRgbR);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.numericUpDownHsvValue);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.numericUpDownHsvSaturation);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.numericUpDownHsvHue);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.hsvColorPicker);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SelectColorDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "SelectColorDialog";
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHsvSaturation)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHsvValue)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRgbB)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRgbG)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRgbR)).EndInit();
      this.tableLayoutPanelPreview.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHsvHue)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
    private Smdn.Windows.Forms.HsvColorPicker hsvColorPicker;
    private System.Windows.Forms.Label label1;
    private Smdn.Windows.Forms.CircularNumericUpDown numericUpDownHsvHue;
    private System.Windows.Forms.NumericUpDown numericUpDownHsvSaturation;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown numericUpDownHsvValue;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown numericUpDownRgbB;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.NumericUpDown numericUpDownRgbG;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.NumericUpDown numericUpDownRgbR;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox textBoxRgb;
    private System.Windows.Forms.Label labelHue;
    private System.Windows.Forms.Label labelColor;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPreview;
  }
}