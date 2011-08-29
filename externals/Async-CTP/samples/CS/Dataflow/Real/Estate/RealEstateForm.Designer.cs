namespace RealEstateSimulation
{
    partial class RealEstateForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.txtActiveBuyers = new System.Windows.Forms.TextBox();
            this.txtActiveListings = new System.Windows.Forms.TextBox();
            this.lbActivityLog = new System.Windows.Forms.ListBox();
            this.lblBuyers = new System.Windows.Forms.Label();
            this.lblForSale = new System.Windows.Forms.Label();
            this.lblLog = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lblLog);
            this.splitContainer1.Panel2.Controls.Add(this.lbActivityLog);
            this.splitContainer1.Size = new System.Drawing.Size(905, 571);
            this.splitContainer1.SplitterDistance = 330;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lblBuyers);
            this.splitContainer2.Panel1.Controls.Add(this.txtActiveBuyers);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lblForSale);
            this.splitContainer2.Panel2.Controls.Add(this.txtActiveListings);
            this.splitContainer2.Size = new System.Drawing.Size(330, 571);
            this.splitContainer2.SplitterDistance = 268;
            this.splitContainer2.TabIndex = 0;
            // 
            // txtActiveBuyers
            // 
            this.txtActiveBuyers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtActiveBuyers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.txtActiveBuyers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActiveBuyers.Location = new System.Drawing.Point(0, 32);
            this.txtActiveBuyers.Multiline = true;
            this.txtActiveBuyers.Name = "txtActiveBuyers";
            this.txtActiveBuyers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtActiveBuyers.Size = new System.Drawing.Size(330, 233);
            this.txtActiveBuyers.TabIndex = 0;
            // 
            // txtActiveListings
            // 
            this.txtActiveListings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtActiveListings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtActiveListings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActiveListings.Location = new System.Drawing.Point(0, 29);
            this.txtActiveListings.Multiline = true;
            this.txtActiveListings.Name = "txtActiveListings";
            this.txtActiveListings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtActiveListings.Size = new System.Drawing.Size(330, 264);
            this.txtActiveListings.TabIndex = 0;
            // 
            // lbActivityLog
            // 
            this.lbActivityLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbActivityLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.lbActivityLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbActivityLog.FormattingEnabled = true;
            this.lbActivityLog.ItemHeight = 20;
            this.lbActivityLog.Location = new System.Drawing.Point(3, 30);
            this.lbActivityLog.Name = "lbActivityLog";
            this.lbActivityLog.Size = new System.Drawing.Size(565, 524);
            this.lbActivityLog.TabIndex = 4;
            // 
            // lblBuyers
            // 
            this.lblBuyers.AutoSize = true;
            this.lblBuyers.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBuyers.Location = new System.Drawing.Point(3, 3);
            this.lblBuyers.Name = "lblBuyers";
            this.lblBuyers.Size = new System.Drawing.Size(80, 26);
            this.lblBuyers.TabIndex = 1;
            this.lblBuyers.Text = "Buyers";
            // 
            // lblForSale
            // 
            this.lblForSale.AutoSize = true;
            this.lblForSale.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblForSale.Location = new System.Drawing.Point(3, 0);
            this.lblForSale.Name = "lblForSale";
            this.lblForSale.Size = new System.Drawing.Size(94, 26);
            this.lblForSale.TabIndex = 2;
            this.lblForSale.Text = "For Sale";
            // 
            // lblLog
            // 
            this.lblLog.AutoSize = true;
            this.lblLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLog.Location = new System.Drawing.Point(447, 3);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(124, 26);
            this.lblLog.TabIndex = 2;
            this.lblLog.Text = "Activity Log";
            // 
            // RealEstateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(905, 571);
            this.Controls.Add(this.splitContainer1);
            this.Name = "RealEstateForm";
            this.Text = "Real Estate Simulation";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox txtActiveBuyers;
        private System.Windows.Forms.TextBox txtActiveListings;
        private System.Windows.Forms.ListBox lbActivityLog;
        private System.Windows.Forms.Label lblBuyers;
        private System.Windows.Forms.Label lblForSale;
        private System.Windows.Forms.Label lblLog;
    }
}

