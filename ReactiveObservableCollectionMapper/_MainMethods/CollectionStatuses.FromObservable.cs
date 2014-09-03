using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Kirinji.LightWands;
using System.Reactive;
using Kirinji.LinqToObservableCollection.Support;
using System.ComponentModel;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        public static ICollectionStatuses<T> Switch<T>(this IObservable<ICollectionStatuses<T>> source, Action<IObserver<INotifyCollectionChangedEvent<T>>, IInitialState<T>> glue)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            throw new NotImplementedException(); // glueはユーザー作成なので信頼できないが、現時点でCheckAndCleanできてないため。

            return source.SwitchCore(glue);
        }

        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        public static ICollectionStatuses<T> Switch<T>(this IObservable<ICollectionStatuses<T>> source, Action<IObserver<INotifyCollectionChangedEvent<T>>, IInitialState<T>, IReadOnlyList<T>> glue)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            throw new NotImplementedException(); //glueはユーザー作成なので信頼できないが、現時点でCheckAndCleanできてないため。

            return source.SwitchCore(glue);
        }

        public static ICollectionStatuses<T> SwitchWithReset<T>(this IObservable<ICollectionStatuses<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.SwitchCore((observer, newInitialState) =>
                {
                    observer.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<T>(newInitialState.Items));
                });
        }

        public static ICollectionStatuses<T> SwitchWithMove<T>(this IObservable<ICollectionStatuses<T>> source, IEqualityComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source.SwitchCore((observer, newInitialState, oldItems) =>
            {
                var events = EventsConverter.CreateEventsToGetEquality(oldItems, newInitialState.Items, comparer);
                events.ForEach(observer.OnNext);
            });
        }
    }
}
