using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class AnyProducer<T> : Producer<bool>
    {
        readonly CollectionStatuses<T> source;
        int? foundIndex;
        readonly List<T> currentItems = new List<T>();
        readonly Func<T, bool> predicate;

        public AnyProducer(CollectionStatuses<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            this.source = source;
            this.predicate = predicate;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(currentItems != null);
            Contract.Invariant(predicate != null);
        }

        protected override IDisposable SubscribeCore(ProducerObserver<bool> observer)
        {
            return source
                .InitialStateAndChanged
                .CheckSynchronization()
                .Subscribe(e =>
                {
                    switch(e.Action)
                    {
                        case NotifyCollectionChangedEventAction.InitialState:
                            {
                                currentItems.AddRange(e.InitialState.Items);
                                foundIndex = currentItems.FirstIndex(predicate);
                                observer.OnNext(foundIndex != null);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                Add(e.Added.StartingIndex, e.Added.Items);

                                if(foundIndex == null)
                                {
                                    foundIndex = currentItems.FirstIndex(predicate);
                                }

                                observer.OnNext(foundIndex != null);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                Remove(e.Removed.StartingIndex, e.Removed.Items.Count);

                                if (foundIndex == null)
                                {
                                    foundIndex = currentItems.FirstIndex(predicate);
                                }

                                observer.OnNext(foundIndex != null);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                Remove(e.Replaced.StartingIndex, e.Replaced.OldItems.Count);
                                Add(e.Replaced.StartingIndex, e.Replaced.NewItems);

                                if (foundIndex == null)
                                {
                                    foundIndex = currentItems.FirstIndex(predicate);
                                }

                                observer.OnNext(foundIndex != null);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                Move(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);

                                if (foundIndex == null)
                                {
                                    foundIndex = currentItems.FirstIndex(predicate);
                                }

                                observer.OnNext(foundIndex != null);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                currentItems.Clear();
                                currentItems.AddRange(e.Reset.Items);
                                foundIndex = currentItems.FirstIndex(predicate);
                                observer.OnNext(foundIndex != null);

                                return;
                            }
                    }

                }, observer.OnError, observer.OnCompleted);
        }

        private void Remove(int startingIndex, int itemsCount)
        {
            currentItems.RemoveAtRange(startingIndex, itemsCount);
            if (foundIndex != null && foundIndex >= startingIndex)
            {
                foundIndex -= itemsCount;
                if (foundIndex < startingIndex)
                {
                    foundIndex = null;
                }
            }
        }

        private void Add(int startingIndex, IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);

            currentItems.InsertRange(startingIndex, items);
            if (foundIndex != null && foundIndex >= startingIndex)
            {
                foundIndex += items.Count;
            }
        }

        private void Move(int oldStartingIndex, int newStartingIndex , int itemsCount)
        {
            var removedItems = currentItems.RemoveAtRange(oldStartingIndex, itemsCount);
            currentItems.InsertRange(newStartingIndex, removedItems);

            if (foundIndex >= oldStartingIndex)
            {
                foundIndex -= itemsCount;
                if (foundIndex < oldStartingIndex)
                {
                    foundIndex = newStartingIndex + (oldStartingIndex - foundIndex - 1);
                    return;
                }

                if (foundIndex >= newStartingIndex)
                {
                    foundIndex += itemsCount;
                }
            }
        }
    }
}
