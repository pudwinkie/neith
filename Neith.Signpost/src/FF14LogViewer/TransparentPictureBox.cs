using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FF14LogViewer
{
    public class TransparentPictureBox : PictureBox
    {
        // Methods
        public TransparentPictureBox()
        {
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.FromArgb(0, 0xff, 0xff, 0xff);
        }
    }

}
