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
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    [ContractClass(typeof(SlimBasicCombineProducerContract<,,>))]
    abstract class SlimBasicCombineProducer<TSource1, TSource2, T> : Producer<T>
    {
        readonly CollectionStatuses<TSource1> leftSource;
        readonly CollectionStatuses<TSource2> rightSource;
        readonly SlimSimplePublishSubject<TSource1> leftSourceSubject = new SlimSimplePublishSubject<TSource1>();
        readonly SlimSimplePublishSubject<TSource2> rightSourceSubject = new SlimSimplePublishSubject<TSource2>();
        readonly List<Tagged<T>> convertedCurrentItems = new List<Tagged<T>>();
        IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading;
        bool isLeftInitialStateArrived;
        bool isRightInitialStateArrived;

        protected SlimBasicCombineProducer(CollectionStatuses<TSource1> leftSource, CollectionStatuses<TSource2> rightSource, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(leftSource != null);
            Contract.Requires<ArgumentNullException>(rightSource != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(schedulingAndThreading, x => x != null));

            this.leftSource = leftSource;
            this.rightSource = rightSource;
            this.schedulingAndThreading = schedulingAndThreading;
        }

        protected abstract IEnumerable<T> ConvertLeftEvent(SlimSimpleNotifyCollectionChangedEvent<TSource1> e);

        protected abstract IEnumerable<T> ConvertRightEvent(SlimSimpleNotifyCollectionChangedEvent<TSource2> e);

        protected IReadOnlyList<Tagged<TSource1>> LeftCollection
        {
            get
            {
                return leftSourceSubject.CurrentItems;
            }
        }

        protected ICollectionStatuses<TSource1> LeftSource
        {
            get
            {
                return leftSourceSubject.ToStatuses();
            }
        }

        protected IReadOnlyList<Tagged<TSource2>> RightCollection
        {
            get
            {
                return rightSourceSubject.CurrentItems;
            }
        }

        protected ICollectionStatuses<TSource2> RightSource
        {
            get
            {
                return rightSourceSubject.ToStatuses();
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
            var mergedStream =
                leftSource
                .SlimSimpleInitialStateAndChanged
                .Select(x => new Choice<SlimSimpleNotifyCollectionChangedEvent<TSource1>, SlimSimpleNotifyCollectionChangedEvent<TSource2>>(x))
                .Merge(rightSource
                    .SlimSimpleInitialStateAndChanged
                    .Select(x => new Choice<SlimSimpleNotifyCollectionChangedEvent<TSource1>, SlimSimpleNotifyCollectionChangedEvent<TSource2>>(x)));

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

                            if (leftEvent.Action == SlimSimpleNotifyCollectionChangedEventAction.InitialState)
                            {
                                isLeftInitialStateArrived = true;
                            }

                            leftSourceSubject.OnNext(leftEvent);
                            newNextEvents.ForEach(observer.OnNext);
                        }, rightEvent =>
                        {
                            var newNextEvents = ConvertRightEvent(rightEvent).ToArray();

                            if (rightEvent.Action == SlimSimpleNotifyCollectionChangedEventAction.InitialState)
                            {
                                isRightInitialStateArrived = true;
                            }

                            rightSourceSubject.OnNext(rightEvent);
                            newNextEvents.ForEach(observer.OnNext);
                        });
                }, observer.OnError, observer.OnCompleted);
        }
    }

    [ContractClassFor(typeof(SlimBasicCombineProducer<,,>))]
    abstract class SlimBasicCombineProducerContract<TSource1, TSource2, T> : SlimBasicCombineProducer<TSource1, TSource2, T>
    {
        private SlimBasicCombineProducerContract()
            : base(null, null, null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<T> ConvertLeftEvent(SlimSimpleNotifyCollectionChangedEvent<TSource1> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<T> ConvertRightEvent(SlimSimpleNotifyCollectionChangedEvent<TSource2> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }
    }
}
