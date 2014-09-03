using Kirinji.LinqToObservableCollection.Impl.Producers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Abort
{
    internal class AbortException : Exception
    {
        public AbortException(bool completed)
        {
            this.Completed = completed;
        }

        public AbortException(Exception error)
        {
            Contract.Requires<ArgumentNullException>(error != null);

            this.Error = error;
        }

        public bool Completed { get; private set; }
        public Exception Error { get; private set; }

        public void PushToObserver<T>(ProducerObserver<T> observer)
        {
            Contract.Requires<ArgumentNullException>(observer != null);

            if (Error != null)
            {
                observer.OnError(Error);
                return;
            }
            if (Completed)
            {
                observer.OnCompleted();
                return;
            }
            observer.Never();
        }
    }
}
