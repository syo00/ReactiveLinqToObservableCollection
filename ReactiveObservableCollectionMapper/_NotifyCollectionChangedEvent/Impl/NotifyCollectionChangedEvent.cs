using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    sealed class NotifyCollectionChangedEvent<T> : INotifyCollectionChangedEvent<T>
    {
        public NotifyCollectionChangedEvent(IInitialState<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);

            this.Action = NotifyCollectionChangedEventAction.InitialState;
            this.initialState = initialState;
        }

        public NotifyCollectionChangedEvent(IAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);

            this.Action = NotifyCollectionChangedEventAction.Add;
            this.added = added;
        }

        public NotifyCollectionChangedEvent(IRemoved<T> removed)
        {
            Contract.Requires<ArgumentNullException>(removed != null);

            this.Action = NotifyCollectionChangedEventAction.Remove;
            this.removed = removed;
        }

        public NotifyCollectionChangedEvent(IMoved<T> moved)
        {
            Contract.Requires<ArgumentNullException>(moved != null);

            this.Action = NotifyCollectionChangedEventAction.Move;
            this.moved = moved;
        }

        public NotifyCollectionChangedEvent(IReplaced<T> replaced)
        {
            Contract.Requires<ArgumentNullException>(replaced != null);

            this.Action = NotifyCollectionChangedEventAction.Replace;
            this.replaced = replaced;
        }

        public NotifyCollectionChangedEvent(IReset<T> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);

            this.Action = NotifyCollectionChangedEventAction.Reset;
            this.reset = reset;
        }

        public NotifyCollectionChangedEventAction Action { get; private set; }

        readonly IInitialState<T> initialState;
        public IInitialState<T> InitialState
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.InitialState ||Contract.Result<IInitialState<T>>() != null);

                return initialState;
            }
        }

        readonly IAdded<T> added;
        public IAdded<T> Added
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Add || Contract.Result<IAdded<T>>() != null);

                return added;
            }
        }

        readonly IRemoved<T> removed;
        public IRemoved<T> Removed
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Remove || Contract.Result<IRemoved<T>>() != null);

                return removed;
            }
        }

        readonly IReplaced<T> replaced;
        public IReplaced<T> Replaced
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Replace || Contract.Result<IReplaced<T>>() != null);

                return replaced;
            }
        }

        readonly IMoved<T> moved;
        public IMoved<T> Moved
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Move || Contract.Result<IMoved<T>>() != null);

                return moved;
            }
        }

        readonly IReset<T> reset;
        public IReset<T> Reset
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Reset || Contract.Result<IReset<T>>() != null);

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
