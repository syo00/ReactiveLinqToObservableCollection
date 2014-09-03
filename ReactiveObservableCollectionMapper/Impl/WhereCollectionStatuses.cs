using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Kirinji.LightWands;
using System.Reactive;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection;

namespace Kirinji.LinqToObservableCollection.Impl
{
    class WhereCollectionStatuses<T> : CollectionStatuses<T>
    {
        readonly CollectionStatuses<T> source;
        readonly Func<T, bool> predicate;

        public WhereCollectionStatuses(CollectionStatuses<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            this.source = source;
            this.predicate = predicate;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(predicate != null);
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            return Materialize().InitialStateAndChanged;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvents.SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            return Materialize().SlimInitialStateAndChanged;
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            return Materialize().SimpleInitialStateAndChanged;
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvents.SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
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

        public override CollectionStatuses<T> CreateWhere(Func<T, bool> predicate)
        {
            return new WhereCollectionStatuses<T>(source, x => this.predicate(x) && predicate(x));
        }

        public override CollectionStatuses<TTo> CreateSelect<TTo>(Func<T, TTo> converter)
        {
            return new WhereSelectCollectionStatuses<T, TTo>(source, predicate, converter);
        }

        CollectionStatuses<T> Materialize()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return 
                ProducerObservable.Create(() => new WhereSelectProducer<T, T>(source, predicate, x => x))
                .CheckWhenDebug(true)
                .ToStatuses();
        }
    }
}
