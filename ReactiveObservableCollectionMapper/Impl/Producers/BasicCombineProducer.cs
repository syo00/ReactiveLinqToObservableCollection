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
    [ContractClass(typeof(BasicCombineProducerContract<,,>))]
    abstract class BasicCombineProducer<TSource1, TSource2, T> : Producer<T>
    {
        readonly CollectionStatuses<TSource1> leftSource;
        readonly CollectionStatuses<TSource2> rightSource;
        readonly SimplePublishSubject<TSource1> leftSourceSubject = new SimplePublishSubject<TSource1>();
        readonly SimplePublishSubject<TSource2> rightSourceSubject = new SimplePublishSubject<TSource2>();
        readonly List<Tagged<T>> convertedCurrentItems = new List<Tagged<T>>();
        IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading;
        bool isLeftInitialStateArrived;
        bool isRightInitialStateArrived;

        protected BasicCombineProducer(CollectionStatuses<TSource1> leftSource, CollectionStatuses<TSource2> rightSource, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(leftSource != null);
            Contract.Requires<ArgumentNullException>(rightSource != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(schedulingAndThreading, x => x != null));

            this.leftSource = leftSource;
            this.rightSource = rightSource;
            this.schedulingAndThreading = schedulingAndThreading;
        }

        protected abstract IEnumerable<T> ConvertLeftEvent(SimpleNotifyCollectionChangedEvent<TSource1> e);

        protected abstract IEnumerable<T> ConvertRightEvent(SimpleNotifyCollectionChangedEvent<TSource2> e);

        protected IReadOnlyList<Tagged<TSource1>> LeftCollection
        {
            get
            {
                return leftSourceSubject.CurrentItems;
            }
        }

        protected ICollectionStatuses<TSource1> LeftStatuses
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

        protected ICollectionStatuses<TSource2> RightStatuses
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
                .SimpleInitialStateAndChanged
                .Select(x => new Choice<SimpleNotifyCollectionChangedEvent<TSource1>, SimpleNotifyCollectionChangedEvent<TSource2>>(x))
                .Merge(rightSource
                    .SimpleInitialStateAndChanged
                    .Select(x => new Choice<SimpleNotifyCollectionChangedEvent<TSource1>, SimpleNotifyCollectionChangedEvent<TSource2>>(x)));

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

                            if (leftEvent.Action == SimpleNotifyCollectionChangedEventAction.InitialState)
                            {
                                isLeftInitialStateArrived = true;
                            }

                            leftSourceSubject.OnNext(leftEvent);
                            newNextEvents.ForEach(observer.OnNext);
                        }, rightEvent =>
                        {
                            var newNextEvents = ConvertRightEvent(rightEvent).ToArray();

                            if (rightEvent.Action == SimpleNotifyCollectionChangedEventAction.InitialState)
                            {
                                isRightInitialStateArrived = true;
                            }

                            rightSourceSubject.OnNext(rightEvent);
                            newNextEvents.ForEach(observer.OnNext);
                        });
                }, observer.OnError, observer.OnCompleted);
        }
    }

    [ContractClassFor(typeof(BasicCombineProducer<,,>))]
    abstract class BasicCombineProducerContract<TSource1, TSource2, T> : BasicCombineProducer<TSource1, TSource2, T>
    {
        private BasicCombineProducerContract()
            : base(null, null, null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<T> ConvertLeftEvent(SimpleNotifyCollectionChangedEvent<TSource1> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }

        protected override IEnumerable<T> ConvertRightEvent(SimpleNotifyCollectionChangedEvent<TSource2> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }
    }
}
