using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class SimplifyProducer<T> : Producer<SimpleNotifyCollectionChangedEvent<T>>
    {
        readonly IObservable<INotifyCollectionChangedEvent<T>> source;

        public SimplifyProducer(IObservable<INotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SimpleNotifyCollectionChangedEvent<T>> observer)
        {
            return source
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    try
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedEventAction.InitialState:
                                observer.OnNext(ConvertInitialState(e.InitialState));
                                return;
                            case NotifyCollectionChangedEventAction.Add:
                                observer.OnNext(ConvertAdded(e.Added));
                                return;
                            case NotifyCollectionChangedEventAction.Remove:
                                observer.OnNext(ConvertRemoved(e.Removed));
                                return;
                            case NotifyCollectionChangedEventAction.Move:
                                observer.OnNext(ConvertMoved(e.Moved));
                                return;
                            case NotifyCollectionChangedEventAction.Replace:
                                observer.OnNext(ConvertReplaced(e.Replaced));
                                return;
                            case NotifyCollectionChangedEventAction.Reset:
                                observer.OnNext(ConvertReset(e.Reset));
                                return;
                        }
                    }
                    catch (InvalidInformationException<T> ex)
                    {
                        observer.OnError(ex);
                    }
                }, observer.OnError, observer.OnCompleted);
        }

        private SimpleNotifyCollectionChangedEvent<T> ConvertInitialState(IInitialState<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(initialState.Items);
        }

        private SimpleNotifyCollectionChangedEvent<T> ConvertAdded(IAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(ConvertAddedCore(added));
        }

        private SimpleNotifyCollectionChangedEvent<T> ConvertRemoved(IRemoved<T> removed, IEqualityComparer<T> comparer = null)
        {
            Contract.Requires<ArgumentNullException>(removed != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(ConvertRemovedCore(removed));
        }

        private SimpleNotifyCollectionChangedEvent<T> ConvertMoved(IMoved<T> moved, IEqualityComparer<T> comparer = null)
        {
            Contract.Requires<ArgumentNullException>(moved != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            var removed = ConvertRemovedCore(NotifyCollectionChangedEvent.CreateRemoved(moved.Items, moved.OldStartingIndex));
            var addedIndexes = ConvertAddedCore(NotifyCollectionChangedEvent.CreateAdded(moved.Items, moved.NewStartingIndex)).Select(unit => unit.Index);
            var added = addedIndexes.Zip(removed, (index, r) => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, r.Item, index)).ToArray().ToReadOnly();

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(removed.Concat(added).ToArray().ToReadOnly());
        }

        private SimpleNotifyCollectionChangedEvent<T> ConvertReplaced(IReplaced<T> replaced, IEqualityComparer<T> comparer = null)
        {
            Contract.Requires<ArgumentNullException>(replaced != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            var removed = ConvertRemovedCore(NotifyCollectionChangedEvent.CreateRemoved(replaced.OldItems, replaced.StartingIndex));
            var added = ConvertAddedCore(NotifyCollectionChangedEvent.CreateAdded(replaced.NewItems, replaced.StartingIndex));

            return SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(removed.Concat(added).ToArray().ToReadOnly());
        }

        private SimpleNotifyCollectionChangedEvent<T> ConvertReset(IReset<T> reset)
        {
            Contract.Requires<ArgumentNullException>(reset != null);
            Contract.Ensures(Contract.Result<SimpleNotifyCollectionChangedEvent<T>>() != null);

            return SimpleNotifyCollectionChangedEvent<T>.CreateReset(reset.Items);
        }

        private IReadOnlyList<AddedOrRemovedUnit<T>> ConvertAddedCore(IAdded<T> added)
        {
            Contract.Requires<ArgumentNullException>(added != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>(), x => x != null));

            if( added.StartingIndex < 0)
            {
                throw new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex);
            }

            var taggedAddedItems = added.Items
                .Select(x => new Tagged<T>(x))
                .ToArray()
                .ToReadOnly();
            return taggedAddedItems.Select((item, i) => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, item, added.StartingIndex + i)).ToArray().ToReadOnly();
        }

        private IReadOnlyList<AddedOrRemovedUnit<T>> ConvertRemovedCore(IRemoved<T> removed)
        {
            Contract.Requires<ArgumentNullException>(removed != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<T>>>(), x => x != null));

            if (removed.StartingIndex < 0)
            {
                throw new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex);
            }

            return removed.Items.Select(item => new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, new Tagged<T>(item), removed.StartingIndex)).ToArray().ToReadOnly();
        }
    }
}
