using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    static class ProducerObservable
    {
        public static IObservable<T> Create<T>(Func<Producer<T>> factory)
        {
            Contract.Requires<ArgumentNullException>(factory != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            return new ProducerObservable<T>(factory);
        }
    }
}
