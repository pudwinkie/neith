using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;

namespace Neith.Logger.Model
{
    public sealed class NeithLogNotify : NeithNotificationModel
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="data"></param>
        public NeithLogNotify(NeithNotificationRec data):base(data)
        {
        }
    }
}
