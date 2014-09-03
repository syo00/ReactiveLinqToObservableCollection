using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
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
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using System.Reactive;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        internal static ICollectionStatuses<T> Do<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.Do(action, _ => { throw new NotSupportedException(); });
        }

        internal static ICollectionStatuses<T> Do<T>(this ICollectionStatuses<T> source, Action<SimpleNotifyCollectionChangedEvent<T>> simpleAction)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(simpleAction != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.Do(_ => { throw new NotSupportedException(); }, simpleAction);
        }

        // イベントの再構築が行われる可能性があるので注意
        internal static ICollectionStatuses<T> Do<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> action, Action<SimpleNotifyCollectionChangedEvent<T>> simpleAction)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentNullException>(simpleAction != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if(instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.None
                || instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.DefaultOne
                || instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance
                    .InitialStateAndChanged
                    .Do(action)
                    .ToStatuses();
            }
            else
            {
                return instance
                    .SimpleInitialStateAndChanged
                    .Do(simpleAction)
                    .ToStatuses();
            }
        }

        internal static IObservable<ValueOrEmpty<T>> TakeOneItem<T>(this ICollectionStatuses<T> source, Func<IReadOnlyList<ValueOrEmpty<T>>, ValueOrEmpty<T>> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<IObservable<ValueOrEmpty<T>>>() != null);
            
            return source
                .Select(x => new ValueOrEmpty<T>(x))
                .SelectToCurrentCollections()
                .Select(attached => attached.Value)
                .Select(converter)
                .Select(x => x.HasValue ? new ValueOrEmpty<T>(x.Value) : new ValueOrEmpty<T>());
        }
    }
}
