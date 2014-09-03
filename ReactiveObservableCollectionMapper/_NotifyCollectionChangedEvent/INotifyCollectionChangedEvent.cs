using System;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(INotifyCollectionChangedEventContract<>))]
    public interface INotifyCollectionChangedEvent<out T>
    {
        NotifyCollectionChangedEventAction Action { get; }
        IAdded<T> Added { get; }
        IInitialState<T> InitialState { get; }
        IMoved<T> Moved { get; }
        IRemoved<T> Removed { get; }
        IReplaced<T> Replaced { get; }
        IReset<T> Reset { get; }
    }

    [ContractClassFor(typeof(INotifyCollectionChangedEvent<>))]
    abstract class INotifyCollectionChangedEventContract<T> : INotifyCollectionChangedEvent<T>
    {
        public NotifyCollectionChangedEventAction Action
        {
            get { throw new NotImplementedException(); }
        }

        public IAdded<T> Added
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Add || Contract.Result<IAdded<T>>() != null);
                throw new NotImplementedException();
            }
        }

        public IInitialState<T> InitialState
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.InitialState || Contract.Result<IInitialState<T>>() != null);
                throw new NotImplementedException();
            }
        }

        public IMoved<T> Moved
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Move || Contract.Result<IMoved<T>>() != null);
                throw new NotImplementedException();
            }
        }

        public IRemoved<T> Removed
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Remove || Contract.Result<IRemoved<T>>() != null);
                throw new NotImplementedException();
            }
        }

        public IReplaced<T> Replaced
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Replace || Contract.Result<IReplaced<T>>() != null);
                throw new NotImplementedException();
            }
        }

        public IReset<T> Reset
        {
            get
            {
                Contract.Ensures(Action != NotifyCollectionChangedEventAction.Reset || Contract.Result<IReset<T>>() != null);
                throw new NotImplementedException();
            }
        }
    }
}
