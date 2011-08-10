﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using ReactiveUI.Serialization;
using ReactiveUI.Xaml;

namespace ReactiveUI.Routing
{
    public interface IRoutableViewModel : ISerializableItem
    {
        string FriendlyUrlName { get; }
    }

    public interface IViewForViewModel
    {
        object ViewModel { get; set; }
    }

    public interface IViewForViewModel<T> : IViewForViewModel
        where T : IReactiveNotifyPropertyChanged
    {
        T ViewModel { get; set; }
    }

    public class ViewContractAttribute : Attribute
    {
        public string Contract { get; set; }
    }

    public static class ObservableUtils
    {
        public static IConnectableObservable<T> PermaRef<T>(this IConnectableObservable<T> This)
        {
            This.Connect();
            return This;
        }
    }
}