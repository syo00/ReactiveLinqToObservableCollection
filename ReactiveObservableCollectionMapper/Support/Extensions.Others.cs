using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal static partial class Extensions
    {
        /// <summary>Checks the observable sequences are synchronized. It is faster than Synchronize().</summary>
        public static IObservable<T> CheckSynchronization<T>(this IObservable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            if (Debug.DebugSetting.CheckSynchronization == Debug.CheckSynchronizationType.Disable)
            {
                return source;
            }

            if (Debug.DebugSetting.CheckSynchronization == Debug.CheckSynchronizationType.Synchronization)
            {
                return source.Synchronize();
            }

            var synchronizationError = new InvalidOperationException("Arrived a value before previous value(s) are not passed through completely. Use Synchronize() or ObserveOn() methods to prevent this error.");

            return Observable.Create<T>(observer =>
            {
                int isWorking = 0;
                return source.Subscribe(x =>
                {
                    var result = Interlocked.Exchange(ref isWorking, 1);

                    if (result == 1)
                    {
                        observer.OnError(synchronizationError);

                        return;
                    }

                    observer.OnNext(x);

                    isWorking = 0;

                }, ex =>
                {
                    var result = Interlocked.Exchange(ref isWorking, 1);

                    if (result == 1)
                    {
                        observer.OnError(synchronizationError);

                        return;
                    }

                    observer.OnError(ex);

                    isWorking = 0;
                }, () =>
                {
                    var result = Interlocked.Exchange(ref isWorking, 1);

                    if (result == 1)
                    {
                        observer.OnError(synchronizationError);

                        return;
                    }

                    observer.OnCompleted();

                    isWorking = 0;
                });
            });
        }

        public static IObservable<INotifyCollectionChangedEvent<T>> ConvertResetToRemoved<T>(this IObservable<INotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                var currentItems = new List<T>();
                return source.Subscribe(e =>
                {
                    if (e.Action == NotifyCollectionChangedEventAction.Reset)
                    {
                        observer.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(currentItems.ToArray().ToReadOnly(), 0));
                    }
                    else
                    {
                        observer.OnNext(e);
                    }

                    currentItems.ApplyChangeEvent(e);
                }, observer.OnError, observer.OnCompleted);

            });
        }
    }
}
