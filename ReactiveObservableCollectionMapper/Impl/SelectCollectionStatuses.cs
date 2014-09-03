using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.Specialized;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Impl
{
    class SelectCollectionStatuses<T, TTo> : CollectionStatuses<TTo>
    {
        readonly CollectionStatuses<T> source;
        readonly Func<T, TTo> converter;

        public SelectCollectionStatuses(CollectionStatuses<T> source, Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);

            this.source = source;
            this.converter = converter;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(converter != null);
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<TTo>> CreateSlimInitialStateAndChanged()
        {
            return Materialize();
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvents.SimpleNotifyCollectionChangedEvent<TTo>> CreateSimpleInitialStateAndChanged()
        {
            return CreateInitialStateAndChanged().Simplify();
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<TTo>> CreateSlimSimpleInitialStateAndChanged()
        {
            return Materialize().ExtractFromSlim().Simplify().ToSlim();
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                return new RecommendedEvent(false, true, false, false);
            }
        }

        public override CollectionStatuses<TTo2> CreateSelect<TTo2>(Func<TTo, TTo2> converter)
        {
            return new SelectCollectionStatuses<T, TTo2>(source, x => converter(this.converter(x)));
        }

        public override CollectionStatuses<TTo2> CreateSelectMany<TTo2>(Func<TTo, IEnumerable<TTo2>> converter)
        {
            return new SelectManyCollectionStatuses<T, TTo2>(source, x => converter(this.converter(x)));
        }

        IObservable<SlimNotifyCollectionChangedEvent<TTo>> Materialize()
        {
            Contract.Ensures(Contract.Result<IObservable<SlimNotifyCollectionChangedEvent<TTo>>>() != null);

            return ProducerObservable.Create(() => new SelectProducer<T, TTo>(source, converter));
        }
    }
}
