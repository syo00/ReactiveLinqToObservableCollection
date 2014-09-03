using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class FlattenProducer<T> : Producer<INotifyCollectionChangedEvent<T>>
    {
        private readonly CollectionStatuses<CollectionStatuses<T>> source;
        private readonly List<Box<List<T>>> currentItems = new List<Box<List<T>>>();
        private readonly List<Reference<IDisposable>> innerSubscriptions = new List<Reference<IDisposable>>();
        private readonly List<INotifyCollectionChangedEvent<T>> cachedEvents = new List<INotifyCollectionChangedEvent<T>>();
        private bool pausePushingEvents;
        private bool isInitialStatePushed;

        public FlattenProducer(CollectionStatuses<CollectionStatuses<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(currentItems != null);
            Contract.Invariant(Contract.ForAll(currentItems, x => x != null));
            Contract.Invariant(cachedEvents != null);
            Contract.Invariant(Contract.ForAll(cachedEvents, x => x != null));
            Contract.Invariant(innerSubscriptions != null);
            Contract.Invariant(Contract.ForAll(innerSubscriptions, x => x != null));
        }


        protected override void OnCompleted()
        {
            innerSubscriptions.Where(x => x.Value != null).ForEach(d => d.Value.Dispose());
        }

        protected override void OnError(Exception e)
        {
            innerSubscriptions.Where(x => x.Value != null).ForEach(d => d.Value.Dispose());
        }

        protected override IDisposable SubscribeCore(ProducerObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            return source
                .InitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(sourceEvent =>
                {
                    switch (sourceEvent.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            {
                                pausePushingEvents = true;
                                sourceEvent.InitialState.Items
                                    .ForEach((sourceInitialStateItem, i) =>
                                    {
                                        AddItem(i, sourceInitialStateItem, observer);
                                    });
                                pausePushingEvents = false;
                                TryRaiseEvent(observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                pausePushingEvents = true;
                                sourceEvent.Added.Items.ForEach((sourceAddedItem, i) =>
                                {
                                    AddItem(sourceEvent.Added.StartingIndex + i, sourceAddedItem, observer);
                                });
                                pausePushingEvents = false;
                                TryRaiseEvent(observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                pausePushingEvents = true;
                                sourceEvent.Removed.Items.ForEach(_ =>
                                {
                                    RemoveItem(sourceEvent.Removed.StartingIndex, observer);
                                });
                                pausePushingEvents = false;
                                TryRaiseEvent(observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                var oldIndex = ObtainLeftItemsInCurrentItemsCount(sourceEvent.Moved.OldStartingIndex);
                                var removingItems = currentItems.RemoveAtRange(sourceEvent.Moved.OldStartingIndex, sourceEvent.Moved.Items.Count);

                                var newIndex = ObtainLeftItemsInCurrentItemsCount(sourceEvent.Moved.NewStartingIndex);
                                currentItems.InsertRange(sourceEvent.Moved.NewStartingIndex, removingItems);

                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateMovedEvent(removingItems.Where(x => x.HasValue).SelectMany(x => x.Value).ToArray().ToReadOnly(), oldIndex, newIndex), observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                pausePushingEvents = true;
                                sourceEvent.Replaced.OldItems.ForEach(_ =>
                                {
                                    RemoveItem(sourceEvent.Replaced.StartingIndex, observer);
                                });
                                sourceEvent.Replaced.NewItems.ForEach((sourceAddedItem, i) =>
                                {
                                    AddItem(sourceEvent.Replaced.StartingIndex + i, sourceAddedItem, observer);
                                });
                                pausePushingEvents = false;
                                TryRaiseEvent(observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                currentItems.Clear();
                                innerSubscriptions.Where(d => d.Value != null).ForEach(d => d.Value.Dispose());
                                innerSubscriptions.Clear();

                                pausePushingEvents = true;
                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateResetEvent(new T[0]), observer);
                                sourceEvent.Reset.Items
                                    .ForEach((sourceInitialStateItem, i) =>
                                    {
                                        AddItem(i, sourceInitialStateItem, observer);
                                    });
                                pausePushingEvents = false;

                                TryRaiseEvent(observer);
                                return;
                            }
                    }
                }, observer.OnError, observer.OnCompleted);
        }

        private void TryRaiseEvent(INotifyCollectionChangedEvent<T> e, IObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Requires<ArgumentNullException>(observer != null);

            cachedEvents.Add(e);
            TryRaiseEvent(observer);
        }

        private void TryRaiseEvent(IObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            Contract.Requires<ArgumentNullException>(observer != null);

            if (pausePushingEvents) return;

            if (currentItems.All(b => b.HasValue))
            {
                if (!isInitialStatePushed && cachedEvents.Count == 0)
                {
                    observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new T[0].ToReadOnly()));
                    isInitialStatePushed = true;
                    return;
                }

                EventsConverter.Combine(cachedEvents).ForEach(e =>
                    {
                        if (!isInitialStatePushed)
                        {
                            if (e.Action != NotifyCollectionChangedEventAction.Add || e.Added.StartingIndex != 0)
                            {
                                throw new InvalidOperationException();
                            }
                            observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(e.Added.Items));
                            isInitialStatePushed = true;
                            return;
                        }
                        observer.OnNext(e);
                    });
                cachedEvents.Clear();
            }
        }

        private void AddItem(int index, CollectionStatuses<T> item, IObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Requires<ArgumentNullException>(observer != null);

            var singleCurrentItems = new Box<List<T>>();
            currentItems.Insert(index, singleCurrentItems);
            var subscription = new Reference<IDisposable>();
            innerSubscriptions.Add(subscription);

            subscription.Value = item
                .InitialStateAndChanged
                .Subscribe(e =>
                {
                    var leftItemsCount = ObtainLeftItemsInCurrentItemsCount(singleCurrentItems);
                    if (leftItemsCount == null)
                    {
                        observer.OnError(new InvalidOperationException());
                        return;
                    }

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            {
                                singleCurrentItems.Value = e.InitialState.Items.ToList();
                                singleCurrentItems.HasValue = true;
                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateAddedEvent(e.InitialState.Items, leftItemsCount.Value), observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                singleCurrentItems.Value.AddRange(e.Added.Items);
                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateAddedEvent(e.Added.Items, e.Added.StartingIndex + leftItemsCount.Value), observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                singleCurrentItems.Value.RemoveAtRange(e.Removed.StartingIndex, e.Removed.Items.Count);
                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateRemovedEvent(e.Removed.Items, e.Removed.StartingIndex + leftItemsCount.Value), observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                singleCurrentItems.Value.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);
                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateMovedEvent(e.Moved.Items, e.Moved.OldStartingIndex + leftItemsCount.Value, e.Moved.NewStartingIndex + leftItemsCount.Value), observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                singleCurrentItems.Value.RemoveAtRange(e.Replaced.StartingIndex, e.Replaced.OldItems.Count);
                                singleCurrentItems.Value.InsertRange(e.Replaced.StartingIndex, e.Replaced.NewItems);
                                TryRaiseEvent(NotifyCollectionChangedEvent.CreateReplacedEvent(e.Replaced.OldItems, e.Replaced.NewItems, e.Replaced.StartingIndex + leftItemsCount.Value), observer);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                var replaced = NotifyCollectionChangedEvent.CreateReplacedEvent(singleCurrentItems.Value.ToArray().ToReadOnly(), e.Reset.Items, leftItemsCount.Value);
                                singleCurrentItems.Value.Clear();
                                TryRaiseEvent(replaced, observer);
                                return;
                            }
                        default:
                            {
                                return;
                            }
                    }
                }, observer.OnError);
        }

        private void RemoveItem(int index, IObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            Contract.Requires<ArgumentNullException>(observer != null);

            if (innerSubscriptions[index].Value != null)
            {
                innerSubscriptions[index].Value.Dispose();
            }
            innerSubscriptions.RemoveAt(index);

            if (currentItems.Count <= index)
            {
                observer.OnError(new InvalidOperationException());
                return;
            }

            var leftItemsCount = ObtainLeftItemsInCurrentItemsCount(currentItems[index]);
            if (leftItemsCount == null)
            {
                observer.OnError(new InvalidOperationException());
                return;
            }
            var removingItem = currentItems[index];
            currentItems.RemoveAt(index);
            if (removingItem.HasValue)
            {
                var removed = NotifyCollectionChangedEvent.CreateRemovedEvent(removingItem.Value.ToArray().ToReadOnly(), leftItemsCount.Value);

                TryRaiseEvent(removed, observer);
                return;
            }
        }

        /// <summary>sourceItems の要素のリストから、そのリストの先頭の要素が、平坦化された sourceItems でどの位置にあるかを求めて返します。sourceItems の要素のリストの HasValue が false の場合、空リストとみなします。</summary>
        private int? ObtainLeftItemsInCurrentItemsCount(Box<List<T>> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var index = currentItems.IndexOf(item);
            if (index < 0) return null;
            return ObtainLeftItemsInCurrentItemsCount(index);
        }

        /// <summary>sourceItems のインデックスから、そのインデックスにあるリストの先頭の要素が、平坦化された sourceItems でどの位置にあるかを求めて返します。sourceItems の要素のリストの HasValue が false の場合、空リストとみなします。</summary>
        private int ObtainLeftItemsInCurrentItemsCount(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return currentItems
                .Take(index)
                .Where(x => x.HasValue)
                .SelectMany(x => x.Value)
                .Count();
        }
    }
}
