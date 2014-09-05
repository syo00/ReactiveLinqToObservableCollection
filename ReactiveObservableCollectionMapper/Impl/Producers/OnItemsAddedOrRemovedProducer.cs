using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class OnItemsAddedOrRemovedProducer<T> : Producer<INotifyCollectionChangedEvent<T>>
    {
        private readonly CollectionStatuses<T> source;
        private readonly List<T> currentItems = new List<T>();
        private readonly Action<IReadOnlyList<T>> addedAction;
        private readonly Action<IReadOnlyList<T>> removedAction;
        private readonly Func<T, T, bool> comparer;

        public OnItemsAddedOrRemovedProducer(CollectionStatuses<T> source, Action<IReadOnlyList<T>> addedAction, Action<IReadOnlyList<T>> removedAction, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(addedAction != null);
            Contract.Requires<ArgumentNullException>(removedAction != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.source = source;
            this.addedAction = addedAction;
            this.removedAction = removedAction;
            this.comparer = comparer;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(currentItems != null);
            Contract.Invariant(addedAction != null);
            Contract.Invariant(removedAction != null);
            Contract.Invariant(comparer != null);
        }

        protected override IDisposable SubscribeCore(ProducerObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            return source
                .InitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            {
                                addedAction(e.InitialState.Items);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                addedAction(e.Added.Items);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                removedAction(e.Removed.Items);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                var tuple = RemoveCoexistedValues(e.Replaced.OldItems, e.Replaced.NewItems, comparer);
                                removedAction(tuple.Item1);
                                addedAction(tuple.Item2);
                                break;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                var tuple = RemoveCoexistedValues(currentItems, e.Reset.Items, comparer);
                                removedAction(tuple.Item1);
                                addedAction(tuple.Item2);
                                break;
                            }
                    }

                    currentItems.ApplyChangeEvent(e);
                    observer.OnNext(e);
                }, observer.OnError, observer.OnCompleted);
        }

        static Tuple<IReadOnlyList<T>, IReadOnlyList<T>> RemoveCoexistedValues(IReadOnlyList<T> left, IReadOnlyList<T> right, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(left != null);
            Contract.Requires<ArgumentNullException>(right != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<Tuple<IReadOnlyList<T>, IReadOnlyList<T>>>() != null);
            Contract.Ensures(Contract.Result<Tuple<IReadOnlyList<T>, IReadOnlyList<T>>>().Item1 != null);
            Contract.Ensures(Contract.Result<Tuple<IReadOnlyList<T>, IReadOnlyList<T>>>().Item2 != null);

            var removingLeftItemIndexes = new List<int>();
            var rightResult = right.ToList();
            int index = 0;
            foreach (var l in left)
            {
                var removingRightIndex = rightResult.FirstIndex(r => comparer(l, r));
                if (removingRightIndex != null)
                {
                    rightResult.RemoveAt(removingRightIndex.Value);
                    removingLeftItemIndexes.Add(index);
                }

                index++;
            }

            var leftResult = left.ToList();
            Contract.Assume(leftResult != null);
            removingLeftItemIndexes.AsEnumerable().Reverse().ForEach(leftResult.RemoveAt);
            return new Tuple<IReadOnlyList<T>, IReadOnlyList<T>>(leftResult.ToReadOnly(), rightResult.ToReadOnly());
        }
    }
}
