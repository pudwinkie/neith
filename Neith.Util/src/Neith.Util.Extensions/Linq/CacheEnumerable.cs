using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq
{
    internal class CacheEnumerable<T> : IList<T>
    {
        private IEnumerator<T> Gen;
        private List<T> Cache = new List<T>();
        private bool HasNext = true;
        private static readonly Tuple<int, T> EmptyItem = new Tuple<int, T>(-1, default(T));

        internal CacheEnumerable(IEnumerable<T> src)
        {
            Gen = src.GetEnumerator();
        }

        /// <summary>
        /// 要素を取得します。
        /// </summary>
        /// <returns></returns>
        private Tuple<int, T> GetNext()
        {
            if (HasNext) HasNext = Gen.MoveNext();
            if (!HasNext) return EmptyItem;
            Cache.Add(Gen.Current);
            return new Tuple<int, T>(Cache.Count - 1, Gen.Current);
        }

        private IEnumerable<Tuple<int, T>> EnGetNext()
        {
            while (HasNext) {
                var t = GetNext();
                if (!HasNext) yield break;
                yield return t;
            }
        }


        /// <summary>
        /// 指定されたインデックスの要素を取得します。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryGetIndexAt(int index, out T result)
        {
            while (index >= Cache.Count) {
                var t = GetNext();
                if (!HasNext) {
                    result = default(T);
                    return false;
                }
            }
            result = Cache[index];
            return true;
        }

        /// <summary>
        /// すべての要素をキャッシュします。
        /// </summary>
        private void GetAll()
        {
            while (HasNext) GetNext();
        }


        #region IList<T> メンバー

        public int IndexOf(T item)
        {
            var index = Cache.IndexOf(item);
            if (index >= 0 || !HasNext) return index;
            var t = EnGetNext()
                .Where(a => item.Equals(a.Item2))
                .DefaultIfEmpty(EmptyItem)
                .FirstOrDefault();
            return t.Item1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                if (index < Cache.Count) return Cache[index];
                if (!HasNext) throw new ArgumentOutOfRangeException();
                var t = EnGetNext()
                    .Where(a => index == a.Item1)
                    .DefaultIfEmpty(EmptyItem)
                    .FirstOrDefault();
                if (t.Item1 < 0) throw new ArgumentOutOfRangeException();
                return t.Item2;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<T> メンバー

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                if (HasNext) GetAll();
                return Cache.Count;
            }
        }

        public bool IsReadOnly { get { return true; } }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> メンバー

        public IEnumerator<T> GetEnumerator()
        {
            return EnItem().GetEnumerator();
        }
        private IEnumerable<T> EnItem()
        {
            for (var i = 0; ; i++) {
                T rc;
                if (!TryGetIndexAt(i, out rc)) yield break;
                yield return rc;
            }
        }

        #endregion

        #region IEnumerable メンバー

        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
