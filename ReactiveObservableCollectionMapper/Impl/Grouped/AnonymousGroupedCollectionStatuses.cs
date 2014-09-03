using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Grouped
{
    sealed class AnonymousGroupedCollectionStatuses<TKey, TElement> : GroupedCollectionStatuses<TKey, TElement>
    {
        readonly CollectionStatuses<TElement> source;
        readonly TKey key;

        public AnonymousGroupedCollectionStatuses(CollectionStatuses<TElement> source, TKey key)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
            this.key = key;
        }

        protected override IObservable<INotifyCollectionChangedEvent<TElement>> CreateInitialStateAndChanged()
        {
            return source.InitialStateAndChanged;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<TElement>> CreateSlimInitialStateAndChanged()
        {
            return source.SlimInitialStateAndChanged;
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<TElement>> CreateSimpleInitialStateAndChanged()
        {
            return source.SimpleInitialStateAndChanged;
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<TElement>> CreateSlimSimpleInitialStateAndChanged()
        {
            return source.SlimSimpleInitialStateAndChanged;
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                return source.RecommendedEvent;
            }
        }

        public override TKey Key
        {
            get
            {
                return key;
            }
        }
    }
}
