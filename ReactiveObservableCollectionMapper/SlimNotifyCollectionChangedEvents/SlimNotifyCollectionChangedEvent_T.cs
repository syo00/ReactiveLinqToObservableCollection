using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    sealed class SlimNotifyCollectionChangedEvent<T>
    {
        public SlimNotifyCollectionChangedEvent(SlimInitialState<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);

            this.Action = NotifyCollectionChangedEventAction.InitialState;
            this.initialState = initialState;
        }

        public SlimNotifyCollectionChangedEvent(SlimAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);

            this.Action = NotifyCollectionChangedEventAction.Add;
            this.added = added;
        }

        public SlimNotifyCollectionChangedEvent(SlimRemoved removed)
        {
            Contract.Requires<ArgumentNullException>(removed != null);

            this.Action = NotifyCollectionChangedEventAction.Remove;
            this.removed = removed;
        }

        public SlimNotifyCollectionChangedEvent(SlimMoved moved)
        {
            Contract.Requires<ArgumentNullException>(moved != null);

            this.Action = NotifyCollectionChangedEventAction.Move;
            this.moved = moved;
        }

        public SlimNotifyCollectionChangedEvent(SlimReplaced<T> replaced)
        {
            Contract.Requires<ArgumentNullException>(replaced != null);

            this.Action = NotifyCollectionChangedEventAction.Replace;
            this.replaced = replaced;
        }

        public SlimNotifyCollectionChangedEvent(SlimReset<T> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);

            this.Action = NotifyCollectionChangedEventAction.Reset;
            this.reset = reset;
        }

        public NotifyCollectionChangedEventAction Action { get; private set; }

        readonly SlimInitialState<T> initialState;
        public SlimInitialState<T> InitialState
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.InitialState || Contract.Result<SlimInitialState<T>>() != null);

                return initialState;
            }
        }

        readonly SlimAdded<T> added;
        public SlimAdded<T> Added
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Add || Contract.Result<SlimAdded<T>>() != null);

                return added;
            }
        }

        readonly SlimRemoved removed;
        public SlimRemoved Removed
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Remove || Contract.Result<SlimRemoved>() != null);

                return removed;
            }
        }

        readonly SlimReplaced<T> replaced;
        public SlimReplaced<T> Replaced
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Replace || Contract.Result<SlimReplaced<T>>() != null);

                return replaced;
            }
        }

        readonly SlimMoved moved;
        public SlimMoved Moved
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Move || Contract.Result<SlimMoved>() != null);

                return moved;
            }
        }

        readonly SlimReset<T> reset;
        public SlimReset<T> Reset
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Reset || Contract.Result<SlimReset<T>>() != null);

                return reset;
            }
        }

        public override string ToString()
        {
            switch(Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    return InitialState.ToString();
                case NotifyCollectionChangedEventAction.Add:
                    return Added.ToString();
                case NotifyCollectionChangedEventAction.Move:
                    return Moved.ToString();
                case NotifyCollectionChangedEventAction.Remove:
                    return Removed.ToString();
                case NotifyCollectionChangedEventAction.Replace:
                    return Replaced.ToString();
                case NotifyCollectionChangedEventAction.Reset:
                    return Reset.ToString();
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

    }
}
