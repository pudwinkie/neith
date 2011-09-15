using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Proxy
{
    public abstract class BaseSignpostService : ISignpostService, IDisposable
    {
        public void Dispose()
        {
            Channel.Leave(Name);
        }

        /// <summary>名前</summary>
        public string Name { get; private set; }

        public ISignpostServiceChannel Channel
        {
            get { return _Channel; }
            set
            {
                if (_Channel != null) _Channel.Leave(Name);
                _Channel = value;
                if (_Channel != null) _Channel.Join(Name);
            }
        }
        private ISignpostServiceChannel _Channel = null;

        /// <summary>状態確認履歴</summary>
        private Queue<DateTime> EchoTimeQueue = new Queue<DateTime>();
        private DateTime LastEchoTime = DateTime.MinValue;
        private DateTime NextEchoTime = DateTime.MinValue;


        /// <summary>サービス辞書</summary>
        public Dictionary<string, DateTime> ServiceDic { get; private set; }

        protected BaseSignpostService(string name)
        {
            Name = name;
            ServiceDic = new Dictionary<string, DateTime>();
        }

        public void Join(string name)
        {
            ServiceDic[name] = DateTime.UtcNow;
        }

        public void Leave(string name)
        {
            ServiceDic.Remove(name);
        }

        public void RequestEcho(string name)
        {
            Join(name);
            Channel.ResponseEcho(Name);

        }
        public void ResponseEcho(string name)
        {
            Join(name);
        }



        #region IDisposable メンバー

        #endregion
    }
}