using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static IDisposable Subscribe<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            return source.InitialStateAndChanged.Subscribe();
        }

        public static IDisposable Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            return source.InitialStateAndChanged.Subscribe(onNext);
        }

        public static void Subscribe<T>(this ICollectionStatuses<T> source, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(token != null);

            source.InitialStateAndChanged.Subscribe(token);
        }

        public static IDisposable Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, Action onCompleted)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            return source.InitialStateAndChanged.Subscribe(onNext, onCompleted);
        }

        public static IDisposable Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, Action<Exception> onError)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            return source.InitialStateAndChanged.Subscribe(onNext, onError);
        }

        public static void Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(token != null);

            source.InitialStateAndChanged.Subscribe(onNext, token);
        }

        public static void Subscribe<T>(this ICollectionStatuses<T> source, IObserver<INotifyCollectionChangedEvent<T>> observer, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(observer != null);
            Contract.Requires<ArgumentNullException>(token != null);

            source.InitialStateAndChanged.Subscribe(observer, token);
        }

        public static void Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, Action onCompleted, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);
            Contract.Requires<ArgumentNullException>(token != null);
            
            source.InitialStateAndChanged.Subscribe(onNext, onCompleted, token);
        }

        public static IDisposable Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, Action<Exception> onError, Action onCompleted)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            return source.InitialStateAndChanged.Subscribe(onNext, onError, onCompleted);
        }

        public static void Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, Action<Exception> onError, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Requires<ArgumentNullException>(token != null);

            source.InitialStateAndChanged.Subscribe(onNext, onError, token);
        }

        public static void Subscribe<T>(this ICollectionStatuses<T> source, Action<INotifyCollectionChangedEvent<T>> onNext, Action<Exception> onError, Action onCompleted, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNext != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);
            Contract.Requires<ArgumentNullException>(token != null);

            source.InitialStateAndChanged.Subscribe(onNext, onError, onCompleted, token);
        }
    }
}
