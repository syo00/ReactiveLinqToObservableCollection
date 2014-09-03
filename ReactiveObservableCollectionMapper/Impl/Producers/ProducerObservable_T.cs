using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class ProducerObservable<T> : IObservable<T>
    {
        readonly Func<Producer<T>> factory;

        public ProducerObservable(Func<Producer<T>> factory)
        {
            Contract.Requires<ArgumentNullException>(factory != null);

            this.factory = factory;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(factory != null);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return 
                Observable.Create<T>(o =>
                {
                    return factory().Subscribe(o);
                })
                .Subscribe(observer);
        }
    }
}
