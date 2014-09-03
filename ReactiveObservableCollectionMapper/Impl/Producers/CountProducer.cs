using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class CountProducer<T> : Producer<int>
    {
        readonly CollectionStatuses<T> source;
        readonly List<bool> matched = new List<bool>();
        readonly Func<T, bool> predicate;
        int lastCount;

        public CountProducer(CollectionStatuses<T> source, Func<T, bool> predicate)
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
            Contract.Invariant(matched != null);
            Contract.Invariant(predicate != null);
        }

        protected override IDisposable SubscribeCore(ProducerObserver<int> observer)
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
                                matched.AddRange(e.InitialState.Items.Select(predicate));
                                lastCount = matched.Count(b => b);
                                observer.OnNext(lastCount);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                matched.InsertRange(e.Added.StartingIndex, e.Added.Items.Select(predicate));
                                lastCount = matched.Count(b => b);
                                observer.OnNext(lastCount);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                matched.RemoveRange(e.Removed.StartingIndex, e.Removed.Items.Count);
                                lastCount = matched.Count(b => b);
                                observer.OnNext(lastCount);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                matched.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);
                                observer.OnNext(lastCount);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                matched.RemoveRange(e.Replaced.StartingIndex, e.Replaced.OldItems.Count);
                                matched.InsertRange(e.Replaced.StartingIndex, e.Replaced.NewItems.Select(predicate));
                                lastCount = matched.Count(b => b);
                                observer.OnNext(lastCount);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                matched.Clear();
                                matched.AddRange(e.Reset.Items.Select(predicate));
                                lastCount = matched.Count(b => b);
                                observer.OnNext(lastCount);
                                return;
                            }
                    }
                }, observer.OnError, observer.OnCompleted);
        }
    }
}
