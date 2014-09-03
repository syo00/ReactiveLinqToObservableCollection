using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    class ReverseCollectionStatuses<T> : CollectionStatuses<T>
    {
        readonly CollectionStatuses<T> source;

        public ReverseCollectionStatuses(CollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            return Materialize().InitialStateAndChanged;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            return Materialize().SlimInitialStateAndChanged;
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            return Materialize().SimpleInitialStateAndChanged;
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            return Materialize().SlimSimpleInitialStateAndChanged;
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                return new RecommendedEvent(false, true, false, false);
            }
        }

        public override CollectionStatuses<T> CreateReverse()
        {
            return source;
        }

        CollectionStatuses<T> Materialize()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return
                ProducerObservable.Create(() => new ReverseProducer<T>(source))
                .ToStatuses();
        }
    }
}
