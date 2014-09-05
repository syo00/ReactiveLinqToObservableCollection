using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static ICollectionStatuses<T> Cast<T>(this ICollectionStatuses<object> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return ProducerObservable.Create(() => new CastProducer<object, T>(source.ToInstance()))
                .ToStatuses();
        }

        public static ICollectionStatuses<T> Concat<T>(this ICollectionStatuses<T> source, ICollectionStatuses<T> second)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);
            
            return new[] { source, second }.Merge();
        }

        public static ICollectionStatuses<T> DefaultIfEmpty<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.DefaultIfEmpty(default(T));
        }

        public static ICollectionStatuses<T> DefaultIfEmpty<T>(this ICollectionStatuses<T> source, T defaultValue)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);
            
            return ProducerObservable.Create(() => new FillValuesIfEmptyProducer<T>(source.ToInstance(), new[] { defaultValue }.ToReadOnly()))
                .ToStatuses();
        }

        public static ICollectionStatuses<T> Distinct<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.Distinct(EqualityComparer<T>.Default);
        }

        public static ICollectionStatuses<T> Distinct<T>(this ICollectionStatuses<T> source, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.Distinct(EqualityComparer.Create(comparer));
        }

        public static ICollectionStatuses<T> Distinct<T>(this ICollectionStatuses<T> source, IEqualityComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return ProducerObservable.Create(() => new DistinctProducer<T>(source.ToInstance(), comparer ?? EqualityComparer<T>.Default))
                .ToStatuses();
        }

        // 重ねがけした場合の最適化は現在していない
        public static ICollectionStatuses<T> DoWhenAddedOrRemovedItem<T>(this ICollectionStatuses<T> source, Action<T> addedAction, Action<T> removedAction)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(addedAction != null);
            Contract.Requires<ArgumentNullException>(removedAction != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.DoWhenAddedOrRemovedItem(addedAction, removedAction, EqualityComparer<T>.Default.Equals);
        }

        // 重ねがけした場合の最適化は現在していない
        public static ICollectionStatuses<T> DoWhenAddedOrRemovedItem<T>(this ICollectionStatuses<T> source, Action<T> addedAction, Action<T> removedAction, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(addedAction != null);
            Contract.Requires<ArgumentNullException>(removedAction != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);
            
            var instance = source.ToInstance();
            return ProducerObservable.Create(() => new OnItemsAddedOrRemovedProducer<T>(instance, added => added.ForEach(addedAction), removed => removed.ForEach(removedAction), comparer))
                .ToStatuses();
        }

        public static ICollectionStatuses<T> Except<T>(this ICollectionStatuses<T> source, ICollectionStatuses<T> second)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.Except(second, EqualityComparer<T>.Default.Equals, new SchedulingAndThreading[0]);
        }

        public static ICollectionStatuses<T> Except<T, TSecond>(this ICollectionStatuses<T> source, ICollectionStatuses<TSecond> second, Func<T, TSecond, bool> comparer, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return ProducerObservable.Create(() => new ExceptProducer<T, TSecond>(source.ToInstance(), second.ToInstance(), schedulingAndThreading, comparer))
                .ToStatuses();
        }

        public static ICollectionStatuses<T> Flatten<T>(this ICollectionStatuses<ICollectionStatuses<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .SelectMany(x => x);
        }

        public static ICollectionStatuses<IGroupedCollectionStatuses<TKey, TElement>> GroupBy<T, TKey, TElement>(this ICollectionStatuses<T> source, Func<T, TKey> keySelector, Func<T, TElement> valueSelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Requires<ArgumentNullException>(valueSelector != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<IGroupedCollectionStatuses<TKey, TElement>>>() != null);

            return ProducerObservable.Create(() => new GroupByProducer<T, TKey, TElement>(source.ToInstance(), keySelector, valueSelector, comparer ?? EqualityComparer<TKey>.Default))
                .ToStatuses();
        }

        public static ICollectionStatuses<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this ICollectionStatuses<TOuter> outer, 
            ICollectionStatuses<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, 
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, ICollectionStatuses<TInner>, TResult> resultSelector, 
            IEqualityComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(outer != null);
            Contract.Requires<ArgumentNullException>(inner != null);
            Contract.Requires<ArgumentNullException>(outerKeySelector != null);
            Contract.Requires<ArgumentNullException>(innerKeySelector != null);
            Contract.Requires<ArgumentNullException>(resultSelector != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TResult>>() != null);

            return ProducerObservable.Create(() => new GroupJoinProducer<TOuter, TInner, TKey, TResult>(outer.ToInstance(), inner.ToInstance(), outerKeySelector, innerKeySelector, resultSelector, comparer ?? EqualityComparer<TKey>.Default))
                .ToStatuses();
        }

        public static ICollectionStatuses<T> Intersect<T>(this ICollectionStatuses<T> source, ICollectionStatuses<T> second)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.Intersect(second, EqualityComparer<T>.Default.Equals, new SchedulingAndThreading[0]);
        }

        public static ICollectionStatuses<T> Intersect<T, TSecond>(this ICollectionStatuses<T> source, ICollectionStatuses<TSecond> second, Func<T, TSecond, bool> comparer, IReadOnlyCollection<SchedulingAndThreading> schedulingAndThreading)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Requires<ArgumentNullException>(schedulingAndThreading != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return ProducerObservable.Create(() => new IntersectProducer<T, TSecond>(source.ToInstance(), second.ToInstance(), schedulingAndThreading, comparer))
                .ToStatuses();
        }

        public static ICollectionStatuses<TResult> Join<TOuter, TInner, TKey, TResult>(
            this ICollectionStatuses<TOuter> outer,
            ICollectionStatuses<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(outer != null);
            Contract.Requires<ArgumentNullException>(inner != null);
            Contract.Requires<ArgumentNullException>(outerKeySelector != null);
            Contract.Requires<ArgumentNullException>(innerKeySelector != null);
            Contract.Requires<ArgumentNullException>(resultSelector != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TResult>>() != null);

            return ProducerObservable.Create(() => new JoinProducer<TOuter, TInner, TKey, TResult>(outer.ToInstance(), inner.ToInstance(), outerKeySelector, innerKeySelector, resultSelector, comparer ?? EqualityComparer<TKey>.Default))
                .ToStatuses();

            //return outer
            //    .GroupJoin(inner, outerKeySelector, innerKeySelector, (o, i) => new { Outer = o, Inners = i }, comparer)
            //    .SelectMany(a => a.Inners.Select(i => resultSelector(a.Outer, i)));
        }

        public static ICollectionStatuses<T> ObserveOn<T>(this ICollectionStatuses<T> source, IScheduler scheduler)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(scheduler != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if(instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.ObserveOn(scheduler).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.ObserveOn(scheduler).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.ObserveOn(scheduler).ToStatuses();
            }

            return instance.InitialStateAndChanged.ObserveOn(scheduler).ToStatuses();
        }

        public static ICollectionStatuses<T> ObserveOn<T>(this ICollectionStatuses<T> source, SynchronizationContext synchronizationContext)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(synchronizationContext != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.Synchronize(synchronizationContext).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.Synchronize(synchronizationContext).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.Synchronize(synchronizationContext).ToStatuses();
            }

            return instance.InitialStateAndChanged.Synchronize(synchronizationContext).ToStatuses();
        }

        public static ICollectionStatuses<T> OfType<T>(this ICollectionStatuses<object> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .Select(x =>
                {
                    if (x is T)
                    {
                        return new ValueOrEmpty<T>((T)x);
                    }
                    return new ValueOrEmpty<T>();
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }

        public static ICollectionStatuses<T> Reverse<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .CreateReverse();
        }

        public static ICollectionStatuses<TTo> Select<T, TTo>(this ICollectionStatuses<T> source, Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);

            return source
                .ToInstance()
                .CreateSelect(converter);
        }

        public static ICollectionStatuses<TTo> Select<T, TTo>(this ICollectionStatuses<T> source, Func<T, IObservable<TTo>> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);

            return source
                .SelectAndWhere(x =>
                    converter(x)
                    .Select(y => new ValueOrEmpty<TTo>(y)));
        }

        public static ICollectionStatuses<TTo> Select<T, TTo>(this ICollectionStatuses<T> source, Func<T, TTo> initialConverter, Func<T, IObservable<TTo>> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(initialConverter != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);

            return source
                .SelectAndWhere(x =>
                    converter(x)
                    .StartWith(initialConverter(x))
                    .Select(y => new ValueOrEmpty<TTo>(y)));
        }

        private static ICollectionStatuses<TTo> SelectAndWhere<T, TTo>(this ICollectionStatuses<T> source, Func<T, IObservable<ValueOrEmpty<TTo>>> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);

            return source
                .Select(item =>
                    converter(item)
                    .ToSingleItemStatuses()
                    )
                .Flatten();
        }

        public static ICollectionStatuses<TTo> SelectMany<T, TTo>(this ICollectionStatuses<T> source, Func<T, IEnumerable<TTo>> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);
            
            return source
                .ToInstance()
                .CreateSelectMany(converter);
        }

        public static ICollectionStatuses<TTo> SelectMany<T, TTo>(this ICollectionStatuses<T> source, Func<T, ICollectionStatuses<TTo>> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);

            return ProducerObservable.Create(() => new FlattenProducer<TTo>(source.Select(x => converter(x).ToInstance()).ToInstance())).ToStatuses();
        }

        public static ICollectionStatuses<T> Skip<T>(this ICollectionStatuses<T> source, int skipCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(skipCount >= 0);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .CreateSkip(skipCount);
        }

        public static ICollectionStatuses<T> StartImmediately<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged
                    .Select(e =>
                    {
                        if (e.Action == NotifyCollectionChangedEventAction.InitialState)
                        {
                            if (e.InitialState.Items.Count == 0)
                            {
                                return null;
                            }

                            return new SlimNotifyCollectionChangedEvent<T>(new SlimAdded<T>(e.InitialState.Items, 0));
                        }
                        return e;
                    })
                    .StartWith(new SlimNotifyCollectionChangedEvent<T>(new SlimInitialState<T>(new T[0].ToReadOnly())))
                    .ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged
                    .Select(e =>
                    {
                        if (e.Action == SimpleNotifyCollectionChangedEventAction.InitialState)
                        {
                            if (e.InitialStateOrReset.Count == 0)
                            {
                                return null;
                            }

                            var newItems = e.InitialStateOrReset
                                .Select((tagged, i) => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, tagged, i))
                                .ToArray()
                                .ToReadOnly();
                            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(newItems);
                        }
                        return e;
                    })
                    .StartWith(SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(new Tagged<T>[0].ToReadOnly()))
                    .ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged
                    .Select(e =>
                    {
                        if (e.Action == SlimSimpleNotifyCollectionChangedEventAction.InitialState)
                        {
                            if (e.InitialStateOrReset.Count == 0)
                            {
                                return null;
                            }

                            var newItems = e.InitialStateOrReset
                                .Select((tagged, i) => new SlimAddedOrRemovedUnit<T>(tagged, i))
                                .ToArray()
                                .ToReadOnly();
                            return SlimSimpleNotifyCollectionChangedEvent<T>.CreateAddedOrRemoved(newItems);
                        }
                        return e;
                    })
                    .StartWith(SlimSimpleNotifyCollectionChangedEvent<T>.CreateInitialState(new Tagged<T>[0].ToReadOnly()))
                    .ToStatuses();
            }

            return instance.InitialStateAndChanged
                .Select(e =>
                {
                    if (e.Action == NotifyCollectionChangedEventAction.InitialState)
                    {
                        if (e.InitialState.Items.Count == 0)
                        {
                            return null;
                        }

                        return NotifyCollectionChangedEvent.CreateAddedEvent(e.InitialState.Items, 0);
                    }
                    return e;
                })
                .Where(e => e != null)
                .StartWith(NotifyCollectionChangedEvent.CreateInitialStateEvent(new T[0].ToReadOnly()))
                .ToStatuses();
        }

        public static ICollectionStatuses<T> SubscribeOn<T>(this ICollectionStatuses<T> source, IScheduler scheduler)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(scheduler != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.SubscribeOn(scheduler).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.SubscribeOn(scheduler).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.SubscribeOn(scheduler).ToStatuses();
            }

            return instance.InitialStateAndChanged.SubscribeOn(scheduler).ToStatuses();
        }

        public static ICollectionStatuses<T> SubscribeOn<T>(this ICollectionStatuses<T> source, SynchronizationContext synchronizationContext)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(synchronizationContext != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.SubscribeOn(synchronizationContext).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.SubscribeOn(synchronizationContext).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.SubscribeOn(synchronizationContext).ToStatuses();
            }

            return instance.InitialStateAndChanged.SubscribeOn(synchronizationContext).ToStatuses();
        }

        public static ICollectionStatuses<T> Synchronize<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.Synchronize().ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.Synchronize().ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.Synchronize().ToStatuses();
            }

            return instance.InitialStateAndChanged.Synchronize().ToStatuses();
        }

        public static ICollectionStatuses<T> Synchronize<T>(this ICollectionStatuses<T> source, object gate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(gate != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.Synchronize(gate).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.Synchronize(gate).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.Synchronize(gate).ToStatuses();
            }

            return instance.InitialStateAndChanged.Synchronize(gate).ToStatuses();
        }

        public static ICollectionStatuses<T> Take<T>(this ICollectionStatuses<T> source, int takeCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(takeCount >= 0);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .CreateTake(takeCount);
        }

        public static ICollectionStatuses<T> Take<T>(this ICollectionStatuses<T> source, TimeSpan duration)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.Take(duration).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.Take(duration).ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.Take(duration).ToStatuses();
            }

            return instance.InitialStateAndChanged.Take(duration).ToStatuses();
        }

        public static ICollectionStatuses<T> Union<T>(this ICollectionStatuses<T> source, ICollectionStatuses<T> second, IEqualityComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .Concat(second)
                .Distinct(comparer);
        }

        public static ICollectionStatuses<T> Where<T>(this ICollectionStatuses<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .CreateWhere(predicate);
        }

        public static ICollectionStatuses<T> Where<T>(this ICollectionStatuses<T> source, Func<T, IObservable<bool>> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .Select(e => 
                    predicate(e)
                    .Select(p => p ? new ValueOrEmpty<T>(e) : new ValueOrEmpty<T>())
                    .DistinctUntilChanged(EqualityComparer.Create<ValueOrEmpty<T>>((x, y) => x.HasValue == y.HasValue))
                    .ToSingleItemStatuses()
                    )
                .Flatten();
        }

        public static ICollectionStatuses<T> Where<T>(this ICollectionStatuses<T> source, Func<T, bool> initialPredicate, Func<T, IObservable<bool>> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(initialPredicate != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .Where(x => predicate(x).StartWith(initialPredicate(x)));
        }
    }
}
