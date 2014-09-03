using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support.Extractors
{
    [ContractClass(typeof(ISimpleNotifyCollectionChangedEventExtractorContract<>))]
    internal interface ISimpleNotifyCollectionChangedEventExtractor<T>
    {
        IReadOnlyList<INotifyCollectionChangedEvent<T>> Extract(SimpleNotifyCollectionChangedEvent<T> source);
    }


    [ContractClassFor(typeof(ISimpleNotifyCollectionChangedEventExtractor<>))]
    abstract class ISimpleNotifyCollectionChangedEventExtractorContract<T> : ISimpleNotifyCollectionChangedEventExtractor<T>
    {
        public IReadOnlyList<INotifyCollectionChangedEvent<T>> Extract(SimpleNotifyCollectionChangedEvent<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IReadOnlyCollection<INotifyCollectionChangedEvent<T>>>() != null);

            throw new NotImplementedException();
        }
    }

}
