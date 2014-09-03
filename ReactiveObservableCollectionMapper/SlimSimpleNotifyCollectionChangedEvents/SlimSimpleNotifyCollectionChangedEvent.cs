using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents
{
    class SlimSimpleNotifyCollectionChangedEvent<T>
    {
        private SlimSimpleNotifyCollectionChangedEvent()
        {

        }

        public static SlimSimpleNotifyCollectionChangedEvent<T> CreateInitialState(IReadOnlyList<Tagged<T>> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(items, item => item != null));
            Contract.Ensures(Contract.Result<SlimSimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SlimSimpleNotifyCollectionChangedEvent<T>();
            result.action = SlimSimpleNotifyCollectionChangedEventAction.InitialState;
            result.initialStateOrReset = items;
            return result;
        }

        public static SlimSimpleNotifyCollectionChangedEvent<T> CreateAddedOrRemoved(IReadOnlyList<SlimAddedOrRemovedUnit<T>> units)
        {
            Contract.Requires<ArgumentNullException>(units != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(units, item => item != null));
            Contract.Ensures(Contract.Result<SlimSimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SlimSimpleNotifyCollectionChangedEvent<T>();
            result.action = SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove;
            result.addedOrRemoved = units;
            return result;
        }

        public static SlimSimpleNotifyCollectionChangedEvent<T> CreateReset(IReadOnlyList<Tagged<T>> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(newItems, item => item != null));
            Contract.Ensures(Contract.Result<SlimSimpleNotifyCollectionChangedEvent<T>>() != null);

            var result = new SlimSimpleNotifyCollectionChangedEvent<T>();
            result.action = SlimSimpleNotifyCollectionChangedEventAction.Reset;
            result.initialStateOrReset = newItems;
            return result;
        }

        SlimSimpleNotifyCollectionChangedEventAction action;
        public SlimSimpleNotifyCollectionChangedEventAction Action
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

        IReadOnlyList<SlimAddedOrRemovedUnit<T>> addedOrRemoved;
        public IReadOnlyList<SlimAddedOrRemovedUnit<T>> AddedOrRemoved
        {
            get
            {
                return addedOrRemoved;
            }
        }

        public override string ToString()
        {
            switch(Action)
            {
                case SlimSimpleNotifyCollectionChangedEventAction.InitialState:
                    {
                        return "Initial state (items: "
                            + Converters.ListToString(InitialStateOrReset, 4)
                            + ")";
                    }
                case SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    {
                        return "Added or removed (items: "
                            + Converters.ListToString(AddedOrRemoved, 3)
                            + ")";
                    }
                case SlimSimpleNotifyCollectionChangedEventAction.Reset:
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
