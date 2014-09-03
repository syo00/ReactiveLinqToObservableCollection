using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl
{
    internal class CollectionBasedCollectionStatuses<T> : CollectionStatuses<T>
    {
        readonly IObservable<INotifyCollectionChangedEvent<T>> initialStateAndChanged;
        readonly CollectionStatuses<T> attachedStatuses;

        private CollectionBasedCollectionStatuses(Func<IReadOnlyList<T>> obtainCurrentCollection, INotifyCollectionChanged changed, ICollectionStatusesAttached<T> statusesAttached)
        {
            Contract.Requires<ArgumentNullException>(obtainCurrentCollection != null);
            Contract.Requires<ArgumentNullException>(changed != null);

            if (statusesAttached != null)
            {
                this.initialStateAndChanged = 
                    statusesAttached
                    .Statuses
                    .InitialStateAndChanged;

                this.attachedStatuses = statusesAttached as CollectionStatuses<T>;
            }
            else
            {
                var collectionChangedEventObservable =
                        Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => new NotifyCollectionChangedEventHandler(h),
                        h => changed.CollectionChanged += h,
                        h => changed.CollectionChanged -= h);

                var changedOnly =
                    collectionChangedEventObservable
                    .Select(x => NotifyCollectionChangedEvent.Convert<T>(x.EventArgs, obtainCurrentCollection));

                this.initialStateAndChanged = Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
                    {
                        return changedOnly
                            .StartWith(NotifyCollectionChangedEvent.CreateInitialStateEvent(obtainCurrentCollection()))
                            .Subscribe(observer);
                    });
            }
        }

        public CollectionBasedCollectionStatuses(ObservableCollection<T> source)
            : this(() => source.ToArray().ToReadOnly(), source, source as ICollectionStatusesAttached<T>)
        {
            Contract.Requires<ArgumentNullException>(source != null);
        }

        public CollectionBasedCollectionStatuses(ReadOnlyObservableCollection<T> source)
            : this(() => source.ToArray().ToReadOnly(), source, source as ICollectionStatusesAttached<T>)
        {
            Contract.Requires<ArgumentNullException>(source != null);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(initialStateAndChanged != null);
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            return initialStateAndChanged;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            if (attachedStatuses != null)
            {
                return attachedStatuses.SlimInitialStateAndChanged;
            }

            return initialStateAndChanged.ToSlim();
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            if(attachedStatuses != null)
            {
                return attachedStatuses.SimpleInitialStateAndChanged;
            }

            return initialStateAndChanged.Simplify();
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            if (attachedStatuses != null)
            {
                return attachedStatuses.SlimSimpleInitialStateAndChanged;
            }

            return initialStateAndChanged.Simplify().ToSlim();
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                if (attachedStatuses != null)
                {
                    return attachedStatuses.RecommendedEvent;
                }

                return new RecommendedEvent(true, false, false, false);
            }
        }
    }
}
