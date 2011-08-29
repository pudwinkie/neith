namespace GameOfLife
{
    partial class MainForm
    {
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
            if (disposing && (components != null))
            {
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
            this.pbLifeDisplay = new System.Windows.Forms.PictureBox();
            this.lblFramesPerSecond = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbLifeDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pbLifeDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLifeDisplay.BackColor = System.Drawing.Color.White;
            this.pbLifeDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbLifeDisplay.Location = new System.Drawing.Point(12, 12);
            this.pbLifeDisplay.Name = "pictureBox1";
            this.pbLifeDisplay.Size = new System.Drawing.Size(497, 433);
            this.pbLifeDisplay.TabIndex = 0;
            this.pbLifeDisplay.TabStop = false;
            // 
            // lblFramesPerSecond
            // 
            this.lblFramesPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFramesPerSecond.AutoSize = true;
            this.lblFramesPerSecond.Location = new System.Drawing.Point(12, 451);
            this.lblFramesPerSecond.Name = "lblFramesPerSecond";
            this.lblFramesPerSecond.Size = new System.Drawing.Size(77, 13);
            this.lblFramesPerSecond.TabIndex = 5;
            this.lblFramesPerSecond.Text = "Frames / Sec: ";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 473);
            this.Controls.Add(this.lblFramesPerSecond);
            this.Controls.Add(this.pbLifeDisplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Conway\'s Game Of Life";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbLifeDisplay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbLifeDisplay;
        private System.Windows.Forms.Label lblFramesPerSecond;
    }
}

