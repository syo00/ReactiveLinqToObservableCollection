using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    sealed class CombineLatestProducer<T> : Producer<IReadOnlyList<T>>
    {
        CollectionStatuses<IObservable<T>> source;
        List<Box<T>> currentItems = new List<Box<T>>();
        List<IDisposable> subscriptions = new List<IDisposable>();
        List<Box<Unit>> completed = new List<Box<Unit>>();

        public CombineLatestProducer(CollectionStatuses<IObservable<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(currentItems != null);
            Contract.Invariant(subscriptions != null);
            Contract.Invariant(currentItems.Count == subscriptions.Count);
            Contract.Invariant(subscriptions.Count == completed.Count);
        }

        protected override IDisposable SubscribeCore(ProducerObserver<IReadOnlyList<T>> observer)
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
                                InitialSteteOrReset(e.InitialState.Items, observer);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                Add(e.Added.StartingIndex, e.Added.Items, observer);

                                SendOnNext(observer);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                currentItems.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);
                                subscriptions.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);
                                completed.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);

                                SendOnNext(observer);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                Remove(e.Removed.StartingIndex, e.Removed.Items.Count);

                                SendOnNext(observer);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                Remove(e.Replaced.StartingIndex, e.Replaced.OldItems.Count);
                                Add(e.Replaced.StartingIndex, e.Replaced.NewItems, observer);

                                SendOnNext(observer);

                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                InitialSteteOrReset(e.Reset.Items, observer);

                                return;
                            }
                    }
                },
                error =>
                {
                    observer.OnError(error);
                },
                observer.OnCompleted);
        }

        void InitialSteteOrReset(IReadOnlyList<IObservable<T>> items, IObserver<IReadOnlyList<T>> observer)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(items, item => item != null));
            Contract.Requires<ArgumentNullException>(observer != null);

            currentItems.Clear();
            subscriptions.ForEach(d => d.Dispose());
            subscriptions.Clear();
            completed.Clear();

            foreach (var item in items)
            {
                var box = new Box<T>();
                var completedBox = new Box<Unit>();
                currentItems.Add(box);
                completed.Add(completedBox);
                var subscription = item
                    .Subscribe(x =>
                    {
                        box.Value = x;
                        box.HasValue = true;
                        SendOnNext(observer);
                    }, error =>
                    {
                        observer.OnError(error);
                    });
                subscriptions.Add(subscription);
            }

            SendOnNext(observer);
        }

        void Add(int startingIndex, IReadOnlyList<IObservable<T>> items, IObserver<IReadOnlyList<T>> observer)
        {
            Contract.Requires<ArgumentOutOfRangeException>(startingIndex >= 0);
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentNullException>(observer != null);

            var loopCount = 0;
            foreach (var item in items)
            {
                if (item == null)
                {
                    observer.OnError(new InvalidInformationException(InvalidInformationExceptionType.NotSupportedItem));
                    return;
                }

                var box = new Box<T>();
                var completedBox = new Box<Unit>();
                currentItems.Insert(startingIndex + loopCount, box);
                completed.Insert(startingIndex + loopCount, completedBox);
                var subscription = item.Subscribe(x =>
                {
                    box.Value = x;
                    box.HasValue = true;
                    SendOnNext(observer);

                }, error =>
                {
                    observer.OnError(error);
                });
                subscriptions.Add(subscription);
                loopCount++;
            }
        }

        void Remove(int startingIndex, int removingItemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(startingIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(removingItemsCount >= 0);

            currentItems.RemoveAtRange(startingIndex, removingItemsCount);
            subscriptions.RemoveAtRange(startingIndex, removingItemsCount).ForEach(d => d.Dispose());
        }

        void SendOnNext(IObserver<IReadOnlyList<T>> observer)
        {
            Contract.Requires<ArgumentNullException>(observer != null);

            if (currentItems.All(box => box.HasValue))
            {
                var items = currentItems.Select(box => box.Value).ToArray().ToReadOnly();
                observer.OnNext(items);
            }
        }
    }
}
