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
        internal static CollectionStatuses<T> ConvertToInitialStateAndChanged<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .InitialStateAndChanged
                .ToStatuses();
        }

        internal static CollectionStatuses<T> ConvertToSimpleInitialStateAndChanged<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .SimpleInitialStateAndChanged
                .ToStatuses();
        }

        internal static CollectionStatuses<T> ConvertToSlimInitialStateAndChanged<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .SlimInitialStateAndChanged
                .ToStatuses();
        }

        internal static CollectionStatuses<T> ConvertToSlimSimpleInitialStateAndChanged<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .SlimSimpleInitialStateAndChanged
                .ToStatuses();
        }

        internal static CollectionStatuses<T> Do<T>(this ICollectionStatuses<T> source, Action<NotifyCollectionChangedEventObject<T>> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var instance = source.ToInstance();
            return instance
                .EventsAsEventObject
                .Do(action)
                .ToStatuses(instance.RecommendedEvent);
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
