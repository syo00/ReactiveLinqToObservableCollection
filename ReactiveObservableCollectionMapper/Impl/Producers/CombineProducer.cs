using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    [ContractClass(typeof(CombineProducerContract2<,,>))]
    abstract class CombineProducer2<TSource1, TSource2, T> : BasicCombineProducer<TSource1, TSource2, NotifyCollectionChangedEventObject<T>>
    {
        protected CombineProducer2(CollectionStatuses<TSource1> leftSource, CollectionStatuses<TSource2> rightSource, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
            : base(leftSource, rightSource, schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(leftSource != null);
            Contract.Requires<ArgumentNullException>(rightSource != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(schedulingAndThreading, x => x != null));
        }

        protected abstract IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertInitialState(IReadOnlyList<TSource1> initialLeftCollection, IReadOnlyList<TSource2> initialRightCollection);

        protected abstract IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftChanged(NotifyCollectionChangedEventObject<TSource1> leftEvent);

        protected abstract IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightChanged(NotifyCollectionChangedEventObject<TSource2> rightEvent);

        protected abstract IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftReset(IReadOnlyList<TSource1> leftReset);

        protected abstract IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightReset(IReadOnlyList<TSource2> rightReset);

        protected sealed override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftEvent(NotifyCollectionChangedEventObject<TSource1> e)
        {
            if (!IsRightInitialStateArrived)
            {
                yield break;
            }

            if (e.IsInitialState == true)
            {
                foreach (var elem in ConvertInitialState(e.InitialStateItems, RightCollection))
                {
                    yield return elem;
                }
                yield break;
            }

            if(e.IsReset == true)
            {
                foreach (var elem in ConvertLeftReset(e.ResetItems))
                {
                    yield return elem;
                }
                yield break;
            }

            foreach (var elem in ConvertLeftChanged(e))
            {
                yield return elem;
            }
        }

        protected sealed override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightEvent(NotifyCollectionChangedEventObject<TSource2> e)
        {
            if (!IsLeftInitialStateArrived)
            {
                yield break;
            }

            if (e.IsInitialState == true)
            {
                foreach (var elem in ConvertInitialState(LeftCollection, e.InitialStateItems))
                {
                    yield return elem;
                }
                yield break;
            }

            if (e.IsReset == true)
            {
                foreach (var elem in ConvertRightReset(e.ResetItems))
                {
                    yield return elem;
                }
                yield break;
            }

            foreach (var elem in ConvertRightChanged(e))
            {
                yield return elem;
            }
        }
    }

    [ContractClassFor(typeof(CombineProducer2<,,>))]
    abstract class CombineProducerContract2<TSource1, TSource2, T> : CombineProducer2<TSource1, TSource2, T>
    {
        private CombineProducerContract2()
            : base(null, null, null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertInitialState(IReadOnlyList<TSource1> initialLeftCollection, IReadOnlyList<TSource2> initialRightCollection)
        {
            Contract.Requires<ArgumentNullException>(initialLeftCollection != null);
            Contract.Requires<ArgumentNullException>(initialRightCollection != null);
            Contract.Ensures(Contract.Result<IEnumerable<NotifyCollectionChangedEventObject<T>>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftChanged(NotifyCollectionChangedEventObject<TSource1> leftEvent)
        {
            Contract.Requires<ArgumentNullException>(leftEvent != null);
            Contract.Ensures(Contract.Result<IEnumerable<NotifyCollectionChangedEventObject<T>>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightChanged(NotifyCollectionChangedEventObject<TSource2> rightEvent)
        {
            Contract.Requires<ArgumentNullException>(rightEvent != null);
            Contract.Ensures(Contract.Result<IEnumerable<NotifyCollectionChangedEventObject<T>>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertLeftReset(IReadOnlyList<TSource1> leftReset)
        {
            Contract.Requires<ArgumentNullException>(leftReset != null);
            Contract.Ensures(Contract.Result<IEnumerable<NotifyCollectionChangedEventObject<T>>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<NotifyCollectionChangedEventObject<T>> ConvertRightReset(IReadOnlyList<TSource2> rightReset)
        {
            Contract.Requires<ArgumentNullException>(rightReset != null);
            Contract.Ensures(Contract.Result<IEnumerable<NotifyCollectionChangedEventObject<T>>>() != null);

            throw new NotImplementedException();
        }
    }
}
