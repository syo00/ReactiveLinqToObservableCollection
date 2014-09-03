using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        internal static IObservable<EventAttached<IReadOnlyList<T>, T>> SelectToCurrentCollections<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<EventAttached<IReadOnlyList<T>, T>>>() != null);

            return Observable.Create<EventAttached<IReadOnlyList<T>, T>>(observer =>
                {
                    var result = new List<T>();
                    return source
                        .InitialStateAndChanged
                        .CheckSynchronization()
                        .Subscribe(e =>
                        {
                            result.ApplyChangeEvent(e);
                            observer.OnNext(EventAttached.Create(result.ToArray().ToReadOnly(), e));
                        }, observer.OnError, observer.OnCompleted);

                });
        }
    }
}
