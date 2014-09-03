using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class CastProducer<T, TTo> : Producer<SlimNotifyCollectionChangedEvent<TTo>>
    {
        private readonly CollectionStatuses<T> source;
        bool skipOnNext;

        public CastProducer(CollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SlimNotifyCollectionChangedEvent<TTo>> observer)
        {
            return source
                .SlimInitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    if(skipOnNext)
                    {
                        return;
                    }

                    SlimNotifyCollectionChangedEvent<TTo> newEvent;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            {
                                IReadOnlyList<TTo> newItems;
                                try
                                {
                                    newItems = e.InitialState.Items.Cast<TTo>().ToArray().ToReadOnly();
                                }
                                catch(InvalidCastException ex)
                                {
                                    observer.OnError(ex);
                                    skipOnNext = true;
                                    return;
                                }

                                var initialState = new SlimInitialState<TTo>(newItems);
                                newEvent = new SlimNotifyCollectionChangedEvent<TTo>(initialState);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                IReadOnlyList<TTo> newItems;
                                try
                                {
                                    newItems = e.Added.Items.Cast<TTo>().ToArray().ToReadOnly();
                                }
                                catch (InvalidCastException ex)
                                {
                                    observer.OnError(ex);
                                    skipOnNext = true;
                                    return;
                                }

                                var added = new SlimAdded<TTo>(newItems, e.Added.StartingIndex);
                                newEvent = new SlimNotifyCollectionChangedEvent<TTo>(added);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                newEvent = new SlimNotifyCollectionChangedEvent<TTo>(e.Moved);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                newEvent = new SlimNotifyCollectionChangedEvent<TTo>(e.Removed);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                IReadOnlyList<TTo> newItems;
                                try
                                {
                                    newItems = e.Replaced.NewItems.Cast<TTo>().ToArray().ToReadOnly();
                                }
                                catch (InvalidCastException ex)
                                {
                                    observer.OnError(ex);
                                    skipOnNext = true;
                                    return;
                                }

                                var replaced = new SlimReplaced<TTo>(e.Replaced.StartingIndex, e.Replaced.OldItemsCount, newItems);
                                newEvent = new SlimNotifyCollectionChangedEvent<TTo>(replaced);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                IReadOnlyList<TTo> newItems;
                                try
                                {
                                    newItems = e.Reset.Items.Cast<TTo>().ToArray().ToReadOnly();
                                }
                                catch (InvalidCastException ex)
                                {
                                    observer.OnError(ex);
                                    skipOnNext = true;
                                    return;
                                }

                                var reset = new SlimReset<TTo>(newItems);
                                newEvent = new SlimNotifyCollectionChangedEvent<TTo>(reset);
                                break;
                            }
                        default:
                            {
                                throw Exceptions.UnpredictableSwitchCasePattern;
                            }
                    }

                    observer.OnNext(newEvent);

                }, observer.OnError, observer.OnCompleted);
        }
    }
}
