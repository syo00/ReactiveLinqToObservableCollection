using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.Producers;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.Support.Extractors;
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
        public static IObservable<SimpleNotifyCollectionChangedEvent<T>> Simplify<T>(this IObservable<INotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

            return ProducerObservable.Create(() => new SimplifyProducer<T>(source));
        }

        public static IReadOnlyList<INotifyCollectionChangedEvent<T>> Extract<T>(this SimpleNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<IReadOnlyCollection<INotifyCollectionChangedEvent<T>>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IReadOnlyCollection<INotifyCollectionChangedEvent<T>>>(), x => x != null));

            var extractor = new DefaultEventExtractor<T>();
            return extractor.Extract(e);
        }

        public static IObservable<INotifyCollectionChangedEvent<T>> Extract<T>(this IObservable<SimpleNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            var extractor = new DefaultEventExtractor<T>();
            return source.SelectMany(x => extractor.Extract(x));
        }

        public static IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> ToSlim<T>(this IObservable<SimpleNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SlimSimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                return source
                    .Subscribe(e =>
                    {
                        switch (e.Action)
                        {
                            case SimpleNotifyCollectionChangedEventAction.InitialState:
                                {
                                    observer.OnNext(SlimSimpleNotifyCollectionChangedEvent<T>.CreateInitialState(e.InitialStateOrReset));
                                    return;
                                }
                            case SimpleNotifyCollectionChangedEventAction.AddOrRemove:
                                {
                                    var addedOrRemoved = e.AddedOrRemoved
                                        .Select(unit => unit.Type == AddOrRemoveUnitType.Add ? new SlimAddedOrRemovedUnit<T>(unit.Item, unit.Index) : new SlimAddedOrRemovedUnit<T>(unit.Index))
                                        .ToArray()
                                        .ToReadOnly();
                                    observer.OnNext(SlimSimpleNotifyCollectionChangedEvent<T>.CreateAddedOrRemoved(addedOrRemoved));
                                    return;
                                }
                            case SimpleNotifyCollectionChangedEventAction.Reset:
                                {
                                    observer.OnNext(SlimSimpleNotifyCollectionChangedEvent<T>.CreateReset(e.InitialStateOrReset));
                                    return;
                                }
                            default:
                                {
                                    throw Exceptions.UnpredictableSwitchCasePattern;
                                }
                        }
                    }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<SimpleNotifyCollectionChangedEvent<T>> ExtractFromSlim<T>(this IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                var currentItems = new TaggedCollection<T>();

                return source
                    .Subscribe(e =>
                    {
                        if (currentItems == null)
                        {
                            return;
                        }

                        switch (e.Action)
                        {
                            case SlimSimpleNotifyCollectionChangedEventAction.InitialState:
                                {
                                    currentItems.AddRange(e.InitialStateOrReset);
                                    observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(e.InitialStateOrReset));
                                    return;
                                }
                            case SlimSimpleNotifyCollectionChangedEventAction.AddOrRemove:
                                {
                                    var newUnits = new List<AddedOrRemovedUnit<T>>();
                                    try
                                    {
                                        foreach (var unit in e.AddedOrRemoved)
                                        {
                                            if (unit.Type == SlimAddOrRemoveUnitType.Add)
                                            {
                                                currentItems.Insert(unit.Index, unit.Item);
                                                newUnits.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Add, unit.Item, unit.Index));
                                            }
                                            else
                                            {
                                                var removed = currentItems.RemoveAtRange<Tagged<T>>(unit.Index, 1).Single();
                                                newUnits.Add(new AddedOrRemovedUnit<T>(AddOrRemoveUnitType.Remove, removed, unit.Index));
                                            }
                                        }
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }

                                    observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateAddOrRemove(newUnits.ToReadOnly()));
                                    return;
                                }
                            case SlimSimpleNotifyCollectionChangedEventAction.Reset:
                                {
                                    currentItems.Clear();
                                    currentItems.AddRange(e.InitialStateOrReset);
                                    observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateReset(e.InitialStateOrReset));
                                    return;
                                }
                            default:
                                {
                                    throw Exceptions.UnpredictableSwitchCasePattern;
                                }
                        }
                    }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<SlimNotifyCollectionChangedEvent<T>> ToSlim<T>(this IObservable<INotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<SlimNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SlimNotifyCollectionChangedEvent<T>>(observer =>
            {
                return source
                    .Subscribe(e =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedEventAction.InitialState:
                                {
                                    var initialState = new SlimInitialState<T>(e.InitialState.Items);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(initialState));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Add:
                                {
                                    if (e.Added.StartingIndex <= -1)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex, e));
                                        return;
                                    }
                                    var added = new SlimAdded<T>(e.Added.Items, e.Added.StartingIndex);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(added));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Remove:
                                {
                                    if (e.Removed.StartingIndex <= -1)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex, e));
                                        return;
                                    }
                                    var removed = new SlimRemoved(e.Removed.StartingIndex, e.Removed.Items.Count);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(removed));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Replace:
                                {
                                    if (e.Replaced.StartingIndex <= -1)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex, e));
                                        return;
                                    }
                                    var replaced = new SlimReplaced<T>(e.Replaced.StartingIndex, e.Replaced.OldItems.Count, e.Replaced.NewItems);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(replaced));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Move:
                                {
                                    if (e.Moved.OldStartingIndex <= -1 || e.Moved.NewStartingIndex <= -1)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.NotSupportedIndex, e));
                                        return;
                                    }
                                    var moved = new SlimMoved(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.Items.Count);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(moved));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Reset:
                                {
                                    var reset = new SlimReset<T>(e.Reset.Items);
                                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(reset));
                                    return;
                                }
                            default:
                                {
                                    throw Exceptions.UnpredictableSwitchCasePattern;
                                }
                        }
                    }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<INotifyCollectionChangedEvent<T>> ExtractFromSlim<T>(this IObservable<SlimNotifyCollectionChangedEvent<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                var currentItems = new List<T>();

                return source
                    .Subscribe(e =>
                    {
                        if (currentItems == null)
                        {
                            return;
                        }

                        switch (e.Action)
                        {
                            case NotifyCollectionChangedEventAction.InitialState:
                                {
                                    currentItems.AddRange(e.InitialState.Items);

                                    observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(e.InitialState.Items));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Add:
                                {
                                    try
                                    {
                                        currentItems.InsertRange(e.Added.StartingIndex, e.Added.Items);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }

                                    observer.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(e.Added.Items, e.Added.StartingIndex));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Remove:
                                {
                                    IReadOnlyList<T> removed;
                                    try
                                    {
                                        removed = currentItems.RemoveAtRange(e.Removed.StartingIndex, e.Removed.ItemsCount);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }

                                    observer.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(removed, e.Removed.StartingIndex));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Replace:
                                {
                                    IReadOnlyList<T> removed;
                                    try
                                    {
                                        removed = currentItems.RemoveAtRange(e.Replaced.StartingIndex, e.Replaced.OldItemsCount);
                                        currentItems.InsertRange(e.Replaced.StartingIndex, e.Replaced.NewItems);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }

                                    observer.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(removed, e.Replaced.NewItems, e.Replaced.StartingIndex));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Move:
                                {
                                    IReadOnlyList<T> moved;
                                    try
                                    {
                                        moved = currentItems.MoveRange(e.Moved.OldStartingIndex, e.Moved.NewStartingIndex, e.Moved.ItemsCount);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        observer.OnError(new InvalidInformationException<T>(InvalidInformationExceptionType.InvalidIndex));
                                        currentItems = null;
                                        return;
                                    }
                                    observer.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(moved, e.Moved.OldStartingIndex, e.Moved.NewStartingIndex));
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Reset:
                                {
                                    currentItems.Clear();
                                    currentItems.AddRange(e.Reset.Items);

                                    observer.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<T>(e.Reset.Items));
                                    return;
                                }
                            default:
                                {
                                    throw Exceptions.UnpredictableSwitchCasePattern;
                                }
                        }
                    }, observer.OnError, observer.OnCompleted);
            });
        }
    }
}
