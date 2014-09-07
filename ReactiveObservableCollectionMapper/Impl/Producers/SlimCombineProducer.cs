using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    [ContractClass(typeof(SlimCombineProducerContract<,,>))]
    abstract class SlimCombineProducer<TSource1, TSource2, T> : SlimBasicCombineProducer<TSource1, TSource2, SlimSimpleNotifyCollectionChangedEvent<T>>
    {
        protected SlimCombineProducer(CollectionStatuses<TSource1> leftSource, CollectionStatuses<TSource2> rightSource, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
            : base(leftSource, rightSource, schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(leftSource != null);
            Contract.Requires<ArgumentNullException>(rightSource != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(schedulingAndThreading, x => x != null));
        }

        protected abstract IReadOnlyList<Tagged<T>> ConvertInitialState(IReadOnlyList<Tagged<TSource1>> initialLeftCollection, IReadOnlyList<Tagged<TSource2>> initialRightCollection);

        protected abstract IReadOnlyList<SlimAddedOrRemovedUnit<T>> ConvertLeftUnits(IReadOnlyList<SlimAddedOrRemovedUnit<TSource1>> leftEvent);

        protected abstract IReadOnlyList<SlimAddedOrRemovedUnit<T>> ConvertRightUnits(IReadOnlyList<SlimAddedOrRemovedUnit<TSource2>> rightEvent);

        protected abstract SlimSimpleNotifyCollectionChangedEvent<T> ConvertLeftReset(IReadOnlyList<Tagged<TSource1>> newLeftItems);

        protected abstract SlimSimpleNotifyCollectionChangedEvent<T> ConvertRightReset(IReadOnlyList<Tagged<TSource2>> newRightItems);


        protected sealed override IEnumerable<SlimSimpleNotifyCollectionChangedEvent<T>> ConvertLeftEvent(SlimSimpleNotifyCollectionChangedEvent<TSource1> e)
        {
            if (!IsRightInitialStateArrived)
            {
                yield break;
            }

            switch (e.Action)
            {
                case SlimSimpleNotifyCollectionChangedEventAction.InitialState:
                    var intialState = ConvertInitialState(e.InitialStateOrReset, RightCollection);
                    yield return SlimSimpleNotifyCollectionChangedEvent<T>.CreateInitialState(intialState);
                    yield break;
                case SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    var leftUnits = ConvertLeftUnits(e.AddedOrRemoved);
                    yield return SlimSimpleNotifyCollectionChangedEvent<T>.CreateAddedOrRemoved(leftUnits);
                    yield break;
                case SlimSimpleNotifyCollectionChangedEventAction.Reset:
                    yield return ConvertLeftReset(e.InitialStateOrReset);
                    yield break;
            }
        }

        protected sealed override IEnumerable<SlimSimpleNotifyCollectionChangedEvent<T>> ConvertRightEvent(SlimSimpleNotifyCollectionChangedEvent<TSource2> e)
        {
            if (!IsLeftInitialStateArrived)
            {
                yield break;
            }

            switch (e.Action)
            {
                case SlimSimpleNotifyCollectionChangedEventAction.InitialState:
                    var intialState = ConvertInitialState(LeftCollection, e.InitialStateOrReset);
                    yield return SlimSimpleNotifyCollectionChangedEvent<T>.CreateInitialState(intialState);
                    yield break;
                case SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove:
                    var leftUnits = ConvertRightUnits(e.AddedOrRemoved);
                    yield return SlimSimpleNotifyCollectionChangedEvent<T>.CreateAddedOrRemoved(leftUnits);
                    yield break;
                case SlimSimpleNotifyCollectionChangedEventAction.Reset:
                    yield return ConvertRightReset(e.InitialStateOrReset);
                    yield break;
            }
        }
    }

    [ContractClassFor(typeof(SlimCombineProducer<,,>))]
    abstract class SlimCombineProducerContract<TSource1, TSource2, T> : SlimCombineProducer<TSource1, TSource2, T>
    {
        private SlimCombineProducerContract()
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

        protected override SlimSimpleNotifyCollectionChangedEvent<T> ConvertLeftReset(IReadOnlyList<Tagged<TSource1>> newLeftItems)
        {
            Contract.Requires<ArgumentNullException>(newLeftItems != null);
            Contract.Ensures(Contract.Result<SlimSimpleNotifyCollectionChangedEvent<T>>() != null);

            throw new NotImplementedException();
        }

        protected override SlimSimpleNotifyCollectionChangedEvent<T> ConvertRightReset(IReadOnlyList<Tagged<TSource2>> newRightItems)
        {
            Contract.Requires<ArgumentNullException>(newRightItems != null);
            Contract.Ensures(Contract.Result<SlimSimpleNotifyCollectionChangedEvent<T>>() != null);

            throw new NotImplementedException();
        }

        protected override IReadOnlyList<SlimAddedOrRemovedUnit<T>> ConvertLeftUnits(IReadOnlyList<SlimAddedOrRemovedUnit<TSource1>> leftEvent)
        {
            Contract.Requires<ArgumentNullException>(leftEvent != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(leftEvent, x => x != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<SlimAddedOrRemovedUnit<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<SlimAddedOrRemovedUnit<T>>>(), x => x != null));

            throw new NotImplementedException();
        }

        protected override IReadOnlyList<SlimAddedOrRemovedUnit<T>> ConvertRightUnits(IReadOnlyList<SlimAddedOrRemovedUnit<TSource2>> rightEvent)
        {
            Contract.Requires<ArgumentNullException>(rightEvent != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(rightEvent, x => x != null));
            Contract.Ensures(Contract.Result<IReadOnlyList<SlimAddedOrRemovedUnit<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<SlimAddedOrRemovedUnit<T>>>(), x => x != null));

            throw new NotImplementedException();
        }
    }
}
