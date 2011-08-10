using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{

    /// <summary>
    /// IDisposableの基本実装を行ないます。
    /// </summary>
    public abstract class DisposableObject : IDisposable
    {
        /// <summary>
        /// リソースの開放を行ないます。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// デストラクタ。アンマネージリソースのみ開放します。
        /// </summary>
        ~DisposableObject()
        {
            Dispose(false);
        }

        private bool disposed = false;
        private void Dispose(bool disposing)
        {
            if (!disposed) {
                disposed = true;
                if (disposing) {
                    DisposeManage();
                }
                DisposeUnManage();
            }
        }

        /// <summary>
        /// 既に破棄処理が行なわれていればtrueを返します。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
            protected set { disposed = value; }
        }

        /// <summary>
        /// マネージリソースの開放処理を行ないます。
        /// Disposeメソッドが呼び出されたときにのみ実行します。
        /// </summary>
        protected virtual void DisposeManage() { }

        /// <summary>
        /// アンマネージリソースの開放処理を行ないます。
        /// Dispose、及びデストラクタにて実行します。
        /// </summary>
        protected virtual void DisposeUnManage() { }

    }
}
