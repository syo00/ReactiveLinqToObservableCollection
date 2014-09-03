using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    // Subscribe が 0 になったら Subject を作り直す
    internal abstract class SubjectBase<T> : ISubject<T>, IDisposable
    {
        readonly List<IObserver<T>> observers = new List<IObserver<T>>();
        readonly object gate = new object();

        protected SubjectBase()
        {
            UpdateSubject();
        }

        void UpdateSubject()
        {
            lock (gate)
            {
                currentSubject = CreateSubject();
            }
        }

        protected abstract ISubject<T> CreateSubject();

        ISubject<T> currentSubject;
        protected ISubject<T> CurrentSubject
        {
            get
            {
                Contract.Ensures(Contract.Result<ISubject<T>>() != null);

                return currentSubject;
            }
        }

        public void OnCompleted()
        {
            CurrentSubject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            CurrentSubject.OnError(error);
        }

        public void OnNext(T value)
        {
            CurrentSubject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (gate)
            {
                ThrowExceptionIfDisposed();
                observers.Add(observer);
            }
            var subscription = CurrentSubject.Subscribe(observer);
            return Disposable.Create(() =>
            {
                lock (gate)
                {
                    observer.OnCompleted();
                    subscription.Dispose();
                    observers.Remove(observer);
                    if (observers.Count == 0)
                    {
                        CurrentSubject.OnCompleted();
                        UpdateSubject();
                    }
                }
            });
        }

        #region IDispose implementation
        private bool IsDisposed
        {
            get;
            set;
        }

        private void ThrowExceptionIfDisposed()
        {
            lock (gate)
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().FullName + " has been already disposed.");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            lock (gate)
            {
                if (IsDisposed) return;
                if (isDisposing) OnDisposingUnManagedResources();
                OnDisposingManagedResources();
                IsDisposed = true;
            }
        }

        protected virtual void OnDisposingManagedResources()
        {

        }

        protected virtual void OnDisposingUnManagedResources()
        {

        }
        #endregion

        ~SubjectBase()
        {
            Dispose(false);
        }
    }
}
