//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: MainForm.cs
//
//--------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class MainForm : Form
    {
        public MainForm() { InitializeComponent(); }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            pbLifeDisplay.Image = null;
            int width = pbLifeDisplay.Width, height = pbLifeDisplay.Height;

            // Initialize the object pool and the game board
            var pool = new ObjectPool<Bitmap>(() => new Bitmap(width, height));
            var game = new GameBoard(width, height, .1, pool);

            // Run until cancellation is requested
            var sw = new Stopwatch();
            while (true)
            {
                // Move to the next board, timing how long it takes
                sw.Restart();
                Bitmap bmp = await TaskEx.Run(() => game.MoveNext());
                var framesPerSecond = 1 / sw.Elapsed.TotalSeconds;

                lblFramesPerSecond.Text = String.Format("Frames / Sec: {0:F2}", framesPerSecond);
                var old = (Bitmap)pbLifeDisplay.Image;
                pbLifeDisplay.Image = bmp;
                if (old != null) pool.PutObject(old);
            }
        }
    }
}