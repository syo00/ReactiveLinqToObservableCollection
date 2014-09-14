using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    // results are not ordered 
    class DistinctProducer<T> : Producer<SimpleNotifyCollectionChangedEvent<T>>
    {
        private readonly CollectionStatuses<T> source;
        readonly List<ValueAndCount> valuesCount = new List<ValueAndCount>();
        readonly IEqualityComparer<T> comparer;

        public DistinctProducer(CollectionStatuses<T> source, IEqualityComparer<T> comparer = null)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
            this.comparer = comparer ?? EqualityComparer<T>.Default;
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
                            var initialStateItems = InitialStateAndReset(e.InitialStateOrReset);
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(initialStateItems));
                            return;
                        case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                            var result = new List<AddedOrRemovedUnit<T>>();
                            foreach (var i in e.AddedOrRemoved)
                            {
                                if (i.Type == AddOrRemoveUnitType.Add)
                                {
                                    var addedIndex = AddItem(i.Item);
                                    if (addedIndex != null)
                                    {
                                        result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, i.Item, addedIndex.Value));
                                    }
                                }
                                else
                                {
                                    var removedUnit = RemoveItem(i.Item);
                                    if (removedUnit != null)
                                    {
                                        result.Add(removedUnit);
                                    }
                                }
                            }

                            if (result.Count >= 1)
                            {
                                observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result.ToReadOnly()));
                            }
                            return;
                        case SimpleNotifyCollectionChangedEventAction.Reset:
                            var resetItems = InitialStateAndReset(e.InitialStateOrReset);
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateReset(resetItems));
                            return;
                    }
                }, observer.OnError, observer.OnCompleted);
        }

        int? AddItem(Tagged<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var matchedIndex = valuesCount.FirstIndex(x => comparer.Equals(x.Value.Item, item.Item));
            if(matchedIndex != null)
            {
                valuesCount[matchedIndex.Value].Count++;
                return null;
            }
            valuesCount.Add(new ValueAndCount(item, 1));
            return valuesCount.Count - 1;
        }

        AddedOrRemovedUnit<T> RemoveItem(Tagged<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var matchedIndex = valuesCount.FirstIndex(x => comparer.Equals(x.Value.Item, item.Item));
            if (matchedIndex == null) return null;

            if (valuesCount[matchedIndex.Value].Count == 1)
            {
                var removedItem = valuesCount[matchedIndex.Value].Value;
                valuesCount.RemoveAt(matchedIndex.Value);
                return new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, removedItem, matchedIndex.Value);
            }
            valuesCount[matchedIndex.Value].Count--;
            return null;
        }

        IReadOnlyList<T> InitialStateAndReset(IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            valuesCount.Clear();
            return items
                .Where(i => AddItem(new Tagged<T>(i)) != null)
                .ToArray()
                .ToReadOnly();
        }

        class ValueAndCount
        {
            public ValueAndCount(Tagged<T> value, int count)
            {
                Contract.Requires<ArgumentNullException>(value != null);
                Contract.Requires<ArgumentOutOfRangeException>(count >= 1);

                this.value = value;
                this.Count = count;
            }

            readonly Tagged<T> value;
            public Tagged<T> Value
            {
                get
                {
                    Contract.Ensures(Contract.Result<Tagged<T>>() != null);

                    return value;
                }
            }

            int count;
            public int Count
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() >= 1);

                    return count;
                }
                set
                {
                    Contract.Requires<ArgumentOutOfRangeException>(value >= 1);

                    count = value;
                }
            }
        }
    }
}
