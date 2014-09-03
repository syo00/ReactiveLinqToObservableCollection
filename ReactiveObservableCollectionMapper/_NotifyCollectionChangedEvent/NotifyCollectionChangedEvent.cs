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

namespace Kirinji.LinqToObservableCollection
{
    public static partial class NotifyCollectionChangedEvent
    {
        public static INotifyCollectionChangedEvent<T> ToEvent<T>(IInitialState<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return new NotifyCollectionChangedEvent<T>(initialState);
        }

        public static INotifyCollectionChangedEvent<T> ToEvent<T>(IAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return new NotifyCollectionChangedEvent<T>(added);
        }

        public static INotifyCollectionChangedEvent<T> ToEvent<T>(IRemoved<T> removed)
        {
            Contract.Requires<ArgumentNullException>(removed != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return new NotifyCollectionChangedEvent<T>(removed);
        }

        public static INotifyCollectionChangedEvent<T> ToEvent<T>(IReplaced<T> replaced)
        {
            Contract.Requires<ArgumentNullException>(replaced != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return new NotifyCollectionChangedEvent<T>(replaced);
        }

        public static INotifyCollectionChangedEvent<T> ToEvent<T>(IMoved<T> moved)
        {
            Contract.Requires<ArgumentNullException>(moved != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return new NotifyCollectionChangedEvent<T>(moved);
        }

        public static INotifyCollectionChangedEvent<T> ToEvent<T>(IReset<T> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return new NotifyCollectionChangedEvent<T>(reset);
        }


        public static IInitialState<T> CreateInitialState<T>(IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<IInitialState<T>>() != null);

            return new InitialState<T>(items);
        }

        public static INotifyCollectionChangedEvent<T> CreateInitialStateEvent<T>(IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateInitialState(items));
        }

        public static IAdded<T> CreateAdded<T>(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<IAdded<T>>() != null);

            return new Added<T>(items, startingIndex);
        }

        public static INotifyCollectionChangedEvent<T> CreateAddedEvent<T>(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result <INotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateAdded(items, startingIndex));
        }

        public static IRemoved<T> CreateRemoved<T>(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<IRemoved<T>>() != null);

            return new Removed<T>(items, startingIndex);
        }

        public static INotifyCollectionChangedEvent<T> CreateRemovedEvent<T>(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateRemoved(items, startingIndex));
        }

        public static IReplaced<T> CreateReplaced<T>(IReadOnlyList<T> oldItems, IReadOnlyList<T> newItems, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(oldItems != null);
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<IReplaced<T>>() != null);

            return new Replaced<T>(oldItems, newItems, startingIndex);
        }

        public static INotifyCollectionChangedEvent<T> CreateReplacedEvent<T>(IReadOnlyList<T> oldItems, IReadOnlyList<T> newItems, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(oldItems != null);
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateReplaced(oldItems, newItems, startingIndex));
        }

        public static IMoved<T> CreateMoved<T>(IReadOnlyList<T> items, int oldStartingIndex, int newStartingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(newStartingIndex >= 0);
            Contract.Ensures(Contract.Result<IMoved<T>>() != null);

            return new Moved<T>(items, oldStartingIndex, newStartingIndex);
        }

        public static INotifyCollectionChangedEvent<T> CreateMovedEvent<T>(IReadOnlyList<T> items, int oldStartingIndex, int newStartingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(newStartingIndex >= 0);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateMoved(items, oldStartingIndex, newStartingIndex));
        }

        public static IReset<T> CreateReset<T>(IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<IReset<T>>() != null);

            return new Reset<T>(newItems);
        }

        public static INotifyCollectionChangedEvent<T> CreateResetEvent<T>(IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<T>>() != null);

            return ToEvent(CreateReset<T>(newItems));
        }


        public static INotifyCollectionChangedEvent<T> Convert<T>(NotifyCollectionChangedEventArgs e, Func<IReadOnlyList<T>> obtainCurrentCollection)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Requires<ArgumentNullException>(obtainCurrentCollection != null);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var newItems = CastAllOrDefault<T>(e.NewItems);
                        if (newItems == null) return null;

                        return CreateAddedEvent(newItems, e.NewStartingIndex);
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        var oldItems = CastAllOrDefault<T>(e.OldItems);
                        if (oldItems == null) return null;

                        return CreateRemovedEvent(oldItems, e.OldStartingIndex);
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        var items = CastAllOrDefault<T>(e.NewItems);
                        if (items == null) return null;

                        return CreateMovedEvent(items, e.OldStartingIndex, e.NewStartingIndex);
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        var oldItems = CastAllOrDefault<T>(e.OldItems);
                        if (oldItems == null) return null;
                        var newItems = CastAllOrDefault<T>(e.NewItems);
                        if (newItems == null) return null;

                        return CreateReplacedEvent(oldItems, newItems, e.NewStartingIndex);
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        var currentCollection = obtainCurrentCollection();
                        if(currentCollection == null)
                        {
                            throw new InvalidOperationException("tried to convert reset event, however obtainCurrentCollection result is null");
                        }
                        return CreateResetEvent<T>(currentCollection);
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        static T[] CastAllOrDefault<T>(IList source)
        {
            if(Debug.DebugSetting.UseCastToConvertEvent)
            {
                if (source == null) return null;
                return source.Cast<T>().ToArray();
            }

            if (source == null) return null;
            var casted = source.OfType<T>().ToArray();
            if (casted.Length != source.Count) return null;
            return casted;
        }
    }
}
