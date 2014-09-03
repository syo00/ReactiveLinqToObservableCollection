using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl
{
    class SelectManyCollectionStatuses<T, TTo> : CollectionStatuses<TTo>
    {
        readonly CollectionStatuses<T> source;
        readonly Func<T, IEnumerable<TTo>> converter;

        public SelectManyCollectionStatuses(CollectionStatuses<T> source, Func<T, IEnumerable<TTo>> converter)
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

        public override CollectionStatuses<TTo2> CreateSelectMany<TTo2>(Func<TTo, IEnumerable<TTo2>> converter)
        {
            return new SelectManyCollectionStatuses<T, TTo2>(source, x => this.converter(x).SelectMany(converter));
        }

        CollectionStatuses<TTo> Materialize()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<TTo>>() != null);

            return
                ProducerObservable.Create(() =>
                    {
                        var a = 
                            source
                            .CreateSelect(x => converter(x).ToArray().ToReadOnly())
                            .CreateSelect(x => CollectionStatuses.Return(x).ToInstance());
                        
                        return new FlattenProducer<TTo>(a);
                    })
                .CheckWhenDebug()
                .ToStatuses();
        }
    }
}
