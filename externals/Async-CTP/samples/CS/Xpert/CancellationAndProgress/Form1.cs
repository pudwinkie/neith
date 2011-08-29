using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;


namespace ProgressAndCancellation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Shown += delegate { MessageBox.Show("This application shows some 'pull' techniques for getting progress reports from a long-running task. Click the buttons, see how the app is still responsive, and look at the source code.", "Progress"); };
        }


        //================================================================================================================================================================
        //================================================================================================================================================================
        //== Latest Progress
        //================================================================================================================================================================
        //================================================================================================================================================================

        private async void btnLatest_Click(object sender, EventArgs e)
        {
            foreach (Control button in panel1.Controls) button.Enabled = false;
            btnCancel.Enabled = true;
            var cts = new CancellationTokenSource();
            btnCancel.Click += cts.EventHandler;

            try
            {
                var progress = new LatestProgress<ScanInfo>();
                var task = ScanImageForLightAsync(pictureBox1.Image, cts.Token, progress);
                float scalex = ((float)pictureBox1.Width) / ((float)pictureBox1.Image.Width);
                float scaley = ((float)pictureBox1.Height) / ((float)pictureBox1.Image.Height);
                using (var g = pictureBox1.CreateGraphics())
                    while (await progress.Progress(task, 50))
                    {
                        pictureBox1.Refresh();
                        g.FillRectangle(Brushes.Yellow, 0, progress.Latest.Row * scaley, pictureBox1.Width, 4 * scaley);
                        await TaskEx.Run(delegate { Console.Beep(progress.Latest.RowCount * 30 + 200, 50); });
                    }
                var c = await task;
                MessageBox.Show("We found " + c + " light pixels", "Count");

            }
            catch (Exception)
            {
                pictureBox1.Invalidate();
            }
            finally
            {
                btnCancel.Click -= cts.EventHandler;
                foreach (Control button in panel1.Controls) button.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        class LatestProgress<T> : IProgress<T>
        {
            T latest;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            public T Latest { get { lock (this) { return latest; } } }

            public async Task<bool> Progress(Task UnderlyingTask, int MinimumDelay = 0)
            {
                await TaskEx.WhenAny(UnderlyingTask, TaskEx.WhenAll(tcs.Task, TaskEx.Delay(MinimumDelay)));
                if (UnderlyingTask.IsCompleted) { await UnderlyingTask; return false; }
                tcs = new TaskCompletionSource<object>(); return true;
            }

            void IProgress<T>.Report(T value)
            {
                lock (this) { latest = value; }
                tcs.TrySetResult(null);
            }
        }



        struct ScanInfo { public int Row; public int RowCount; public int TotalCount;}

        private async Task<int> ScanImageForLightAsync(Image img0, CancellationToken cancel, IProgress<ScanInfo> progress)
        {
            // This routine counts how many light pixels there are in an image.
            // So as to give useful feedback, it does it in a sweep from bottom to top,
            // and it makes a tone to show how much light each column had.

            var bmp = new Bitmap(img0);
            var count = 0;
            for (int y = bmp.Height-1; y >=0; y--)
            {
                cancel.ThrowIfCancellationRequested();

                var rowcount = 0;
                for (int x = 0; x < bmp.Width; x++)
                {
                    var col = bmp.GetPixel(x, y);
                    var lightness = 0.0 + col.R + col.G + col.B;
                    if (lightness > 350) rowcount++;
                }
                count += rowcount;
                if (progress != null) progress.Report(new ScanInfo() { Row = y, RowCount = rowcount, TotalCount = count });

                // Actually the code above runs to completion almost immediately. So let's have some artificial slowdown...
                await TaskEx.Delay(20);
            }
            return count;
        }



        //================================================================================================================================================================
        //================================================================================================================================================================
        //== Queued Progress
        //================================================================================================================================================================
        //================================================================================================================================================================
        
        private async void btnQueued_Click(object sender, EventArgs e)
        {
            foreach (Control button in panel1.Controls) button.Enabled = false;
            btnCancel.Enabled = true;
            var cts = new CancellationTokenSource();
            btnCancel.Click += cts.EventHandler;
            pictureBox1.Invalidate();

            try
            {
                var progress = new QueuedProgress<Point>();
                var task = MakeImageLightAsync(pictureBox1.Image, cts.Token, progress);
                float scalex = ((float)pictureBox1.Width) / ((float)pictureBox1.Image.Width);
                float scaley = ((float)pictureBox1.Height) / ((float)pictureBox1.Image.Height);
                using (var g = pictureBox1.CreateGraphics())
                while (await progress.NextProgress(task))
                {
                    g.FillRectangle(Brushes.Yellow, progress.Current.X*scalex, progress.Current.Y*scaley, 4*scalex, 4*scaley);
                }
            }
            catch (Exception)
            {
                pictureBox1.Invalidate();
            }
            finally
            {
                btnCancel.Click -= cts.EventHandler;
                foreach (Control button in panel1.Controls) button.Enabled = true;
                btnCancel.Enabled = false;
            }
        }


        class QueuedProgress<T> : IProgress<T>
        {

            bool pastFirstElement = false;
            System.Collections.Concurrent.ConcurrentQueue<T> reports = new System.Collections.Concurrent.ConcurrentQueue<T>();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            public T Current { get { T value; if (!reports.TryPeek(out value)) throw new InvalidOperationException("Must await NextProgress"); return value; } }

            public async Task<bool> NextProgress(Task UnderlyingTask)
            {
                T value;
                if (pastFirstElement) { if (!reports.TryDequeue(out value)) throw new Exception("Invalid state"); }
                pastFirstElement = true;
                await TaskEx.WhenAny(UnderlyingTask, tcs.Task);
                if (UnderlyingTask.IsCompleted) { await UnderlyingTask; return false; }
                tcs = new TaskCompletionSource<object>();
                if (!reports.TryPeek(out value)) throw new Exception("Invalid state");
                return true;
            }

            void IProgress<T>.Report(T value)
            {
                reports.Enqueue(value);
                tcs.TrySetResult(null);
            }
        }



        private async Task<Bitmap> MakeImageLightAsync(Image img0, CancellationToken cancel, IProgress<Point> progress)
        {
            // 1. Get the raw bits that make up the bitmap. We're doing this low-level bitwise
            // manipulation because we need the speed.
            var bmp = new Bitmap(img0.Width, img0.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp)) g.DrawImage(img0, 0, 0);
            var bounds = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            byte[] p = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, p, 0, bmpData.Stride * bmpData.Height);
            var r = new Random();

            // 2: One by one, find the lightest pixel on the image. This work is computationally expensive
            // so we're going to run it on a background thread to take the heat off the UI thread
            // (if we had several CPU cores free, we might want to run it on all of them.)
            await TaskEx.Run(delegate
            {
                while (true)
                {
                    cancel.ThrowIfCancellationRequested();

                    // find the lightest pixel in the image that's not already light
                    int x1 = 0, y1 = 0, i1 = 0, lightness1 = -1;
                    int yc = r.Next(bmp.Height), xc = r.Next(bmp.Width);
                    for (int y = Math.Max(0, yc-60); y < Math.Min(yc+60, bmp.Height); y+=2)
                    {
                        for (int x = Math.Max(0,xc-60); x < Math.Min(xc+60, bmp.Width); x+=2)
                        {
                            var i = y * bmpData.Stride + x * 4;
                            var lightness = (int)p[i + 2] + p[i + 1] + p[i];
                            if (lightness >= 760) continue; // (almost light is light enough)
                            if (lightness > lightness1) { lightness1 = lightness; x1 = x; y1 = y; i1 = i; }
                        }
                    }

                    // 3. If we didn't find any non-light pixels, then we're finished:
                    if (lightness1 == -1) break;

                    // 4. But if we did find non-light pixels, then mark them as light and report progress:
                    for (int y = 0; y < 4; y++)
                        for (int x = 0; x < 4; x++)
                        {
                            if (y1+y >= bmp.Height || x1+x >= bmp.Width) continue;
                            var offset = y * bmpData.Stride + x * 4;
                            p[i1 + offset + 2] = 255;
                            p[i1 + offset + 1] = 255;
                            p[i1 + offset + 0] = 255;
                        }
                    if (progress != null) progress.Report(new Point(x1, y1));
                }
            });

            // 5. Once we've made everything light, we can return our bitmap.
            bmp.UnlockBits(bmpData);
            return bmp;
        }



    }
}


//================================================================================================================================================================

public static class Extensions
{
    public static void EventHandler(this CancellationTokenSource cts, object sender, EventArgs e)
    {
        cts.Cancel();
    }

}
