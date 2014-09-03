using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;
using System.Reactive.Concurrency;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    // results are not ordered and not distincted like Enumerable.Except
    class ExceptProducer<T, TSecond> : CombineProducer<T, TSecond, T>
    {
        readonly List<Tagged<T>> currentItems = new List<Tagged<T>>();

        public ExceptProducer(CollectionStatuses<T> source, CollectionStatuses<TSecond> second, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading, Func<T, TSecond, bool> comparer)
            : base(source, second, schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.comparer = comparer;
        }

        Func<T, TSecond, bool> comparer;
        Func<T, TSecond, bool> Comparer
        {
            get
            {
                Contract.Ensures(Contract.Result<Func<T, TSecond, bool>>() != null);

                return comparer;
            }
        }

        protected override IReadOnlyList<Tagged<T>> ConvertInitialState(IReadOnlyList<Tagged<T>> initialLeftCollection, IReadOnlyList<Tagged<TSecond>> initialRightCollection)
        {
            return InitialStateOrReset(initialLeftCollection, initialRightCollection);
        }

        protected override IReadOnlyList<AddedOrRemovedUnit<T>> ConvertLeftUnits(IReadOnlyList<AddedOrRemovedUnit<T>> leftEvent)
        {
            var result = new List<AddedOrRemovedUnit<T>>();

            foreach(var e in leftEvent)
            {
                if(e.Type == AddOrRemoveUnitType.Add)
                {
                    var items = OnLeftItemAdded(e.Item);
                    if (items != null)
                    {
                        result.Add(items);
                    }
                }
                else
                {
                    var items = OnLeftItemRemoved(e.Item);
                    if (items != null)
                    {
                        result.Add(items);
                    }
                }
            }

            return result.ToReadOnly();
        }

        protected override SimpleNotifyCollectionChangedEvent<T> ConvertLeftReset(IReadOnlyList<Tagged<T>> newLeftItems)
        {
            var items = InitialStateOrReset(newLeftItems, RightCollection);
            return SimpleNotifyCollectionChangedEvent<T>.CreateReset(items);
        }

        protected override IReadOnlyList<AddedOrRemovedUnit<T>> ConvertRightUnits(IReadOnlyList<AddedOrRemovedUnit<TSecond>> rightEvent)
        {
            var result = new List<AddedOrRemovedUnit<T>>();

            foreach (var e in rightEvent)
            {
                if (e.Type == AddOrRemoveUnitType.Add)
                {
                    var items = OnRightItemAdded(e.Item);
                    result.AddRange(items);

                }
                else
                {
                    var items = OnRightItemRemoved(e.Item);
                    result.AddRange(items);
                }
            }

            return result.ToReadOnly();
        }

        protected override SimpleNotifyCollectionChangedEvent<T> ConvertRightReset(IReadOnlyList<Tagged<TSecond>> newRightItems)
        {
            var items = InitialStateOrReset(LeftCollection, newRightItems);
            return SimpleNotifyCollectionChangedEvent<T>.CreateReset(items);
        }

        private IReadOnlyList<Tagged<T>> InitialStateOrReset(IReadOnlyList<Tagged<T>> leftCollection, IReadOnlyList<Tagged<TSecond>> rightCollection)
        {
            Contract.Requires<ArgumentNullException>(leftCollection != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(leftCollection, item => item != null));
            Contract.Requires<ArgumentNullException>(rightCollection != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(rightCollection, item => item != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<Tagged<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<Tagged<T>>>(), item => item != null));

            currentItems.Clear();
            var result = leftCollection
                .Where(x => !rightCollection.Any(y => Comparer(x.Item, y.Item)))
                .ToArray()
                .ToReadOnly();
            currentItems.AddRange(result);
            return result;
        }

        private AddedOrRemovedUnit<T> OnLeftItemAdded(Tagged<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            if (RightCollection.Any(x => Comparer(item.Item, x.Item)))
            {
                return null;
            }

            var result = new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, item, currentItems.Count);
            currentItems.Add(item);
            return result;
        }

        private AddedOrRemovedUnit<T> OnLeftItemRemoved(Tagged<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var matchedIndex = currentItems.FirstIndex(x => Object.Equals(x, item));
            if (matchedIndex != null)
            {
                currentItems.RemoveAt(matchedIndex.Value);
                return new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, item, matchedIndex.Value);
            }
            return null;
        }

        private IReadOnlyList<AddedOrRemovedUnit<T>> OnRightItemAdded(Tagged<TSecond> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);

            return OnRightItemAddedCore(item)
                .ToArray()
                .ToReadOnly();
        }

        private IEnumerable<AddedOrRemovedUnit<T>> OnRightItemAddedCore(Tagged<TSecond> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Ensures(Contract.Result<IEnumerable<AddedOrRemovedUnit<T>>>() != null);

            var matchedItems = currentItems
                .Where(x => Comparer(x.Item, item.Item))
                .ToArray();
            foreach (var m in matchedItems)
            {
                var index = currentItems.IndexOf(m);
                yield return new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, m, index);
                currentItems.RemoveAt(index);
            }
        }

        private IReadOnlyList<AddedOrRemovedUnit<T>> OnRightItemRemoved(Tagged<TSecond> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);
            
            var addingItems =
                LeftCollection
                .Except(currentItems)
                .Where(x => Comparer(x.Item, item.Item))
                .ToArray();

            var result = addingItems.Select((x, i) => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, x, currentItems.Count + i)).ToArray().ToReadOnly();
            currentItems.AddRange(addingItems);
            return result;
        }
    }
}
