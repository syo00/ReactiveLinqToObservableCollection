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
        public static IObservable<INotifyCollectionChangedEvent<T>> Check<T>(this IObservable<INotifyCollectionChangedEvent<T>> source, IEqualityComparer<T> equalityComparer = null)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                var isInitialStateStreamed = false;
                var currentItems = new List<T>();

                return source
                    .CheckSynchronization()
                    .Where(x => x != null)
                    .Subscribe(x =>
                    {
                        if (isInitialStateStreamed)
                        {
                            if (x.Action == NotifyCollectionChangedEventAction.InitialState)
                            {
                                observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, x));
                                return;
                            }
                        }
                        else
                        {
                            isInitialStateStreamed = true;
                            if (x.Action != NotifyCollectionChangedEventAction.InitialState)
                            {
                                observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, x));
                                return;
                            }
                        }

                        try
                        {
                            currentItems.ApplyChangeEvent(x, true, equalityComparer);
                        }
                        catch (InvalidInformationException<T> e)
                        {
                            observer.OnError(e);
                            return;
                        }

                        observer.OnNext(x);
                    }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<SimpleNotifyCollectionChangedEvent<T>> Check<T>(this IObservable<SimpleNotifyCollectionChangedEvent<T>> source, bool checkRemovingItemsEquality)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                var isInitialStateStreamed = false;
                var currentItems = new TaggedCollection<T>();

                return source
                    .CheckSynchronization()
                    .Where(x => x != null)
                    .Subscribe(e =>
                    {
                        switch (e.Action)
                        {
                            case SimpleNotifyCollectionChangedEventAction.InitialState:
                                {
                                    if (isInitialStateStreamed)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, e));
                                        return;
                                    }
                                    isInitialStateStreamed = true;

                                    break;
                                }
                            default:
                                {
                                    if (!isInitialStateStreamed)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, e));
                                        return;
                                    }

                                    break;
                                }
                        }

                        try
                        {
                            ApplySimpleChangeEvent(currentItems, e, checkRemovingItemsEquality);
                        }
                        catch (InvalidInformationException<T> ex)
                        {
                            observer.OnError(ex);
                            return;
                        }
                        observer.OnNext(e);
                        return;
                    }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<INotifyCollectionChangedEvent<T>> CheckWhenDebug<T>(this IObservable<INotifyCollectionChangedEvent<T>> source, IEqualityComparer<T> equalityComparer = null)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

#if DEBUG
            return source.CheckSynchronization().Check(equalityComparer);
#else
            return source;
#endif
        }

        internal static IObservable<SimpleNotifyCollectionChangedEvent<T>> CheckWhenDebug<T>(this IObservable<SimpleNotifyCollectionChangedEvent<T>> source, bool checkRemovingItemsEquality)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

#if DEBUG
            return source.CheckSynchronization().Check(checkRemovingItemsEquality);
#else
            return source;
#endif
        }

        /// <summary>remove items that its count is 0</summary>
        public static IObservable<IReadOnlyList<INotifyCollectionChangedEvent<T>>> CleanLight<T>(this IObservable<INotifyCollectionChangedEvent<T>> source)
        {
            return source
                .Select(e =>
                {
                    switch(e.Action)
                    {
                        case NotifyCollectionChangedEventAction.Add:
                            {
                                if(e.Added.Items.Count == 0)
                                {
                                    return new INotifyCollectionChangedEvent<T>[0].ToReadOnly();
                                }
                                return new[] { e };
                            }
                        case NotifyCollectionChangedEventAction.Remove:
                            {
                                if (e.Removed.Items.Count == 0)
                                {
                                    return new INotifyCollectionChangedEvent<T>[0].ToReadOnly();
                                }
                                return new[] { e };
                            }
                        case NotifyCollectionChangedEventAction.Move:
                            {
                                if (e.Moved.Items.Count == 0)
                                {
                                    return new INotifyCollectionChangedEvent<T>[0].ToReadOnly();
                                }
                                return new[] { e };
                            }
                        case NotifyCollectionChangedEventAction.Replace:
                            {
                                if (e.Replaced.OldItems.Count == 0 && e.Replaced.NewItems.Count == 0)
                                {
                                    return new INotifyCollectionChangedEvent<T>[0].ToReadOnly();
                                }
                                if (e.Replaced.OldItems.Count == 0)
                                {
                                    return new[] { NotifyCollectionChangedEvent.CreateAddedEvent(e.Replaced.NewItems, e.Replaced.StartingIndex) };
                                }
                                if (e.Replaced.NewItems.Count == 0)
                                {
                                    return new[] { NotifyCollectionChangedEvent.CreateRemovedEvent(e.Replaced.OldItems, e.Replaced.StartingIndex) };
                                }
                                return new[] { e };
                            }
                        default:
                            {
                                return new[] { e };
                            }
                    }
                });
        }

        /// <summary>checks events consistency, remove items that its count is 0, and fixes negative index</summary>
        public static IObservable<IReadOnlyList<INotifyCollectionChangedEvent<T>>> CheckAndClean<T>(this IObservable<INotifyCollectionChangedEvent<T>> source, bool checkRemovingItemsEquality, IEqualityComparer<T> equalityComparer = null)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<IReadOnlyList<INotifyCollectionChangedEvent<T>>>>() != null);

            return Observable.Create<IReadOnlyList<INotifyCollectionChangedEvent<T>>>(observer =>
            {
                var isInitialStateStreamed = false;
                var currentItems = new List<T>();

                return source
                    .CheckSynchronization()
                    .Subscribe(e =>
                    {
                        if (e == null)
                        {
                            observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.EventIsNull));
                            return;
                        }

                        if (isInitialStateStreamed)
                        {
                            if (e.Action == NotifyCollectionChangedEventAction.InitialState)
                            {
                                observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, e));
                                return;
                            }
                        }
                        else
                        {
                            isInitialStateStreamed = true;
                            if (e.Action != NotifyCollectionChangedEventAction.InitialState)
                            {
                                observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, e));
                                return;
                            }
                        }

                        IReadOnlyList<INotifyCollectionChangedEvent<T>> nextEvents;
                        try
                        {
                            nextEvents = currentItems.ApplyChangeEvent(e, checkRemovingItemsEquality, equalityComparer);
                        }
                        catch (InvalidInformationException<T> ex)
                        {
                            observer.OnError(ex);
                            return;
                        }

                        observer.OnNext(nextEvents);
                    }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<SimpleNotifyCollectionChangedEvent<T>> CheckAndClean<T>(this IObservable<SimpleNotifyCollectionChangedEvent<T>> source, bool checkRemovingItemsEquality)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                var isInitialStateStreamed = false;
                var currentItems = new TaggedCollection<T>();

                return source
                    .CheckSynchronization()
                    .Where(x => x != null)
                    .Subscribe(e =>
                    {
                        switch (e.Action)
                        {
                            case SimpleNotifyCollectionChangedEventAction.InitialState:
                                {
                                    if (isInitialStateStreamed)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, e));
                                        return;
                                    }
                                    isInitialStateStreamed = true;

                                    currentItems.AddRange(e.InitialStateOrReset);

                                    observer.OnNext(ApplySimpleChangeEvent(currentItems, e, checkRemovingItemsEquality));
                                    return;
                                }
                            default:
                                {
                                    if (!isInitialStateStreamed)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotFollowingEventSequenceRule, e));
                                        return;
                                    }

                                    SimpleNotifyCollectionChangedEvent<T> nextEvent;
                                    try
                                    {
                                        nextEvent = ApplySimpleChangeEvent(currentItems, e, checkRemovingItemsEquality);
                                    }
                                    catch (InvalidInformationException<T> ex)
                                    {
                                        observer.OnError(ex);
                                        return;
                                    }
                                    observer.OnNext(nextEvent);
                                    return;
                                }
                        }
                    }, observer.OnError, observer.OnCompleted);
            });
        }
    }
}
