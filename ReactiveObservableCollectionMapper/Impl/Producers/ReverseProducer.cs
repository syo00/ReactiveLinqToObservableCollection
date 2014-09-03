using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class ReverseProducer<T> : Producer<SlimNotifyCollectionChangedEvent<T>>
    {
        int collectionCount = 0;
        readonly CollectionStatuses<T> source;

        public ReverseProducer(CollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SlimNotifyCollectionChangedEvent<T>> observer)
        {
            return source
                .SlimInitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            {
                                collectionCount = e.InitialState.Items.Count;

                                var newItems = e.InitialState.Items.Reverse().ToArray().ToReadOnly();
                                var initialState = new SlimInitialState<T>(newItems);
                                observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(initialState));
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                var added = Insert(e.Added.StartingIndex, e.Added.Items);
                                observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(added));

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                var removed = Remove(e.Removed.StartingIndex, e.Removed.ItemsCount);
                                observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(removed));

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                var removedIndex = Remove(e.Moved.OldStartingIndex, e.Moved.ItemsCount).StartingIndex;
                                var addedIndex = Insert(e.Moved.NewStartingIndex, e.Moved.ItemsCount);

                                var moved = new SlimMoved(removedIndex, addedIndex, e.Moved.ItemsCount);
                                observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(moved));
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                collectionCount = e.Reset.Items.Count;

                                var newItems = e.Reset.Items.Reverse().ToArray().ToReadOnly();
                                var reset = new SlimReset<T>(newItems);
                                observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(reset));
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                var removed = Remove(e.Replaced.StartingIndex, e.Replaced.OldItemsCount);
                                var added = Insert(e.Replaced.StartingIndex, e.Replaced.NewItems);

                                var replaced = new SlimReplaced<T>(removed.StartingIndex, removed.ItemsCount, added.Items);
                                observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(replaced));
                                return;
                            }
                        default:
                            {
                                throw Exceptions.UnpredictableSwitchCasePattern;
                            }
                    }
                }, observer.OnError, observer.OnCompleted);
        }

        SlimAdded<T> Insert(int index, IReadOnlyCollection<T> items)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<SlimAdded<T>>() != null);

            var newIndex = collectionCount - index;
            var addedItems = items.Reverse().ToArray().ToReadOnly();
            collectionCount += items.Count;

            return new SlimAdded<T>(addedItems, newIndex);
        }

        int Insert(int index, int itemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);

            var newIndex = collectionCount - index;
            collectionCount += itemsCount;

            return newIndex;
        }

        SlimRemoved Remove(int index, int itemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);
            Contract.Ensures(Contract.Result<SlimRemoved>() != null);

            var newIndex = collectionCount - index - itemsCount;
            collectionCount -= itemsCount;
            return new SlimRemoved(newIndex, itemsCount);
        }
    }
}
