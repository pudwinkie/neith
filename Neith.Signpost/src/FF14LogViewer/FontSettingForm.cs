using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace FF14LogViewer
{
public class FontSettingForm : Form
{
    // Fields
    private Button BGColorButton;
    private Button CancelButton;
    private CheckBox checkEmoteBox;
    private CheckBox checkLINKSHELL1Box;
    private CheckBox checkLINKSHELL2Box;
    private CheckBox checkLINKSHELL3Box;
    private CheckBox checkLINKSHELL4Box;
    private CheckBox checkLINKSHELL5Box;
    private CheckBox checkLINKSHELL6Box;
    private CheckBox checkLINKSHELL7Box;
    private CheckBox checkLINKSHELL8Box;
    private CheckBox checkPARTYBox;
    private CheckBox checkSayBox;
    private CheckBox checkTELLBox;
    private CheckBox CheckTransparencyButton;
    private ColorDialog colorDialog1;
    private IContainer components;
    private Button EmoteColorBox;
    private TextBox EmoteFontBox;
    private FontDialog fontDialogEmote;
    private FontDialog fontDialogLINKSHELL1;
    private FontDialog fontDialogLINKSHELL2;
    private FontDialog fontDialogLINKSHELL3;
    private FontDialog fontDialogLINKSHELL4;
    private FontDialog fontDialogLINKSHELL5;
    private FontDialog fontDialogLINKSHELL6;
    private FontDialog fontDialogLINKSHELL7;
    private FontDialog fontDialogLINKSHELL8;
    private FontDialog fontDialogPARTY;
    private FontDialog fontDialogSay;
    private FontDialog fontDialogTELL;
    private Button FontEmoteButton;
    private Button FontLINKSHELL1Button;
    private Button FontLINKSHELL2Button;
    private Button FontLINKSHELL3Button;
    private Button FontLINKSHELL4Button;
    private Button FontLINKSHELL5Button;
    private Button FontLINKSHELL6Button;
    private Button FontLINKSHELL7Button;
    private Button FontLINKSHELL8Button;
    private Button FontPartyButton;
    private Button FontSayButton;
    private Button FontTellButton;
    private Label label1;
    private Button LINKSHELL1ColorBox;
    private TextBox LINKSHELL1FontBox;
    private Button LINKSHELL2ColorBox;
    private TextBox LINKSHELL2FontBox;
    private Button LINKSHELL3ColorBox;
    private TextBox LINKSHELL3FontBox;
    private Button LINKSHELL4ColorBox;
    private TextBox LINKSHELL4FontBox;
    private Button LINKSHELL5ColorBox;
    private TextBox LINKSHELL5FontBox;
    private Button LINKSHELL6ColorBox;
    private TextBox LINKSHELL6FontBox;
    private Button LINKSHELL7ColorBox;
    private TextBox LINKSHELL7FontBox;
    private Button LINKSHELL8ColorBox;
    private TextBox LINKSHELL8FontBox;
    private Button OKButton;
    private Button PARTYColorBox;
    private TextBox PARTYFontBox;
    private Button SayColorBox;
    private TextBox SayFontBox;
    private Button TELLColorBox;
    private TextBox TELLFontBox;

    // Methods
    public FontSettingForm()
    {
        this.InitializeComponent();
        this.fontDialogSay.Font = this.SayFontBox.Font;
        this.fontDialogSay.Color = this.SayColor;
        this.fontDialogLINKSHELL1.Font = this.LINKSHELL1FontBox.Font;
        this.fontDialogLINKSHELL1.Color = this.LINKSHELL1Color;
        this.fontDialogLINKSHELL2.Font = this.LINKSHELL2FontBox.Font;
        this.fontDialogLINKSHELL2.Color = this.LINKSHELL2Color;
        this.fontDialogLINKSHELL3.Font = this.LINKSHELL3FontBox.Font;
        this.fontDialogLINKSHELL3.Color = this.LINKSHELL3Color;
        this.fontDialogLINKSHELL4.Font = this.LINKSHELL4FontBox.Font;
        this.fontDialogLINKSHELL4.Color = this.LINKSHELL4Color;
        this.fontDialogLINKSHELL5.Font = this.LINKSHELL5FontBox.Font;
        this.fontDialogLINKSHELL5.Color = this.LINKSHELL5Color;
        this.fontDialogLINKSHELL6.Font = this.LINKSHELL6FontBox.Font;
        this.fontDialogLINKSHELL6.Color = this.LINKSHELL6Color;
        this.fontDialogLINKSHELL7.Font = this.LINKSHELL7FontBox.Font;
        this.fontDialogLINKSHELL7.Color = this.LINKSHELL7Color;
        this.fontDialogLINKSHELL8.Font = this.LINKSHELL8FontBox.Font;
        this.fontDialogLINKSHELL8.Color = this.LINKSHELL8Color;
        this.fontDialogPARTY.Font = this.PARTYFontBox.Font;
        this.fontDialogPARTY.Color = this.PARTYColor;
        this.fontDialogTELL.Font = this.TELLFontBox.Font;
        this.fontDialogTELL.Color = this.TELLColor;
        this.fontDialogEmote.Font = this.EmoteFontBox.Font;
        this.fontDialogEmote.Color = this.EmoteColor;
        this.LINKSHELL1ColorBox.BackColor = this.LINKSHELL1Color;
        this.LINKSHELL2ColorBox.BackColor = this.LINKSHELL2Color;
        this.LINKSHELL3ColorBox.BackColor = this.LINKSHELL3Color;
        this.LINKSHELL4ColorBox.BackColor = this.LINKSHELL4Color;
        this.LINKSHELL5ColorBox.BackColor = this.LINKSHELL5Color;
        this.LINKSHELL6ColorBox.BackColor = this.LINKSHELL6Color;
        this.LINKSHELL7ColorBox.BackColor = this.LINKSHELL7Color;
        this.LINKSHELL8ColorBox.BackColor = this.LINKSHELL8Color;
        this.PARTYColorBox.BackColor = this.PARTYColor;
        this.TELLColorBox.BackColor = this.TELLColor;
        this.EmoteColorBox.BackColor = this.EmoteColor;
        this.SayColorBox.BackColor = this.SayColor;
        this.IsTransparent = Settings.Default.IsTransparent;
        this.LINKSHELL1Enable = Settings.Default.LINKSHELL1;
        this.LINKSHELL2Enable = Settings.Default.LINKSHELL2;
        this.LINKSHELL3Enable = Settings.Default.LINKSHELL3;
        this.LINKSHELL4Enable = Settings.Default.LINKSHELL4;
        this.LINKSHELL5Enable = Settings.Default.LINKSHELL5;
        this.LINKSHELL6Enable = Settings.Default.LINKSHELL6;
        this.LINKSHELL7Enable = Settings.Default.LINKSHELL7;
        this.LINKSHELL8Enable = Settings.Default.LINKSHELL8;
        this.PARTYEnable = Settings.Default.PARTY;
        this.SayEnable = Settings.Default.SAY;
        this.TELLEnable = Settings.Default.TELL;
        this.EmoteEnable = Settings.Default.EMOTE;
    }

    private void BGColorButton_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.BGColor;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.BGColor = this.colorDialog1.Color;
        }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        base.DialogResult = DialogResult.Cancel;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (this.components != null))
        {
            this.components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void EmoteColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.EmoteColor;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.EmoteColor = this.colorDialog1.Color;
        }
    }

    private void EmoteFontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.EmoteColorBox.BackColor = this.EmoteFontBox.ForeColor;
    }

    private void FontEmoteButton_Click(object sender, EventArgs e)
    {
        this.fontDialogEmote.Color = this.EmoteColor;
        if (this.fontDialogEmote.ShowDialog() == DialogResult.OK)
        {
            this.EmoteColor = this.fontDialogEmote.Color;
            this.FontEmote = this.fontDialogEmote.Font;
        }
    }

    private void FontLINKSHELL1Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL1.Color = this.LINKSHELL1Color;
        if (this.fontDialogLINKSHELL1.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL1Color = this.fontDialogLINKSHELL1.Color;
            this.FontLINKSHELL1 = this.fontDialogLINKSHELL1.Font;
        }
    }

    private void FontLINKSHELL2Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL2.Color = this.LINKSHELL2Color;
        if (this.fontDialogLINKSHELL2.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL2Color = this.fontDialogLINKSHELL2.Color;
            this.FontLINKSHELL2 = this.fontDialogLINKSHELL2.Font;
        }
    }

    private void FontLINKSHELL3Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL3.Color = this.LINKSHELL3Color;
        if (this.fontDialogLINKSHELL3.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL3Color = this.fontDialogLINKSHELL3.Color;
            this.FontLINKSHELL3 = this.fontDialogLINKSHELL3.Font;
        }
    }

    private void FontLINKSHELL4Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL4.Color = this.LINKSHELL4Color;
        if (this.fontDialogLINKSHELL4.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL4Color = this.fontDialogLINKSHELL4.Color;
            this.FontLINKSHELL4 = this.fontDialogLINKSHELL4.Font;
        }
    }

    private void FontLINKSHELL5Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL5.Color = this.LINKSHELL5Color;
        if (this.fontDialogLINKSHELL5.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL5Color = this.fontDialogLINKSHELL5.Color;
            this.FontLINKSHELL5 = this.fontDialogLINKSHELL5.Font;
        }
    }

    private void FontLINKSHELL6Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL6.Color = this.LINKSHELL6Color;
        if (this.fontDialogLINKSHELL6.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL6Color = this.fontDialogLINKSHELL6.Color;
            this.FontLINKSHELL6 = this.fontDialogLINKSHELL6.Font;
        }
    }

    private void FontLINKSHELL7Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL7.Color = this.LINKSHELL7Color;
        if (this.fontDialogLINKSHELL7.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL7Color = this.fontDialogLINKSHELL7.Color;
            this.FontLINKSHELL7 = this.fontDialogLINKSHELL7.Font;
        }
    }

    private void FontLINKSHELL8Button_Click(object sender, EventArgs e)
    {
        this.fontDialogLINKSHELL8.Color = this.LINKSHELL8Color;
        if (this.fontDialogLINKSHELL8.ShowDialog() == DialogResult.OK)
        {
            this.LINKSHELL8Color = this.fontDialogLINKSHELL8.Color;
            this.FontLINKSHELL8 = this.fontDialogLINKSHELL8.Font;
        }
    }

    private void FontPartyButton_Click(object sender, EventArgs e)
    {
        this.fontDialogPARTY.Color = this.PARTYColor;
        if (this.fontDialogPARTY.ShowDialog() == DialogResult.OK)
        {
            this.PARTYColor = this.fontDialogPARTY.Color;
            this.FontPARTY = this.fontDialogPARTY.Font;
        }
    }

    private void FontSayButton_Click(object sender, EventArgs e)
    {
        if (this.fontDialogSay.ShowDialog() == DialogResult.OK)
        {
            this.SayColor = this.fontDialogSay.Color;
            this.FontSay = this.fontDialogSay.Font;
        }
    }

    private void FontSettingForm_Load(object sender, EventArgs e)
    {
    }

    private void FontTellButton_Click(object sender, EventArgs e)
    {
        this.fontDialogTELL.Color = this.TELLColor;
        if (this.fontDialogTELL.ShowDialog() == DialogResult.OK)
        {
            this.TELLColor = this.fontDialogTELL.Color;
            this.FontTELL = this.fontDialogTELL.Font;
        }
    }

    private void InitializeComponent()
    {
        ComponentResourceManager manager = new ComponentResourceManager(typeof(FontSettingForm));
        this.FontLINKSHELL1Button = new Button();
        this.fontDialogSay = new FontDialog();
        this.FontPartyButton = new Button();
        this.FontTellButton = new Button();
        this.FontSayButton = new Button();
        this.FontEmoteButton = new Button();
        this.OKButton = new Button();
        this.CancelButton = new Button();
        this.fontDialogLINKSHELL1 = new FontDialog();
        this.fontDialogPARTY = new FontDialog();
        this.fontDialogTELL = new FontDialog();
        this.fontDialogEmote = new FontDialog();
        this.LINKSHELL1ColorBox = new Button();
        this.PARTYColorBox = new Button();
        this.TELLColorBox = new Button();
        this.SayColorBox = new Button();
        this.EmoteColorBox = new Button();
        this.colorDialog1 = new ColorDialog();
        this.label1 = new Label();
        this.LINKSHELL2ColorBox = new Button();
        this.FontLINKSHELL2Button = new Button();
        this.LINKSHELL3ColorBox = new Button();
        this.FontLINKSHELL3Button = new Button();
        this.LINKSHELL4ColorBox = new Button();
        this.FontLINKSHELL4Button = new Button();
        this.LINKSHELL5ColorBox = new Button();
        this.FontLINKSHELL5Button = new Button();
        this.LINKSHELL6ColorBox = new Button();
        this.FontLINKSHELL6Button = new Button();
        this.LINKSHELL7ColorBox = new Button();
        this.FontLINKSHELL7Button = new Button();
        this.LINKSHELL8ColorBox = new Button();
        this.FontLINKSHELL8Button = new Button();
        this.fontDialogLINKSHELL2 = new FontDialog();
        this.fontDialogLINKSHELL3 = new FontDialog();
        this.fontDialogLINKSHELL4 = new FontDialog();
        this.fontDialogLINKSHELL5 = new FontDialog();
        this.fontDialogLINKSHELL6 = new FontDialog();
        this.fontDialogLINKSHELL7 = new FontDialog();
        this.fontDialogLINKSHELL8 = new FontDialog();
        this.LINKSHELL8FontBox = new TextBox();
        this.checkLINKSHELL8Box = new CheckBox();
        this.LINKSHELL7FontBox = new TextBox();
        this.checkLINKSHELL7Box = new CheckBox();
        this.LINKSHELL6FontBox = new TextBox();
        this.checkLINKSHELL6Box = new CheckBox();
        this.LINKSHELL5FontBox = new TextBox();
        this.checkLINKSHELL5Box = new CheckBox();
        this.LINKSHELL4FontBox = new TextBox();
        this.checkLINKSHELL4Box = new CheckBox();
        this.LINKSHELL3FontBox = new TextBox();
        this.checkLINKSHELL3Box = new CheckBox();
        this.LINKSHELL2FontBox = new TextBox();
        this.checkLINKSHELL2Box = new CheckBox();
        this.CheckTransparencyButton = new CheckBox();
        this.BGColorButton = new Button();
        this.EmoteFontBox = new TextBox();
        this.checkEmoteBox = new CheckBox();
        this.SayFontBox = new TextBox();
        this.checkSayBox = new CheckBox();
        this.TELLFontBox = new TextBox();
        this.checkTELLBox = new CheckBox();
        this.PARTYFontBox = new TextBox();
        this.checkPARTYBox = new CheckBox();
        this.LINKSHELL1FontBox = new TextBox();
        this.checkLINKSHELL1Box = new CheckBox();
        base.SuspendLayout();
        this.FontLINKSHELL1Button.Location = new Point(0xf5, 0x7c);
        this.FontLINKSHELL1Button.Name = "FontLINKSHELL1Button";
        this.FontLINKSHELL1Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL1Button.TabIndex = 3;
        this.FontLINKSHELL1Button.Text = "Font...";
        this.FontLINKSHELL1Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL1Button.Click += new EventHandler(this.FontLINKSHELL1Button_Click);
        this.fontDialogSay.Color = Color.White;
        this.fontDialogSay.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.FontPartyButton.Location = new Point(0xf5, 5);
        this.FontPartyButton.Name = "FontPartyButton";
        this.FontPartyButton.Size = new Size(0x4b, 0x17);
        this.FontPartyButton.TabIndex = 7;
        this.FontPartyButton.Text = "Font...";
        this.FontPartyButton.UseVisualStyleBackColor = true;
        this.FontPartyButton.Click += new EventHandler(this.FontPartyButton_Click);
        this.FontTellButton.Location = new Point(0xf5, 0x22);
        this.FontTellButton.Name = "FontTellButton";
        this.FontTellButton.Size = new Size(0x4b, 0x17);
        this.FontTellButton.TabIndex = 11;
        this.FontTellButton.Text = "Font...";
        this.FontTellButton.UseVisualStyleBackColor = true;
        this.FontTellButton.Click += new EventHandler(this.FontTellButton_Click);
        this.FontSayButton.Location = new Point(0xf5, 0x40);
        this.FontSayButton.Name = "FontSayButton";
        this.FontSayButton.Size = new Size(0x4b, 0x17);
        this.FontSayButton.TabIndex = 15;
        this.FontSayButton.Text = "Font...";
        this.FontSayButton.UseVisualStyleBackColor = true;
        this.FontSayButton.Click += new EventHandler(this.FontSayButton_Click);
        this.FontEmoteButton.Location = new Point(0xf5, 0x5e);
        this.FontEmoteButton.Name = "FontEmoteButton";
        this.FontEmoteButton.Size = new Size(0x4b, 0x17);
        this.FontEmoteButton.TabIndex = 0x13;
        this.FontEmoteButton.Text = "Font...";
        this.FontEmoteButton.UseVisualStyleBackColor = true;
        this.FontEmoteButton.Click += new EventHandler(this.FontEmoteButton_Click);
        this.OKButton.Location = new Point(0x61, 0x18b);
        this.OKButton.Name = "OKButton";
        this.OKButton.Size = new Size(0x4b, 0x17);
        this.OKButton.TabIndex = 20;
        this.OKButton.Text = "OK";
        this.OKButton.UseVisualStyleBackColor = true;
        this.OKButton.Click += new EventHandler(this.OKButton_Click);
        this.CancelButton.Location = new Point(0xb2, 0x18b);
        this.CancelButton.Name = "CancelButton";
        this.CancelButton.Size = new Size(0x4b, 0x17);
        this.CancelButton.TabIndex = 0x15;
        this.CancelButton.Text = "CANCEL";
        this.CancelButton.UseVisualStyleBackColor = true;
        this.CancelButton.Click += new EventHandler(this.CancelButton_Click);
        this.fontDialogLINKSHELL1.Color = Color.White;
        this.fontDialogLINKSHELL1.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogPARTY.Color = Color.White;
        this.fontDialogPARTY.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogTELL.Color = Color.White;
        this.fontDialogTELL.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogEmote.Color = Color.White;
        this.fontDialogEmote.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL1ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL1ColorBox.Location = new Point(0xd6, 0x7d);
        this.LINKSHELL1ColorBox.Name = "LINKSHELL1ColorBox";
        this.LINKSHELL1ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL1ColorBox.TabIndex = 2;
        this.LINKSHELL1ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL1ColorBox.Click += new EventHandler(this.LINKSHELL1ColorBox_Click);
        this.PARTYColorBox.FlatStyle = FlatStyle.Flat;
        this.PARTYColorBox.Location = new Point(0xd6, 5);
        this.PARTYColorBox.Name = "PARTYColorBox";
        this.PARTYColorBox.Size = new Size(0x19, 0x17);
        this.PARTYColorBox.TabIndex = 6;
        this.PARTYColorBox.UseVisualStyleBackColor = true;
        this.PARTYColorBox.Click += new EventHandler(this.PARTYColorBox_Click);
        this.TELLColorBox.FlatStyle = FlatStyle.Flat;
        this.TELLColorBox.Location = new Point(0xd6, 0x23);
        this.TELLColorBox.Name = "TELLColorBox";
        this.TELLColorBox.Size = new Size(0x19, 0x17);
        this.TELLColorBox.TabIndex = 10;
        this.TELLColorBox.UseVisualStyleBackColor = true;
        this.TELLColorBox.Click += new EventHandler(this.TELLColorBox_Click);
        this.SayColorBox.FlatStyle = FlatStyle.Flat;
        this.SayColorBox.Location = new Point(0xd6, 0x41);
        this.SayColorBox.Name = "SayColorBox";
        this.SayColorBox.Size = new Size(0x19, 0x17);
        this.SayColorBox.TabIndex = 14;
        this.SayColorBox.UseVisualStyleBackColor = true;
        this.SayColorBox.Click += new EventHandler(this.SayColorBox_Click);
        this.EmoteColorBox.FlatStyle = FlatStyle.Flat;
        this.EmoteColorBox.Location = new Point(0xd6, 0x5f);
        this.EmoteColorBox.Name = "EmoteColorBox";
        this.EmoteColorBox.Size = new Size(0x19, 0x17);
        this.EmoteColorBox.TabIndex = 0x12;
        this.EmoteColorBox.UseVisualStyleBackColor = true;
        this.EmoteColorBox.Click += new EventHandler(this.EmoteColorBox_Click);
        this.label1.AutoSize = true;
        this.label1.Location = new Point(0xa7, 0x170);
        this.label1.Name = "label1";
        this.label1.Size = new Size(0x29, 12);
        this.label1.TabIndex = 0x18;
        this.label1.Text = "背景色";
        this.LINKSHELL2ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL2ColorBox.Location = new Point(0xd6, 0x9b);
        this.LINKSHELL2ColorBox.Name = "LINKSHELL2ColorBox";
        this.LINKSHELL2ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL2ColorBox.TabIndex = 0x1b;
        this.LINKSHELL2ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL2ColorBox.Click += new EventHandler(this.LINKSHELL2ColorBox_Click);
        this.FontLINKSHELL2Button.Location = new Point(0xf5, 0x9a);
        this.FontLINKSHELL2Button.Name = "FontLINKSHELL2Button";
        this.FontLINKSHELL2Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL2Button.TabIndex = 0x1c;
        this.FontLINKSHELL2Button.Text = "Font...";
        this.FontLINKSHELL2Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL2Button.Click += new EventHandler(this.FontLINKSHELL2Button_Click);
        this.LINKSHELL3ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL3ColorBox.Location = new Point(0xd6, 0xb9);
        this.LINKSHELL3ColorBox.Name = "LINKSHELL3ColorBox";
        this.LINKSHELL3ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL3ColorBox.TabIndex = 0x1f;
        this.LINKSHELL3ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL3ColorBox.Click += new EventHandler(this.LINKSHELL3ColorBox_Click);
        this.FontLINKSHELL3Button.Location = new Point(0xf5, 0xb8);
        this.FontLINKSHELL3Button.Name = "FontLINKSHELL3Button";
        this.FontLINKSHELL3Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL3Button.TabIndex = 0x20;
        this.FontLINKSHELL3Button.Text = "Font...";
        this.FontLINKSHELL3Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL3Button.Click += new EventHandler(this.FontLINKSHELL3Button_Click);
        this.LINKSHELL4ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL4ColorBox.Location = new Point(0xd6, 0xd7);
        this.LINKSHELL4ColorBox.Name = "LINKSHELL4ColorBox";
        this.LINKSHELL4ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL4ColorBox.TabIndex = 0x23;
        this.LINKSHELL4ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL4ColorBox.Click += new EventHandler(this.LINKSHELL4ColorBox_Click);
        this.FontLINKSHELL4Button.Location = new Point(0xf5, 0xd6);
        this.FontLINKSHELL4Button.Name = "FontLINKSHELL4Button";
        this.FontLINKSHELL4Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL4Button.TabIndex = 0x24;
        this.FontLINKSHELL4Button.Text = "Font...";
        this.FontLINKSHELL4Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL4Button.Click += new EventHandler(this.FontLINKSHELL4Button_Click);
        this.LINKSHELL5ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL5ColorBox.Location = new Point(0xd6, 0xf5);
        this.LINKSHELL5ColorBox.Name = "LINKSHELL5ColorBox";
        this.LINKSHELL5ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL5ColorBox.TabIndex = 0x27;
        this.LINKSHELL5ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL5ColorBox.Click += new EventHandler(this.LINKSHELL5ColorBox_Click);
        this.FontLINKSHELL5Button.Location = new Point(0xf5, 0xf4);
        this.FontLINKSHELL5Button.Name = "FontLINKSHELL5Button";
        this.FontLINKSHELL5Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL5Button.TabIndex = 40;
        this.FontLINKSHELL5Button.Text = "Font...";
        this.FontLINKSHELL5Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL5Button.Click += new EventHandler(this.FontLINKSHELL5Button_Click);
        this.LINKSHELL6ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL6ColorBox.Location = new Point(0xd6, 0x113);
        this.LINKSHELL6ColorBox.Name = "LINKSHELL6ColorBox";
        this.LINKSHELL6ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL6ColorBox.TabIndex = 0x2b;
        this.LINKSHELL6ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL6ColorBox.Click += new EventHandler(this.LINKSHELL6ColorBox_Click);
        this.FontLINKSHELL6Button.Location = new Point(0xf5, 0x112);
        this.FontLINKSHELL6Button.Name = "FontLINKSHELL6Button";
        this.FontLINKSHELL6Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL6Button.TabIndex = 0x2c;
        this.FontLINKSHELL6Button.Text = "Font...";
        this.FontLINKSHELL6Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL6Button.Click += new EventHandler(this.FontLINKSHELL6Button_Click);
        this.LINKSHELL7ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL7ColorBox.Location = new Point(0xd6, 0x131);
        this.LINKSHELL7ColorBox.Name = "LINKSHELL7ColorBox";
        this.LINKSHELL7ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL7ColorBox.TabIndex = 0x2f;
        this.LINKSHELL7ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL7ColorBox.Click += new EventHandler(this.LINKSHELL7ColorBox_Click);
        this.FontLINKSHELL7Button.Location = new Point(0xf5, 0x130);
        this.FontLINKSHELL7Button.Name = "FontLINKSHELL7Button";
        this.FontLINKSHELL7Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL7Button.TabIndex = 0x30;
        this.FontLINKSHELL7Button.Text = "Font...";
        this.FontLINKSHELL7Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL7Button.Click += new EventHandler(this.FontLINKSHELL7Button_Click);
        this.LINKSHELL8ColorBox.FlatStyle = FlatStyle.Flat;
        this.LINKSHELL8ColorBox.Location = new Point(0xd6, 0x14e);
        this.LINKSHELL8ColorBox.Name = "LINKSHELL8ColorBox";
        this.LINKSHELL8ColorBox.Size = new Size(0x19, 0x17);
        this.LINKSHELL8ColorBox.TabIndex = 0x34;
        this.LINKSHELL8ColorBox.UseVisualStyleBackColor = true;
        this.LINKSHELL8ColorBox.Click += new EventHandler(this.LINKSHELL8ColorBox_Click);
        this.FontLINKSHELL8Button.Location = new Point(0xf5, 0x14e);
        this.FontLINKSHELL8Button.Name = "FontLINKSHELL8Button";
        this.FontLINKSHELL8Button.Size = new Size(0x4b, 0x17);
        this.FontLINKSHELL8Button.TabIndex = 0x35;
        this.FontLINKSHELL8Button.Text = "Font...";
        this.FontLINKSHELL8Button.UseVisualStyleBackColor = true;
        this.FontLINKSHELL8Button.Click += new EventHandler(this.FontLINKSHELL8Button_Click);
        this.fontDialogLINKSHELL2.Color = Color.White;
        this.fontDialogLINKSHELL2.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogLINKSHELL3.Color = Color.White;
        this.fontDialogLINKSHELL3.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogLINKSHELL4.Color = Color.White;
        this.fontDialogLINKSHELL4.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogLINKSHELL5.Color = Color.White;
        this.fontDialogLINKSHELL5.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogLINKSHELL6.Color = Color.White;
        this.fontDialogLINKSHELL6.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogLINKSHELL7.Color = Color.White;
        this.fontDialogLINKSHELL7.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.fontDialogLINKSHELL8.Color = Color.White;
        this.fontDialogLINKSHELL8.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL8FontBox.BackColor = Color.Black;
        this.LINKSHELL8FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL8_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL8FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL8_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL8FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL8_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL8FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL8FontBox.ForeColor = Settings.Default.LINKSHELL8_COLOR;
        this.LINKSHELL8FontBox.Location = new Point(0x6c, 0x14b);
        this.LINKSHELL8FontBox.Name = "LINKSHELL8FontBox";
        this.LINKSHELL8FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL8FontBox.TabIndex = 0x33;
        this.LINKSHELL8FontBox.Text = Settings.Default.LINKSHELL8_FONTNAME;
        this.LINKSHELL8FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL8FontBox_ForeColorChanged);
        this.checkLINKSHELL8Box.AutoSize = true;
        this.checkLINKSHELL8Box.Checked = Settings.Default.LINKSHELL8;
        this.checkLINKSHELL8Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL8Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL8", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL8Box.Location = new Point(20, 0x153);
        this.checkLINKSHELL8Box.Name = "checkLINKSHELL8Box";
        this.checkLINKSHELL8Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL8Box.TabIndex = 50;
        this.checkLINKSHELL8Box.Text = "LINKSHELL8";
        this.checkLINKSHELL8Box.UseVisualStyleBackColor = true;
        this.LINKSHELL7FontBox.BackColor = Color.Black;
        this.LINKSHELL7FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL7_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL7FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL7_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL7FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL7_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL7FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL7FontBox.ForeColor = Settings.Default.LINKSHELL7_COLOR;
        this.LINKSHELL7FontBox.Location = new Point(0x6c, 300);
        this.LINKSHELL7FontBox.Name = "LINKSHELL7FontBox";
        this.LINKSHELL7FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL7FontBox.TabIndex = 0x2e;
        this.LINKSHELL7FontBox.Text = Settings.Default.LINKSHELL7_FONTNAME;
        this.LINKSHELL7FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL7FontBox_ForeColorChanged);
        this.checkLINKSHELL7Box.AutoSize = true;
        this.checkLINKSHELL7Box.Checked = Settings.Default.LINKSHELL7;
        this.checkLINKSHELL7Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL7Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL7", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL7Box.Location = new Point(20, 0x135);
        this.checkLINKSHELL7Box.Name = "checkLINKSHELL7Box";
        this.checkLINKSHELL7Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL7Box.TabIndex = 0x2d;
        this.checkLINKSHELL7Box.Text = "LINKSHELL7";
        this.checkLINKSHELL7Box.UseVisualStyleBackColor = true;
        this.LINKSHELL6FontBox.BackColor = Color.Black;
        this.LINKSHELL6FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL6_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL6FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL6_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL6FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL6_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL6FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL6FontBox.ForeColor = Settings.Default.LINKSHELL6_COLOR;
        this.LINKSHELL6FontBox.Location = new Point(0x6c, 270);
        this.LINKSHELL6FontBox.Name = "LINKSHELL6FontBox";
        this.LINKSHELL6FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL6FontBox.TabIndex = 0x2a;
        this.LINKSHELL6FontBox.Text = Settings.Default.LINKSHELL6_FONTNAME;
        this.LINKSHELL6FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL6FontBox_ForeColorChanged);
        this.checkLINKSHELL6Box.AutoSize = true;
        this.checkLINKSHELL6Box.Checked = Settings.Default.LINKSHELL6;
        this.checkLINKSHELL6Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL6Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL6", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL6Box.Location = new Point(20, 0x117);
        this.checkLINKSHELL6Box.Name = "checkLINKSHELL6Box";
        this.checkLINKSHELL6Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL6Box.TabIndex = 0x29;
        this.checkLINKSHELL6Box.Text = "LINKSHELL6";
        this.checkLINKSHELL6Box.UseVisualStyleBackColor = true;
        this.LINKSHELL5FontBox.BackColor = Color.Black;
        this.LINKSHELL5FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL5_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL5FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL5_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL5FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL5_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL5FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL5FontBox.ForeColor = Settings.Default.LINKSHELL5_COLOR;
        this.LINKSHELL5FontBox.Location = new Point(0x6c, 240);
        this.LINKSHELL5FontBox.Name = "LINKSHELL5FontBox";
        this.LINKSHELL5FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL5FontBox.TabIndex = 0x26;
        this.LINKSHELL5FontBox.Text = Settings.Default.LINKSHELL5_FONTNAME;
        this.LINKSHELL5FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL5FontBox_ForeColorChanged);
        this.checkLINKSHELL5Box.AutoSize = true;
        this.checkLINKSHELL5Box.Checked = Settings.Default.LINKSHELL5;
        this.checkLINKSHELL5Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL5Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL5", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL5Box.Location = new Point(20, 0xf9);
        this.checkLINKSHELL5Box.Name = "checkLINKSHELL5Box";
        this.checkLINKSHELL5Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL5Box.TabIndex = 0x25;
        this.checkLINKSHELL5Box.Text = "LINKSHELL5";
        this.checkLINKSHELL5Box.UseVisualStyleBackColor = true;
        this.LINKSHELL4FontBox.BackColor = Color.Black;
        this.LINKSHELL4FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL4_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL4FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL4_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL4FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL4_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL4FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL4FontBox.ForeColor = Settings.Default.LINKSHELL4_COLOR;
        this.LINKSHELL4FontBox.Location = new Point(0x6c, 210);
        this.LINKSHELL4FontBox.Name = "LINKSHELL4FontBox";
        this.LINKSHELL4FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL4FontBox.TabIndex = 0x22;
        this.LINKSHELL4FontBox.Text = Settings.Default.LINKSHELL4_FONTNAME;
        this.LINKSHELL4FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL4FontBox_ForeColorChanged);
        this.checkLINKSHELL4Box.AutoSize = true;
        this.checkLINKSHELL4Box.Checked = Settings.Default.LINKSHELL4;
        this.checkLINKSHELL4Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL4Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL4", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL4Box.Location = new Point(20, 0xdb);
        this.checkLINKSHELL4Box.Name = "checkLINKSHELL4Box";
        this.checkLINKSHELL4Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL4Box.TabIndex = 0x21;
        this.checkLINKSHELL4Box.Text = "LINKSHELL4";
        this.checkLINKSHELL4Box.UseVisualStyleBackColor = true;
        this.LINKSHELL3FontBox.BackColor = Color.Black;
        this.LINKSHELL3FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL3_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL3FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL3_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL3FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL3_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL3FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL3FontBox.ForeColor = Settings.Default.LINKSHELL3_COLOR;
        this.LINKSHELL3FontBox.Location = new Point(0x6c, 180);
        this.LINKSHELL3FontBox.Name = "LINKSHELL3FontBox";
        this.LINKSHELL3FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL3FontBox.TabIndex = 30;
        this.LINKSHELL3FontBox.Text = Settings.Default.LINKSHELL3_FONTNAME;
        this.LINKSHELL3FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL3FontBox_ForeColorChanged);
        this.checkLINKSHELL3Box.AutoSize = true;
        this.checkLINKSHELL3Box.Checked = Settings.Default.LINKSHELL3;
        this.checkLINKSHELL3Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL3Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL3", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL3Box.Location = new Point(20, 0xbd);
        this.checkLINKSHELL3Box.Name = "checkLINKSHELL3Box";
        this.checkLINKSHELL3Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL3Box.TabIndex = 0x1d;
        this.checkLINKSHELL3Box.Text = "LINKSHELL3";
        this.checkLINKSHELL3Box.UseVisualStyleBackColor = true;
        this.LINKSHELL2FontBox.BackColor = Color.Black;
        this.LINKSHELL2FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL2_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL2FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL2_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL2FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL2_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL2FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold);
        this.LINKSHELL2FontBox.ForeColor = Settings.Default.LINKSHELL2_COLOR;
        this.LINKSHELL2FontBox.Location = new Point(0x6c, 150);
        this.LINKSHELL2FontBox.Name = "LINKSHELL2FontBox";
        this.LINKSHELL2FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL2FontBox.TabIndex = 0x1a;
        this.LINKSHELL2FontBox.Text = Settings.Default.LINKSHELL2_FONTNAME;
        this.LINKSHELL2FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL2FontBox_ForeColorChanged);
        this.checkLINKSHELL2Box.AutoSize = true;
        this.checkLINKSHELL2Box.Checked = Settings.Default.LINKSHELL2;
        this.checkLINKSHELL2Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL2Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL2", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL2Box.Location = new Point(20, 0x9f);
        this.checkLINKSHELL2Box.Name = "checkLINKSHELL2Box";
        this.checkLINKSHELL2Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL2Box.TabIndex = 0x19;
        this.checkLINKSHELL2Box.Text = "LINKSHELL2";
        this.checkLINKSHELL2Box.UseVisualStyleBackColor = true;
        this.CheckTransparencyButton.AutoSize = true;
        this.CheckTransparencyButton.Checked = Settings.Default.IsTransparent;
        this.CheckTransparencyButton.CheckState = CheckState.Checked;
        this.CheckTransparencyButton.DataBindings.Add(new Binding("Checked", Settings.Default, "IsTransparent", true, DataSourceUpdateMode.OnPropertyChanged));
        this.CheckTransparencyButton.Location = new Point(20, 0x16c);
        this.CheckTransparencyButton.Name = "CheckTransparencyButton";
        this.CheckTransparencyButton.Size = new Size(0x30, 0x10);
        this.CheckTransparencyButton.TabIndex = 0x17;
        this.CheckTransparencyButton.Text = "透明";
        this.CheckTransparencyButton.UseVisualStyleBackColor = true;
        this.BGColorButton.BackColor = Settings.Default.BGColor;
        this.BGColorButton.DataBindings.Add(new Binding("BackColor", Settings.Default, "BGColor", true, DataSourceUpdateMode.OnPropertyChanged));
        this.BGColorButton.FlatStyle = FlatStyle.Flat;
        this.BGColorButton.Location = new Point(0xd6, 0x16b);
        this.BGColorButton.Name = "BGColorButton";
        this.BGColorButton.Size = new Size(0x19, 0x17);
        this.BGColorButton.TabIndex = 0x16;
        this.BGColorButton.UseVisualStyleBackColor = false;
        this.BGColorButton.Click += new EventHandler(this.BGColorButton_Click);
        this.EmoteFontBox.BackColor = Color.Black;
        this.EmoteFontBox.DataBindings.Add(new Binding("Text", Settings.Default, "EMOTE_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.EmoteFontBox.DataBindings.Add(new Binding("Font", Settings.Default, "EMOTE_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.EmoteFontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "EMOTE_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.EmoteFontBox.Font = Settings.Default.EMOTE_FONT;
        this.EmoteFontBox.ForeColor = Settings.Default.EMOTE_COLOR;
        this.EmoteFontBox.Location = new Point(0x6c, 90);
        this.EmoteFontBox.Name = "EmoteFontBox";
        this.EmoteFontBox.Size = new Size(100, 0x1b);
        this.EmoteFontBox.TabIndex = 0x11;
        this.EmoteFontBox.Text = Settings.Default.EMOTE_FONTNAME;
        this.EmoteFontBox.ForeColorChanged += new EventHandler(this.EmoteFontBox_ForeColorChanged);
        this.checkEmoteBox.AutoSize = true;
        this.checkEmoteBox.Checked = Settings.Default.EMOTE;
        this.checkEmoteBox.CheckState = CheckState.Checked;
        this.checkEmoteBox.DataBindings.Add(new Binding("Checked", Settings.Default, "EMOTE", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkEmoteBox.Location = new Point(20, 0x63);
        this.checkEmoteBox.Name = "checkEmoteBox";
        this.checkEmoteBox.Size = new Size(0x38, 0x10);
        this.checkEmoteBox.TabIndex = 0x10;
        this.checkEmoteBox.Text = "Emote";
        this.checkEmoteBox.UseVisualStyleBackColor = true;
        this.SayFontBox.BackColor = Color.Black;
        this.SayFontBox.DataBindings.Add(new Binding("Text", Settings.Default, "SAY_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.SayFontBox.DataBindings.Add(new Binding("Font", Settings.Default, "SAY_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.SayFontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "SAY_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.SayFontBox.Font = Settings.Default.SAY_FONT;
        this.SayFontBox.ForeColor = Settings.Default.SAY_COLOR;
        this.SayFontBox.Location = new Point(0x6c, 60);
        this.SayFontBox.Name = "SayFontBox";
        this.SayFontBox.Size = new Size(100, 0x1b);
        this.SayFontBox.TabIndex = 13;
        this.SayFontBox.Text = Settings.Default.SAY_FONTNAME;
        this.SayFontBox.ForeColorChanged += new EventHandler(this.SayFontBox_ForeColorChanged);
        this.checkSayBox.AutoSize = true;
        this.checkSayBox.Checked = Settings.Default.SAY;
        this.checkSayBox.CheckState = CheckState.Checked;
        this.checkSayBox.DataBindings.Add(new Binding("Checked", Settings.Default, "SAY", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkSayBox.Location = new Point(20, 0x45);
        this.checkSayBox.Name = "checkSayBox";
        this.checkSayBox.Size = new Size(0x2b, 0x10);
        this.checkSayBox.TabIndex = 12;
        this.checkSayBox.Text = "Say";
        this.checkSayBox.UseVisualStyleBackColor = true;
        this.TELLFontBox.BackColor = Color.Black;
        this.TELLFontBox.DataBindings.Add(new Binding("Text", Settings.Default, "TELL_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.TELLFontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "TELL_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.TELLFontBox.DataBindings.Add(new Binding("Font", Settings.Default, "TELL_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.TELLFontBox.Font = Settings.Default.TELL_FONT;
        this.TELLFontBox.ForeColor = Settings.Default.TELL_COLOR;
        this.TELLFontBox.Location = new Point(0x6c, 30);
        this.TELLFontBox.Name = "TELLFontBox";
        this.TELLFontBox.Size = new Size(100, 0x1b);
        this.TELLFontBox.TabIndex = 9;
        this.TELLFontBox.Text = Settings.Default.TELL_FONTNAME;
        this.TELLFontBox.ForeColorChanged += new EventHandler(this.TELLFontBox_ForeColorChanged);
        this.checkTELLBox.AutoSize = true;
        this.checkTELLBox.Checked = Settings.Default.TELL;
        this.checkTELLBox.CheckState = CheckState.Checked;
        this.checkTELLBox.DataBindings.Add(new Binding("Checked", Settings.Default, "TELL", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkTELLBox.Location = new Point(20, 0x27);
        this.checkTELLBox.Name = "checkTELLBox";
        this.checkTELLBox.Size = new Size(50, 0x10);
        this.checkTELLBox.TabIndex = 8;
        this.checkTELLBox.Text = "TELL";
        this.checkTELLBox.UseVisualStyleBackColor = true;
        this.PARTYFontBox.BackColor = Color.Black;
        this.PARTYFontBox.DataBindings.Add(new Binding("Text", Settings.Default, "PARTY_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.PARTYFontBox.DataBindings.Add(new Binding("Font", Settings.Default, "PARTY_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.PARTYFontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "PARTY_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.PARTYFontBox.Font = Settings.Default.PARTY_FONT;
        this.PARTYFontBox.ForeColor = Settings.Default.PARTY_COLOR;
        this.PARTYFontBox.Location = new Point(0x6c, 1);
        this.PARTYFontBox.Name = "PARTYFontBox";
        this.PARTYFontBox.Size = new Size(100, 0x1b);
        this.PARTYFontBox.TabIndex = 5;
        this.PARTYFontBox.Text = Settings.Default.PARTY_FONTNAME;
        this.PARTYFontBox.ForeColorChanged += new EventHandler(this.PARTYFontBox_ForeColorChanged);
        this.checkPARTYBox.AutoSize = true;
        this.checkPARTYBox.Checked = Settings.Default.PARTY;
        this.checkPARTYBox.CheckState = CheckState.Checked;
        this.checkPARTYBox.DataBindings.Add(new Binding("Checked", Settings.Default, "PARTY", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkPARTYBox.Location = new Point(20, 9);
        this.checkPARTYBox.Name = "checkPARTYBox";
        this.checkPARTYBox.Size = new Size(0x3d, 0x10);
        this.checkPARTYBox.TabIndex = 4;
        this.checkPARTYBox.Text = "PARTY";
        this.checkPARTYBox.UseVisualStyleBackColor = true;
        this.LINKSHELL1FontBox.BackColor = Color.Black;
        this.LINKSHELL1FontBox.DataBindings.Add(new Binding("Font", Settings.Default, "LINKSHELL1_FONT", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL1FontBox.DataBindings.Add(new Binding("ForeColor", Settings.Default, "LINKSHELL1_COLOR", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL1FontBox.DataBindings.Add(new Binding("Text", Settings.Default, "LINKSHELL1_FONTNAME", true, DataSourceUpdateMode.OnPropertyChanged));
        this.LINKSHELL1FontBox.Font = new Font("Meiryo UI", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0x80);
        this.LINKSHELL1FontBox.ForeColor = Settings.Default.LINKSHELL1_COLOR;
        this.LINKSHELL1FontBox.Location = new Point(0x6c, 120);
        this.LINKSHELL1FontBox.Name = "LINKSHELL1FontBox";
        this.LINKSHELL1FontBox.Size = new Size(100, 0x1b);
        this.LINKSHELL1FontBox.TabIndex = 1;
        this.LINKSHELL1FontBox.Text = Settings.Default.LINKSHELL1_FONTNAME;
        this.LINKSHELL1FontBox.ForeColorChanged += new EventHandler(this.LINKSHELL1FontBox_ForeColorChanged);
        this.checkLINKSHELL1Box.AutoSize = true;
        this.checkLINKSHELL1Box.Checked = Settings.Default.LINKSHELL1;
        this.checkLINKSHELL1Box.CheckState = CheckState.Checked;
        this.checkLINKSHELL1Box.DataBindings.Add(new Binding("Checked", Settings.Default, "LINKSHELL1", true, DataSourceUpdateMode.OnPropertyChanged));
        this.checkLINKSHELL1Box.Location = new Point(20, 0x81);
        this.checkLINKSHELL1Box.Name = "checkLINKSHELL1Box";
        this.checkLINKSHELL1Box.Size = new Size(0x58, 0x10);
        this.checkLINKSHELL1Box.TabIndex = 0;
        this.checkLINKSHELL1Box.Text = "LINKSHELL1";
        this.checkLINKSHELL1Box.UseVisualStyleBackColor = true;
        base.AutoScaleDimensions = new SizeF(6f, 12f);
        base.AutoScaleMode = AutoScaleMode.Font;
        base.ClientSize = new Size(0x159, 0x1a9);
        base.Controls.Add(this.LINKSHELL8ColorBox);
        base.Controls.Add(this.LINKSHELL8FontBox);
        base.Controls.Add(this.FontLINKSHELL8Button);
        base.Controls.Add(this.checkLINKSHELL8Box);
        base.Controls.Add(this.LINKSHELL7ColorBox);
        base.Controls.Add(this.LINKSHELL7FontBox);
        base.Controls.Add(this.FontLINKSHELL7Button);
        base.Controls.Add(this.checkLINKSHELL7Box);
        base.Controls.Add(this.LINKSHELL6ColorBox);
        base.Controls.Add(this.LINKSHELL6FontBox);
        base.Controls.Add(this.FontLINKSHELL6Button);
        base.Controls.Add(this.checkLINKSHELL6Box);
        base.Controls.Add(this.LINKSHELL5ColorBox);
        base.Controls.Add(this.LINKSHELL5FontBox);
        base.Controls.Add(this.FontLINKSHELL5Button);
        base.Controls.Add(this.checkLINKSHELL5Box);
        base.Controls.Add(this.LINKSHELL4ColorBox);
        base.Controls.Add(this.LINKSHELL4FontBox);
        base.Controls.Add(this.FontLINKSHELL4Button);
        base.Controls.Add(this.checkLINKSHELL4Box);
        base.Controls.Add(this.LINKSHELL3ColorBox);
        base.Controls.Add(this.LINKSHELL3FontBox);
        base.Controls.Add(this.FontLINKSHELL3Button);
        base.Controls.Add(this.checkLINKSHELL3Box);
        base.Controls.Add(this.LINKSHELL2ColorBox);
        base.Controls.Add(this.LINKSHELL2FontBox);
        base.Controls.Add(this.FontLINKSHELL2Button);
        base.Controls.Add(this.checkLINKSHELL2Box);
        base.Controls.Add(this.label1);
        base.Controls.Add(this.CheckTransparencyButton);
        base.Controls.Add(this.BGColorButton);
        base.Controls.Add(this.EmoteColorBox);
        base.Controls.Add(this.SayColorBox);
        base.Controls.Add(this.TELLColorBox);
        base.Controls.Add(this.PARTYColorBox);
        base.Controls.Add(this.LINKSHELL1ColorBox);
        base.Controls.Add(this.CancelButton);
        base.Controls.Add(this.OKButton);
        base.Controls.Add(this.EmoteFontBox);
        base.Controls.Add(this.FontEmoteButton);
        base.Controls.Add(this.checkEmoteBox);
        base.Controls.Add(this.SayFontBox);
        base.Controls.Add(this.FontSayButton);
        base.Controls.Add(this.checkSayBox);
        base.Controls.Add(this.TELLFontBox);
        base.Controls.Add(this.FontTellButton);
        base.Controls.Add(this.checkTELLBox);
        base.Controls.Add(this.PARTYFontBox);
        base.Controls.Add(this.FontPartyButton);
        base.Controls.Add(this.checkPARTYBox);
        base.Controls.Add(this.LINKSHELL1FontBox);
        base.Controls.Add(this.FontLINKSHELL1Button);
        base.Controls.Add(this.checkLINKSHELL1Box);
        base.FormBorderStyle = FormBorderStyle.Fixed3D;
        base.Icon = (Icon) manager.GetObject("$this.Icon");
        base.Name = "FontSettingForm";
        this.Text = "FontSettingForm";
        base.Load += new EventHandler(this.FontSettingForm_Load);
        base.ResumeLayout(false);
        base.PerformLayout();
    }

    private void LINKSHELL1ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL1Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL1Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL1FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL1ColorBox.BackColor = this.LINKSHELL1FontBox.ForeColor;
    }

    private void LINKSHELL2ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL2Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL2Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL2FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL2ColorBox.BackColor = this.LINKSHELL2FontBox.ForeColor;
    }

    private void LINKSHELL3ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL3Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL3Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL3FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL3ColorBox.BackColor = this.LINKSHELL3FontBox.ForeColor;
    }

    private void LINKSHELL4ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL4Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL4Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL4FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL4ColorBox.BackColor = this.LINKSHELL4FontBox.ForeColor;
    }

    private void LINKSHELL5ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL5Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL5Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL5FontBox_ForeColorChanged(object sender, EventArgs e)
    {
    }

    private void LINKSHELL6ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL6Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL6Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL6FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL6ColorBox.BackColor = this.LINKSHELL6FontBox.ForeColor;
    }

    private void LINKSHELL7ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL7Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL7Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL7FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL7ColorBox.BackColor = this.LINKSHELL7FontBox.ForeColor;
    }

    private void LINKSHELL8ColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.LINKSHELL8Color;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.LINKSHELL8Color = this.colorDialog1.Color;
        }
    }

    private void LINKSHELL8FontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.LINKSHELL8ColorBox.BackColor = this.LINKSHELL8FontBox.ForeColor;
    }

    private void OKButton_Click(object sender, EventArgs e)
    {
        Settings.Default.Save();
        base.DialogResult = DialogResult.OK;
    }

    private void PARTYColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.PARTYColor;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.PARTYColor = this.colorDialog1.Color;
        }
    }

    private void PARTYFontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.PARTYColorBox.BackColor = this.PARTYFontBox.ForeColor;
    }

    private void SayColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.SayColor;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.SayColor = this.colorDialog1.Color;
        }
    }

    private void SayFontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.SayColorBox.BackColor = this.SayFontBox.ForeColor;
    }

    private void TELLColorBox_Click(object sender, EventArgs e)
    {
        this.colorDialog1.Color = this.TELLColor;
        if (this.colorDialog1.ShowDialog(this) == DialogResult.OK)
        {
            this.TELLColor = this.colorDialog1.Color;
        }
    }

    private void TELLFontBox_ForeColorChanged(object sender, EventArgs e)
    {
        this.TELLColorBox.BackColor = this.TELLFontBox.ForeColor;
    }

    // Properties
    public Color BGColor
    {
        get
        {
            return this.BGColorButton.BackColor;
        }
        set
        {
            this.BGColorButton.BackColor = value;
        }
    }

    public Color EmoteColor
    {
        get
        {
            return this.EmoteFontBox.ForeColor;
        }
        set
        {
            this.EmoteFontBox.ForeColor = value;
        }
    }

    public bool EmoteEnable
    {
        get
        {
            return this.checkEmoteBox.Checked;
        }
        set
        {
            this.checkEmoteBox.Checked = value;
        }
    }

    public Font FontEmote
    {
        get
        {
            return this.EmoteFontBox.Font;
        }
        set
        {
            this.EmoteFontBox.Font = value;
            this.EmoteFontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL1
    {
        get
        {
            return this.LINKSHELL1FontBox.Font;
        }
        set
        {
            this.LINKSHELL1FontBox.Font = value;
            this.LINKSHELL1FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL2
    {
        get
        {
            return this.LINKSHELL2FontBox.Font;
        }
        set
        {
            this.LINKSHELL2FontBox.Font = value;
            this.LINKSHELL2FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL3
    {
        get
        {
            return this.LINKSHELL3FontBox.Font;
        }
        set
        {
            this.LINKSHELL3FontBox.Font = value;
            this.LINKSHELL3FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL4
    {
        get
        {
            return this.LINKSHELL4FontBox.Font;
        }
        set
        {
            this.LINKSHELL4FontBox.Font = value;
            this.LINKSHELL4FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL5
    {
        get
        {
            return this.LINKSHELL5FontBox.Font;
        }
        set
        {
            this.LINKSHELL5FontBox.Font = value;
            this.LINKSHELL5FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL6
    {
        get
        {
            return this.LINKSHELL6FontBox.Font;
        }
        set
        {
            this.LINKSHELL6FontBox.Font = value;
            this.LINKSHELL6FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL7
    {
        get
        {
            return this.LINKSHELL7FontBox.Font;
        }
        set
        {
            this.LINKSHELL7FontBox.Font = value;
            this.LINKSHELL7FontBox.Text = value.Name;
        }
    }

    public Font FontLINKSHELL8
    {
        get
        {
            return this.LINKSHELL8FontBox.Font;
        }
        set
        {
            this.LINKSHELL8FontBox.Font = value;
            this.LINKSHELL8FontBox.Text = value.Name;
        }
    }

    public Font FontPARTY
    {
        get
        {
            return this.PARTYFontBox.Font;
        }
        set
        {
            this.PARTYFontBox.Font = value;
            this.PARTYFontBox.Text = value.Name;
        }
    }

    public Font FontSay
    {
        get
        {
            return this.SayFontBox.Font;
        }
        set
        {
            this.SayFontBox.Font = value;
            this.SayFontBox.Text = value.Name;
        }
    }

    public Font FontTELL
    {
        get
        {
            return this.TELLFontBox.Font;
        }
        set
        {
            this.TELLFontBox.Font = value;
            this.TELLFontBox.Text = value.Name;
        }
    }

    public bool IsTransparent
    {
        get
        {
            return this.CheckTransparencyButton.Checked;
        }
        set
        {
            this.CheckTransparencyButton.Checked = value;
        }
    }

    public Color LINKSHELL1Color
    {
        get
        {
            return this.LINKSHELL1FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL1FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL1Enable
    {
        get
        {
            return this.checkLINKSHELL1Box.Checked;
        }
        set
        {
            this.checkLINKSHELL1Box.Checked = value;
        }
    }

    public Color LINKSHELL2Color
    {
        get
        {
            return this.LINKSHELL2FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL2FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL2Enable
    {
        get
        {
            return this.checkLINKSHELL2Box.Checked;
        }
        set
        {
            this.checkLINKSHELL2Box.Checked = value;
        }
    }

    public Color LINKSHELL3Color
    {
        get
        {
            return this.LINKSHELL3FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL3FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL3Enable
    {
        get
        {
            return this.checkLINKSHELL3Box.Checked;
        }
        set
        {
            this.checkLINKSHELL3Box.Checked = value;
        }
    }

    public Color LINKSHELL4Color
    {
        get
        {
            return this.LINKSHELL4FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL4FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL4Enable
    {
        get
        {
            return this.checkLINKSHELL4Box.Checked;
        }
        set
        {
            this.checkLINKSHELL4Box.Checked = value;
        }
    }

    public Color LINKSHELL5Color
    {
        get
        {
            return this.LINKSHELL5FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL5FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL5Enable
    {
        get
        {
            return this.checkLINKSHELL5Box.Checked;
        }
        set
        {
            this.checkLINKSHELL5Box.Checked = value;
        }
    }

    public Color LINKSHELL6Color
    {
        get
        {
            return this.LINKSHELL6FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL6FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL6Enable
    {
        get
        {
            return this.checkLINKSHELL6Box.Checked;
        }
        set
        {
            this.checkLINKSHELL6Box.Checked = value;
        }
    }

    public Color LINKSHELL7Color
    {
        get
        {
            return this.LINKSHELL7FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL7FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL7Enable
    {
        get
        {
            return this.checkLINKSHELL7Box.Checked;
        }
        set
        {
            this.checkLINKSHELL7Box.Checked = value;
        }
    }

    public Color LINKSHELL8Color
    {
        get
        {
            return this.LINKSHELL8FontBox.ForeColor;
        }
        set
        {
            this.LINKSHELL8FontBox.ForeColor = value;
        }
    }

    public bool LINKSHELL8Enable
    {
        get
        {
            return this.checkLINKSHELL8Box.Checked;
        }
        set
        {
            this.checkLINKSHELL8Box.Checked = value;
        }
    }

    public Color PARTYColor
    {
        get
        {
            return this.PARTYFontBox.ForeColor;
        }
        set
        {
            this.PARTYFontBox.ForeColor = value;
        }
    }

    public bool PARTYEnable
    {
        get
        {
            return this.checkPARTYBox.Checked;
        }
        set
        {
            this.checkPARTYBox.Checked = value;
        }
    }

    public Color SayColor
    {
        get
        {
            return this.SayFontBox.ForeColor;
        }
        set
        {
            this.SayFontBox.ForeColor = value;
        }
    }

    public bool SayEnable
    {
        get
        {
            return this.checkSayBox.Checked;
        }
        set
        {
            this.checkSayBox.Checked = value;
        }
    }

    public Color TELLColor
    {
        get
        {
            return this.TELLFontBox.ForeColor;
        }
        set
        {
            this.TELLFontBox.ForeColor = value;
        }
    }

    public bool TELLEnable
    {
        get
        {
            return this.checkTELLBox.Checked;
        }
        set
        {
            this.checkTELLBox.Checked = value;
        }
    }
}

 
}
