using Kirinji.LinqToObservableCollection.Impl.Producers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl
{
    internal class WhereSelectCollectionStatuses<T, TTo> : CollectionStatuses<TTo>
    {
        readonly CollectionStatuses<T> source;
        readonly Func<T, bool> predicate;
        readonly Func<T, TTo> converter;

        public WhereSelectCollectionStatuses(CollectionStatuses<T> source, Func<T, bool> predicate, Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Requires<ArgumentNullException>(converter != null);

            this.source = source;
            this.predicate = predicate;
            this.converter = converter;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(predicate != null);
            Contract.Invariant(converter != null);
        }

        protected override IObservable<INotifyCollectionChangedEvent<TTo>> CreateInitialStateAndChanged()
        {
            return Materialize().InitialStateAndChanged;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvents.SlimNotifyCollectionChangedEvent<TTo>> CreateSlimInitialStateAndChanged()
        {
            return Materialize().SlimInitialStateAndChanged;
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvents.SimpleNotifyCollectionChangedEvent<TTo>> CreateSimpleInitialStateAndChanged()
        {
            return Materialize().SimpleInitialStateAndChanged;
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvents.SlimSimpleNotifyCollectionChangedEvent<TTo>> CreateSlimSimpleInitialStateAndChanged()
        {
            return Materialize().SlimSimpleInitialStateAndChanged;
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                return new RecommendedEvent(true, false, false, false);
            }
        }

        public override CollectionStatuses<TTo2> CreateSelect<TTo2>(Func<TTo, TTo2> converter)
        {
            return new WhereSelectCollectionStatuses<T, TTo2>(source, predicate, x => converter(this.converter(x)));
        }

        CollectionStatuses<TTo> Materialize()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<TTo>>() != null);

            return
                ProducerObservable.Create(() => new WhereSelectProducer<T, TTo>(source, predicate, converter))
                .CheckWhenDebug(true)
                .ToStatuses();
        }
    }
}
