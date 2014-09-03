using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Diagnostics.Contracts;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Subjects;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.Connectable;
using Kirinji.LinqToObservableCollection.Impl.Subjects;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static IConnectableCollectionStatuses<T> Publish<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IConnectableCollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged
                    .Multicast(new SlimPublishSubject<T>())
                    .ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged
                    .Multicast(new SimplePublishSubject<T>())
                    .ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged
                    .Multicast(new SlimSimplePublishSubject<T>())
                    .ToStatuses();
            }

            return instance.InitialStateAndChanged
                    .Multicast(new PublishSubject<T>())
                    .ToStatuses();
        }

        public static IConnectableCollectionStatuses<T> Replay<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IConnectableCollectionStatuses<T>>() != null);

            var instance = source.ToInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.SlimInitialStateAndChanged.Replay().ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.SimpleInitialStateAndChanged.Replay().ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.SlimSimpleInitialStateAndChanged.Replay().ToStatuses();
            }

            return instance.InitialStateAndChanged.Replay().ToStatuses();
        }

        public static ICollectionStatuses<T> RefCount<T>(this IConnectableCollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);


            var instance = source.ToConnectableInstance();

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimOne)
            {
                return instance.ConnectableSlimInitialStateAndChanged.RefCount().ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SimpleOne)
            {
                return instance.ConnectableSimpleInitialStateAndChanged.RefCount().ToStatuses();
            }

            if (instance.RecommendedEvent.RecommendedEventType == RecommendedEventType.SlimSimpleOne)
            {
                return instance.ConnectableSlimSimpleInitialStateAndChanged.RefCount().ToStatuses();
            }

            return instance.ConnectableInitialStateAndChanged.RefCount().ToStatuses();
        }
    }
}
