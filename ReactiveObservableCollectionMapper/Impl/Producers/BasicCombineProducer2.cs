using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.Impl.Subjects;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    [ContractClass(typeof(BasicCombineProducerContract2<,,>))]
    abstract class BasicCombineProducer2<TSource1, TSource2, T> : Producer<T>
    {
        readonly CollectionStatuses<TSource1> leftSource;
        readonly CollectionStatuses<TSource2> rightSource;
        List<TSource1> leftCollection;
        CollectionStatuses<TSource1> leftStatuses;
        List<TSource2> rightCollection;
        CollectionStatuses<TSource2> rightStatuses;
        readonly List<Tagged<T>> convertedCurrentItems = new List<Tagged<T>>();
        IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading;
        bool isLeftInitialStateArrived;
        bool isRightInitialStateArrived;

        protected BasicCombineProducer2(CollectionStatuses<TSource1> leftSource, CollectionStatuses<TSource2> rightSource, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(leftSource != null);
            Contract.Requires<ArgumentNullException>(rightSource != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(schedulingAndThreading, x => x != null));

            this.leftSource = leftSource;
            this.rightSource = rightSource;
            this.schedulingAndThreading = schedulingAndThreading;
        }

        protected abstract IEnumerable<T> ConvertLeftEvent(NotifyCollectionChangedEventObject<TSource1> e);

        protected abstract IEnumerable<T> ConvertRightEvent(NotifyCollectionChangedEventObject<TSource2> e);

        protected IReadOnlyList<TSource1> LeftCollection
        {
            get
            {
                if(leftCollection == null)
                {
                    return null;
                }

                return leftCollection.ToReadOnly();
            }
        }

        protected ICollectionStatuses<TSource1> LeftStatuses
        {
            get
            {
                return leftStatuses;
            }
        }

        protected IReadOnlyList<TSource2> RightCollection
        {
            get
            {
                if(rightCollection == null)
                {
                    return null;
                }

                return rightCollection.ToReadOnly();
            }
        }

        protected ICollectionStatuses<TSource2> RightStatuses
        {
            get
            {
                return rightStatuses;
            }
        }

        protected bool IsLeftInitialStateArrived
        {
            get
            {
                return isLeftInitialStateArrived;
            }

        }

        protected bool IsRightInitialStateArrived
        {
            get
            {
                return isRightInitialStateArrived;
            }

        }

        protected sealed override IDisposable SubscribeCore(ProducerObserver<T> observer)
        {
            var leftStatuses = new DelegationCollectionStatuses<TSource1, List<TSource1>, IList<Tagged<TSource1>>>(leftSource, () => new List<TSource1>());
            var rightStatuses = new DelegationCollectionStatuses<TSource2, List<TSource2>, IList<Tagged<TSource2>>>(rightSource, () => new List<TSource2>());
            this.leftStatuses = leftStatuses;
            this.rightStatuses = rightStatuses;
            leftCollection = leftStatuses.CurrentCollection;
            rightCollection = rightStatuses.CurrentCollection;

            var mergedStream =
                leftStatuses
                .EventsAsEventObject
                .Select(x => new Choice<NotifyCollectionChangedEventObject<TSource1>, NotifyCollectionChangedEventObject<TSource2>>(x))
                .Merge(rightStatuses
                    .EventsAsEventObject
                    .Select(x => new Choice<NotifyCollectionChangedEventObject<TSource1>, NotifyCollectionChangedEventObject<TSource2>>(x)));

            var subscribingStream = mergedStream;
            foreach (var item in schedulingAndThreading)
            {
                switch (item.Type)
                {
                    case SchedulingAndThreadingType.ObserveOn:
                        subscribingStream = subscribingStream.ObserveOn(item.ObserveOnScheduler);
                        break;
                    case SchedulingAndThreadingType.Synchronize:
                        subscribingStream = item.SynchronizationObject == null ? subscribingStream.Synchronize() : subscribingStream.Synchronize(item.SynchronizationObject);
                        break;
                }
            }

            return subscribingStream
                .CheckSynchronization()
                .Subscribe(x =>
                {
                    x.Action(leftEvent =>
                    {
                        var newNextEvents = ConvertLeftEvent(leftEvent).ToArray();

                        if (leftEvent.IsInitialState == true)
                        {
                            isLeftInitialStateArrived = true;
                        }
                        if (leftEvent.IsInitialState == null)
                        {
                            throw new InvalidOperationException();
                        }
                        
                        newNextEvents.ForEach(observer.OnNext);
                    }, rightEvent =>
                    {
                        var newNextEvents = ConvertRightEvent(rightEvent).ToArray();

                        if (rightEvent.IsInitialState == true)
                        {
                            isRightInitialStateArrived = true;
                        }
                        if (rightEvent.IsInitialState == null)
                        {
                            throw new InvalidOperationException();
                        }

                        newNextEvents.ForEach(observer.OnNext);
                    });
                }, observer.OnError, observer.OnCompleted);
        }
    }

    [ContractClassFor(typeof(BasicCombineProducer2<,,>))]
    abstract class BasicCombineProducerContract2<TSource1, TSource2, T> : BasicCombineProducer2<TSource1, TSource2, T>
    {
        private BasicCombineProducerContract2()
            : base(null, null, null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<T> ConvertLeftEvent(NotifyCollectionChangedEventObject<TSource1> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<T> ConvertRightEvent(NotifyCollectionChangedEventObject<TSource2> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }
    }
}
