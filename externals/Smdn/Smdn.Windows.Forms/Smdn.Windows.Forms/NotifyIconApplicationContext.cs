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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Smdn.Windows.Forms {
  public class NotifyIconApplicationContext : ApplicationContext {
    /*
     * NotifyIcon properties
     */
    public string Text {
      get { return notifyIcon.Text; }
      set { notifyIcon.Text = value; }
    }

    public Icon Icon {
      get { return notifyIcon.Icon; }
      set { notifyIcon.Icon = value; }
    }

    public bool Visible {
      get { return notifyIcon.Visible; }
      set { notifyIcon.Visible = value; }
    }

    public NotifyIconApplicationContext()
    {
      this.components = new Container();
      this.notifyIcon = new NotifyIcon(components);

      this.Text = string.Empty;
      this.Visible = true;
      this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

      /*
       * NotifyIcon events
       * http://natchan-develop.seesaa.net/article/24562238.html
       */
      notifyIcon.Click += delegate(object sender, EventArgs e) { OnClick(e); };
      notifyIcon.DoubleClick += delegate(object sender, EventArgs e) { OnDoubleClick(e); };
      notifyIcon.MouseClick += delegate(object sender, MouseEventArgs e) { OnMouseClick(e); };
      notifyIcon.MouseDoubleClick += delegate(object sender, MouseEventArgs e) { OnMouseDoubleClick(e); };
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        notifyIcon.Visible = false;

        if (components != null)
          components.Dispose();
      }

      base.Dispose(disposing);
    }

    protected virtual void OnClick(EventArgs e)
    {
    }

    protected virtual void OnDoubleClick(EventArgs e)
    {
    }

    protected virtual void OnMouseClick(MouseEventArgs e)
    {
    }

    protected virtual void OnMouseDoubleClick(MouseEventArgs e)
    {
    }

    public virtual void ShowContextMenu(MouseButtons button)
    {
      var contextMenu = GetContextMenu(button);

      if (contextMenu == null)
        return;

      try {
        notifyIcon.ContextMenuStrip = contextMenu;

        if (Runtime.IsRunningOnNetFx)
          // XXX: http://social.msdn.microsoft.com/forums/en-US/winforms/thread/8de03b21-e144-4614-96cd-d382c2a2fbe9/
          notifyIcon.GetType().GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(notifyIcon, null);
        else
          notifyIcon.ContextMenuStrip.Show(Cursor.Position);
      }
      finally {
        notifyIcon.ContextMenuStrip = null;
      }
    }

    protected virtual ContextMenuStrip GetContextMenu(MouseButtons button)
    {
      return null;
    }

    public void ShowBalloonTip(int timeout)
    {
      notifyIcon.ShowBalloonTip(timeout);
    }

    public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
    {
      notifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
    }

    private IContainer components;
    private NotifyIcon notifyIcon;
  }
}