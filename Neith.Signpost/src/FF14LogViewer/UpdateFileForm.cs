using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;

namespace FF14LogViewer
{
public class UpdateFileForm : Form
{
    // Fields
    private BackgroundWorker backgroundWorker1;
    private Button CancelButton;
    private WebClient client;
    private IContainer components;
    private Label label1;
    private Label StatusLabel;
    public string uri;
    public string xmltext;

    // Methods
    public UpdateFileForm()
    {
        this.InitializeComponent();
    }

    private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
        try
        {
            this.backgroundWorker1.ReportProgress(0, string.Format("{0}に接続中・・・", this.uri));
            this.client = new WebClient();
            while (this.client.IsBusy)
            {
                Thread.Sleep(100);
                if (this.backgroundWorker1.CancellationPending)
                {
                    return;
                }
            }
            Stream stream = this.client.OpenRead(this.uri);
            this.backgroundWorker1.ReportProgress(0, string.Format("{0}に接続完了", this.uri));
            this.backgroundWorker1.ReportProgress(0, string.Format("データを読み込み中", this.uri));
            StreamReader reader = new StreamReader(stream);
            string str = reader.ReadToEnd();
            this.xmltext = str;
            reader.Close();
            this.backgroundWorker1.ReportProgress(0, string.Format("完了", this.uri));
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        this.StatusLabel.Text = e.UserState.ToString();
    }

    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            base.DialogResult = DialogResult.Cancel;
        }
        else if (e.Error != null)
        {
            MessageBox.Show(e.Error.Message, "エラー", MessageBoxButtons.OK);
            base.DialogResult = DialogResult.Cancel;
        }
        base.DialogResult = DialogResult.OK;
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        this.backgroundWorker1.CancelAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (this.components != null))
        {
            this.components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.StatusLabel = new Label();
        this.CancelButton = new Button();
        this.label1 = new Label();
        this.backgroundWorker1 = new BackgroundWorker();
        base.SuspendLayout();
        this.StatusLabel.AutoSize = true;
        this.StatusLabel.Location = new Point(12, 0x27);
        this.StatusLabel.Name = "StatusLabel";
        this.StatusLabel.Size = new Size(50, 12);
        this.StatusLabel.TabIndex = 6;
        this.StatusLabel.Text = "ステータス";
        this.CancelButton.Location = new Point(0x7c, 0x40);
        this.CancelButton.Name = "CancelButton";
        this.CancelButton.Size = new Size(0x4b, 0x17);
        this.CancelButton.TabIndex = 5;
        this.CancelButton.Text = "Cancel";
        this.CancelButton.UseVisualStyleBackColor = true;
        this.CancelButton.Click += new EventHandler(this.CancelButton_Click);
        this.label1.AutoSize = true;
        this.label1.Location = new Point(12, 9);
        this.label1.Name = "label1";
        this.label1.Size = new Size(0xa3, 12);
        this.label1.TabIndex = 4;
        this.label1.Text = "定型文辞書を更新しています・・・";
        this.backgroundWorker1.WorkerReportsProgress = true;
        this.backgroundWorker1.WorkerSupportsCancellation = true;
        this.backgroundWorker1.DoWork += new DoWorkEventHandler(this.backgroundWorker1_DoWork);
        this.backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
        this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
        base.AutoScaleDimensions = new SizeF(6f, 12f);
        base.AutoScaleMode = AutoScaleMode.Font;
        base.ClientSize = new Size(0x137, 0x63);
        base.Controls.Add(this.StatusLabel);
        base.Controls.Add(this.CancelButton);
        base.Controls.Add(this.label1);
        base.FormBorderStyle = FormBorderStyle.FixedSingle;
        base.Name = "UpdateFileForm";
        base.StartPosition = FormStartPosition.CenterParent;
        this.Text = "UpdateFileForm";
        base.Shown += new EventHandler(this.UpdateXmlForm_Load);
        base.ResumeLayout(false);
        base.PerformLayout();
    }

    private void UpdateXmlForm_Load(object sender, EventArgs e)
    {
        this.backgroundWorker1.RunWorkerAsync();
    }
}

}
