using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System.Reactive.Disposables;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection.Impl
{
    class AnonymousEventObjectCollectionStatuses<T> : CollectionStatuses<T>
    {
        readonly Func<IObservable<NotifyCollectionChangedEventObject<T>>> eventsAsEventObject;
        readonly Lazy<RecommendedEvent> recommendedEvent;

        public AnonymousEventObjectCollectionStatuses(IObservable<NotifyCollectionChangedEventObject<T>> eventsAsEventObject, RecommendedEvent recommendedEvent)
        {
            Contract.Requires<ArgumentNullException>(eventsAsEventObject != null);

            this.eventsAsEventObject = () => eventsAsEventObject;
            this.recommendedEvent = new Lazy<RecommendedEvent>(() => recommendedEvent);
        }

        public AnonymousEventObjectCollectionStatuses(Func<IObservable<NotifyCollectionChangedEventObject<T>>> eventsAsEventObject, Func<RecommendedEvent> recommendedEvent)
        {
            Contract.Requires<ArgumentNullException>(eventsAsEventObject != null);

            this.eventsAsEventObject = eventsAsEventObject;
            this.recommendedEvent = new Lazy<RecommendedEvent>(recommendedEvent);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(eventsAsEventObject != null);
        }

        protected override IObservable<NotifyCollectionChangedEventObject<T>> CreateEventsAsEventObject()
        {
            var result = eventsAsEventObject();
            if (result == null)
            {
                throw new InvalidOperationException("eventsAsEventObject() == null");
            }

            return result;
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
                {
                    return Start(observer,
                    s => s,
                    s => s.Extract(),
                    s => s.ExtractFromSlim(),
                    s => s.ExtractFromSlim().Extract());
                });
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            return Observable.Create<SimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                return Start(observer,
                    s => s.Simplify(),
                    s => s,
                    s => s.ExtractFromSlim().Simplify(),
                    s => s.ExtractFromSlim());
            });
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            return Observable.Create<SlimNotifyCollectionChangedEvent<T>>(observer =>
            {
                return Start(observer,
                    s => s.ToSlim(),
                    s => s.Extract().ToSlim(),
                    s => s,
                    s => s.ExtractFromSlim().Extract().ToSlim());
            });
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            return Observable.Create<SlimSimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                return Start(observer,
                    s => s.Simplify().ToSlim(),
                    s => s.ToSlim(),
                    s => s.ExtractFromSlim().Simplify().ToSlim(),
                    s => s);
            });
        }

        IDisposable Start<TResult>(
            IObserver<TResult> observer,
            Func<IObservable<INotifyCollectionChangedEvent<T>>, IObservable<TResult>> converter1,
            Func<IObservable<SimpleNotifyCollectionChangedEvent<T>>, IObservable<TResult>> converter2,
            Func<IObservable<SlimNotifyCollectionChangedEvent<T>>, IObservable<TResult>> converter3,
            Func<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>, IObservable<TResult>> converter4)
        {
            Contract.Requires<ArgumentNullException>(observer != null);
            Contract.Requires<ArgumentNullException>(converter1 != null);
            Contract.Requires<ArgumentNullException>(converter2 != null);
            Contract.Requires<ArgumentNullException>(converter3 != null);
            Contract.Requires<ArgumentNullException>(converter4 != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            var subject = new ReplaySubject<INotifyCollectionChangedEvent<T>>();
            var simpleSubject = new ReplaySubject<SimpleNotifyCollectionChangedEvent<T>>();
            var slimSubject = new ReplaySubject<SlimNotifyCollectionChangedEvent<T>>();
            var slimSimpleSubject = new ReplaySubject<SlimSimpleNotifyCollectionChangedEvent<T>>();

            NotifyCollectionChangedEventType? type = null;
            var subscription1 = EventsAsEventObject
                .Subscribe(e =>
                {
                    if (type != null && type != e.EventType)
                    {
                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidEventType));
                    }

                    type = e.EventType;

                    switch (e.EventType)
                    {
                        case NotifyCollectionChangedEventType.DefaultOne:
                            subject.OnNext(e.DefaultOne);
                            return;
                        case NotifyCollectionChangedEventType.SimpleOne:
                            simpleSubject.OnNext(e.SimpleOne);
                            return;
                        case NotifyCollectionChangedEventType.SlimOne:
                            slimSubject.OnNext(e.SlimOne);
                            return;
                        case NotifyCollectionChangedEventType.SlimSimpleOne:
                            slimSimpleSubject.OnNext(e.SlimSimpleOne);
                            return;
                    }
                }, observer.OnError, observer.OnCompleted);

            var subscription2 = Observable.Merge(converter1(subject), converter2(simpleSubject), converter3(slimSubject), converter4(slimSimpleSubject))
                .Subscribe(observer);

            return new CompositeDisposable { subscription2, subscription1, subject, simpleSubject, slimSubject, slimSimpleSubject };
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                return recommendedEvent.Value;
            }
        }
    }
}
