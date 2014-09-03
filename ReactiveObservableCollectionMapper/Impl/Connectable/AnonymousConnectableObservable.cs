using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Kirinji.LinqToObservableCollection.Impl.Connectable
{
    class AnonymousConnectableObservable<T> : IConnectableObservable<T>
    {
        readonly Func<IObserver<T>, IDisposable> subscribe;
        IDisposable connecting;
        readonly Func<IDisposable> connect;
        readonly object gate = new object();

        public AnonymousConnectableObservable(Func<IObserver<T>, IDisposable> subscribe, Func<IDisposable> connect)
        {
            Contract.Requires<ArgumentNullException>(subscribe != null);
            Contract.Requires<ArgumentNullException>(connect != null);

            this.subscribe = subscribe;
            this.connect = connect;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(subscribe != null);
            Contract.Invariant(connect != null);
        }

        public IDisposable Connect()
        {
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            lock (gate)
            {
                if (connecting == null)
                {
                    var subscription = connect();
                    connecting = Disposable.Create(() =>
                        {
                            lock (gate)
                            {
                                subscription.Dispose();
                                connecting = null;
                            }
                        });
                    return connecting;
                }

                return connecting;
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return subscribe(observer);
        }
    }
}
