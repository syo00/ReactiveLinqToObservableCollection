using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static ICollectionStatuses<T> ToStatusesWithMove<T>(this IObservable<IEnumerable<T>> source, IEqualityComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
                {
                    bool isInitialValueArrived = false;
                    IReadOnlyList<T> lastValue = null;

                    return source
                        .Select(x => x ?? new T[0])
                        .Select(collection => collection.ToArray().ToReadOnly())
                        .Subscribe(collection =>
                        {
                            if (!isInitialValueArrived)
                            {
                                lastValue = collection;
                                observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(lastValue));
                                isInitialValueArrived = true;
                                return;
                            }

                            var nextEvents = EventsConverter.CreateEventsToGetEquality(lastValue, collection, comparer);
                            lastValue = collection;
                            nextEvents.ForEach(observer.OnNext);
                        }, observer.OnError, observer.OnCompleted);
                })
                .ToStatuses();
        }

        public static ICollectionStatuses<T> ToStatusesWithReset<T>(this IObservable<IEnumerable<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                bool isInitialValueArrived = false;
                IReadOnlyList<T> lastValue = null;

                return source
                    .Select(x => x ?? new T[0])
                    .Select(collection => collection.ToArray().ToReadOnly())
                    .Subscribe(collection =>
                    {
                        if (!isInitialValueArrived)
                        {
                            lastValue = collection;
                            observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(lastValue));
                            isInitialValueArrived = true;
                            return;
                        }

                        lastValue = collection;
                        observer.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(collection));
                    }, observer.OnError, observer.OnCompleted);
            })
                .ToStatuses();
        }

        public static ICollectionStatuses<T> ToStatuses<T>(this ObservableCollection<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return new CollectionBasedCollectionStatuses<T>(source);
        }

        public static ICollectionStatuses<T> ToStatuses<T>(this ReadOnlyObservableCollection<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return new CollectionBasedCollectionStatuses<T>(source);
        }

        /// <summary>コレクションの要素の個数が 0 または 1 の ICollectionStatuses を作成します。</summary>
        public static ICollectionStatuses<T> ToSingleItemStatuses<T>(this IObservable<ValueOrEmpty<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                bool isInitialStateReceived = false;
                bool isLastEventAdded = false;
                T lastValue = default(T);

                return source
                    .CheckSynchronization()
                    .Subscribe(value =>
                    {
                        if (value.HasValue)
                        {
                            if (!isInitialStateReceived)
                            {
                                observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { value.Value }.ToReadOnly()));
                                isInitialStateReceived = true;
                                lastValue = value.Value;
                                isLastEventAdded = true;
                                return;
                            }

                            if (isLastEventAdded)
                            {
                                observer.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { lastValue }.ToReadOnly(), new[] { value.Value }.ToReadOnly(), 0));
                                lastValue = value.Value;
                                return;
                            }

                            observer.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { value.Value }.ToReadOnly(), 0));
                            isLastEventAdded = true;
                            lastValue = value.Value;
                            return;
                        }

                        if (!isInitialStateReceived)
                        {
                            observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new T[0].ToReadOnly()));
                            isInitialStateReceived = true;
                            return;
                        }

                        if (isLastEventAdded)
                        {
                            observer.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { lastValue }.ToReadOnly(), 0));
                            isLastEventAdded = false;
                            return;
                        }
                    }, observer.OnError, observer.OnCompleted);
            })
                .ToStatuses();
        }
    }
}
