using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using FFXIVRuby;

namespace FF14LogViewer
{
public class LogViewerForm : Form
{
    // Fields
    private BackgroundWorker backgroundWorker1;
    private IContainer components;
    private FileSystemWatcher fileSystemWatcher1;
    private FontSettingForm fontsettingfrm;
    private string LastLogFile;
    private List<FFXIVLog> LogListFromFile;
    private List<FFXIVLog> LogListFromMemory;
    private FFXIVLogStatus logstat;
    private int logterminal;
    private MenuStrip menuStrip1;
    private PictureBox pictureBox1;
    private RichTextBox richTextBox1;
    private TabJapanese tabjp = new TabJapanese();
    private Timer timer1;
    private ToolStripMenuItem ツールTToolStripMenuItem;
    private ToolStripMenuItem ファイルFToolStripMenuItem;
    private ToolStripMenuItem 設定OToolStripMenuItem;
    private ToolStripMenuItem 定型文辞書更新UToolStripMenuItem;
    private ToolStripMenuItem 閉じるXToolStripMenuItem;

    // Methods
    public LogViewerForm()
    {
        this.InitializeComponent();
        this.fontsettingfrm = new FontSettingForm();
        this.fontsettingfrm.IsTransparent = Settings.Default.IsTransparent;
        this.LogListFromFile = new List<FFXIVLog>();
        this.LogListFromMemory = new List<FFXIVLog>();
        this.pictureBox1.BackColor = this.fontsettingfrm.BGColor;
    }

    private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
        Process fFXIVGameProcess = FFXIVMemoryProvidor.GetFFXIVGameProcess();
        if (fFXIVGameProcess != null)
        {
            this.logstat = new LogStatusSearcher(new FFXIVProcess(fFXIVGameProcess)).Search();
        }
    }

    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (this.logstat != null)
        {
            this.Text = this.Text + "★";
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (this.components != null))
        {
            this.components.Dispose();
        }
        base.Dispose(disposing);
    }

    private Point DrawString(Graphics g, string text, Font font, Brush brush, Point p, int width, int fontsize, bool dan)
    {
        new StringBuilder();
        string str = text;
        string str2 = "";
        for (int i = text.Length - 1; (TextRenderer.MeasureText(str, font).Width > width) && (str.Length > 0); i--)
        {
            str = text.Substring(0, i);
            str2 = text.Substring(i, text.Length - i);
        }
        int num2 = 0;
        if (dan)
        {
            num2 = fontsize;
        }
        float num3 = 1f;
        g.DrawString(str, font, new SolidBrush(Color.FromArgb(1, 1, 1)), (float) ((num2 + p.X) - num3), (float) (p.Y - num3));
        g.DrawString(str, font, new SolidBrush(Color.FromArgb(1, 1, 1)), (float) ((num2 + p.X) - num3), (float) (p.Y + num3));
        g.DrawString(str, font, new SolidBrush(Color.FromArgb(1, 1, 1)), (float) ((num2 + p.X) + num3), (float) (p.Y - num3));
        g.DrawString(str, font, new SolidBrush(Color.FromArgb(1, 1, 1)), (float) ((num2 + p.X) + num3), (float) (p.Y + num3));
        g.DrawString(str, font, brush, (float) (num2 + p.X), (float) p.Y);
        if (str2.Length > 0)
        {
            return this.DrawString(g, str2, font, brush, new Point(p.X, p.Y + fontsize), width, fontsize, true);
        }
        p.Y += fontsize;
        return p;
    }

    private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
    {
        if ((this.LastLogFile == null) || (this.LastLogFile != e.FullPath))
        {
            try
            {
                if ((e.ChangeType == WatcherChangeTypes.Created) || (e.ChangeType == WatcherChangeTypes.Changed))
                {
                    this.LoadLogs(e.FullPath);
                    this.LastLogFile = e.FullPath;
                    this.UpdateLogs();
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private void Form1_Activated(object sender, EventArgs e)
    {
        this.pictureBox1.Hide();
        this.richTextBox1.ScrollToCaret();
        this.menuStrip1.Show();
        base.FormBorderStyle = FormBorderStyle.Sizable;
        base.AllowTransparency = false;
        this.AutoScroll = true;
        this.richTextBox1.ScrollBars = RichTextBoxScrollBars.Both;
    }

    private void Form1_Deactivate(object sender, EventArgs e)
    {
        this.pictureBox1.Show();
        this.menuStrip1.Hide();
        base.FormBorderStyle = FormBorderStyle.None;
        base.AllowTransparency = this.fontsettingfrm.IsTransparent;
        if (this.fontsettingfrm.IsTransparent)
        {
            base.TransparencyKey = this.fontsettingfrm.BGColor;
        }
        base.TopMost = true;
        this.richTextBox1.ScrollBars = RichTextBoxScrollBars.None;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        this.fileSystemWatcher1.Path = this.FF14UserFolderPath;
        try
        {
            this.tabjp.ReadXml(tabStrinLibXmlFile);
        }
        catch (Exception)
        {
            MessageBox.Show("定型文辞書ファイルの読み込みに失敗しました。");
        }
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
        this.LoadLogsFromFiles();
        this.UpdateLogs();
        this.timer1.Start();
    }

    private bool GetEnableLog(FFXIVLog log)
    {
        switch (log.MessageType)
        {
            case FFXIVLog.FFXILogMessageType.SAY:
                return this.fontsettingfrm.SayEnable;

            case FFXIVLog.FFXILogMessageType.TELL:
                return this.fontsettingfrm.TELLEnable;

            case FFXIVLog.FFXILogMessageType.PARTY:
                return this.fontsettingfrm.PARTYEnable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL1:
                return this.fontsettingfrm.LINKSHELL1Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL2:
                return this.fontsettingfrm.LINKSHELL2Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL3:
                return this.fontsettingfrm.LINKSHELL3Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL4:
                return this.fontsettingfrm.LINKSHELL4Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL5:
                return this.fontsettingfrm.LINKSHELL5Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL6:
                return this.fontsettingfrm.LINKSHELL6Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL7:
                return this.fontsettingfrm.LINKSHELL7Enable;

            case FFXIVLog.FFXILogMessageType.LINKSHELL8:
                return this.fontsettingfrm.LINKSHELL8Enable;

            case FFXIVLog.FFXILogMessageType.MY_TELL:
                return this.fontsettingfrm.TELLEnable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL1:
                return this.fontsettingfrm.LINKSHELL1Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL2:
                return this.fontsettingfrm.LINKSHELL2Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL3:
                return this.fontsettingfrm.LINKSHELL3Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL4:
                return this.fontsettingfrm.LINKSHELL4Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL5:
                return this.fontsettingfrm.LINKSHELL5Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL6:
                return this.fontsettingfrm.LINKSHELL6Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL7:
                return this.fontsettingfrm.LINKSHELL7Enable;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL8:
                return this.fontsettingfrm.LINKSHELL8Enable;

            case FFXIVLog.FFXILogMessageType.EMOTE:
                return this.fontsettingfrm.EmoteEnable;
        }
        return false;
    }

    private Point GetEndPoint(Graphics g, List<FFXIVLog> LogList, int fontsize, Font font, Brush brush)
    {
        int num = this.pictureBox1.Height / fontsize;
        int num2 = LogList.Count - num;
        if (num2 < 0)
        {
            num2 = 0;
        }
        Point p = new Point(0, 0);
        for (int i = num2; i < LogList.Count; i++)
        {
            string japanese = this.tabjp.GetJapanese(LogList[i].ToString());
            p = this.DrawString(g, japanese, font, brush, p, this.pictureBox1.Width - 20, fontsize, false);
        }
        return p;
    }

    private Font GetFont(FFXIVLog log)
    {
        switch (log.MessageType)
        {
            case FFXIVLog.FFXILogMessageType.SAY:
                return this.fontsettingfrm.FontSay;

            case FFXIVLog.FFXILogMessageType.TELL:
                return this.fontsettingfrm.FontTELL;

            case FFXIVLog.FFXILogMessageType.PARTY:
                return this.fontsettingfrm.FontPARTY;

            case FFXIVLog.FFXILogMessageType.LINKSHELL1:
                return this.fontsettingfrm.FontLINKSHELL1;

            case FFXIVLog.FFXILogMessageType.LINKSHELL2:
                return this.fontsettingfrm.FontLINKSHELL2;

            case FFXIVLog.FFXILogMessageType.LINKSHELL3:
                return this.fontsettingfrm.FontLINKSHELL3;

            case FFXIVLog.FFXILogMessageType.LINKSHELL4:
                return this.fontsettingfrm.FontLINKSHELL4;

            case FFXIVLog.FFXILogMessageType.LINKSHELL5:
                return this.fontsettingfrm.FontLINKSHELL5;

            case FFXIVLog.FFXILogMessageType.LINKSHELL6:
                return this.fontsettingfrm.FontLINKSHELL6;

            case FFXIVLog.FFXILogMessageType.LINKSHELL7:
                return this.fontsettingfrm.FontLINKSHELL7;

            case FFXIVLog.FFXILogMessageType.LINKSHELL8:
                return this.fontsettingfrm.FontLINKSHELL8;

            case FFXIVLog.FFXILogMessageType.MY_TELL:
                return this.fontsettingfrm.FontTELL;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL1:
                return this.fontsettingfrm.FontLINKSHELL1;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL2:
                return this.fontsettingfrm.FontLINKSHELL2;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL3:
                return this.fontsettingfrm.FontLINKSHELL3;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL4:
                return this.fontsettingfrm.FontLINKSHELL4;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL5:
                return this.fontsettingfrm.FontLINKSHELL5;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL6:
                return this.fontsettingfrm.FontLINKSHELL6;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL7:
                return this.fontsettingfrm.FontLINKSHELL7;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL8:
                return this.fontsettingfrm.FontLINKSHELL8;

            case FFXIVLog.FFXILogMessageType.EMOTE:
                return this.fontsettingfrm.FontEmote;
        }
        return this.fontsettingfrm.FontSay;
    }

    private int GetLineCount(FFXIVLog[] LogList, int height, ref Point p)
    {
        Graphics g = Graphics.FromImage(new Bitmap(this.pictureBox1.Width - 20, this.pictureBox1.Height));
        for (int i = LogList.Length - 1; i >= 0; i--)
        {
            string text = LogList[i].ToString();
            text = this.tabjp.GetJapanese(text);
            Font font = this.GetFont(LogList[i]);
            p = this.DrawString(g, text, font, Brushes.Black, p, this.pictureBox1.Width - 20, font.Height, false);
            if (p.Y > height)
            {
                return (LogList.Length - i);
            }
        }
        return LogList.Length;
    }

    private Color GetLogColor(FFXIVLog log)
    {
        switch (log.MessageType)
        {
            case FFXIVLog.FFXILogMessageType.SAY:
                return this.fontsettingfrm.SayColor;

            case FFXIVLog.FFXILogMessageType.TELL:
                return this.fontsettingfrm.TELLColor;

            case FFXIVLog.FFXILogMessageType.PARTY:
                return this.fontsettingfrm.PARTYColor;

            case FFXIVLog.FFXILogMessageType.LINKSHELL1:
                return this.fontsettingfrm.LINKSHELL1Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL2:
                return this.fontsettingfrm.LINKSHELL2Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL3:
                return this.fontsettingfrm.LINKSHELL3Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL4:
                return this.fontsettingfrm.LINKSHELL4Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL5:
                return this.fontsettingfrm.LINKSHELL5Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL6:
                return this.fontsettingfrm.LINKSHELL6Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL7:
                return this.fontsettingfrm.LINKSHELL7Color;

            case FFXIVLog.FFXILogMessageType.LINKSHELL8:
                return this.fontsettingfrm.LINKSHELL8Color;

            case FFXIVLog.FFXILogMessageType.MY_TELL:
                return this.fontsettingfrm.TELLColor;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL1:
                return this.fontsettingfrm.LINKSHELL1Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL2:
                return this.fontsettingfrm.LINKSHELL2Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL3:
                return this.fontsettingfrm.LINKSHELL3Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL4:
                return this.fontsettingfrm.LINKSHELL4Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL5:
                return this.fontsettingfrm.LINKSHELL5Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL6:
                return this.fontsettingfrm.LINKSHELL6Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL7:
                return this.fontsettingfrm.LINKSHELL7Color;

            case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL8:
                return this.fontsettingfrm.LINKSHELL8Color;

            case FFXIVLog.FFXILogMessageType.EMOTE:
                return this.fontsettingfrm.EmoteColor;
        }
        return this.fontsettingfrm.SayColor;
    }

    private string[] GetUserFolders()
    {
        List<string> list = new List<string>();
        list.AddRange(Directory.GetDirectories(this.FF14UserFolderPath));
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Substring(list[i].Length - 8, 8) == "00000000")
            {
                list.RemoveAt(i);
            }
        }
        return list.ToArray();
    }

    private void InitializeComponent()
    {
        this.components = new Container();
        ComponentResourceManager manager = new ComponentResourceManager(typeof(LogViewerForm));
        this.fileSystemWatcher1 = new FileSystemWatcher();
        this.timer1 = new Timer(this.components);
        this.backgroundWorker1 = new BackgroundWorker();
        this.richTextBox1 = new RichTextBox();
        this.pictureBox1 = new PictureBox();
        this.menuStrip1 = new MenuStrip();
        this.ファイルFToolStripMenuItem = new ToolStripMenuItem();
        this.閉じるXToolStripMenuItem = new ToolStripMenuItem();
        this.ツールTToolStripMenuItem = new ToolStripMenuItem();
        this.設定OToolStripMenuItem = new ToolStripMenuItem();
        this.定型文辞書更新UToolStripMenuItem = new ToolStripMenuItem();
        this.fileSystemWatcher1.BeginInit();
        ((ISupportInitialize) this.pictureBox1).BeginInit();
        this.menuStrip1.SuspendLayout();
        base.SuspendLayout();
        this.fileSystemWatcher1.EnableRaisingEvents = true;
        this.fileSystemWatcher1.Filter = "*.LOG";
        this.fileSystemWatcher1.IncludeSubdirectories = true;
        this.fileSystemWatcher1.NotifyFilter = NotifyFilters.LastWrite;
        this.fileSystemWatcher1.SynchronizingObject = this;
        this.fileSystemWatcher1.Changed += new FileSystemEventHandler(this.fileSystemWatcher1_Changed);
        this.timer1.Interval = 0x3e8;
        this.timer1.Tick += new EventHandler(this.timer1_Tick);
        this.backgroundWorker1.DoWork += new DoWorkEventHandler(this.backgroundWorker1_DoWork);
        this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
        this.richTextBox1.BorderStyle = BorderStyle.None;
        this.richTextBox1.DataBindings.Add(new Binding("BackColor", Settings.Default, "BGColor", true, DataSourceUpdateMode.OnPropertyChanged));
        this.richTextBox1.Dock = DockStyle.Fill;
        this.richTextBox1.ForeColor = Color.WhiteSmoke;
        this.richTextBox1.Location = new Point(0, 0x1a);
        this.richTextBox1.Name = "richTextBox1";
        this.richTextBox1.ReadOnly = true;
        this.richTextBox1.Size = new Size(0x220, 0x11a);
        this.richTextBox1.TabIndex = 0;
        this.richTextBox1.Text = "";
        this.pictureBox1.BackColor = Settings.Default.BGColor;
        this.pictureBox1.DataBindings.Add(new Binding("BackColor", Settings.Default, "BGColor", true, DataSourceUpdateMode.OnPropertyChanged));
        this.pictureBox1.Dock = DockStyle.Fill;
        this.pictureBox1.Location = new Point(0, 0x1a);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new Size(0x220, 0x11a);
        this.pictureBox1.TabIndex = 1;
        this.pictureBox1.TabStop = false;
        this.pictureBox1.Paint += new PaintEventHandler(this.PictureBox1_Paint);
        this.menuStrip1.Items.AddRange(new ToolStripItem[] { this.ファイルFToolStripMenuItem, this.ツールTToolStripMenuItem });
        this.menuStrip1.Location = new Point(0, 0);
        this.menuStrip1.Name = "menuStrip1";
        this.menuStrip1.Size = new Size(0x220, 0x1a);
        this.menuStrip1.TabIndex = 2;
        this.menuStrip1.Text = "menuStrip1";
        this.ファイルFToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.閉じるXToolStripMenuItem });
        this.ファイルFToolStripMenuItem.Name = "ファイルFToolStripMenuItem";
        this.ファイルFToolStripMenuItem.Size = new Size(0x55, 0x16);
        this.ファイルFToolStripMenuItem.Text = "ファイル(&F)";
        this.閉じるXToolStripMenuItem.Name = "閉じるXToolStripMenuItem";
        this.閉じるXToolStripMenuItem.Size = new Size(130, 0x16);
        this.閉じるXToolStripMenuItem.Text = "閉じる(&X)";
        this.閉じるXToolStripMenuItem.Click += new EventHandler(this.閉じるXToolStripMenuItem_Click);
        this.ツールTToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.設定OToolStripMenuItem, this.定型文辞書更新UToolStripMenuItem });
        this.ツールTToolStripMenuItem.Name = "ツールTToolStripMenuItem";
        this.ツールTToolStripMenuItem.Size = new Size(0x4a, 0x16);
        this.ツールTToolStripMenuItem.Text = "ツール(&T)";
        this.設定OToolStripMenuItem.Name = "設定OToolStripMenuItem";
        this.設定OToolStripMenuItem.Size = new Size(0xb3, 0x16);
        this.設定OToolStripMenuItem.Text = "設定(&O)";
        this.設定OToolStripMenuItem.Click += new EventHandler(this.設定OToolStripMenuItem_Click);
        this.定型文辞書更新UToolStripMenuItem.Name = "定型文辞書更新UToolStripMenuItem";
        this.定型文辞書更新UToolStripMenuItem.Size = new Size(0xb3, 0x16);
        this.定型文辞書更新UToolStripMenuItem.Text = "定型文辞書更新(&U)";
        this.定型文辞書更新UToolStripMenuItem.Click += new EventHandler(this.定型文辞書更新UToolStripMenuItem_Click);
        base.AutoScaleDimensions = new SizeF(6f, 12f);
        base.AutoScaleMode = AutoScaleMode.Font;
        this.AutoScroll = true;
        this.BackColor = SystemColors.Control;
        base.ClientSize = Settings.Default.ClientSize;
        base.Controls.Add(this.pictureBox1);
        base.Controls.Add(this.richTextBox1);
        base.Controls.Add(this.menuStrip1);
        base.DataBindings.Add(new Binding("Location", Settings.Default, "MainFormLocation", true, DataSourceUpdateMode.OnPropertyChanged));
        base.DataBindings.Add(new Binding("ClientSize", Settings.Default, "ClientSize", true, DataSourceUpdateMode.OnPropertyChanged));
        base.Icon = (Icon) manager.GetObject("$this.Icon");
        base.Location = Settings.Default.MainFormLocation;
        base.MainMenuStrip = this.menuStrip1;
        this.MinimumSize = new Size(100, 50);
        base.Name = "LogViewerForm";
        this.Text = "FF14LogViewer";
        base.TopMost = true;
        base.Activated += new EventHandler(this.Form1_Activated);
        base.Deactivate += new EventHandler(this.Form1_Deactivate);
        base.FormClosing += new FormClosingEventHandler(this.LogViewerForm_FormClosing);
        base.Load += new EventHandler(this.Form1_Load);
        base.Shown += new EventHandler(this.Form1_Shown);
        this.fileSystemWatcher1.EndInit();
        ((ISupportInitialize) this.pictureBox1).EndInit();
        this.menuStrip1.ResumeLayout(false);
        this.menuStrip1.PerformLayout();
        base.ResumeLayout(false);
        base.PerformLayout();
    }

    private void LoadLogs(string file)
    {
        foreach (FFXIVLog log in FFXIVLog.GetLogs(File.ReadAllBytes(file), Encoding.GetEncoding("utf-8")))
        {
            if (this.GetEnableLog(log))
            {
                this.LogListFromFile.Add(log);
            }
        }
    }

    private void LoadLogsFromFiles()
    {
        DateTime minValue = DateTime.MinValue;
        string path = "";
        foreach (string str2 in this.GetUserFolders())
        {
            string str3 = Path.Combine(str2, "log");
            if (Directory.Exists(str3))
            {
                foreach (string str4 in Directory.GetFiles(str3, "*.LOG"))
                {
                    if (File.GetLastWriteTime(str4) > minValue)
                    {
                        minValue = File.GetLastWriteTime(str4);
                        path = str3;
                    }
                }
            }
        }
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.LOG");
            DateTime lastWriteTime = DateTime.MinValue;
            for (int i = 0; i < files.Length; i++)
            {
                string str5 = Path.Combine(path, string.Format("{0}.LOG", i.ToString("X").PadLeft(8, '0')));
                if (File.Exists(str5) && (File.GetLastWriteTime(str5) > lastWriteTime))
                {
                    this.LoadLogs(str5);
                    lastWriteTime = File.GetLastWriteTime(str5);
                }
            }
        }
    }

    private void LoadLogsFromProcess()
    {
        this.LogListFromMemory.Clear();
        if (this.logstat != null)
        {
            foreach (FFXIVLog log in this.logstat.GetLogs())
            {
                if (this.GetEnableLog(log))
                {
                    this.LogListFromMemory.Add(log);
                }
            }
        }
    }

    private void LogViewerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Settings.Default.Save();
    }

    private void PictureBox1_Paint(object sender, PaintEventArgs e)
    {
        lock (this)
        {
            if (base.WindowState != FormWindowState.Minimized)
            {
                List<FFXIVLog> list = new List<FFXIVLog>();
                list.AddRange(this.LogListFromFile);
                list.AddRange(this.LogListFromMemory);
                Graphics g = e.Graphics;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                Brush whiteSmoke = Brushes.WhiteSmoke;
                Point empty = Point.Empty;
                int num = this.GetLineCount(list.ToArray(), this.pictureBox1.Height, ref empty);
                empty.Y = this.pictureBox1.Height - empty.Y;
                if (empty.Y > 0)
                {
                    empty.Y = 0;
                }
                g.Clear(this.fontsettingfrm.BGColor);
                int num2 = list.Count - num;
                if (num2 < 0)
                {
                    num2 = 0;
                }
                for (int i = num2; i < list.Count; i++)
                {
                    switch (list[i].MessageType)
                    {
                        case FFXIVLog.FFXILogMessageType.SAY:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.SayColor);
                            break;

                        case FFXIVLog.FFXILogMessageType.TELL:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.TELLColor);
                            break;

                        case FFXIVLog.FFXILogMessageType.PARTY:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.PARTYColor);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL1:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL1Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL2:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL2Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL3:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL3Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL4:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL4Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL5:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL5Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL6:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL6Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL7:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL7Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.LINKSHELL8:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL8Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.MY_TELL:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.TELLColor);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL1:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL1Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL2:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL2Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL3:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL3Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL4:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL4Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL5:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL5Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL6:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL6Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL7:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL7Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.CURRENT_LINKSHELL8:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.LINKSHELL8Color);
                            break;

                        case FFXIVLog.FFXILogMessageType.EMOTE:
                            whiteSmoke = new SolidBrush(this.fontsettingfrm.EmoteColor);
                            break;
                    }
                    Font font = this.GetFont(list[i]);
                    empty = this.DrawString(g, this.tabjp.GetJapanese(list[i].ToString()), font, whiteSmoke, empty, this.pictureBox1.Width - 20, font.Height, false);
                }
            }
        }
    }

    private void RefreshLogs()
    {
        this.LogListFromFile.Clear();
        this.LogListFromMemory.Clear();
        this.LoadLogsFromFiles();
        this.LoadLogsFromProcess();
        this.UpdateLogs();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if ((this.logstat == null) && !this.backgroundWorker1.IsBusy)
        {
            this.backgroundWorker1.RunWorkerAsync();
        }
        if ((this.logstat != null) && (this.logstat.GetTerminalPoint() != this.logterminal))
        {
            this.LoadLogsFromProcess();
            this.logterminal = this.logstat.GetTerminalPoint();
            this.UpdateLogs();
        }
    }

    private void UpdateLogs()
    {
        List<FFXIVLog> list = new List<FFXIVLog>();
        list.AddRange(this.LogListFromFile);
        list.AddRange(this.LogListFromMemory);
        this.richTextBox1.Clear();
        foreach (FFXIVLog log in list)
        {
            string text = string.Format("{0}\r\n", this.tabjp.GetJapanese(log.ToString()));
            Font font = this.GetFont(log);
            this.richTextBox1.SelectionFont = font;
            this.richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            this.richTextBox1.SelectionColor = this.GetLogColor(log);
            this.richTextBox1.AppendText(text);
        }
        this.pictureBox1.Refresh();
    }

    private void 設定OToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (this.fontsettingfrm.ShowDialog() == DialogResult.OK)
        {
            this.RefreshLogs();
        }
    }

    private void 定型文辞書更新UToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string fixedFormSentenceLibUri = Settings.Default.FixedFormSentenceLibUri;
        UpdateFileForm form = new UpdateFileForm();
        form.uri = Settings.Default.FixedFormSentenceLibUri;
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            MemoryStream st = new MemoryStream(Encoding.GetEncoding("utf-8").GetBytes(form.xmltext));
            this.tabjp.ReadXml(st);
            this.RefreshLogs();
            File.WriteAllBytes(tabStrinLibXmlFile, st.ToArray());
        }
    }

    private void 閉じるXToolStripMenuItem_Click(object sender, EventArgs e)
    {
        base.Close();
    }

    // Properties
    private string FF14UserFolderPath
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Settings.Default.FF14UserFolder);
        }
    }

    public static string tabStrinLibXmlFile
    {
        get
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FixedFormSentence.xml");
        }
    }
}

}
