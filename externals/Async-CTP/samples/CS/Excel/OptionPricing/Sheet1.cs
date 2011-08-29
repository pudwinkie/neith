//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: Sheet1.cs
//
//--------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace OptionPricing
{
    public partial class Sheet1
    {
        #region VSTO Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            this.Startup += new System.EventHandler(this.Sheet1_Startup);

        }

        #endregion

        private Excel.Range rngUp, rngDown, rngInitial, rngExercise, rngInterest, rngPeriods, rngRuns, rngRemote;
        private CancellationTokenSource _cancellation;

        private void Sheet1_Startup(object sender, System.EventArgs e)
        {
            rngUp = Range["B2"];
            rngDown = Range["B3"];
            rngInterest = Range["B4"];
            rngInitial = Range["B5"];
            rngPeriods = Range["B6"];
            rngExercise = Range["B7"];
            rngRuns = Range["B8"];
            rngRemote = Range["B9"];
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.Range["D2", "M11"].ClearContents();
            this.Range["D13", "D18"].ClearContents();
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            Application.StartAsyncMethod();

            // Set up a cancellation source to use to cancel background work
            if (_cancellation != null)
            {
                _cancellation.Cancel();
                return;
            }
            _cancellation = new CancellationTokenSource();
            var cancellationToken = _cancellation.Token;

            // Get data from form
            double initial = (double)rngInitial.Value2, exercise = (double)rngInitial.Value2, interest = (double)rngInterest.Value2;
            double up = (double)rngUp.Value2, down = (double)rngDown.Value2;
            int periods = Convert.ToInt32(rngPeriods.Value2);
            int runs = Convert.ToInt32(rngRuns.Value2);

            // Run for a number of iterations
            string[] columns = { "D", "E", "F", "G", "H", "I", "J", "K", "L", "M" };
            int[] rows = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            this.Range["C2", missing].Value2 = "Calculating...";
            btnRun.Text = "Cancel";
            btnClear.Enabled = false;
            try
            {
                var cells = from column in columns
                            from row in rows
                            select new { column, row };

                var results = from cell in cells.AsParallel().WithCancellation(cancellationToken).WithMergeOptions(ParallelMergeOptions.NotBuffered)
                              let price = PriceAsianOptions(initial, exercise, up, down, interest, periods, runs)
                              select new
                              {
                                  Price = price,
                                  Column = cell.column,
                                  Row = cell.row
                              };

                // Initialize aggregation data
                int count = 0;
                double sumPrice = 0.0, sumSquarePrice = 0.0, min = double.MaxValue, max = double.MinValue;
                double stdDev = 0.0, stdErr = 0.0;

                // Run the query and process its results
                using (var enumerator = results.GetEnumerator())
                {
                    while (await TaskEx.Run(() => enumerator.MoveNext()))
                    {
                        var result = enumerator.Current;
                        count++;
                        sumPrice += result.Price;
                        Range["D13"].Value2 = sumPrice / count;

                        min = Math.Min(min, result.Price);
                        max = Math.Max(max, result.Price);
                        Range["D14"].Value2 = min;
                        Range["D15"].Value2 = max;

                        sumSquarePrice += result.Price * result.Price;
                        stdDev = Math.Sqrt(sumSquarePrice - sumPrice * sumPrice / count) / ((count == 1) ? 1 : count - 1);
                        stdErr = stdDev / Math.Sqrt(count);
                        Range["D16"].Value2 = stdDev;
                        Range["D17"].Value2 = stdErr;

                        Range[string.Format("{0}{1}", result.Column, result.Row)].Value2 = result.Price;
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                // Reset controls
                btnRun.Text = "Run";
                Range["C2"].ClearContents();
                btnClear.Enabled = true;
                _cancellation = null;
            }
        }

        private static double PriceAsianOptions(double initial, double exercise, double up, double down, double interest, int periods, int runs)
        {
            double[] pricePath = new double[periods + 1];
            double piup = (interest - down) / (up - down);
            double pidown = 1 - piup;
            double temp = 0.0, priceAverage = 0.0, callPayOff = 0.0;

            for (int index = 0; index < runs; index++)
            {
                double sumPricePath = initial;
                for (int i = 1; i <= periods; i++)
                {
                    pricePath[0] = initial;
                    double rn = s_rand.Value.NextDouble();
                    pricePath[i] = pricePath[i - 1] *  (rn > pidown ? up : down);
                    sumPricePath += pricePath[i];
                }
                priceAverage = sumPricePath / (periods + 1);
                callPayOff = Math.Max(priceAverage - exercise, 0);
                temp += callPayOff;
            }
            return (temp / Math.Pow(interest, periods)) / runs;
        }

        private static ThreadLocal<Random> s_rand = new ThreadLocal<Random>(() => 
            new Random(Thread.CurrentThread.ManagedThreadId ^ Environment.TickCount));
    }
}