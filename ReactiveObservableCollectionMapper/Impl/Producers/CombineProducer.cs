using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    [ContractClass(typeof(CombineProducerContract<,,>))]
    abstract class CombineProducer<TSource1, TSource2, T> : BasicCombineProducer<TSource1, TSource2, T>
    {
        protected CombineProducer(CollectionStatuses<TSource1> leftSource, CollectionStatuses<TSource2> rightSource, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
            : base(leftSource, rightSource, schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(leftSource != null);
            Contract.Requires<ArgumentNullException>(rightSource != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(schedulingAndThreading, x => x != null));
        }

        protected abstract IReadOnlyList<Tagged<T>> ConvertInitialState(IReadOnlyList<Tagged<TSource1>> initialLeftCollection, IReadOnlyList<Tagged<TSource2>> initialRightCollection);

        protected abstract IReadOnlyList<AddedOrRemovedUnit<T>> ConvertLeftUnits(IReadOnlyList<AddedOrRemovedUnit<TSource1>> leftEvent);

        protected abstract IReadOnlyList<AddedOrRemovedUnit<T>> ConvertRightUnits(IReadOnlyList<AddedOrRemovedUnit<TSource2>> rightEvent);

        protected abstract SimpleNotifyCollectionChangedEvent<T> ConvertLeftReset(IReadOnlyList<Tagged<TSource1>> newLeftItems);

        protected abstract SimpleNotifyCollectionChangedEvent<T> ConvertRightReset(IReadOnlyList<Tagged<TSource2>> newRightItems);


        protected sealed override IEnumerable<SimpleNotifyCollectionChangedEvent<T>> ConvertLeftEvent(SimpleNotifyCollectionChangedEvent<TSource1> e)
        {
            if (!IsRightInitialStateArrived)
            {
                yield break;
            }

            switch (e.Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    var intialState = ConvertInitialState(e.InitialStateOrReset, RightCollection);
                    yield return SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(intialState);
                    yield break;
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    var leftUnits = ConvertLeftUnits(e.AddedOrRemoved);
                    yield return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(leftUnits);
                    yield break;
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    yield return ConvertLeftReset(e.InitialStateOrReset);
                    yield break;
            }
        }

        protected sealed override IEnumerable<SimpleNotifyCollectionChangedEvent<T>> ConvertRightEvent(SimpleNotifyCollectionChangedEvent<TSource2> e)
        {
            if (!IsLeftInitialStateArrived)
            {
                yield break;
            }

            switch (e.Action)
            {
                case SimpleNotifyCollectionChangedEventAction.InitialState:
                    var intialState = ConvertInitialState(LeftCollection, e.InitialStateOrReset);
                    yield return SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(intialState);
                    yield break;
                case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    var leftUnits = ConvertRightUnits(e.AddedOrRemoved);
                    yield return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(leftUnits);
                    yield break;
                case SimpleNotifyCollectionChangedEventAction.Reset:
                    yield return ConvertRightReset(e.InitialStateOrReset);
                    yield break;
            }
        }
    }

    [ContractClassFor(typeof(CombineProducer<,,>))]
    abstract class CombineProducerContract<TSource1, TSource2, T> : CombineProducer<TSource1, TSource2, T>
    {
        private CombineProducerContract()
            : base(null, null, null)
        {
            throw new NotImplementedException();
        }

        protected override IReadOnlyList<Tagged<T>> ConvertInitialState(IReadOnlyList<Tagged<TSource1>> initialLeftCollection, IReadOnlyList<Tagged<TSource2>> initialRightCollection)
        {
            Contract.Requires<ArgumentNullException>(initialLeftCollection != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(initialLeftCollection, x => x != null));
            Contract.Requires<ArgumentNullException>(initialRightCollection != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(initialRightCollection, x => x != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<Tagged<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<Tagged<T>>>(), x => x != null));

            throw new NotImplementedException();
        }

        protected override IReadOnlyList<AddedOrRemovedUnit<T>> ConvertLeftUnits(IReadOnlyList<AddedOrRemovedUnit<TSource1>> leftEvent)
        {
            Contract.Requires<ArgumentNullException>(leftEvent != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(leftEvent, x => x != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>(), x => x != null));

            throw new NotImplementedException();
        }

        protected override IReadOnlyList<AddedOrRemovedUnit<T>> ConvertRightUnits(IReadOnlyList<AddedOrRemovedUnit<TSource2>> rightEvent)
        {
            Contract.Requires<ArgumentNullException>(rightEvent != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(rightEvent, x => x != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>(), x => x != null));

            throw new NotImplementedException();
        }

        protected override SimpleNotifyCollectionChangedEvent<T> ConvertLeftReset(IReadOnlyList<Tagged<TSource1>> newLeftItems)
        {
            Contract.Requires<ArgumentNullException>(newLeftItems != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            throw new NotImplementedException();
        }

        protected override SimpleNotifyCollectionChangedEvent<T> ConvertRightReset(IReadOnlyList<Tagged<TSource2>> newRightItems)
        {
            Contract.Requires<ArgumentNullException>(newRightItems != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            throw new NotImplementedException();
        }
    }
}
