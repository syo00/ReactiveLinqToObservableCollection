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
    // Added, Removed, Moved に変換する
    // Replaced にはしない (単純なものなら Replaced にする予定)
    // Items の個数は最適化されず全て1となる
    internal class SimpleEventExtractor<T> : ISimpleNotifyCollectionChangedEventExtractor<T>
    {
        public IReadOnlyList<INotifyCollectionChangedEvent<T>> Extract(SimpleNotifyCollectionChangedEvent<T> source)
        {
            switch(source.Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        return new[] { NotifyCollectionChangedEvent.CreateInitialStateEvent(source.InitialStateOrReset.Select(x => x.Item).ToArray().ToReadOnly()) };
                    }
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        return ExtractCore(source.AddedOrRemoved).ToArray().ToReadOnly();
                    }
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        return new[] { NotifyCollectionChangedEvent.CreateResetEvent(source.InitialStateOrReset.Select(x => x.Item).ToArray().ToReadOnly()) };
                    }
                default:
                    {
                        throw Exceptions.UnpredictableSwitchCasePattern;
                    }
            }
        }

        static IEnumerable<INotifyCollectionChangedEvent<T>> ExtractCore(IEnumerable<AddedOrRemovedUnit<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<INotifyCollectionChangedEvent<T>>>() != null);

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

            AddedOrRemovedUnit<T> removingEventCache = null;
            foreach (var s in set)
            {
                if (removingEventCache != null)
                {
                    if (s.Item.Equals(removingEventCache.Item))
                    {
                        if (s.Index != removingEventCache.Index)
                        {
                            yield return NotifyCollectionChangedEvent.CreateMovedEvent(new[] { s.Item.Item }.ToReadOnly(), removingEventCache.Index, s.Index);
                        }
                        removingEventCache = null;
                        continue;
                    }
                    else
                    {
                        yield return NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { removingEventCache.Item.Item }.ToReadOnly(), removingEventCache.Index);
                        removingEventCache = null;
                    }
                }

                if (s.Type == AddOrRemoveUnitType.Remove)
                {
                    removingEventCache = s;
                }
                else
                {
                    yield return NotifyCollectionChangedEvent.CreateAddedEvent(new[] { s.Item.Item }.ToReadOnly(), s.Index);
                }
            }

            if (removingEventCache != null)
            {
                yield return NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { removingEventCache.Item.Item }.ToReadOnly(), removingEventCache.Index);
            }
        }
    }
}
