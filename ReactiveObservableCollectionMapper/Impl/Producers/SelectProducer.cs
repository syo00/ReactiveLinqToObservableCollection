using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class SelectProducer<T, TTo> : Producer<SlimNotifyCollectionChangedEvent<TTo>>
    {
        private readonly CollectionStatuses<T> source;
        private readonly Func<T, TTo> converter;

        public SelectProducer(CollectionStatuses<T> source, Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);

            this.source = source;
            this.converter = converter;
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SlimNotifyCollectionChangedEvent<TTo>> observer)
        {
            return source
                .SlimInitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    try
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedEventAction.InitialState:
                                {
                                    var initialState = new SlimInitialState<TTo>(e.InitialState.Items.Select(converter).ToArray().ToReadOnly());
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<TTo>(initialState));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Add:
                                {
                                    var added = new SlimAdded<TTo>(e.Added.Items.Select(converter).ToArray().ToReadOnly(), e.Added.StartingIndex);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<TTo>(added));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Move:
                                {
                                    var moved = new SlimMoved(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.ItemsCount);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<TTo>(moved));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Remove:
                                {
                                    var removed = new SlimRemoved(e.Removed.StartingIndex, e.Removed.ItemsCount);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<TTo>(removed));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Replace:
                                {
                                    var replaced = new SlimReplaced<TTo>(e.Replaced.StartingIndex, e.Replaced.OldItemsCount, e.Replaced.NewItems.Select(converter).ToArray().ToReadOnly());
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<TTo>(replaced));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Reset:
                                {
                                    var reset = new SlimReset<TTo>(e.Reset.Items.Select(converter).ToArray().ToReadOnly());
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<TTo>(reset));
                                    return;
                                }
                            default:
                                {
                                    throw Exceptions.UnpredictableSwitchCasePattern;
                                }
                        }
                    }
                    catch (Abort.AbortException ex)
                    {
                        ex.PushToObserver(observer);
                    }
                }, observer.OnError, observer.OnCompleted);
        }
    }
}
