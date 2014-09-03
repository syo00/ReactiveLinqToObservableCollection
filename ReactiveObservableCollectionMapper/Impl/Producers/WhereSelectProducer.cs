using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection.Support;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LightWands;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class WhereSelectProducer<T, TTo> : Producer<SimpleNotifyCollectionChangedEvent<TTo>>
    {
        // source.InitialStateAndChanged のうち、Convert によって除外されたものを HasValue == false に置き換えたものを示す
        readonly List<ValueOrEmpty<Tagged<TTo>>> sourceInitialStateAndChanged = new List<ValueOrEmpty<Tagged<TTo>>>();
        readonly CollectionStatuses<T> source;
        readonly Func<T, bool> predicate;
        readonly Func<T, TTo> converter;

        public WhereSelectProducer(CollectionStatuses<T> source, Func<T, bool> predicate, Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Requires<ArgumentNullException>(converter != null);

            this.source = source;
            this.predicate = predicate;
            this.converter = converter;
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SimpleNotifyCollectionChangedEvent<TTo>> observer)
        {
            return source
                .SimpleInitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    switch (e.Action)
                    {
                        case SimpleNotifyCollectionChangedEventAction.InitialState:
                            var initialStateItems = InitialStateOrReset(e.InitialStateOrReset);
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<TTo>.CreateInitialState(initialStateItems));
                            return;
                        case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                            var newAddedAndRemoved = e.AddedOrRemoved
                                .Select(x => x.Type == AddOrRemoveUnitType.Add ? Insert(x.Index, x.Item) : Remove(x.Index))
                                .Where(x => x != null)
                                .ToArray()
                                .ToReadOnly();
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<TTo>.CreateAddOrRemove(newAddedAndRemoved));
                            return;
                        case SimpleNotifyCollectionChangedEventAction.Reset:
                            var resetItems = InitialStateOrReset(e.InitialStateOrReset);
                            observer.OnNext(SimpleNotifyCollectionChangedEvent<TTo>.CreateReset(resetItems));
                            return;
                    }
                }, observer.OnError, observer.OnCompleted);
        }

        IReadOnlyList<Tagged<TTo>> InitialStateOrReset(IEnumerable<Tagged<T>> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(items, item => item != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<Tagged<TTo>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<Tagged<TTo>>>(), item => item != null));

            sourceInitialStateAndChanged.Clear();
            var newInitialStateItems = items
                .Select((x, i) => Insert(i, x))
                .Where(x => x != null)
                .Select(x => x.Item)
                .ToArray()
                .ToReadOnly();
            return newInitialStateItems;
        }

        AddedOrRemovedUnit<TTo> Insert(int index, Tagged<T> item)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);

            if (predicate(item.Item))
            {
                var newIndex = sourceInitialStateAndChanged.Take(index).Where(x => x.HasValue).Count();
                var convertedItem = new Tagged<TTo>(converter(item.Item), item.Tag);
                sourceInitialStateAndChanged.Insert(index, new ValueOrEmpty<Tagged<TTo>>(convertedItem));
                return new AddedOrRemovedUnit<TTo>(AddOrRemoveUnitType.Add, convertedItem, newIndex);
            }
            else
            {
                sourceInitialStateAndChanged.Insert(index, new ValueOrEmpty<Tagged<TTo>>());
                return null;
            }
        }

        AddedOrRemovedUnit<TTo> Remove(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);

            var removingItem = sourceInitialStateAndChanged[index];
            if (!removingItem.HasValue)
            {
                sourceInitialStateAndChanged.RemoveAt(index);
                return null;
            }

            var newIndex = sourceInitialStateAndChanged.Take(index).Where(x => x.HasValue).Count();
            sourceInitialStateAndChanged.RemoveAt(index);
            return new AddedOrRemovedUnit<TTo>(AddOrRemoveUnitType.Remove, removingItem.Value, newIndex);
        }
    }
}
