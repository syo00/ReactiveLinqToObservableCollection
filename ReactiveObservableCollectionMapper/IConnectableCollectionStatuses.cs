using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IConnectableCollectionStatusesContract<>))]
    public interface IConnectableCollectionStatuses<out T> : ICollectionStatuses<T>
    {
        IDisposable Connect();

        // IConnectableObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged { get; }
        // ↑この形式だと InitialStateAndChanged <-> SimpleInitialStateAndChanged の変換などが面倒っぽいので見送り
    }

    [ContractClassFor(typeof(IConnectableCollectionStatuses<>))]
    abstract class IConnectableCollectionStatusesContract<T> : IConnectableCollectionStatuses<T>
    {
        public IDisposable Connect()
        {
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            throw new NotImplementedException();
        }

        public IObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged
        {
            get { throw new NotImplementedException(); }
        }
    }
}
