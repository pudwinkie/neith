using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Neith.Util
{
    /// <summary>
    /// オブジェクト生存期間の管理クラス。
    /// </summary>
    public class LifeTimeScope : IDisposable
    {

        /// <summary>
        /// 管理している参照カウンタのリストです。
        /// </summary>
        private readonly List<IDisposable>
            theListOfWrapperObjectsToCallDisposeOn = new List<IDisposable>();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LifeTimeScope()
        {
        }

        /// <summary>
        /// 開放処理です。
        /// </summary>
        public virtual void Dispose()
        {
            foreach (IDisposable theObject in
                theListOfWrapperObjectsToCallDisposeOn) {
                theObject.Dispose();
            }
        }

        /// <summary>
        /// 管理対象のオブジェクトを追加します。
        /// </summary>
        /// <param name="disposableObject"></param>
        /// <returns></returns>
        public void Add(IDisposable disposableObject)
        {
            theListOfWrapperObjectsToCallDisposeOn.Add(disposableObject);
        }

        /// <summary>
        /// 参照カウンタに登録します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        public T AddRef<T>(T resource) where T : IDisposable
        {
            Add(ReferenceCountingExtensions.AddRef<T>(resource));
            return resource;
        }

    }


    /// <summary>
    /// usingスコープ内で登録されたIDisposableオブジェクトをまとめて破棄するヘルパークラスです。
    /// 必ずusing節で利用してください。ジェネレータ内のusing節では利用できません。
    /// </summary>
    public sealed class UsingLifeScope : LifeTimeScope
    {
        /// <summary>
        /// 現在のスコープ（スレッドでひとつ）。
        /// </summary>
        [ThreadStatic]
        private static UsingLifeScope _Current = null;

        /// <summary>
        /// スコープのスタック（スレッドでひとつ）
        /// </summary>
        [ThreadStatic]
        private static readonly Stack<UsingLifeScope> theScopeStack =
            new Stack<UsingLifeScope>();

        /// <summary>
        /// 現在のスコープ管理オブジェクトを返します。
        /// </summary>
        public static UsingLifeScope Current
        {
            get { return _Current; }
        }

        /// <summary>
        /// 管理している参照カウンタのリストです。
        /// </summary>
        private readonly List<IDisposable>
            theListOfWrapperObjectsToCallDisposeOn = new List<IDisposable>();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public UsingLifeScope()
            : base()
        {
            _Current = this;
            theScopeStack.Push(this);
        }

        /// <summary>
        /// 開放。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _Current = theScopeStack.Pop();
        }

        /// <summary>
        /// 参照カウンタに登録します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static new T AddRef<T>(T resource) where T : IDisposable
        {
            return ((LifeTimeScope)Current).AddRef<T>(resource);
        }

    }


    internal static class ReferenceCountingExtensions
    {
        /// <summary>
        /// 参照カウンタ管理テーブル（システムでひとつ）
        /// </summary>
        static private readonly Dictionary<object, IRefCounted>
            theHashOfReferenceCountedClasses =
            new Dictionary<object, IRefCounted>();

        /// <summary>
        /// オブジェクトの参照カウンタをインクリメントします。
        /// 新しい管理対象の場合は参照カウンタを作成します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static IRefCounted AddRef<T>(T resource)
        {
            IRefCounted theObject = null;
            theHashOfReferenceCountedClasses.TryGetValue(resource,
                out theObject);
            if (theObject == null) {
                if (Marshal.IsComObject(resource)) theObject = new ComRefCounted<T>(resource);
                else theObject = new RefCounted<T>(resource);
                theHashOfReferenceCountedClasses[resource] = theObject;
            }
            else {
                theObject.AddRef();
            }
            return theObject;
        }

    }

    /// <summary>
    /// 参照カウンタインターフェースです。
    /// </summary>
    internal interface IRefCounted : IDisposable
    {
        /// <summary>
        /// 参照カウンタをインクリメントします。
        /// </summary>
        /// <returns></returns>
        void AddRef();
    }

    /// <summary>
    /// 参照カウンタです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RefCounted<T> : IRefCounted
    {
        private T theResource;
        protected bool IsFinalDisposed = false;
        int RefCounter = 0;

        /// <summary>
        /// リソースを取得します。
        /// </summary>
        public T Resource { get { return theResource; } }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="inResource"></param>
        public RefCounted(T inResource)
        {
            theResource = inResource;
            System.Threading.Interlocked.Increment(ref RefCounter);
        }
        /// <summary>
        /// デストラクタ。
        /// </summary>
        ~RefCounted()
        {
            if (!IsFinalDisposed) {
                Dispose();
            }
        }
        /// <summary>
        /// 開放処理です。
        /// 参照カウンタをデクリメントします。
        /// </summary>
        public void Dispose()
        {
            System.Threading.Interlocked.Decrement(ref RefCounter);
            if ((RefCounter == 0) && (!IsFinalDisposed)) FinalDispose();
        }

        /// <summary>
        /// 参照カウンタが０のときの処理です。
        /// </summary>
        virtual public void FinalDispose()
        {
            ((IDisposable)theResource).Dispose();
            IsFinalDisposed = true;
        }

        /// <summary>
        /// 参照カウンタをインクリメントします。
        /// </summary>
        /// <returns></returns>
        public void AddRef()
        {
            System.Threading.Interlocked.Increment(ref RefCounter);
        }

    }

    /// <summary>
    /// Com用の参照カウンタです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ComRefCounted<T> : RefCounted<T>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="inResource"></param>
        public ComRefCounted(T inResource)
            : base(inResource)
        {
        }

        /// <summary>
        /// 参照カウンタが０のときの処理です。
        /// </summary>
        override public void FinalDispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(Resource);
            IsFinalDisposed = true;
        }
    }

}