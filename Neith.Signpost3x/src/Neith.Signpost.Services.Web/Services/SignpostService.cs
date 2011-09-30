using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;

namespace Neith.Signpost.Services
{
    // TODO: アプリケーション ロジックを含むメソッドを作成します。
    [EnableClientAccess()]
    public partial class SignpostService : DomainService
    {
        public SignpostService()
            : base()
        {
        }


        [Invoke]
        public DateTime GetServerTime()
        {
            var now = DateTime.Now;
            Debug.WriteLine("SignpostService::GetServerTime " + now);
            return now;
        }

        [Invoke]
        public bool GetBool()
        {
            var now = DateTimeOffset.Now;
            Debug.WriteLine("SignpostService::GetBool " + now);
            return true;
        }

        public override object Invoke(InvokeDescription invokeDescription, out IEnumerable<ValidationResult> validationErrors)
        {
            return base.Invoke(invokeDescription, out validationErrors);
        }
    }

}