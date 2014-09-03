using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.Connectable;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        internal static CollectionStatuses<T> ToInstance<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            var statuses = source as CollectionStatuses<T>;
            if (statuses != null)
            {
                return statuses;
            }
            else
            {
                return source.InitialStateAndChanged.ToStatuses();
            }
        }

        internal static OrderedCollectionStatuses<T> ToOrderedInstance<T>(this IOrderedCollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<OrderedCollectionStatuses<T>>() != null);

            var orderedStatuses = source as OrderedCollectionStatuses<T>;
            if (orderedStatuses != null)
            {
                return orderedStatuses;
            }
            else
            {
                return new OrderedCollectionStatuses<T>(source);
            }
        }

        internal static ConnectableCollectionStatuses<T> ToConnectableInstance<T>(this IConnectableCollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ConnectableCollectionStatuses<T>>() != null);

            var orderedStatuses = source as ConnectableCollectionStatuses<T>;
            if (orderedStatuses != null)
            {
                return orderedStatuses;
            }
            else
            {
                return new AnonymousConnectableCollectionStatuses<T>(source.InitialStateAndChanged.ToStatuses(), source.Connect);
            }
        }

        internal static CollectionStatuses<T> ToStatuses<T>(this IObservable<INotifyCollectionChangedEvent<T>> source, bool checkConsistency = false)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            if (checkConsistency)
            {
                return new AnonymousCollectionStatuses<T>(source.CheckAndClean(true).SelectMany(x => x));
            }
            else
            {
                return new AnonymousCollectionStatuses<T>(source.CleanLight().SelectMany(x => x));
            }
        }

        internal static CollectionStatuses<T> ToStatuses<T>(this IObservable<SlimNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return new AnonymousCollectionStatuses<T>(source);
        }

        internal static CollectionStatuses<T> ToStatuses<T>(this IObservable<SimpleNotifyCollectionChangedEvent<T>> source, bool checkConsistency = false)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            if (checkConsistency)
            {
                return new AnonymousCollectionStatuses<T>(source.CheckAndClean(true));
            }
            else
            {
                return new AnonymousCollectionStatuses<T>(source);
            }
        }

        internal static CollectionStatuses<T> ToStatuses<T>(this IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            return new AnonymousCollectionStatuses<T>(source);
        }

        internal static ConnectableCollectionStatuses<T> ToStatuses<T>(this IConnectableObservable<INotifyCollectionChangedEvent<T>> source, bool checkConsistency = false)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            IObservable<INotifyCollectionChangedEvent<T>> s = source;
            return new AnonymousConnectableCollectionStatuses<T>(s.ToStatuses(checkConsistency), source.Connect);
        }

        internal static ConnectableCollectionStatuses<T> ToStatuses<T>(this IConnectableObservable<SlimNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            IObservable<SlimNotifyCollectionChangedEvent<T>> s = source;
            return new AnonymousConnectableCollectionStatuses<T>(s.ToStatuses(), source.Connect);
        }

        internal static ConnectableCollectionStatuses<T> ToStatuses<T>(this IConnectableObservable<SimpleNotifyCollectionChangedEvent<T>> source, bool checkConsistency = false)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            IObservable<SimpleNotifyCollectionChangedEvent<T>> s = source;
            return new AnonymousConnectableCollectionStatuses<T>(s.ToStatuses(checkConsistency), source.Connect);
        }

        internal static ConnectableCollectionStatuses<T> ToStatuses<T>(this IConnectableObservable<SlimSimpleNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<CollectionStatuses<T>>() != null);

            IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> s = source;
            return new AnonymousConnectableCollectionStatuses<T>(s.ToStatuses(), source.Connect);
        }
    }
}
