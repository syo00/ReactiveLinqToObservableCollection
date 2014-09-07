using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl
{
    [ContractClass(typeof(CollectionStatusesContract<>))]
    internal abstract class CollectionStatuses<T> : ICollectionStatuses<T>
    {
        readonly Lazy<GeneratedObservableCollection<T>> toObservableCollection;
        readonly Lazy<IObservable<INotifyCollectionChangedEvent<T>>> initialStateAndChanged;
        readonly Lazy<IObservable<SlimNotifyCollectionChangedEvent<T>>> slimInitialStateAndChanged;
        readonly Lazy<IObservable<SimpleNotifyCollectionChangedEvent<T>>> simpleInitialStateAndChanged;
        readonly Lazy<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>> slimSimpleInitialStateAndChanged;
        readonly Lazy<IObservable<NotifyCollectionChangedEventObject<T>>> eventsAsEventObject;

        public CollectionStatuses()
        {
            this.initialStateAndChanged = new Lazy<IObservable<INotifyCollectionChangedEvent<T>>>(() => CreateInitialStateAndChanged());
            this.slimInitialStateAndChanged = new Lazy<IObservable<SlimNotifyCollectionChangedEvent<T>>>(() => CreateSlimInitialStateAndChanged());
            this.simpleInitialStateAndChanged = new Lazy<IObservable<SimpleNotifyCollectionChangedEvent<T>>>(() => CreateSimpleInitialStateAndChanged());
            this.slimSimpleInitialStateAndChanged = new Lazy<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>>(() => CreateSlimSimpleInitialStateAndChanged());
            this.eventsAsEventObject = new Lazy<IObservable<NotifyCollectionChangedEventObject<T>>>(() => CreateEventsAsEventObject());
            this.toObservableCollection
                = new Lazy<GeneratedObservableCollection<T>>(() => new GeneratedObservableCollection<T>(this));
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(toObservableCollection != null);
            Contract.Invariant(initialStateAndChanged != null);
            Contract.Invariant(slimInitialStateAndChanged != null);
            Contract.Invariant(simpleInitialStateAndChanged != null);
            Contract.Invariant(slimSimpleInitialStateAndChanged != null);
            Contract.Invariant(eventsAsEventObject != null);
        }

        public IObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

                var result = initialStateAndChanged.Value;
                Contract.Assume(result != null);
                return result;
            }
        }

        public IObservable<SlimNotifyCollectionChangedEvent<T>> SlimInitialStateAndChanged
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<SlimNotifyCollectionChangedEvent<T>>>() != null);

                var result = slimInitialStateAndChanged.Value;
                Contract.Assume(result != null);
                return result;
            }
        }

        public IObservable<SimpleNotifyCollectionChangedEvent<T>> SimpleInitialStateAndChanged
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

                var result = simpleInitialStateAndChanged.Value;
                Contract.Assume(result != null);
                return result;
            }
        }

        public IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> SlimSimpleInitialStateAndChanged
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>>() != null);

                var result = slimSimpleInitialStateAndChanged.Value;
                Contract.Assume(result != null);
                return result;
            }
        }

        public IObservable<NotifyCollectionChangedEventObject<T>> EventsAsEventObject
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<NotifyCollectionChangedEventObject<T>>>() != null);

                var result = eventsAsEventObject.Value;
                Contract.Assume(result != null);
                return result;
            }
        }

        public abstract RecommendedEvent RecommendedEvent { get; }

        protected abstract IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged();

        protected virtual IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            return CreateSlimInitialStateAndChanged().ExtractFromSlim();
        }

        protected abstract IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged();

        protected virtual IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

            return CreateSlimSimpleInitialStateAndChanged().ExtractFromSlim();
        }

        protected virtual IObservable<NotifyCollectionChangedEventObject<T>> CreateEventsAsEventObject()
        {
            Contract.Ensures(Contract.Result<IObservable<NotifyCollectionChangedEventObject<T>>>() != null);

            if (RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return SlimInitialStateAndChanged
                    .Select(e => new NotifyCollectionChangedEventObject<T>(e));
            }

            if (RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return SimpleInitialStateAndChanged
                    .Select(e => new NotifyCollectionChangedEventObject<T>(e));
            }

            if (RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return SlimSimpleInitialStateAndChanged
                    .Select(e => new NotifyCollectionChangedEventObject<T>(e));
            }

            return InitialStateAndChanged
                    .Select(e => new NotifyCollectionChangedEventObject<T>(e));
        }

        public virtual GeneratedObservableCollection<T> ToObservableCollection()
        {
            Contract.Ensures(Contract.Result<GeneratedObservableCollection<T>>() != null);

            return toObservableCollection.Value;
        }

        public virtual CollectionStatuses<T> CreateWhere(Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return new WhereCollectionStatuses<T>(this, predicate);
        }

        public virtual CollectionStatuses<TTo> CreateSelect<TTo>(Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<TTo>>() != null);

            return new SelectCollectionStatuses<T, TTo>(this, converter);
        }

        public virtual CollectionStatuses<TTo> CreateSelectMany<TTo>(Func<T, IEnumerable<TTo>> converter)
        {
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<TTo>>() != null);

            return new SelectManyCollectionStatuses<T, TTo>(this, converter);
        }

        public virtual OrderedCollectionStatuses<T> CreateOrdered<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<OrderedCollectionStatuses<T>>() != null);

            return new OrderedCollectionStatuses<T>(this, new OrderingComparer<T, TKey>(keySelector, comparer, descending));
        }

        public virtual CollectionStatuses<T> CreateReverse()
        {
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return new ReverseCollectionStatuses<T>(this);
        }

        public virtual CollectionStatuses<T> CreateSkip(int skipCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(skipCount >= 0);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return new SkipAndTakeCollectionStatuses<T>(this, skipCount, null);
        }

        public virtual CollectionStatuses<T> CreateTake(int takeCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(takeCount >= 0);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return new SkipAndTakeCollectionStatuses<T>(this, 0, takeCount);
        }
    }

    [ContractClassFor(typeof(CollectionStatuses<>))]
    abstract class CollectionStatusesContract<T> : CollectionStatuses<T>
    {
        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            Contract.Ensures(Contract.Result<IObservable<SlimNotifyCollectionChangedEvent<T>>>() != null);

            throw new NotImplementedException();
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            Contract.Ensures(Contract.Result<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>>() != null);

            throw new NotImplementedException();
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
