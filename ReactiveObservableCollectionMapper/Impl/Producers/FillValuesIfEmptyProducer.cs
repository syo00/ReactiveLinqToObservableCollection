using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Kirinji.LightWands;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class FillValuesIfEmptyProducer<T> : Producer<INotifyCollectionChangedEvent<T>>
    {
        readonly CollectionStatuses<T> source;
        readonly List<T> notConvertedCurrentItems = new List<T>();
        readonly IReadOnlyList<T> fillingValues;
        bool filled;

        public FillValuesIfEmptyProducer(CollectionStatuses<T> source, IReadOnlyList<T> fillingValues)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(fillingValues != null);
            Contract.Requires<ArgumentException>(fillingValues.Count >= 1);

            this.source = source;
            this.fillingValues = fillingValues;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(notConvertedCurrentItems != null);
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
                                notConvertedCurrentItems.ApplyChangeEvent(e);

                                if (e.InitialState.Items.Count == 0)
                                {
                                    observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(fillingValues));
                                    filled = true;
                                    return;
                                }

                                observer.OnNext(e);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                notConvertedCurrentItems.ApplyChangeEvent(e);

                                if (filled)
                                {
                                    observer.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(fillingValues, e.Added.Items, e.Added.StartingIndex));
                                    filled = false;
                                    return;
                                }

                                observer.OnNext(e);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                notConvertedCurrentItems.ApplyChangeEvent(e);

                                if (notConvertedCurrentItems.Count == 0)
                                {
                                    observer.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(e.Removed.Items, fillingValues, e.Removed.StartingIndex));
                                    filled = true;
                                    return;
                                }

                                observer.OnNext(e);
                                return;
                            }
                        case NotifyCollectionChangedEventAction.Reset:
                            {
                                if (filled)
                                {
                                    return;
                                }

                                if(e.Reset.Items.Count != 0)
                                {
                                    observer.OnNext(e);
                                    notConvertedCurrentItems.ApplyChangeEvent(e);
                                    return;
                                }

                                observer.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(fillingValues));
                                filled = true;
                                notConvertedCurrentItems.ApplyChangeEvent(e);
                                return;
                            }
                        default:
                            {
                                notConvertedCurrentItems.ApplyChangeEvent(e);

                                observer.OnNext(e);
                                return;
                            }
                    }
                }, observer.OnError, observer.OnCompleted);
        }
    }
}
