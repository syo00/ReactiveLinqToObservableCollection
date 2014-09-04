using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.ComponentModel;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ICollectionStatuses<TTo> SelectTest<T, TTo>(this ICollectionStatuses<T> source, Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<TTo>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<TTo>>(observer =>
                {
                    return source
                        .InitialStateAndChanged
                        .Subscribe(e =>
                        {
                            switch (e.Action)
                            {
                                case NotifyCollectionChangedEventAction.InitialState:
                                    {
                                        observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(e.InitialState.Items.Select(converter).ToArray().ToReadOnly()));
                                        return;
                                    }
                                case NotifyCollectionChangedEventAction.Add:
                                    {
                                        observer.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(e.Added.Items.Select(converter).ToArray().ToReadOnly(), e.Added.StartingIndex));
                                        return;
                                    }
                                default:
                                    {
                                        observer.OnError(new NotSupportedException());
                                        return;
                                    }

                            }
                        }, observer.OnError, observer.OnCompleted);
                })
                .ToStatuses();
        }
    }
}
