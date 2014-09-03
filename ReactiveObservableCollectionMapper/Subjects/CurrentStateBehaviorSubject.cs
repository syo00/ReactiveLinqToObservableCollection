using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Subjects
{
    public class CurrentStateBehaviorSubject<T> : ICollectionStatusesSubject<T>
    {
        readonly object gate = new object();
        List<T> currentState = new List<T>();
        bool isInitialStateArrived;
        bool isCompleted;
        ValueOrEmpty<Exception> lastError;
        Subject<INotifyCollectionChangedEvent<T>> initialStateAndChanged = new Subject<INotifyCollectionChangedEvent<T>>();
        IObservable<INotifyCollectionChangedEvent<T>> observableCore;

        public CurrentStateBehaviorSubject()
        {
            this.observableCore = Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
                {
                    lock (gate)
                    {
                        if (lastError.HasValue)
                        {
                            if (isInitialStateArrived)
                            {
                                observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(CurrentState));
                            }

                            observer.OnError(lastError.Value);
                            return System.Reactive.Disposables.Disposable.Empty;
                        }

                        if (isCompleted)
                        {
                            if (isInitialStateArrived)
                            {
                                observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(CurrentState));
                            }

                            observer.OnCompleted();
                            return System.Reactive.Disposables.Disposable.Empty;
                        }

                        if (isInitialStateArrived)
                        {
                            return Changed
                                .StartWith(NotifyCollectionChangedEvent.CreateInitialStateEvent(CurrentState))
                                .Subscribe(observer);
                        }
                        else
                        {
                            return initialStateAndChanged
                                .Subscribe(observer);
                        }
                    }
                });
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(gate != null);
            Contract.Invariant(IsDisposed || currentState != null);
            Contract.Invariant(IsDisposed || initialStateAndChanged != null);
            Contract.Invariant(IsDisposed || observableCore != null);
        }

        IObservable<INotifyCollectionChangedEvent<T>> Changed
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

                lock (gate)
                {
                    ThrowExceptionIfDisposed();

                    return initialStateAndChanged
                        .Where(e => e.Action != NotifyCollectionChangedEventAction.InitialState);
                }
            }
        }

        public bool IsInitialStateArrived
        {
            get
            {
                lock (gate)
                {
                    ThrowExceptionIfDisposed();

                    return isInitialStateArrived;
                }
            }
        }

        public IReadOnlyList<T> CurrentState
        {
            get
            {
                lock (gate)
                {
                    ThrowExceptionIfDisposed();

                    return currentState.ToArray().ToReadOnly();
                }
            }
        }

        public void OnCompleted()
        {
            lock (gate)
            {
                ThrowExceptionIfDisposed();

                if (lastError.HasValue || isCompleted)
                {
                    return;
                }

                isCompleted = true;
                initialStateAndChanged.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            lock (gate)
            {
                ThrowExceptionIfDisposed();

                if (lastError.HasValue || isCompleted)
                {
                    return;
                }

                lastError = new ValueOrEmpty<Exception>(error);
                initialStateAndChanged.OnError(error);
            }
        }

        public void OnNext(INotifyCollectionChangedEvent<T> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            lock (gate)
            {
                ThrowExceptionIfDisposed();

                if(lastError.HasValue || isCompleted)
                {
                    return;
                }

                if (value.Action == NotifyCollectionChangedEventAction.InitialState)
                {
                    if (isInitialStateArrived)
                    {
                        var error = new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, value);
                        lastError = new ValueOrEmpty<Exception>(error);
                        initialStateAndChanged.OnError(error);
                        return;
                    }

                    isInitialStateArrived = true;
                }
                else
                {
                    if (!isInitialStateArrived)
                    {
                        var error = new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, value);
                        lastError = new ValueOrEmpty<Exception>(error);
                        initialStateAndChanged.OnError(error);
                        return;
                    }
                }

                try
                {
                    currentState.ApplyChangeEvent(value);
                }
                catch(InvalidInformationException<T> e)
                {
                    lastError = new ValueOrEmpty<Exception>(e);
                    initialStateAndChanged.OnError(e);
                    return;
                }

                initialStateAndChanged.OnNext(value);
            }
        }

        public IObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged
        {
            get
            {
                lock (gate)
                {
                    ThrowExceptionIfDisposed();

                    return observableCore;
                }
            }
        }

        #region IDispose implementation
        private bool IsDisposed
        {
            get;
            set;
        }

        private void ThrowExceptionIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName + " has been already disposed.");
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (IsDisposed) return;

                initialStateAndChanged.Dispose();
                initialStateAndChanged = null;
                currentState = null;
                lastError = new ValueOrEmpty<Exception>();
                observableCore = null;

                IsDisposed = true;
            }
        }
        #endregion
    }
}
