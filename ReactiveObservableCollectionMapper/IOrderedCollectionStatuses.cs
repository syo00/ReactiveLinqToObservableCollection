using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IOrderedCollectionStatuses_Contract<>))]
    public interface IOrderedCollectionStatuses<TElement> : ICollectionStatuses<TElement>
    {
        IOrderedCollectionStatuses<TElement> CreateOrderedCollectionStatuses<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }

    [ContractClassFor(typeof(IOrderedCollectionStatuses<>))]
    abstract class IOrderedCollectionStatuses_Contract<TElement> : IOrderedCollectionStatuses<TElement>
    {
        public IOrderedCollectionStatuses<TElement> CreateOrderedCollectionStatuses<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<IOrderedCollectionStatuses<TElement>>() != null);

            throw new NotImplementedException();
        }

        public IEnumerable<TElement> Current
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<INotifyCollectionChangedEvent<TElement>> InitialStateAndChanged
        {
            get { throw new NotImplementedException(); }
        }
    }
}
