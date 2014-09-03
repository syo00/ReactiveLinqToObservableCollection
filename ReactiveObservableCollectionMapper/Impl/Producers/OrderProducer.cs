using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class OrderProducer<T> : Producer<SimpleNotifyCollectionChangedEvent<T>>
    {
        private readonly CollectionStatuses<T> source;
        private readonly IOrderingComparer<T> order;
        private List<KeyValuePair<int, T>> ordered = new List<KeyValuePair<int, T>>(); // order した状態の現在のコレクションの状態を表す。キーは notOrdered における位置を表します。
        private List<T> notOrdered = new List<T>(); // order していない状態の現在のコレクションの状態を表す。

        public OrderProducer(CollectionStatuses<T> source, IOrderingComparer<T> order)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(order != null);

            this.source = source;
            this.order = order;
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SimpleNotifyCollectionChangedEvent<T>> observer)
        {
            return source
                .SimpleInitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(sourceEvent =>
                {
                    switch (sourceEvent.Action)
                    {
                        case SimpleNotifyCollectionChangedEventAction.InitialState:
                            var initialStateItems = InitialStateOrReset(sourceEvent.InitialStateOrReset);
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(initialStateItems));
                            return;
                        case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                            var result = new List<AddedOrRemovedUnit<T>>();
                            foreach (var e in sourceEvent.AddedOrRemoved)
                            {
                                if (e.Type == AddOrRemoveUnitType.Add)
                                {
                                    result.Add(Insert(e.Index, e.Item));
                                }
                                else
                                {
                                    var index = RemoveAt(e.Index);
                                    result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, e.Item, index));
                                }
                            }
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result.ToReadOnly()));
                            return;
                        case SimpleNotifyCollectionChangedEventAction.Reset:
                            var resetItems = InitialStateOrReset(sourceEvent.InitialStateOrReset);
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateReset(resetItems));
                            return;
                    }
                }, observer.OnError, observer.OnCompleted);
        }

        private int FindInsertingIndex(T value)
        {
            var source = ordered.Select(x => x.Value).ToArray().ToReadOnly();

            var leftIndex = 0;
            var rightIndex = source.Count;

            while (true)
            {
                int comparingIndex = (leftIndex + rightIndex) / 2;
                if (leftIndex == rightIndex)
                {
                    return comparingIndex;
                }

                var compared = order.Compare(source[comparingIndex], value);
                if (compared > 0)
                {
                    rightIndex = comparingIndex;
                }
                else if (compared == 0)
                {
                    return comparingIndex;
                }
                else
                {
                    if (rightIndex - leftIndex == 1)
                    {
                        return comparingIndex + 1;
                    }

                    leftIndex = comparingIndex;
                }
            }
        }

        private IReadOnlyList<Tagged<T>> InitialStateOrReset(IReadOnlyList<Tagged<T>> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(items, x => x != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<Tagged<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<Tagged<T>>>(), x => x != null));

            ordered.Clear();
            notOrdered.Clear();
            var converted =
               order.Order(items.Select((x, i) => new KeyValuePair<int, Tagged<T>>(i, x)), pair => pair.Value.Item)
                .ToArray()
                .ToReadOnly();
            notOrdered =
                items
                .Select(tagged => tagged.Item)
                .ToList();
            ordered = converted
                .Select(pair => new KeyValuePair<int, T>(pair.Key, pair.Value.Item))
                .ToList();
            return converted.Select(pair => pair.Value).ToArray().ToReadOnly();
        }

        private AddedOrRemovedUnit<T> Insert(int index, Tagged<T> item)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Ensures(Contract.Result<AddedOrRemovedUnit<T>>() != null);

            notOrdered.Insert(index, item.Item);

            var convertedInsertingIndex = FindInsertingIndex(item.Item);
            ordered = ordered.Select(pair => pair.Key >= index ? new KeyValuePair<int, T>(pair.Key + 1, pair.Value) : pair).ToList();
            ordered.Insert(convertedInsertingIndex, new KeyValuePair<int, T>(index, item.Item));
            return new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, item, convertedInsertingIndex);
        }

        private int RemoveAt(int index)
        {
            var removed = notOrdered[index];
            notOrdered.RemoveAt(index);

            var convertedRemovingIndex = ordered.FirstIndex(pair => pair.Key == index).Value;
            ordered.RemoveAt(convertedRemovingIndex);
            ordered = ordered.Select(pair => pair.Key > index ? new KeyValuePair<int, T>(pair.Key - 1, pair.Value) : pair).ToList();

            return convertedRemovingIndex;
        }
    }
}
