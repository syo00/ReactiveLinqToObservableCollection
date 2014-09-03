using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System.ComponentModel;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static ICollectionStatuses<T> Create<T>(IObservable<INotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .CheckAndClean(false)
                .SelectMany(x => x)
                .ToStatuses();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ICollectionStatuses<T> Defer<T>(Func<ICollectionStatuses<T>> factory)
        {
            Contract.Requires<ArgumentNullException>(factory != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            throw new NotImplementedException();

            //return new AnonymousCollectionStatuses<T>(
            //    () => factory().InitialStateAndChanged,
            //    () => factory().ToInstance().SimpleInitialStateAndChanged);
        }

        public static ICollectionStatuses<T> Empty<T>()
        {
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return Return<T>(new T[0].ToReadOnly());
        }

        public static ICollectionStatuses<T> Return<T>(IReadOnlyList<T> initialState)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            var coreObservable = Observable.Return<INotifyCollectionChangedEvent<T>>(NotifyCollectionChangedEvent.CreateInitialStateEvent(initialState));
            return new AnonymousCollectionStatuses<T>(coreObservable);
        }
    }
}
