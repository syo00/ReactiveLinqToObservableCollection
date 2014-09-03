using Kirinji.LightWands;
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

namespace Kirinji.LinqToObservableCollection.Impl.Grouped
{
    internal abstract class GroupedCollectionStatuses<TKey, TElement> : CollectionStatuses<TElement>, IGroupedCollectionStatuses<TKey, TElement>
    {
        public IGroupedObservable<TKey, INotifyCollectionChangedEvent<TElement>> ConnectableInitialStateAndChanged
        {
            get
            {
                return new AnonymousGroupedObservable<TKey, INotifyCollectionChangedEvent<TElement>>(InitialStateAndChanged.Subscribe, Key);
            }
        }

        public IGroupedObservable<TKey, SlimNotifyCollectionChangedEvent<TElement>> ConnectableSlimInitialStateAndChanged
        {
            get
            {
                return new AnonymousGroupedObservable<TKey, SlimNotifyCollectionChangedEvent<TElement>>(SlimInitialStateAndChanged.Subscribe, Key);
            }
        }

        public IGroupedObservable<TKey, SimpleNotifyCollectionChangedEvent<TElement>> ConnectableSimpleInitialStateAndChanged
        {
            get
            {
                return new AnonymousGroupedObservable<TKey, SimpleNotifyCollectionChangedEvent<TElement>>(SimpleInitialStateAndChanged.Subscribe, Key);
            }
        }

        public IGroupedObservable<TKey, SlimSimpleNotifyCollectionChangedEvent<TElement>> ConnectableSlimSimpleInitialStateAndChanged
        {
            get
            {
                return new AnonymousGroupedObservable<TKey, SlimSimpleNotifyCollectionChangedEvent<TElement>>(SlimSimpleInitialStateAndChanged.Subscribe, Key);
            }
        }

        public abstract TKey Key { get; }

        public override string ToString()
        {
            return "Key: " + ObjectEx.ToString(Key) + " (" + GetType().Name + ")";
        }
    }
}
