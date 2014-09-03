using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Kirinji.LightWands;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class SkipProducer<T> : Producer<SimpleNotifyCollectionChangedEvent<T>>
    {
        readonly CollectionStatuses<T> source;
        readonly int skipCount;
        readonly List<Tagged<T>> currentItems = new List<Tagged<T>>();

        public SkipProducer(CollectionStatuses<T> source, int skipCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(skipCount >= 1);

            this.source = source;
            this.skipCount = skipCount;
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SimpleNotifyCollectionChangedEvent<T>> observer)
        {
            return source
                .SimpleInitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                    {
                        switch (e.Action)
                        {
                            case SimpleNotifyCollectionChangedEventAction.InitialState:
                                {
                                    currentItems.AddRange(e.InitialStateOrReset);

                                    var newEvent = SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(e.InitialStateOrReset.Skip(skipCount).ToArray().ToReadOnly());
                                    observer.OnNext(newEvent);

                                    return;
                                }
                            case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                                {
                                    var result = new List<AddedOrRemovedUnit<T>>();

                                    foreach (var u in e.AddedOrRemoved)
                                    {
                                        if (u.Type == AddOrRemoveUnitType.Add)
                                        {
                                            var newIndex = u.Index - skipCount;

                                            if (newIndex >= 0)
                                            {
                                                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, u.Item, newIndex));
                                            }
                                            else
                                            {
                                                if (currentItems.Count >= skipCount)
                                                {
                                                    result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, currentItems[skipCount - 1], 0));
                                                }
                                            }

                                            currentItems.Insert(u.Index, u.Item);
                                        }
                                        else
                                        {
                                            var newIndex = u.Index - skipCount;

                                            if (newIndex >= 0)
                                            {
                                                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, u.Item, newIndex));
                                            }
                                            else
                                            {
                                                if (currentItems.Count > skipCount)
                                                {
                                                    result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, currentItems[skipCount], 0));
                                                }
                                            }

                                            currentItems.RemoveAt(u.Index);
                                        }
                                    }

                                    var newEvent = SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result.ToReadOnly());
                                    observer.OnNext(newEvent);
                                }

                                return;
                            case SimpleNotifyCollectionChangedEventAction.Reset:
                                {
                                    currentItems.Clear();
                                    currentItems.AddRange(e.InitialStateOrReset);

                                    var newEvent = SimpleNotifyCollectionChangedEvent<T>.CreateReset(e.InitialStateOrReset.Skip(skipCount).ToArray().ToReadOnly());
                                    observer.OnNext(newEvent);

                                    return;
                                }
                            default:
                                {
                                    throw Exceptions.UnpredictableSwitchCasePattern;
                                }
                        }
                    }, observer.OnError, observer.OnCompleted);
        }
    }
}
