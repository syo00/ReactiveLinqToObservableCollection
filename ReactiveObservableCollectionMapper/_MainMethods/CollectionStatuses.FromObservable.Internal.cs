using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        internal static ICollectionStatuses<T> SwitchCore<T>(this IObservable<ICollectionStatuses<T>> source, Action<IObserver<INotifyCollectionChangedEvent<T>>, IInitialState<T>> glue)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                bool isInitialStateArrived = false;
                List<T> currentItems = new List<T>();

                return
                    CleanSwitchingObservable(source)
                    .Subscribe(x =>
                    {
                        if (!isInitialStateArrived)
                        {
                            if (x.Action != NotifyCollectionChangedEventAction.InitialState)
                            {
                                observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, x));
                                return;
                            }

                            observer.OnNext(x);
                            isInitialStateArrived = true;
                            return;
                        }

                        if (x.Action == NotifyCollectionChangedEventAction.InitialState)
                        {
                            glue(observer, x.InitialState);
                            return;
                        }

                        observer.OnNext(x);
                    }, observer.OnError, observer.OnCompleted);
            })
                .ToStatuses();
        }

        // converter の第三引数は次の ICollectionStatuses が来る直前のコレクションの状態
        internal static ICollectionStatuses<T> SwitchCore<T>(this IObservable<ICollectionStatuses<T>> source, Action<IObserver<INotifyCollectionChangedEvent<T>>, IInitialState<T>, IReadOnlyList<T>> glue)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                bool isInitialStateArrived = false;
                List<T> currentItems = new List<T>();

                return
                    CleanSwitchingObservable(source)
                    .Subscribe(x =>
                    {
                        if (!isInitialStateArrived)
                        {
                            if (x.Action != NotifyCollectionChangedEventAction.InitialState)
                            {
                                observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, x));
                                return;
                            }

                            try
                            {
                                currentItems.ApplyChangeEvent(x);
                            }
                            catch (InvalidInformationException<T> ex)
                            {
                                observer.OnError(ex);
                                return;
                            }
                            catch
                            {
                                throw;
                            }

                            observer.OnNext(x);
                            isInitialStateArrived = true;
                            return;
                        }

                        if (x.Action == NotifyCollectionChangedEventAction.InitialState)
                        {
                            glue(observer, x.InitialState, currentItems);
                            return;
                        }

                        try
                        {
                            currentItems.ApplyChangeEvent(x);
                        }
                        catch (InvalidInformationException<T> ex)
                        {
                            observer.OnError(ex);
                            return;
                        }
                        catch
                        {
                            throw;
                        }

                        observer.OnNext(x);
                    }, observer.OnError, observer.OnCompleted);
            })
                .ToStatuses();
        }

        static IObservable<INotifyCollectionChangedEvent<T>> CleanSwitchingObservable<T>(IObservable<ICollectionStatuses<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            return source
                .UseObserver((o, s) =>
                {
                    if (s == null)
                    {
                        o.OnError(new InvalidOperationException(typeof(ICollectionStatuses<T>).Name + "is null."));
                        return;
                    }
                    o.OnNext(s);
                })
                .Select(s => s.InitialStateAndChanged)
                .Switch()
                .CheckSynchronization();
        }
    }
}
