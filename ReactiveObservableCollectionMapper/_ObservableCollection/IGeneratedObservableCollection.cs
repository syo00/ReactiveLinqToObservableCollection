using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IGeneratedObservableCollectionContract<>))]
    public interface IGeneratedObservableCollection<out T> : IReadOnlyList<T>, ICollectionStatusesAttached<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        bool IsInitialStateArrived { get; }
        event EventHandler InitialStateArrived;
        bool IsCompleted { get; }
        event EventHandler Completed;
        Exception RaisedError { get; }
        event EventHandler<ErrorEventArgs> ErrorRaised;
    }

    [ContractClassFor(typeof(IGeneratedObservableCollection<>))]
    abstract class IGeneratedObservableCollectionContract<T> : IGeneratedObservableCollection<T>
    {
        public bool IsInitialStateArrived
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler InitialStateArrived
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public bool IsCompleted
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler Completed
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public Exception RaisedError
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<ErrorEventArgs> ErrorRaised
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public T this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public ICollectionStatuses<T> Statuses
        {
            get { throw new NotImplementedException(); }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }
    }

}
