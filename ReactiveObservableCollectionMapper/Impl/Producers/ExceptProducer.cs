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
    class ExceptProducer<T, TSecond> : CombineProducer2<T, TSecond, T>
    {
        readonly TaggedCollection<Item> currentItems = new TaggedCollection<Item>();

        public ExceptProducer(CollectionStatuses<T> source, CollectionStatuses<TSecond> second, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading, Func<T, TSecond, bool> comparer)
            : base(source.ConvertToSimpleInitialStateAndChanged(), second.ConvertToSimpleInitialStateAndChanged(), schedulingAndThreading)
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

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertInitialState(IReadOnlyList<T> initialLeftCollection, IReadOnlyList<TSecond> initialRightCollection)
        {
            var items = OnInitialStateOrReset(initialLeftCollection, initialRightCollection);
            var e = SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(items);
            yield return new NotifyCollectionChangedEventObject<T>(e);
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftChanged(NotifyCollectionChangedEventObject<T> leftEvent)
        {
            var units = new List<AddedOrRemovedUnit<T>>();

            foreach (var e in leftEvent.SimpleOne.AddedOrRemoved)
            {
                if (e.Type == AddOrRemoveUnitType.Add)
                {
                    var items = OnLeftItemAdded(e.Index, e.Item);
                    if (items != null)
                    {
                        units.Add(items);
                    }
                }
                else
                {
                    var items = OnLeftItemRemoved(e.Index, e.Item);
                    if (items != null)
                    {
                        units.Add(items);
                    }
                }
            }

            var core = SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(units);
            yield return new NotifyCollectionChangedEventObject<T>(core);
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightChanged(NotifyCollectionChangedEventObject<TSecond> rightEvent)
        {
            var units = new List<AddedOrRemovedUnit<T>>();

            foreach (var e in rightEvent.SimpleOne.AddedOrRemoved)
            {
                if (e.Type == AddOrRemoveUnitType.Add)
                {
                    var items = OnRightItemAdded(e.Item);
                    if (items != null)
                    {
                        units.AddRange(items);
                    }
                }
                else
                {
                    var items = OnRightItemRemoved(e.Item);
                    if (items != null)
                    {
                        units.AddRange(items);
                    }
                }
            }

            var core = SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(units);
            yield return new NotifyCollectionChangedEventObject<T>(core);
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftReset(IReadOnlyList<T> leftReset)
        {
            var items = OnInitialStateOrReset(leftReset, RightCollection);
            var e = SimpleNotifyCollectionChangedEvent<T>.CreateReset(items);
            yield return new NotifyCollectionChangedEventObject<T>(e);
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightReset(IReadOnlyList<TSecond> rightReset)
        {
            var items = OnInitialStateOrReset(LeftCollection, rightReset);
            var e = SimpleNotifyCollectionChangedEvent<T>.CreateReset(items);
            yield return new NotifyCollectionChangedEventObject<T>(e);
        }

        private IReadOnlyList<T> OnInitialStateOrReset(IReadOnlyList<T> leftCollection, IReadOnlyList<TSecond> rightCollection)
        {
            Contract.Requires<ArgumentNullException>(leftCollection != null);
            Contract.Requires<ArgumentNullException>(rightCollection != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            currentItems.Clear();
            var items = leftCollection
                .Select(x => new Item { HasValue = !rightCollection.Any(y => Comparer(x, y)), Value = x })
                .ToArray()
                .ToReadOnly();
            currentItems.AddRange(items);
            return items.Where(x => x.HasValue).Select(x => x.Value).ToArray().ToReadOnly();
        }

        private AddedOrRemovedUnit<T> OnLeftItemAdded(int index, Tagged<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var matched = RightCollection.Any<TSecond>(x => Comparer(item.Item, x));
            currentItems.Insert(index, new Tagged<Item>(new Item { Value = item.Item, HasValue = !matched }, item.Tag));

            if (matched)
            {
                return null;
            }

            return new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, item, currentItems.Take<Item>(index).Where(x => x.HasValue).Count());
        }

        private AddedOrRemovedUnit<T> OnLeftItemRemoved(int index, Tagged<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var removing = currentItems[index];
            currentItems.RemoveAt(index);
            if (!removing.Item.HasValue)
            {
                return null;
            }

            return new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, item, currentItems.Take<Item>(index).Where(x => x.HasValue).Count());
        }

        private IReadOnlyList<AddedOrRemovedUnit<T>> OnRightItemAdded(Tagged<TSecond> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);

            var result = new List<AddedOrRemovedUnit<T>>();
            var existingItemsindex = 0;
            foreach (var c in currentItems)
            {
                if (c.Item.HasValue)
                {
                    if (Comparer(c.Item.Value, item.Item))
                    {
                        c.Item.HasValue = false;
                        result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, new Tagged<T>(c.Item.Value, c.Tag), existingItemsindex));
                    }
                    else
                    {
                        existingItemsindex++;
                    }
                }
            }

            return result;
        }

        private IReadOnlyList<AddedOrRemovedUnit<T>> OnRightItemRemoved(Tagged<TSecond> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);

            var result = new List<AddedOrRemovedUnit<T>>();
            var existingItemsindex = 0;
            foreach (var c in currentItems)
            {
                if (!c.Item.HasValue)
                {
                    if (Comparer(c.Item.Value, item.Item))
                    {
                        c.Item.HasValue = true;
                        result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, new Tagged<T>(c.Item.Value, c.Tag), existingItemsindex));
                        existingItemsindex++;
                    }
                }
                else
                {
                    existingItemsindex++;
                }
            }

            return result;
        }

        class Item
        {
            public T Value { get; set; }
            public bool HasValue { get; set; }

            public override string ToString()
            {
                return HasValue.ToString() + ": " + ObjectEx.ToString(Value);
            }
        }
    }
}
