using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class TakeProducer<T> : Producer<SimpleNotifyCollectionChangedEvent<T>>
    {
        readonly CollectionStatuses<T> source;
        readonly int takeCount;
        readonly TaggedCollection<T> currentItems = new TaggedCollection<T>();

        public TakeProducer(CollectionStatuses<T> source, int takeCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(takeCount >= 1);

            this.source = source;
            this.takeCount = takeCount;
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

                                var newEvent = SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(e.InitialStateOrReset.Take(takeCount).ToArray().ToReadOnly());
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
                                        if (u.Index < takeCount)
                                        {
                                            if (currentItems.Count >= takeCount)
                                            {
                                                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, currentItems[takeCount - 1], takeCount - 1));
                                            }

                                            result.Add(u);
                                        }

                                        currentItems.Insert(u.Index, u.Item);
                                    }
                                    else
                                    {
                                        if (u.Index < takeCount)
                                        {
                                            result.Add(u);

                                            if (currentItems.Count > takeCount)
                                            {
                                                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, currentItems[takeCount], takeCount - 1));
                                            }
                                        }

                                        currentItems.RemoveAt(u.Index);
                                    }
                                }

                                var newEvent = SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result.ToReadOnly());
                                observer.OnNext(newEvent);
                                return;
                            }
                        case SimpleNotifyCollectionChangedEventAction.Reset:
                            {
                                currentItems.Clear();
                                currentItems.AddRange(e.InitialStateOrReset);

                                var newEvent = SimpleNotifyCollectionChangedEvent<T>.CreateReset(e.InitialStateOrReset.Take(takeCount).ToArray().ToReadOnly());
                                observer.OnNext(newEvent);

                                return;
                            }
                    }
                }, observer.OnError, observer.OnCompleted);
        }
    }
}

