using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public class GeneratedObservableCollection<T> : ReadOnlyObservableCollection<T>, IGeneratedObservableCollection<T>, IDisposable
    {
        IDisposable subscription;

        public GeneratedObservableCollection()
            : this(CollectionStatuses.Empty<T>().ToInstance())
        {

        }

        internal GeneratedObservableCollection(CollectionStatuses<T> statuses)
            : base(new MultiValuesObservableCollection<T>())
        {
            Contract.Requires<ArgumentNullException>(statuses != null);

            this.items = new Lazy<MultiValuesObservableCollection<T>>(() => (MultiValuesObservableCollection<T>)base.Items);

            var x = new DelegationObservableCollectionCollectionStatuses<T>(
                statuses,
                () => this.items.Value,
                () => IsInitialStateArrived = true,
                ex => RaisedError = ex,
                () => IsCompleted = true);

            this.subscription = x;
            this.statuses = x;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(items != null);
        }

        Lazy<MultiValuesObservableCollection<T>> items;
        protected new MultiValuesObservableCollection<T> Items
        {
            get
            {
                Contract.Ensures(Contract.Result<MultiValuesObservableCollection<T>>() != null);

                return items.Value;
            }
        }

        bool isCompleted;
        public bool IsCompleted
        {
            get
            {
                ThrowExceptionIfDisposed();

                return isCompleted;
            }
            private set
            {
                if (isCompleted && Completed != null)
                {
                    Completed(this, new EventArgs());
                }

                isCompleted = value;
            }
        }

        public event EventHandler Completed;

        Exception raisedError;
        public Exception RaisedError
        {
            get
            {
                ThrowExceptionIfDisposed();

                return raisedError;
            }
            private set
            {
                Contract.Requires<ArgumentNullException>(value != null);

                if (raisedError == null && ErrorRaised != null)
                {
                    ErrorRaised(this, new ErrorEventArgs(value));
                }

                raisedError = value;
            }
        }

        public event EventHandler<ErrorEventArgs> ErrorRaised;

        bool isInitialStateArrived;
        public bool IsInitialStateArrived
        {
            get
            {
                ThrowExceptionIfDisposed();

                return isInitialStateArrived;
            }
            private set
            {
                Contract.Requires<ArgumentException>(value == true);

                if (!isInitialStateArrived && InitialStateArrived != null)
                {
                    InitialStateArrived(this, new EventArgs());
                }

                isInitialStateArrived = value;
            }
        }

        public event EventHandler InitialStateArrived;

        ICollectionStatuses<T> statuses;
        public ICollectionStatuses<T> Statuses
        {
            get
            {
                Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);
                ThrowExceptionIfDisposed();

                return statuses;
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (IsDisposed) return;
            if (isDisposing) OnDisposingUnManagedResources();
            OnDisposingManagedResources();
            IsDisposed = true;
        }

        protected virtual void OnDisposingManagedResources()
        {
            if (subscription != null)
            {
                subscription.Dispose();
            }
        }

        protected virtual void OnDisposingUnManagedResources()
        {

        }
        #endregion

        ~GeneratedObservableCollection()
        {
            Dispose(false);
        }
    }
}
