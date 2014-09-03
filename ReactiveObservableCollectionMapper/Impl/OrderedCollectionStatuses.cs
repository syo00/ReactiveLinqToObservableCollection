using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    internal class OrderedCollectionStatuses<T> : CollectionStatuses<T>, IOrderedCollectionStatuses<T>
    {
        readonly CollectionStatuses<T> source;
        readonly CompositeOrderingComparer<T> order;
        readonly Lazy<CollectionStatuses<T>> materialized;

        public OrderedCollectionStatuses(IOrderedCollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = new AnonymousCollectionStatuses<T>(source.InitialStateAndChanged);
            this.materialized = new Lazy<CollectionStatuses<T>>(() => Materialize());
        }

        public OrderedCollectionStatuses(CollectionStatuses<T> source, IOrderingComparer<T> order)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(order != null);

            this.source = source;
            this.order = new CompositeOrderingComparer<T>(new[] { order });
            this.materialized = new Lazy<CollectionStatuses<T>>(() => Materialize());
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(materialized != null);
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            return materialized.Value.InitialStateAndChanged;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            return materialized.Value.SlimInitialStateAndChanged;
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            return materialized.Value.SimpleInitialStateAndChanged;
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            return materialized.Value.SlimSimpleInitialStateAndChanged;
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                return materialized.Value.RecommendedEvent;
            }
        }

        IOrderedCollectionStatuses<T> IOrderedCollectionStatuses<T>.CreateOrderedCollectionStatuses<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            IOrderingComparer<T> order = new OrderingComparer<T, TKey>(keySelector, comparer, descending);
            return CreateThenBy(order);
        }

        public OrderedCollectionStatuses<T> CreateThenBy(IOrderingComparer<T> order)
        {
            Contract.Requires<ArgumentNullException>(order != null);
            Contract.Ensures(Contract.Result<OrderedCollectionStatuses<T>>() != null);

            if (this.order == null)
            {
                return new OrderedCollectionStatuses<T>(source, order);
            }
            else
            {
                return new OrderedCollectionStatuses<T>(source, this.order.CombineToLast(order));
            }
        }

        //↓本家 LINQ to Objects では OrderBy(Descending)? の重ねがけは最適化されていないっぽい？ので、それに合わせて一応こちらも無効に
        //public override OrderedCollectionStatuses<T> CreateOrdered<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        //{
        //    IOrderingComparer<T> order = new OrderingComparer<T, TKey>(keySelector, comparer, descending);
        //    return new OrderedCollectionStatuses<T>(source, this.order.CombineToFirst(order));
        //}

        CollectionStatuses<T> Materialize()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            if (order == null)
            {
                return source;
            }
            else
            {
                var o = order;
                var simpleEvents = ProducerObservable.Create(() => new OrderProducer<T>(source, o));
                return simpleEvents.ToStatuses();
            }
        }
    }
}
