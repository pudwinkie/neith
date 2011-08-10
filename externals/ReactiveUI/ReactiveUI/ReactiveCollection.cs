using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Reactive.Disposables;

namespace ReactiveUI
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the objects in the collection.</typeparam>
    public class ReactiveCollection<T> : ObservableCollection<T>, IReactiveCollection<T>, IDisposable
    {
        /// <summary>
        /// Constructs a ReactiveCollection.
        /// </summary>
        public ReactiveCollection() { setupRx(); }

        /// <summary>
        /// Constructs a ReactiveCollection given an existing list.
        /// </summary>
        /// <param name="list">The existing list with which to populate the new
        /// list.</param>
        public ReactiveCollection(IEnumerable<T> list) { setupRx(list); }

        [OnDeserialized]
        void setupRx(StreamingContext _) { setupRx(); }

        void setupRx(IEnumerable<T> List = null)
        {
            _BeforeItemsAdded = new ScheduledSubject<T>(RxApp.DeferredScheduler);
            _BeforeItemsRemoved = new ScheduledSubject<T>(RxApp.DeferredScheduler);
            aboutToClear = new Subject<int>();

            if (List != null) {
                foreach(var v in List) { this.Add(v); }
            }

            var ocChangedEvent = new Subject<NotifyCollectionChangedEventArgs>();
            CollectionChanged += (o, e) => ocChangedEvent.OnNext(e);

            _ItemsAdded = ocChangedEvent
                .Where(x =>
                    x.Action == NotifyCollectionChangedAction.Add ||
                    x.Action == NotifyCollectionChangedAction.Replace)
                .SelectMany(x =>
                    (x.NewItems != null ? x.NewItems.OfType<T>() : Enumerable.Empty<T>())
                    .ToObservable())
                .Multicast(new ScheduledSubject<T>(RxApp.DeferredScheduler))
                .PermaRef();

            _ItemsRemoved = ocChangedEvent
                .Where(x =>
                    x.Action == NotifyCollectionChangedAction.Remove ||
                    x.Action == NotifyCollectionChangedAction.Replace ||
                    x.Action == NotifyCollectionChangedAction.Reset)
                .SelectMany(x =>
                    (x.OldItems != null ? x.OldItems.OfType<T>() : Enumerable.Empty<T>())
                    .ToObservable())
                .Multicast(new ScheduledSubject<T>(RxApp.DeferredScheduler))
                .PermaRef();

            _CollectionCountChanging = Observable.Merge(
                _BeforeItemsAdded.Select(_ => this.Count),
                _BeforeItemsRemoved.Select(_ => this.Count),
                aboutToClear
            );

            _CollectionCountChanged = ocChangedEvent
                .Select(x => this.Count)
                .DistinctUntilChanged();

            _ItemChanging = new ScheduledSubject<IObservedChange<T, object>>(RxApp.DeferredScheduler);
            _ItemChanged = new ScheduledSubject<IObservedChange<T,object>>(RxApp.DeferredScheduler);

            // TODO: Fix up this selector nonsense once SL/WP7 gets Covariance
            _Changing = Observable.Merge(
                _BeforeItemsAdded.Select<T, IObservedChange<object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName =  "Items", Sender = this, Value = this}),
                _BeforeItemsRemoved.Select<T, IObservedChange<object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName =  "Items", Sender = this, Value = this}),
                aboutToClear.Select<int, IObservedChange<object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName = "Items", Sender = this, Value = this}),
                _ItemChanging.Select<IObservedChange<T, object>, IObservedChange <object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName = x.PropertyName, Sender = x.Sender, Value = x.Value}));

            _Changed = Observable.Merge(
                _ItemsAdded.Select<T, IObservedChange<object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName = "Items", Sender = this, Value = this}),
                _ItemsRemoved.Select<T, IObservedChange<object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName =  "Items", Sender = this, Value = this}),
                _ItemChanged.Select<IObservedChange<T, object>, IObservedChange<object, object>>(x => 
                    new ObservedChange<object, object>() {PropertyName = x.PropertyName, Sender = x.Sender, Value = x.Value}));

            _ItemsAdded.Subscribe(x => {
                this.Log().DebugFormat("Item Added to {0:X} - {1}", this.GetHashCode(), x);
                if (propertyChangeWatchers == null)
                    return;
                addItemToPropertyTracking(x);
            });

            _ItemsRemoved.Subscribe(x => {
                this.Log().DebugFormat("Item removed from {0:X} - {1}", this.GetHashCode(), x);
                if (propertyChangeWatchers == null || !propertyChangeWatchers.ContainsKey(x))
                    return;

                removeItemFromPropertyTracking(x);
            });

#if DEBUG
            _ItemChanged.Subscribe(x => 
                this.Log().DebugFormat("Object {0} changed in collection {1:X}", x, this.GetHashCode()));
#endif
        }

        void addItemToPropertyTracking(T toTrack)
        {
            var item = toTrack as IReactiveNotifyPropertyChanged;
            if (item == null)
                return;

            if (propertyChangeWatchers.ContainsKey(toTrack)) {
                propertyChangeWatchers[toTrack].AddRef();
                return;
            }

            var to_dispose = new[] {
                item.Changing.Subscribe(before_change =>
                    _ItemChanging.OnNext(new ObservedChange<T, object>() { 
                        Sender = toTrack, PropertyName = before_change.PropertyName })),
                item.Changed.Subscribe(change => 
                    _ItemChanged.OnNext(new ObservedChange<T,object>() { 
                        Sender = toTrack, PropertyName = change.PropertyName })),
            };

            propertyChangeWatchers.Add(toTrack, 
                new RefcountDisposeWrapper(Disposable.Create(() => {
                    to_dispose[0].Dispose(); to_dispose[1].Dispose();
                    propertyChangeWatchers.Remove(toTrack);
            })));
        }

        void removeItemFromPropertyTracking(T toUntrack)
        {
            propertyChangeWatchers[toUntrack].Release();
        }

        [IgnoreDataMember]
        protected IObservable<T> _ItemsAdded;

        /// <summary>
        /// Fires when items are added to the collection, once per item added.
        /// Functions that add multiple items such as AddRange should fire this
        /// multiple times. The object provided is the item that was added.
        /// </summary>
        public IObservable<T> ItemsAdded {
            get { return _ItemsAdded.Where(_ => areChangeNotificationsEnabled); }
        }

        [IgnoreDataMember]
        protected ISubject<T> _BeforeItemsAdded;

        /// <summary>
        /// Fires before an item is going to be added to the collection.
        /// </summary>
        public IObservable<T> BeforeItemsAdded {
            get { return _BeforeItemsAdded.Where(_ => areChangeNotificationsEnabled); }
        }

        [IgnoreDataMember]
        protected IObservable<T> _ItemsRemoved;

        /// <summary>
        /// Fires once an item has been removed from a collection, providing the
        /// item that was removed.
        /// </summary>
        public IObservable<T> ItemsRemoved {
            get { return _ItemsRemoved.Where(_ => areChangeNotificationsEnabled); }
        }

        [IgnoreDataMember]
        protected ISubject<T> _BeforeItemsRemoved;

        /// <summary>
        /// Fires before an item will be removed from a collection, providing
        /// the item that will be removed. 
        /// </summary>
        public IObservable<T> BeforeItemsRemoved {
            get { return _BeforeItemsRemoved.Where(_ => areChangeNotificationsEnabled); }
        }

        [IgnoreDataMember]
        protected Subject<int> aboutToClear;

        [IgnoreDataMember]
        protected IObservable<int> _CollectionCountChanging;

        /// <summary>
        /// Fires before a collection is about to change, providing the previous
        /// Count.
        /// </summary>
        public IObservable<int> CollectionCountChanging {
            get { return _CollectionCountChanging.Where(_ => areChangeNotificationsEnabled); }
        }

        [IgnoreDataMember]
        protected IObservable<int> _CollectionCountChanged;

        /// <summary>
        /// Fires whenever the number of items in a collection has changed,
        /// providing the new Count.
        /// </summary>
        public IObservable<int> CollectionCountChanged {
            get { return _CollectionCountChanged.Where(_ => areChangeNotificationsEnabled); }
        }

        [IgnoreDataMember]
        protected ISubject<IObservedChange<T, object>> _ItemChanging;

        /// <summary>
        /// Provides Item Changed notifications for any item in collection that
        /// implements IReactiveNotifyPropertyChanged. This is only enabled when
        /// ChangeTrackingEnabled is set to True.
        /// </summary>
        public IObservable<IObservedChange<T, object>> ItemChanging {
            get { return _ItemChanging.Where(_ => areChangeNotificationsEnabled); }
        }
        IObservable<IObservedChange<object, object>> IReactiveCollection.ItemChanging {
            get { return (IObservable<IObservedChange<object, object>>)ItemChanging; }
        }

        [IgnoreDataMember]
        protected ISubject<IObservedChange<T, object>> _ItemChanged;

        /// <summary>
        /// Provides Item Changing notifications for any item in collection that
        /// implements IReactiveNotifyPropertyChanged. This is only enabled when
        /// </summary>
        public IObservable<IObservedChange<T, object>> ItemChanged {
            get { return _ItemChanged.Where(_ => areChangeNotificationsEnabled); }
        }
        IObservable<IObservedChange<object, object>> IReactiveCollection.ItemChanged {
            get { return (IObservable<IObservedChange<object, object>>)ItemChanged; }
        }

        [IgnoreDataMember]
        protected IObservable<IObservedChange<object, object>> _Changing;

        /// <summary>
        /// Fires when anything in the collection or any of its items (if Change
        /// Tracking is enabled) are about to change.
        /// </summary>
        public IObservable<IObservedChange<object, object>> Changing {
            get { return _Changing.Where(_ => areChangeNotificationsEnabled);  }
        }

        [IgnoreDataMember]
        protected IObservable<IObservedChange<object, object>> _Changed;

        /// <summary>
        /// Fires when anything in the collection or any of its items (if Change
        /// Tracking is enabled) have changed.
        /// </summary>
        public IObservable<IObservedChange<object, object>> Changed {
            get { return _Changed.Where(_ => areChangeNotificationsEnabled);  }
        }

        [field:IgnoreDataMember]
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Enables the ItemChanging and ItemChanged properties; when this is
        /// enabled, whenever a property on any object implementing
        /// IReactiveNotifyPropertyChanged changes, the change will be
        /// rebroadcast through ItemChanging/ItemChanged.
        /// </summary>
        public bool ChangeTrackingEnabled {
            get { return (propertyChangeWatchers != null); }
            set {
                if ((propertyChangeWatchers != null) == value)
                    return;
                if (propertyChangeWatchers == null) {
                    propertyChangeWatchers = new Dictionary<object, RefcountDisposeWrapper>();
                    foreach (var v in this) {
                        addItemToPropertyTracking(v);
                    }
                } else {
                    releasePropChangeWatchers();
                    propertyChangeWatchers = null;
                }
            }
        }

        [IgnoreDataMember]
        Dictionary<object, RefcountDisposeWrapper> _propertyChangeWatchers;
        Dictionary<object, RefcountDisposeWrapper> propertyChangeWatchers {
            get { return _propertyChangeWatchers; }
            set { _propertyChangeWatchers = value; }
        }

        protected void releasePropChangeWatchers()
        {
            if (propertyChangeWatchers == null) {
                return;
            }

            foreach(var x in propertyChangeWatchers.Values.ToArray()) { x.Release(); }
            propertyChangeWatchers.Clear();
        }

        protected override void InsertItem(int index, T item)
        {
            _BeforeItemsAdded.OnNext(item);
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _BeforeItemsRemoved.OnNext(this[index]);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            _BeforeItemsRemoved.OnNext(this[index]);
            _BeforeItemsAdded.OnNext(item);
            base.SetItem(index, item);
        }

        protected override void ClearItems()
        {
            aboutToClear.OnNext(this.Count);

            // N.B: Reset doesn't give us the items that were cleared out,
            // we have to release the watchers or else we leak them.
            releasePropChangeWatchers();
            base.ClearItems();
        }

        public void Dispose()
        {
            ChangeTrackingEnabled = false;
        }

        [IgnoreDataMember]
        int changeNotificationsSuppressed = 0;

        /// <summary>
        /// When this method is called, an object will not fire change
        /// notifications (neither traditional nor Observable notifications)
        /// until the return value is disposed.
        /// </summary>
        /// <returns>An object that, when disposed, reenables change
        /// notifications.</returns>
        public IDisposable SuppressChangeNotifications()
        {
            Interlocked.Increment(ref changeNotificationsSuppressed);
            return Disposable.Create(() =>
                Interlocked.Decrement(ref changeNotificationsSuppressed));
        }

        protected bool areChangeNotificationsEnabled {
            get { 
                // N.B. On most architectures, machine word aligned reads are 
                // guaranteed to be atomic - sorry WP7, you're out of luck
                return changeNotificationsSuppressed == 0;
            }
        }

        IObservable<object> IReactiveCollection.ItemsAdded {
            get { return ItemsAdded.Select(x => (object) x); }
        }
        IObservable<object> IReactiveCollection.BeforeItemsAdded {
            get { return BeforeItemsAdded.Select(x => (object) x); }
        }
        IObservable<object> IReactiveCollection.ItemsRemoved {
            get { return ItemsRemoved.Select(x => (object) x); }
        }
        IObservable<object> IReactiveCollection.BeforeItemsRemoved {
            get { return BeforeItemsRemoved.Select(x => (object)x); }
        }

#if !SILVERLIGHT
        //
        // N.B: This is a hack to make sure that the ObservableCollection bits 
        // don't end up in the serialized output.
        //

        [field:IgnoreDataMember]
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        [IgnoreDataMember]
        private PropertyChangedEventHandler _propertyChangedEventHandler;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add {
                _propertyChangedEventHandler = 
                    Delegate.Combine(_propertyChangedEventHandler, value) as PropertyChangedEventHandler;
            }
            remove {
                _propertyChangedEventHandler = 
                    Delegate.Remove(_propertyChangedEventHandler, value) as PropertyChangedEventHandler;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;

            if (handler != null) {
                handler(this, e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = _propertyChangedEventHandler;

            if (handler != null) {
                handler(this, e);
            }
        }
#endif
    }

    public static class ReactiveCollectionMixins
    {
        /// <summary>
        /// Creates a collection based on an an Observable by adding items
        /// provided until the Observable completes, optionally ensuring a
        /// delay. Note that if the Observable never completes and withDelay is
        /// set, this method will leak a Timer. This method also guarantees that
        /// items are always added via the UI thread.
        /// </summary>
        /// <param name="fromObservable">The Observable whose items will be put
        /// into the new collection.</param>
        /// <param name="withDelay">If set, items will be populated in the
        /// collection no faster than the delay provided.</param>
        /// <returns>A new collection which will be populated with the
        /// Observable.</returns>
        public static ReactiveCollection<T> CreateCollection<T>(
            this IObservable<T> fromObservable, 
            TimeSpan? withDelay = null)
        {
            var ret = new ReactiveCollection<T>();
            if (withDelay == null) {
                fromObservable.ObserveOn(RxApp.DeferredScheduler).Subscribe(ret.Add);
                return ret;
            }

            // On a timer, dequeue items from queue if they are available
            var queue = new Queue<T>();
            var disconnect = Observable.Timer(withDelay.Value, RxApp.DeferredScheduler)
                .Subscribe(_ => {
                    if (queue.Count > 0) { 
                        ret.Add(queue.Dequeue());
                    }
                });

            // When new items come in from the observable, stuff them in the queue.
            // Using the DeferredScheduler guarantees we'll always access the queue
            // from the same thread.
            fromObservable.ObserveOn(RxApp.DeferredScheduler).Subscribe(queue.Enqueue);

            // This is a bit clever - keep a running count of the items actually 
            // added and compare them to the final count of items provided by the
            // Observable. Combine the two values, and when they're equal, 
            // disconnect the timer
            ret.ItemsAdded.Scan(0, ((acc, _) => acc+1)).Zip(fromObservable.Aggregate(0, (acc,_) => acc+1), 
                (l,r) => (l == r)).Where(x => x).Subscribe(_ => disconnect.Dispose());

            return ret;
        }

        /// <summary>
        /// Creates a collection based on an an Observable by adding items
        /// provided until the Observable completes, optionally ensuring a
        /// delay. Note that if the Observable never completes and withDelay is
        /// set, this method will leak a Timer. This method also guarantees that
        /// items are always added via the UI thread.
        /// </summary>
        /// <param name="fromObservable">The Observable whose items will be put
        /// into the new collection.</param>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <param name="withDelay">If set, items will be populated in the
        /// collection no faster than the delay provided.</param>
        /// <returns>A new collection which will be populated with the
        /// Observable.</returns>
        public static ReactiveCollection<TRet> CreateCollection<T, TRet>(
            this IObservable<T> fromObservable, 
            Func<T, TRet> selector, 
            TimeSpan? withDelay = null)
        {
            Contract.Requires(selector != null);
            return fromObservable.Select(selector).CreateCollection(withDelay);
        }
    }

    public static class ObservableCollectionMixin
    {
        /// <summary>
        /// Creates a collection whose contents will "follow" another
        /// collection; this method is useful for creating ViewModel collections
        /// that are automatically updated when the respective Model collection
        /// is updated.
        /// </summary>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <returns>A new collection whose items are equivalent to
        /// Collection.Select(selector) and will mirror the initial collection.</returns>
        public static ReactiveCollection<TNew> CreateDerivedCollection<T, TNew>(
            this ObservableCollection<T> This, 
            Func<T, TNew> selector)
        {
            Contract.Requires(selector != null);
#if !IOS    // Contract.Result is borked in Mono
            Contract.Ensures(Contract.Result<ReactiveCollection<TNew>>().Count == This.Count);
#endif
            var ret = new ReactiveCollection<TNew>(This.Select(selector));

            var coll_changed = new Subject<NotifyCollectionChangedEventArgs>();
            This.CollectionChanged += (o, e) => coll_changed.OnNext(e);

            /* XXX: Ditto as from above
            var coll_changed = Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                x => This.CollectionChanged += x, x => This.CollectionChanged -= x);
             */

            coll_changed.Subscribe(x => {
                switch(x.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    // NB: SL4 fills in OldStartingIndex with -1 on Replace :-/
                    int old_index = (x.Action == NotifyCollectionChangedAction.Replace ?
                        x.NewStartingIndex : x.OldStartingIndex);

                    if (x.OldItems != null) {
                        foreach(object _ in x.OldItems) {
                            ret.RemoveAt(old_index);
                        }
                    }
                    if (x.NewItems != null) {
                        foreach(T item in x.NewItems.Cast<T>()) {
                            ret.Insert(x.NewStartingIndex, selector(item));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ret.Clear();
                    break;
                default:
                    break;
                }
            });

            return ret;
        }
    }
}

// vim: tw=120 ts=4 sw=4 et :
