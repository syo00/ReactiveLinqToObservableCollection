using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    internal static class SlimNotifyCollectionChangedEvent
    {
        public static SlimNotifyCollectionChangedEvent<T> ToEvent<T>(SlimInitialState<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return new SlimNotifyCollectionChangedEvent<T>(initialState);
        }

        public static SlimNotifyCollectionChangedEvent<T> ToEvent<T>(SlimAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return new SlimNotifyCollectionChangedEvent<T>(added);
        }

        public static SlimNotifyCollectionChangedEvent<T> ToEvent<T>(SlimRemoved removed)
        {
            Contract.Requires<ArgumentNullException>(removed != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return new SlimNotifyCollectionChangedEvent<T>(removed);
        }

        public static SlimNotifyCollectionChangedEvent<T> ToEvent<T>(SlimReplaced<T> replaced)
        {
            Contract.Requires<ArgumentNullException>(replaced != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return new SlimNotifyCollectionChangedEvent<T>(replaced);
        }

        public static SlimNotifyCollectionChangedEvent<T> ToEvent<T>(SlimMoved moved)
        {
            Contract.Requires<ArgumentNullException>(moved != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return new SlimNotifyCollectionChangedEvent<T>(moved);
        }

        public static SlimNotifyCollectionChangedEvent<T> ToEvent<T>(SlimReset<T> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return new SlimNotifyCollectionChangedEvent<T>(reset);
        }


        public static SlimInitialState<T> CreateInitialState<T>(IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<SlimInitialState<T>>() != null);

            return new SlimInitialState<T>(items);
        }

        public static SlimNotifyCollectionChangedEvent<T> CreateInitialStateEvent<T>(IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateInitialState(items));
        }

        public static SlimAdded<T> CreateAdded<T>(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<SlimAdded<T>>() != null);

            return new SlimAdded<T>(items, startingIndex);
        }

        public static SlimNotifyCollectionChangedEvent<T> CreateAddedEvent<T>(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateAdded(items, startingIndex));
        }

        public static SlimRemoved CreateRemoved(int startingIndex, int itemsCount)
        {
            Contract.Requires<ArgumentException>(startingIndex >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);
            Contract.Ensures(Contract.Result<SlimRemoved>() != null);

            return new SlimRemoved(startingIndex, itemsCount);
        }

        public static SlimNotifyCollectionChangedEvent<T> CreateRemovedEvent<T>(int startingIndex, int itemsCount)
        {
            Contract.Requires<ArgumentException>(startingIndex >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return ToEvent<T>(CreateRemoved(startingIndex, itemsCount));
        }

        public static SlimReplaced<T> CreateReplaced<T>(int startingIndex, int oldItemsCount, IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentException>(startingIndex >= 0);
            Contract.Requires<ArgumentException>(oldItemsCount >= 1);
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<SlimReplaced<T>>() != null);

            return new SlimReplaced<T>(startingIndex, oldItemsCount, newItems);
        }

        public static SlimNotifyCollectionChangedEvent<T> CreateReplacedEvent<T>(int startingIndex, int oldItemsCount, IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentException>(startingIndex >= 0);
            Contract.Requires<ArgumentException>(oldItemsCount >= 1);
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateReplaced(startingIndex, oldItemsCount, newItems));
        }

        public static SlimMoved CreateMoved(int oldStartingIndex, int newStartingIndex, int itemsCount)
        {
            Contract.Requires<ArgumentException>(oldStartingIndex >= 0);
            Contract.Requires<ArgumentException>(newStartingIndex >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);
            Contract.Ensures(Contract.Result<SlimMoved>() != null);

            return new SlimMoved(oldStartingIndex, newStartingIndex, itemsCount);
        }

        public static SlimNotifyCollectionChangedEvent<T> CreateMovedEvent<T>(int oldStartingIndex, int newStartingIndex, int itemsCount)
        {
            Contract.Requires<ArgumentException>(oldStartingIndex >= 0);
            Contract.Requires<ArgumentException>(newStartingIndex >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return ToEvent<T>(CreateMoved(oldStartingIndex, newStartingIndex, itemsCount));
        }

        public static SlimReset<T> CreateReset<T>(IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<SlimReset<T>>() != null);

            return new SlimReset<T>(newItems);
        }

        public static SlimNotifyCollectionChangedEvent<T> CreateResetEvent<T>(IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<SlimNotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateReset<T>(newItems));
        }
    }
}
