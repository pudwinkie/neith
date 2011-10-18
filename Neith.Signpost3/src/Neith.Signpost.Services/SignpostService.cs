using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// Signpostサービス。
    /// </summary>
    public class SignpostService : ISignpostContext
    {
        public DateTimeOffset GetServerTime()
        {
            var now = DateTimeOffset.Now;
            Debug.WriteLine("[SignpostService::GetServerTime] " + now);
            return now;
        }
        private static readonly TaskFactory tf = new TaskFactory();

        public IAsyncResult BeginGetServerTime(AsyncCallback callback, object state)
        {
            var t1 = new Task<DateTimeOffset>(st => GetServerTime(), state);
            t1.Start();
            var t2 = tf.ContinueWhenAll(new[] { t1 }, a => callback(t1));
            return t1;
        }

        public DateTimeOffset EndGetServerTime(IAsyncResult result)
        {
            var task = result as Task<DateTimeOffset>;
            return task.Result;
        }







        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null) {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue) {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
