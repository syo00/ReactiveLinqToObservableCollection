using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    class SkipAndTakeCollectionStatuses<T> : CollectionStatuses<T>
    {
        readonly CollectionStatuses<T> source;
        readonly int skipCount;
        readonly int? takeCount;


        public SkipAndTakeCollectionStatuses(CollectionStatuses<T> source, int skipCount, int? takeCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(skipCount >= 0);
            Contract.Requires<ArgumentException>(takeCount == null || takeCount.Value >= 0);

            this.source = source;
            this.skipCount = skipCount;
            this.takeCount = takeCount;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(skipCount >= 0);
            Contract.Invariant(takeCount == null || takeCount.Value >= 0);
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
                return new RecommendedEvent(false, false, true, false);
            }
        }

        public override CollectionStatuses<T> CreateSkip(int skipCount)
        {
            return new SkipAndTakeCollectionStatuses<T>(source, this.skipCount + skipCount, takeCount - skipCount);
        }

        public override CollectionStatuses<T> CreateTake(int takeCount)
        {
            if (this.takeCount != null && takeCount >= this.takeCount)
            {
                return this;
            }
            else
            {
                return new SkipAndTakeCollectionStatuses<T>(source, skipCount, takeCount);
            }
        }

        CollectionStatuses<T> Materialize()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            var result = source;

            if(takeCount != null && takeCount.Value == 0)
            {
                return CollectionStatuses.Empty<T>().ToInstance(); 
            }

            if (skipCount >= 1)
            {
                var inner = result;
                result = ProducerObservable.Create(() => new SkipProducer<T>(inner, skipCount)).ToStatuses();
            }
            if (takeCount != null)
            {
                var inner = result;
                result = ProducerObservable.Create(() => new TakeProducer<T>(inner, takeCount.Value)).ToStatuses();
            }

            return result;
        }
    }
}
