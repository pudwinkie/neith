using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Excel;

public static class ExcelInterop
{
    public static void StartAsyncMethod(this Microsoft.Office.Interop.Excel.Application Application)
    {
        if (s_app == null) s_app = Application;
        if (s_uiSC == null) { if (SynchronizationContext.Current == null) { var c = new System.Windows.Forms.Control(); var _ = c.Handle; c.Dispose(); } s_uiSC = SynchronizationContext.Current; }
        if (s_excelSC == null) s_excelSC = new ExcelSynchronizationContext();
        if ((SynchronizationContext.Current as ExcelSynchronizationContext) == null) SynchronizationContext.SetSynchronizationContext(s_excelSC);
    }

    private static Queue<Tuple<SendOrPostCallback, object>> s_queue = new Queue<Tuple<SendOrPostCallback, object>>();
    private static Microsoft.Office.Interop.Excel.Application s_app = null;
    private static SynchronizationContext s_uiSC = null;
    private static SynchronizationContext s_excelSC = new ExcelSynchronizationContext();

    private class ExcelSynchronizationContext : SynchronizationContext
    {
        public override void Send(SendOrPostCallback d, object state) { throw new NotSupportedException(); }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (s_queue)
            {
                s_queue.Enqueue(Tuple.Create(d, state));
                if (s_queue.Count == 1) s_uiSC.Post(OnThread, null);
            }
        }

        private void OnThread(object state)
        {
            SynchronizationContext.SetSynchronizationContext(s_excelSC);
            bool isReady = false;
            try
            {
                var range = (Range)((dynamic)s_app.Sheets[1]).Cells[1, 1];
                range.Value = range.Value;
                if (s_app.Ready) isReady = true;
            }
            catch (COMException ex) { if (ex.ErrorCode != -2146827284 && ex.ErrorCode != -2146777998) throw; }

            if (!isReady)
            {
                new Timer(self =>
                {
                    ((IDisposable)self).Dispose();
                    s_uiSC.Post(OnThread, null);
                }).Change(250, Timeout.Infinite);
            }
            else
            {
                Queue<Tuple<SendOrPostCallback, object>> q2;
                lock (s_queue)
                {
                    q2 = new Queue<Tuple<SendOrPostCallback, object>>(s_queue);
                    s_queue.Clear();
                }
                foreach (var ds in q2) ds.Item1(ds.Item2);
            }
        }
    }
}