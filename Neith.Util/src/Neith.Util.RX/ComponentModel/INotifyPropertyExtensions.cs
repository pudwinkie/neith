using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Neith.ComponentModel
{
    public static class INotifyPropertyExtensions
    {
        /// <summary>
        /// プロパティの値変更イベントをキャプチャします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dep"></param>
        /// <returns></returns>
        public static IObservable<PropertyChangedEventArgs> RxNotifyPropertyChanged(
            this INotifyPropertyChanged src, string name)
        {
            return Observable
                .FromEventPattern<PropertyChangedEventArgs>(src, "PropertyChanged")
                .Where(a => a.EventArgs.PropertyName == name)
                .Select(a => a.EventArgs);
        }

        /// <summary>
        /// プロパティ値通知を作成します。
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="src"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IObservable<TValue> RxPropertyChangedValue<TSender, TValue>(
            this TSender src, Expression<Func<TSender, TValue>> property)
            where TSender : INotifyPropertyChanged
        {
            var name = property.ToPropertyName();
            var func = property.Compile();
            return src
                .RxNotifyPropertyChanged(name)
                .Select(a => func(src));
        }

        /// <summary>
        /// プロパティ値通知を作成します。初回に現在のプロパティを通知します。
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="src"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IObservable<TValue> RxPropertyChangedValueEx<TSender, TValue>(
            this TSender src, Expression<Func<TSender, TValue>> property)
            where TSender : INotifyPropertyChanged
        {
            var func = property.Compile();
            return Observable
                .Return(func(src))
                .Concat(src.RxPropertyChangedValue(property));
        }

    }
}
