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

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Smdn.Imaging.Formats.Ico;
using Icon = Smdn.Imaging.Formats.Ico.Icon;

namespace Smdn.Windows.Forms {
  public partial class SelectIconDialog : Form {
    public event EventHandler IconFileChanged;

    private const int maxIconWidth  = 32;
    private const int maxIconHeight = 32;

    private const int marginIconX = 2;
    private const int marginIconY = 2;

    public SelectIconDialog()
    {
      InitializeComponent();
      Initialize();
    }

    private void Initialize()
    {
      Description = "アイコンを選択してください。";

      textBoxFileName.Text = null;

      listBoxIcons.Items.Clear();
      listBoxIcons.MultiColumn = true;
      listBoxIcons.DrawMode = DrawMode.OwnerDrawFixed;
      listBoxIcons.ColumnWidth = maxIconWidth  + marginIconX * 2;
      listBoxIcons.ItemHeight  = maxIconHeight + marginIconY * 2;

      listBoxIcons.DrawItem += listBoxIcons_DrawItem;
    }

    public string Description {
      get { return labelDescription.Text; }
      set { labelDescription.Text = value; }
    }

    public string IconFile {
      get { return textBoxFileName.Text; }
      set
      {
        if (textBoxFileName.Text != value) {
          textBoxFileName.Text = value;
          OnIconFileChanged(EventArgs.Empty);
        }
      }
    }

    public int IconIndex {
      get { return listBoxIcons.SelectedIndex; }
      set
      {
        if (0 <= value && value < listBoxIcons.Items.Count)
          listBoxIcons.SelectedIndex = value;
        else
          listBoxIcons.SelectedIndex = ListBox.NoMatches;
      }
    }

    public Bitmap SelectedIcon {
      get
      {
        if (listBoxIcons.SelectedItem == null)
          return null;
        else
          return listBoxIcons.SelectedItem as Bitmap;
      }
    }

    protected virtual void OnIconFileChanged(EventArgs e)
    {
      try {
        this.Cursor = Cursors.WaitCursor;

        try {
          listBoxIcons.BeginUpdate();

          var oldItems = new object[listBoxIcons.Items.Count];

          listBoxIcons.Items.CopyTo(oldItems, 0);
          listBoxIcons.Items.Clear();

          foreach (Bitmap icon in oldItems) {
            icon.Dispose();
          }

          if (File.Exists(IconFile)) {
            listBoxIcons.Items.AddRange(Smdn.Imaging.Formats.Ico.Icon.ExtractAll(IconFile, true));

            if (0 < listBoxIcons.Items.Count)
              listBoxIcons.SelectedIndex = 0;
          }
        }
        finally {
          listBoxIcons.EndUpdate();
        }
      }
      finally {
        this.Cursor = Cursors.Default;
      }

      var ev = this.IconFileChanged;

      if (ev != null)
        ev(this, e);
    }

    private void buttonChangeFile_Click(object sender, EventArgs e)
    {
      using (var dialog = new OpenFileDialog()) {
        dialog.FileName = IconFile;
        dialog.Filter = Smdn.IO.FileDialogFilter.CreateFilterString(new string[][] {
          new[] {"アイコン ファイル", "*.ico"},
          new[] {"実行可能ファイル", "*.exe"},
          new[] {"DLLファイル", "*.dll"},
          new[] {"すべてのファイル", "*.*"},
        });
        dialog.RestoreDirectory = true;
        dialog.ShowReadOnly = true;

        if (IconFile == null)
          dialog.InitialDirectory = Environment.CurrentDirectory;
        else
          dialog.InitialDirectory = Path.GetDirectoryName(IconFile);

        if (dialog.ShowDialog(this) == DialogResult.OK)
          this.IconFile = dialog.FileName;
      }
    }

    private void listBoxIcons_DrawItem(object sender, DrawItemEventArgs e)
    {
      var listBox = sender as ListBox;
      var icon = (0 <= e.Index)
        ? listBox.Items[e.Index] as Bitmap
        : null;

      e.DrawBackground();

      if (icon != null) {
        var rect = new Rectangle(e.Bounds.X + marginIconX,
                                 e.Bounds.Y + marginIconY,
                                 maxIconWidth  + marginIconX * 2,
                                 maxIconHeight + marginIconY * 2);

        if (icon.Width <= maxIconWidth && icon.Height <= maxIconHeight)
          e.Graphics.DrawImageUnscaled(icon, rect.X, rect.Y);
        else
          e.Graphics.DrawImage(icon, rect);
      }

      e.DrawFocusRectangle();
    }
  }
}
