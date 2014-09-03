using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal static partial class Extensions
    {
        public static void ApplyChangeEvent<T>(this MultiValuesObservableCollection<T> collection, INotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);

            switch (e.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        if (e.InitialState.Items != null)
                        {
                            collection.AddRange(e.InitialState.Items);
                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        int insertingIndex = e.Added.StartingIndex;
                        if (insertingIndex <= -1)
                        {
                            collection.AddRange(e.Added.Items);
                        }
                        else
                        {
                            try
                            {
                                collection.Insert(insertingIndex, e.Added.Items);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                            }
                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        try
                        {
                            collection.Move(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                        }

                        return;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        if (e.Removed.StartingIndex <= -1)
                        {
                            try
                            {
                                collection.RemoveAt(collection.Count - e.Removed.Items.Count, e.Removed.Items.Count);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                            }
                        }
                        else
                        {
                            try
                            {
                                collection.RemoveAt(e.Removed.StartingIndex, e.Removed.Items.Count);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                            }
                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        try
                        {
                            collection.Replace(e.Replaced.StartingIndex, e.Replaced.OldItems.Count, e.Replaced.NewItems);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        if (e.Reset.Items != null)
                        {
                            collection.Reset(e.Reset.Items);
                        }
                        else
                        {
                            collection.Clear();
                        }

                        return;
                    }
            }
        }

        // returns infomation-filled events
        public static IReadOnlyList<INotifyCollectionChangedEvent<T>> ApplyChangeEvent<T>(this IList<T> collection, INotifyCollectionChangedEvent<T> e, bool checkRemovingItemsEquality = false, IEqualityComparer<T> removingItemsEqualityComparer = null)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<INotifyCollectionChangedEvent<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<INotifyCollectionChangedEvent<T>>>(), x => x != null));

            var removingItemsEqualityComparerNotNull = removingItemsEqualityComparer ?? EqualityComparer<T>.Default;

            // filter to ensure (Old|New)Items.Count >= 1
            INotifyCollectionChangedEvent<T> filteredEvent;
            switch (e.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    filteredEvent = e;
                    break;
                case NotifyCollectionChangedEventAction.Add:
                    if (e.Added.Items.Count == 0)
                    {
                        return new INotifyCollectionChangedEvent<T>[0];
                    }
                    filteredEvent = e;
                    break;
                case NotifyCollectionChangedEventAction.Remove:
                    if (e.Removed.Items.Count == 0)
                    {
                        return new INotifyCollectionChangedEvent<T>[0];
                    }
                    filteredEvent = e;
                    break;
                case NotifyCollectionChangedEventAction.Move:
                    if (e.Moved.Items.Count == 0)
                    {
                        return new INotifyCollectionChangedEvent<T>[0];
                    }
                    filteredEvent = e;
                    break;
                case NotifyCollectionChangedEventAction.Replace:
                    if (e.Replaced.OldItems.Count == 0 && e.Replaced.NewItems.Count == 0)
                    {
                        return new INotifyCollectionChangedEvent<T>[0];
                    }
                    else if (e.Replaced.OldItems.Count == 0)
                    {
                        filteredEvent = NotifyCollectionChangedEvent.CreateAddedEvent(e.Replaced.NewItems, e.Replaced.StartingIndex);
                    }
                    else if (e.Replaced.NewItems.Count == 0)
                    {
                        filteredEvent = NotifyCollectionChangedEvent.CreateRemovedEvent(e.Replaced.OldItems, e.Replaced.StartingIndex);
                    }
                    else
                    {
                        filteredEvent = e;
                    }
                    break;
                case NotifyCollectionChangedEventAction.Reset:
                    filteredEvent = e;
                    break;
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }

            switch (filteredEvent.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        if (filteredEvent.InitialState.Items != null)
                        {
                            collection.AddRange(filteredEvent.InitialState.Items);
                        }
                        return new[] { filteredEvent }.ToReadOnly();
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        int insertingIndex = filteredEvent.Added.StartingIndex < 0 ? collection.Count : filteredEvent.Added.StartingIndex;
                        try
                        {
                            collection.InsertRange(insertingIndex, filteredEvent.Added.Items);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                        }
                        return new[] { NotifyCollectionChangedEvent.CreateAddedEvent(filteredEvent.Added.Items, insertingIndex) }.ToReadOnly();
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        if (e.Moved.OldStartingIndex == e.Moved.NewStartingIndex && !checkRemovingItemsEquality)
                        {
                            return new INotifyCollectionChangedEvent<T>[0];
                        }

                        ReadOnlyCollection<T> movedItems;
                        try
                        {
                            movedItems = collection.MoveRange(filteredEvent.Moved.OldStartingIndex, filteredEvent.Moved.NewStartingIndex, filteredEvent.Moved.Items.Count);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                        }

                        if (checkRemovingItemsEquality)
                        {
                            if (!movedItems.SequenceEqual(filteredEvent.Moved.Items, removingItemsEqualityComparerNotNull))
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidItem, filteredEvent);
                            }
                        }

                        if (e.Moved.OldStartingIndex == e.Moved.NewStartingIndex)
                        {
                            return new INotifyCollectionChangedEvent<T>[0];
                        }
                        else
                        {
                            return new[] { filteredEvent }.ToReadOnly();
                        }
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        if (filteredEvent.Removed.StartingIndex <= -1)
                        {
                            IEnumerable<IRemoved<T>> removed;
                            try
                            {
                                removed = FindAndRemove(collection, filteredEvent.Removed.Items, removingItemsEqualityComparerNotNull);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }

                            return removed
                                .Select(r => NotifyCollectionChangedEvent.ToEvent(r))
                                .ToArray()
                                .ToReadOnly();
                        }
                        else
                        {
                            ReadOnlyCollection<T> removedItems;
                            try
                            {
                                removedItems = collection.RemoveAtRange(filteredEvent.Removed.StartingIndex, filteredEvent.Removed.Items.Count);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }

                            if (checkRemovingItemsEquality)
                            {
                                if (!removedItems.SequenceEqual(filteredEvent.Removed.Items, removingItemsEqualityComparerNotNull))
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidItem, filteredEvent);
                                }
                            }
                            return new[] { filteredEvent }.ToReadOnly();
                        }
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        if (filteredEvent.Replaced.StartingIndex < 0)
                        {
                            IReadOnlyList<IRemoved<T>> removed;
                            try
                            {
                                removed = FindAndRemove(collection, filteredEvent.Replaced.OldItems, removingItemsEqualityComparerNotNull).ToArray();
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }

                            if (removed.Count >= 2)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidItem, filteredEvent);
                            }

                            try
                            {
                                collection.InsertRange(removed.Single().StartingIndex, filteredEvent.Replaced.NewItems);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }

                            var replaced = NotifyCollectionChangedEvent.CreateReplacedEvent(filteredEvent.Replaced.OldItems, filteredEvent.Replaced.NewItems, removed.Single().StartingIndex);
                            return new[] { replaced }.ToReadOnly();
                        }
                        else
                        {
                            ReadOnlyCollection<T> removedItems;
                            try
                            {
                                removedItems = collection.RemoveAtRange(filteredEvent.Replaced.StartingIndex, filteredEvent.Replaced.OldItems.Count);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }

                            if (checkRemovingItemsEquality)
                            {
                                if (!removedItems.SequenceEqual(filteredEvent.Replaced.OldItems, removingItemsEqualityComparerNotNull))
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidItem, filteredEvent);
                                }
                            }
                            try
                            {
                                collection.InsertRange(filteredEvent.Replaced.StartingIndex, filteredEvent.Replaced.NewItems);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, filteredEvent);
                            }

                            return new[] { filteredEvent }.ToReadOnly();
                        }
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Clear();
                        collection.AddRange(filteredEvent.Reset.Items);

                        return new[] { filteredEvent }.ToReadOnly();
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        public static void ApplySlimChangeEvent<T>(this MultiValuesObservableCollection<T> collection, SlimNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);

            switch (e.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialState.Items);
                        return;
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        try
                        {
                            collection.Insert(e.Added.StartingIndex, e.Added.Items);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }

                        return;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        try
                        {
                            collection.Move(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.ItemsCount);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }

                        return;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        try
                        {
                            collection.RemoveAt(e.Removed.StartingIndex, e.Removed.ItemsCount);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);

                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        try
                        {
                            collection.Replace(e.Replaced.StartingIndex, e.Replaced.OldItemsCount, e.Replaced.NewItems);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Reset(e.Reset.Items);

                        return;
                    }
            }
        }

        public static void ApplySlimChangeEvent<T>(this IList<T> collection, SlimNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);

            switch (e.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialState.Items);
                        return;
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        try
                        {
                            collection.InsertRange(e.Added.StartingIndex, e.Added.Items);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }

                        return;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        try
                        {
                            collection.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.ItemsCount);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }

                        return;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        try
                        {
                            collection.RemoveAtRange(e.Removed.StartingIndex, e.Removed.ItemsCount);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);

                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        try
                        {
                            collection.RemoveAtRange(e.Replaced.StartingIndex, e.Replaced.OldItemsCount);
                            collection.InsertRange(e.Replaced.StartingIndex, e.Replaced.NewItems);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                        }
                        return;
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Clear();
                        collection.AddRange(e.Reset.Items);

                        return;
                    }
            }
        }

        public static SimpleNotifyCollectionChangedEvent<T> ApplySimpleChangeEvent<T>(this IList<Tagged<T>> collection, SimpleNotifyCollectionChangedEvent<T> e, bool checkRemovingItemsEquality = false)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            switch (e.Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset);

                        return e;
                    }
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        var result = new List<AddedOrRemovedUnit<T>>();

                        foreach (var i in e.AddedOrRemoved)
                        {
                            if (i.Type == AddOrRemoveUnitType.Add)
                            {
                                try
                                {
                                    collection.Insert(i.Index, i.Item);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                                result.Add(i);
                            }
                            else
                            {
                                Tagged<T> removing;
                                try
                                {
                                    removing = collection[i.Index];
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }

                                if (checkRemovingItemsEquality && !Object.Equals(removing.Item, i.Item.Item))
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidItem, e);
                                }

                                collection.RemoveAt(i.Index);

                                if (i.Item == null)
                                {
                                    result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, removing, i.Index));
                                }
                                else
                                {
                                    result.Add(i);
                                }

                            }
                        }

                        return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result.ToReadOnly());
                    }
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset);

                        return e;
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        public static void ApplySimpleChangeEventWithoutTag<T>(this IList<T> collection, SimpleNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);

            switch (e.Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset.Select(tagged => tagged.Item));

                        return;
                    }
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        foreach (var i in e.AddedOrRemoved)
                        {
                            if (i.Type == AddOrRemoveUnitType.Add)
                            {
                                try
                                {
                                    collection.Insert(i.Index, i.Item.Item);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                            }
                            else
                            {
                                try
                                {
                                    collection.RemoveAt(i.Index);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex, e);
                                }
                            }
                        }

                        return;
                    }
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset.Select(tagged => tagged.Item));

                        return;
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        public static void ApplySlimSimpleChangeEvent<T>(this IList<Tagged<T>> collection, SlimSimpleNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);

            switch (e.Action)
            {
                case SlimSimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset);

                        return;
                    }
                case SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        foreach (var i in e.AddedOrRemoved)
                        {
                            if (i.Type == SlimAddOrRemoveUnitType.Add)
                            {
                                try
                                {
                                    collection.Insert(i.Index, i.Item);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                            }
                            else
                            {
                                try
                                {
                                    collection.RemoveAt(i.Index);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                            }
                        }

                        return;
                    }
                case SlimSimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset);

                        return;
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        public static void ApplySlimSimpleChangeEventWithoutTag<T>(this IList<T> collection, SlimSimpleNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(e != null);

            switch (e.Action)
            {
                case SlimSimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset.Select(tagged => tagged.Item));

                        return;
                    }
                case SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        foreach (var i in e.AddedOrRemoved)
                        {
                            if (i.Type == SlimAddOrRemoveUnitType.Add)
                            {
                                try
                                {
                                    collection.Insert(i.Index, i.Item.Item);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                            }
                            else
                            {
                                try
                                {
                                    collection.RemoveAt(i.Index);
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    throw new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex);
                                }
                            }
                        }

                        return;
                    }
                case SlimSimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        collection.Clear();
                        collection.AddRange(e.InitialStateOrReset.Select(tagged => tagged.Item));

                        return;
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        public static IEnumerable<IRemoved<T>> FindAndRemove<T>(IList<T> collection, IEnumerable<T> removingItems, IEqualityComparer<T> comparer = null)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
            Contract.Requires<ArgumentNullException>(removingItems != null);
            Contract.Ensures(Contract.Result<IEnumerable<IRemoved<T>>>() != null);

            var comparerNotNull = comparer ?? EqualityComparer<T>.Default;
            var collectionBackup = collection.ToArray();

            var removingIndexesAndItems = new List<Tuple<int, T>>();
            foreach (var ri in removingItems)
            {
                var removingIndex = collection.FirstIndex(item => comparerNotNull.Equals(ri, item));
                if (removingIndex == null)
                {
                    collection.Clear();
                    collection.AddRange(collectionBackup);
                    throw new InvalidOperationException();
                }
                var removeingItem = collection[removingIndex.Value];
                collection.RemoveAt(removingIndex.Value);
                removingIndexesAndItems.Add(Tuple.Create(removingIndex.Value, removeingItem));
            }

            var itemsQueue = new List<T>();
            int lastIndex = -1;
            foreach (var tuple in removingIndexesAndItems)
            {
                if (lastIndex == tuple.Item1)
                {
                    itemsQueue.Add(tuple.Item2);
                }
                else if (lastIndex < 0)
                {
                    lastIndex = tuple.Item1;
                    itemsQueue.Add(tuple.Item2);
                }
                else
                {
                    yield return NotifyCollectionChangedEvent.CreateRemoved(itemsQueue.ToArray().ToReadOnly(), lastIndex);
                    itemsQueue.Clear();
                    itemsQueue.Add(tuple.Item2);
                    lastIndex = tuple.Item1;
                }
            }

            if (lastIndex >= 0)
            {
                yield return NotifyCollectionChangedEvent.CreateRemoved(itemsQueue.ToArray().ToReadOnly(), lastIndex);
            }
        }
    }
}
