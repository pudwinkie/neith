using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;

namespace Neith.Signpost.Services
{
    // TODO: アプリケーション ロジックを含むメソッドを作成します。
    [EnableClientAccess()]
    public class SignpostService : DomainService
    {
        [Invoke]
        public DateTimeOffset GetServerTime()
        {
            return DateTimeOffset.Now;
        }

    }
}