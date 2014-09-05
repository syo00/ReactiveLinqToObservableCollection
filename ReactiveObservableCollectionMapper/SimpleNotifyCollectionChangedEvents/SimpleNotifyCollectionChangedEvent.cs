using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents
{
    internal class SimpleNotifyCollectionChangedEvent<T>
    {
        private SimpleNotifyCollectionChangedEvent()
        {

        }

        public static SimpleNotifyCollectionChangedEvent<T> CreateInitialState(IReadOnlyList<Tagged<T>> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(initialState, item => item != null));
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SimpleNotifyCollectionChangedEvent<T>();
            result.action = SimpleNotifyCollectionChangedEventAction.InitialState;
            result.initialStateOrReset = initialState;
            return result;
        }

        public static SimpleNotifyCollectionChangedEvent<T> CreateAddOrRemove(AddedOrRemovedUnit<T> addedOrRemoved)
        {
            Contract.Requires<ArgumentNullException>(addedOrRemoved != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SimpleNotifyCollectionChangedEvent<T>();
            result.action = SimpleNotifyCollectionChangedEventAction.AddOrRemove;
            result.addedOrRemoved = new[] { addedOrRemoved }.ToReadOnly();
            return result;
        }

        public static SimpleNotifyCollectionChangedEvent<T> CreateAddOrRemove(IReadOnlyList<AddedOrRemovedUnit<T>> addedOrRemoved)
        {
            Contract.Requires<ArgumentNullException>(addedOrRemoved != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(addedOrRemoved, item => item != null));
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SimpleNotifyCollectionChangedEvent<T>();
            result.action = SimpleNotifyCollectionChangedEventAction.AddOrRemove;
            result.addedOrRemoved = addedOrRemoved;
            return result;
        }

        public static SimpleNotifyCollectionChangedEvent<T> CreateReset(IReadOnlyList<Tagged<T>> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(reset, item => item != null));
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SimpleNotifyCollectionChangedEvent<T>();
            result.action = SimpleNotifyCollectionChangedEventAction.Reset;
            result.initialStateOrReset = reset;
            return result;
        }

        SimpleNotifyCollectionChangedEventAction action;
        public SimpleNotifyCollectionChangedEventAction Action
        {
            get
            {
                return action;
            }
        }

        IReadOnlyList<Tagged<T>> initialStateOrReset;
        public IReadOnlyList<Tagged<T>> InitialStateOrReset
        {
            get
            {
                return initialStateOrReset;
            }
        }

        IReadOnlyList<AddedOrRemovedUnit<T>> addedOrRemoved;
        public IReadOnlyList<AddedOrRemovedUnit<T>> AddedOrRemoved
        {
            get
            {
                return addedOrRemoved;
            }
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(INotifyCollectionChangedEvent<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            switch(source.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        return SimpleNotifyCollectionChangedEvent<T>.Create(source.InitialState);
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        return SimpleNotifyCollectionChangedEvent<T>.Create(source.Added);
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        return SimpleNotifyCollectionChangedEvent<T>.Create(source.Removed);
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        return SimpleNotifyCollectionChangedEvent<T>.Create(source.Moved);
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        return SimpleNotifyCollectionChangedEvent<T>.Create(source.Replaced);
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        return SimpleNotifyCollectionChangedEvent<T>.Create(source.Reset);
                    }
                default:
                    {
                        throw Exceptions.UnpredictableSwitchCasePattern;
                    }
            }
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(IInitialState<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(initialState.Items.Select(x => new Tagged<T>(x)).ToArray().ToReadOnly());
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(IAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            if (added.StartingIndex < 0) throw new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex);

            var simpleAdded = added
                .Items
                .Select((x, i) => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, new Tagged<T>(x), added.StartingIndex + i)).ToArray();

            Contract.Assume(simpleAdded != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(simpleAdded);
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(IRemoved<T> removed)
        {
            Contract.Requires<ArgumentNullException>(removed != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            if (removed.StartingIndex < 0) throw new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex);

            var simpleRemoved = removed
                .Items
                .Select((x, i) => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, new Tagged<T>(x), removed.StartingIndex)).ToArray();

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(simpleRemoved);
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(IMoved<T> moved)
        {
            Contract.Requires<ArgumentNullException>(moved != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            if (moved.OldStartingIndex < 0)
            {
                throw new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex);
            }

            var items = moved.Items.Select(x => new Tagged<T>(x));

            var result = new List<AddedOrRemovedUnit<T>>();
            foreach (var i in items)
            {
                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, i, moved.OldStartingIndex));
            }
            int addingIndex = 0;
            foreach (var i in items)
            {
                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, i, moved.NewStartingIndex + addingIndex));
                addingIndex++;
            }

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result);
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(IReplaced<T> replaced)
        {
            Contract.Requires<ArgumentNullException>(replaced != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            if (replaced.StartingIndex < 0) throw new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex);

            var result = new List<AddedOrRemovedUnit<T>>();
            foreach (var i in replaced.OldItems)
            {
                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, new Tagged<T>(i), replaced.StartingIndex));
            }
            int addingIndex = 0;
            foreach (var i in replaced.NewItems)
            {
                result.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, new Tagged<T>(i), replaced.StartingIndex + addingIndex));
                addingIndex++;
            }

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(result);
        }

        public static SimpleNotifyCollectionChangedEvent<T> Create(IReset<T> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateReset(reset.Items.Select(x => new Tagged<T>(x)).ToArray().ToReadOnly());
        }

        public override string ToString()
        {
            switch(Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        return "Initial state (items: "
                            + Converters.ListToString(InitialStateOrReset, 4)
                            + ")";
                    }
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        return "Added or removed (items: "
                            + Converters.ListToString(AddedOrRemoved, 4)
                            + ")";
                    }
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    {
                        return "Reset";
                    }
                default:
                    {
                        throw Exceptions.UnpredictableSwitchCasePattern;
                    }
            }
        }
    }
}
