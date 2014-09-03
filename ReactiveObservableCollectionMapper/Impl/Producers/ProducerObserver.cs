using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    internal class ProducerObserver<T> : IObserver<T>
    {
        readonly IObserver<T> core;

        public ProducerObserver(IObserver<T> observer)
        {
            Contract.Requires<ArgumentNullException>(observer != null);

            this.core = Observer.Create<T>(value =>
                {
                    if (IsStopped)
                    {
                        return;
                    }
                    observer.OnNext(value);
                }, ex =>
                {
                    if (IsStopped)
                    {
                        return;
                    }
                    observer.OnError(ex);
                    this.Error = ex;
                }, () =>
                {
                    if (IsStopped)
                    {
                        return;
                    }
                    observer.OnCompleted();
                    this.IsCompleted = true;
                });
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(core != null);
        }


        public Exception Error { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool NeverPush { get; private set; }

        public bool IsStopped
        {
            get
            {
                return Error != null || IsCompleted || NeverPush;
            }
        }

        public void OnCompleted()
        {
            core.OnCompleted();
        }

        public void OnError(Exception error)
        {
            core.OnError(error);
        }

        public void OnNext(T value)
        {
            core.OnNext(value);
        }

        public void Never()
        {
            this.NeverPush = true;
        }
    }
}
