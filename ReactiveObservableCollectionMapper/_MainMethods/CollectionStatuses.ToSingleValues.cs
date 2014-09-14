using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    // 原則として、イベントが来るたびに、前と同じ値であっても値を毎回流す
    public static partial class CollectionStatuses
    {
        public static IObservable<ValueOrEmpty<T>> Aggregate<T>(this ICollectionStatuses<T> source, Func<T, T, T> accumulator)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(accumulator != null);
            Contract.Ensures(Contract.Result<IObservable<ValueOrEmpty<T>>>() != null);

            return source
                .SelectToCurrentCollections()
                .Select(attached => attached.Value)
                .Select(collection => collection.Count == 0 ? new ValueOrEmpty<T>() : new ValueOrEmpty<T>(collection.Aggregate(accumulator)));
        }

        public static IObservable<TAccumulate> Aggregate<T, TAccumulate>(this ICollectionStatuses<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(accumulator != null);
            Contract.Ensures(Contract.Result<IObservable<TAccumulate>>() != null);

            return source.SelectToCurrentCollections()
                .Select(attached => attached.Value)
                .Select(collection => collection.Aggregate(seed, accumulator));
        }

        public static IObservable<TResult> Aggregate<T, TAccumulate, TResult>(this ICollectionStatuses<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(accumulator != null);
            Contract.Requires<ArgumentNullException>(resultSelector != null);
            Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

            return source
                .Aggregate(seed, accumulator)
                .Select(resultSelector);
        }

        public static IObservable<bool> All<T>(this ICollectionStatuses<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<IObservable<bool>>() != null);


            Func<T, bool> negatedPredicate = x => !predicate(x);
            return ProducerObservable.Create(() => new AnyProducer<T>(source.ToInstance(), negatedPredicate))
                .Select(x => !x);
        }

        public static IObservable<bool> Any<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<bool>>() != null);

            return source
                .Count()
                .Select(i => i != 0);
        }

        public static IObservable<bool> Any<T>(this ICollectionStatuses<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<IObservable<bool>>() != null);
            
            return ProducerObservable.Create(() => new AnyProducer<T>(source.ToInstance(), predicate));
        }

        public static IObservable<IReadOnlyList<T>> CombineLatest<T>(this ICollectionStatuses<IObservable<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<IReadOnlyList<T>>>() != null);

            return ProducerObservable.Create(() => new CombineLatestProducer<T>(source.ToInstance()));
        }

        public static IObservable<bool> Contains<T>(this ICollectionStatuses<T> source, T value)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<bool>>() != null);
            
            return source.Contains(value, null);
        }

        public static IObservable<bool> Contains<T>(this ICollectionStatuses<T> source, T value, IEqualityComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<bool>>() != null);
            
            var comparerNotNull = comparer ?? EqualityComparer<T>.Default;
            return ProducerObservable.Create(() => new AnyProducer<T>(source.ToInstance(), item => comparerNotNull.Equals(item, value)));
        }

        public static IObservable<int> Count<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<int>>() != null);

            return Observable.Create<int>(observer =>
            {
                int lastCount = 0;
                return source.InitialStateAndChanged.Subscribe(value =>
                {
                    switch (value.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            observer.OnNext(value.InitialState.Items.Count);
                            lastCount = value.InitialState.Items.Count;
                            break;
                        case NotifyCollectionChangedEventAction.Add:
                            observer.OnNext(lastCount + value.Added.Items.Count);
                            lastCount += value.Added.Items.Count;
                            break;
                        case NotifyCollectionChangedEventAction.Remove:
                            observer.OnNext(lastCount - value.Removed.Items.Count);
                            lastCount -= value.Removed.Items.Count;
                            break;
                        case NotifyCollectionChangedEventAction.Replace:
                            observer.OnNext(lastCount + value.Replaced.NewItems.Count - value.Replaced.OldItems.Count);
                            lastCount += (value.Replaced.NewItems.Count - value.Replaced.OldItems.Count);
                            break;
                        case NotifyCollectionChangedEventAction.Move:
                            observer.OnNext(lastCount);
                            break;
                        case NotifyCollectionChangedEventAction.Reset:
                            observer.OnNext(value.Reset.Items.Count);
                            lastCount = value.Reset.Items.Count;
                            break;
                    }
                }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<int> Count<T>(this ICollectionStatuses<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<IObservable<int>>() != null);

            return ProducerObservable.Create(() => new CountProducer<T>(source.ToInstance(), predicate));
        }

        public static IObservable<ValueOrEmpty<T>> ElementAt<T>(this ICollectionStatuses<T> source, int index)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<ValueOrEmpty<T>>>() != null);
            
            return source
                .TakeOneItem(collection => collection.ElementAtOrDefault(index));
        }

        public static IObservable<ValueOrEmpty<T>> First<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<ValueOrEmpty<T>>>() != null);

            return source
                .ElementAt(0);
        }

        public static IObservable<ValueOrEmpty<T>> Last<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<ValueOrEmpty<T>>>() != null);
            
            return source
                .TakeOneItem(collection => collection.LastOrDefault());
        }

        public static IObservable<T> Merge<T>(this ICollectionStatuses<IObservable<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);
            
            return Observable.Create<T>(observer =>
                {
                    var subscriptions = new Dictionary<object, IDisposable>();

                    return source
                        .Select(x => new KeyValuePair<object, IObservable<T>>(new object(), x))
                        .DoWhenAddedOrRemovedItem(addedItem =>
                        {
                            var subscription = addedItem.Value.Subscribe(observer.OnNext, observer.OnError);
                            subscriptions.Add(addedItem.Key, subscription);

                        }, removedItem =>
                        {

                            subscriptions[removedItem.Key].Dispose();
                            subscriptions.Remove(removedItem.Key);

                        })
                        .Subscribe();
                });
        }

        public static IObservable<bool> SequenceEqual<TSource, TSecond>(this ICollectionStatuses<TSource> source, ICollectionStatuses<TSecond> second, Func<TSource, TSecond, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<IObservable<bool>>() != null);

            return ProducerObservable.Create(() => new SequenceEqualProducer<TSource, TSecond>(source.ToInstance(), second.ToInstance(), comparer));
        }
    }
}
