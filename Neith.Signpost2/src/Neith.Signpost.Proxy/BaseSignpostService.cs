using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Neith.Signpost.Proxy
{
    public abstract class BaseSignpostService : ISignpostService, IDisposable
    {
        #region 開放
        public void Dispose()
        {
        }

        #endregion
        #region プロパティ


        #endregion


        protected BaseSignpostService()
        {
        }


        #region ISignpostActionService メンバー

        public virtual void SendAlt( char c)
        {
            throw new NotImplementedException();
        }

        public virtual void SendCtrl(char c)
        {
            throw new NotImplementedException();
        }

        public virtual void SendShift( char c)
        {
            throw new NotImplementedException();
        }

        public virtual void SendCtrlInput(char c, params Key[] ctrlKey)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}