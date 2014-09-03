using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Kirinji.LightWands;
using System.Diagnostics.Contracts;
using System.Threading;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Support.Extractors
{
    class DefaultEventExtractor<T> : ISimpleNotifyCollectionChangedEventExtractor<T>
    {
        public IReadOnlyList<INotifyCollectionChangedEvent<T>> Extract(SimpleNotifyCollectionChangedEvent<T> source)
        {
            switch(source.Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        return new[] { NotifyCollectionChangedEvent.CreateInitialStateEvent(source.InitialStateOrReset.Select(x => x.Item).ToArray().ToReadOnly()) }.ToReadOnly();
                    }
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        return ExtractCore(source.AddedOrRemoved).ToArray().ToReadOnly();
                    }
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        return new[] { NotifyCollectionChangedEvent.CreateResetEvent<T>(source.InitialStateOrReset.Select(x => x.Item).ToArray().ToReadOnly()) };
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        static IEnumerable<INotifyCollectionChangedEvent<T>> ExtractCore(IEnumerable<AddedOrRemovedUnit<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<INotifyCollectionChangedEvent<T>>>() != null);

            return EventsConverter.ConvertToReplaced(EventsConverter.ConvertDuplicatedUnitItemsToSingle(ConvertItemsToMoved(source)))
                .Select(e => Select(e, tagged => tagged.Item))
                .ToArray()
                .ToReadOnly();
        }

        static IEnumerable<INotifyCollectionChangedEvent<Tagged<T>>> ConvertItemsToMoved(IEnumerable<AddedOrRemovedUnit<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<INotifyCollectionChangedEvent<Tagged<T>>>>() != null);

            var set = new AddedOrRemovedUnitSet<T>(source);

            var movingItems = set
                .ToLookup(x => x.Item)
                .Where(group => group.Count() == 2)
                .Select(group => group.OrderBy(x => x.Type == AddOrRemoveUnitType.Add ? 0 : 1));

            foreach (var m in movingItems)
            {
                var addingEventIndex = set.IndexOf(m.First());
                var removingItemIndex = set.IndexOf(m.Last());

                if (addingEventIndex > removingItemIndex)
                {
                    set.Move(removingItemIndex, addingEventIndex - 1);
                }
                else
                {
                    set.Move(removingItemIndex, addingEventIndex);
                }
            }

            var removingCache = new List<AddedOrRemovedUnit<T>>();
            var addingCache = new List<AddedOrRemovedUnit<T>>();

            foreach (var s in set)
            {
                if (s.Type == AddOrRemoveUnitType.Add)
                {
                    addingCache.Add(s);
                }
                else
                {
                    removingCache.Add(s);
                }

                if (removingCache.Count != 0 && removingCache.Count == addingCache.Count)
                {
                    if (removingCache.Select(u => u.Item).SequenceEqual(addingCache.Select(u => u.Item))
                        && removingCache.Select(u => u.Index).Distinct().Count() == 1
                        && IsIncrementedByOne(addingCache.Select(u => u.Index)))
                    {
                        if (removingCache[0].Index != addingCache[0].Index)
                        {
                            yield return NotifyCollectionChangedEvent.CreateMovedEvent(removingCache.Select(u => u.Item).ToArray().ToReadOnly(), removingCache[0].Index, addingCache[0].Index);
                        }

                        removingCache.Clear();
                        addingCache.Clear();
                        continue;
                    }

                    foreach (var r in removingCache)
                    {
                        yield return NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { r.Item }, r.Index);
                    }
                    foreach (var a in addingCache)
                    {
                        yield return NotifyCollectionChangedEvent.CreateAddedEvent(new[] { a.Item }, a.Index);
                    }

                    removingCache.Clear();
                    addingCache.Clear();
                    continue;
                }

                if (addingCache.Count > removingCache.Count)
                {
                    foreach (var r in removingCache)
                    {
                        yield return NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { r.Item }, r.Index);
                    }
                    foreach (var a in addingCache)
                    {
                        yield return NotifyCollectionChangedEvent.CreateAddedEvent(new[] { a.Item }, a.Index);
                    }

                    removingCache.Clear();
                    addingCache.Clear();
                }
            }

            foreach (var r in removingCache)
            {
                yield return NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { r.Item }, r.Index);
            }
            foreach (var a in addingCache)
            {
                yield return NotifyCollectionChangedEvent.CreateAddedEvent(new[] { a.Item }, a.Index);
            }
        }

        // [2, 3, 4] -> true
        // [9, 2, 5] -> false
        // [1, 3, 5, 7] -> false
        static bool IsIncrementedByOne(IEnumerable<int> collection)
        {
            int? lastItem = null;
            foreach(var item in collection)
            {
                if(lastItem != null && lastItem.Value != item - 1)
                {
                    return false;
                }
                lastItem = item;
            }
            return true;
        }

        static INotifyCollectionChangedEvent<TTo> Select<TFrom, TTo>(INotifyCollectionChangedEvent<TFrom> source, Func<TFrom, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<TTo>>() != null);

            switch (source.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    return NotifyCollectionChangedEvent.CreateInitialStateEvent(source.InitialState.Items.Select(converter).ToArray().ToReadOnly());
                case NotifyCollectionChangedEventAction.Add:
                    return NotifyCollectionChangedEvent.CreateAddedEvent(source.Added.Items.Select(converter).ToArray().ToReadOnly(), source.Added.StartingIndex);
                case NotifyCollectionChangedEventAction.Remove:
                    return NotifyCollectionChangedEvent.CreateRemovedEvent(source.Removed.Items.Select(converter).ToArray().ToReadOnly(), source.Removed.StartingIndex);
                case NotifyCollectionChangedEventAction.Move:
                    return NotifyCollectionChangedEvent.CreateMovedEvent(source.Moved.Items.Select(converter).ToArray().ToReadOnly(), source.Moved.OldStartingIndex, source.Moved.NewStartingIndex);
                case NotifyCollectionChangedEventAction.Replace:
                    return NotifyCollectionChangedEvent.CreateReplacedEvent(source.Replaced.OldItems.Select(converter).ToArray().ToReadOnly(), source.Replaced.NewItems.Select(converter).ToArray().ToReadOnly(), source.Replaced.StartingIndex);
                case NotifyCollectionChangedEventAction.Reset:
                    return NotifyCollectionChangedEvent.CreateResetEvent<TTo>(source.Reset.Items.Select(converter).ToArray().ToReadOnly());
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        private static INotifyCollectionChangedEvent<Tagged<T>> ToEventItem(AddedOrRemovedItems<T> cacheItem)
        {
            Contract.Requires<ArgumentNullException>(cacheItem != null);

            INotifyCollectionChangedEvent<Tagged<T>> result;
            if (cacheItem.Type == AddOrRemoveUnitType.Add)
            {
                result = NotifyCollectionChangedEvent.CreateAddedEvent(cacheItem.Items, cacheItem.Index);
            }
            else
            {
                result = NotifyCollectionChangedEvent.CreateRemovedEvent(cacheItem.Items, cacheItem.Index);
            }
            return result;
        }

        class AddedOrRemovedItems<TItems>
        {
            public AddedOrRemovedItems(AddOrRemoveUnitType type, IReadOnlyList<Tagged<TItems>> items, int index)
            {
                Contract.Requires<ArgumentNullException>(items != null);
                Contract.Requires<ArgumentException>(Contract.ForAll(items, x => x != null));

                this.Type = type;
                this.items = items;
                this.Index = index;
            }

            [ContractInvariantMethod]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
            private void ObjectInvariant()
            {
                Contract.Invariant(items != null);
                Contract.Invariant(Contract.ForAll(items, x => x != null));
            }

            public AddOrRemoveUnitType Type { get; private set; }

            readonly IReadOnlyList<Tagged<TItems>> items;
            public IReadOnlyList<Tagged<TItems>> Items
            {
                get
                {
                    Contract.Ensures(Contract.Result<IReadOnlyList<Tagged<TItems>>>() != null);
                    Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<Tagged<TItems>>>(), x => x != null));

                    return items;
                }
            }

            public int Index { get; private set; }
        }
    }
}
