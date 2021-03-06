﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LightWands;
using System.Reactive.Subjects;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl
{
    // イベントに応じて IList の変更を任せるクラス
    sealed class DelegationCollectionStatuses<T, TCollection, TTaggedCollection> : CollectionStatuses<T>, IDisposable
        where TCollection : IList<T>
        where TTaggedCollection : IList<Tagged<T>>
    {
        IDisposable subscriptions;
        Exception error;
        bool isCompleted;
        bool isInitialStateArrived;
        Subject<INotifyCollectionChangedEvent<T>> notifyCollectionChangedEventSubject;
        Subject<SlimNotifyCollectionChangedEvent<T>> slimNotifyCollectionChangedEventSubject;
        Subject<SimpleNotifyCollectionChangedEvent<T>> simpleNotifyCollectionChangedEventSubject;
        Subject<SlimSimpleNotifyCollectionChangedEvent<T>> slimSimpleNotifyCollectionChangedEventSubject;
        readonly Func<TCollection> collectionCreator;
        readonly Func<TTaggedCollection> taggedCollectionCreator;

        public DelegationCollectionStatuses(CollectionStatuses<T> source, Func<TCollection> collectionCreator, Func<TTaggedCollection> taggedCollectionCreator)
            : this(source, collectionCreator, taggedCollectionCreator, () => { }, _ => { }, () => { })
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(collectionCreator != null);
            Contract.Requires<ArgumentNullException>(taggedCollectionCreator != null);
        }

        public DelegationCollectionStatuses(CollectionStatuses<T> source, Func<TCollection> collectionCreator, Func<TTaggedCollection> taggedCollectionCreator, Action onInitialStateArrived, Action<Exception> onError, Action onCompleted)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(collectionCreator != null);
            Contract.Requires<ArgumentNullException>(taggedCollectionCreator != null);
            Contract.Requires<ArgumentNullException>(onInitialStateArrived != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);

            this.collectionCreator = collectionCreator;
            this.taggedCollectionCreator = taggedCollectionCreator;

            Apply(source, onInitialStateArrived, onError, onCompleted);
        }

        void Apply(CollectionStatuses<T> source, Action onInitialStateArrived, Action<Exception> onError, Action onCompleted)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onInitialStateArrived != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);

            Action<Exception> subscribedOnError = ex =>
            {
                error = ex;
                onError(ex);
            };

            Action subscribedOnCompleted = () =>
            {
                isCompleted = true;
                onCompleted();
            };

            switch (source.RecommendedEvent.RecommendedEventType)
            {
                case RecommendedEventType.SlimOne:
                    {
                        slimNotifyCollectionChangedEventSubject = new Subject<SlimNotifyCollectionChangedEvent<T>>();
                        subscribedOnError += slimNotifyCollectionChangedEventSubject.OnError;
                        subscribedOnCompleted += slimNotifyCollectionChangedEventSubject.OnCompleted;
                        CreateCurrentCollection();
                        subscriptions = source
                            .SlimInitialStateAndChanged
                            .UseObserver((observer, e) =>
                                {
                                    try
                                    {
                                        CurrentCollection.ApplySlimChangeEvent(e);
                                    }
                                    catch (InvalidInformationException<T> ex)
                                    {
                                        observer.OnError(ex);
                                    }

                                    observer.OnNext(e);

                                    if (e.Action == NotifyCollectionChangedEventAction.InitialState)
                                    {
                                        isInitialStateArrived = true;
                                        onInitialStateArrived();
                                    }
                                })
                            .Subscribe(slimNotifyCollectionChangedEventSubject.OnNext, subscribedOnError, subscribedOnCompleted);
                        return;
                    }
                case RecommendedEventType.SimpleOne:
                    {
                        simpleNotifyCollectionChangedEventSubject = new Subject<SimpleNotifyCollectionChangedEvent<T>>();
                        subscribedOnError += simpleNotifyCollectionChangedEventSubject.OnError;
                        subscribedOnCompleted += simpleNotifyCollectionChangedEventSubject.OnCompleted;
                        CreateCurrentTaggedCollection();
                        subscriptions = source
                            .SimpleInitialStateAndChanged
                            .UseObserver((observer, e) =>
                            {
                                try
                                {
                                    CurrentTaggedCollection.ApplySimpleChangeEvent(e);
                                }
                                catch (InvalidInformationException<T> ex)
                                {
                                    observer.OnError(ex);
                                }

                                observer.OnNext(e);

                                if (e.Action == SimpleNotifyCollectionChangedEventAction.InitialState)
                                {
                                    isInitialStateArrived = true;
                                    onInitialStateArrived();
                                }
                            })
                            .Subscribe(simpleNotifyCollectionChangedEventSubject.OnNext, subscribedOnError, subscribedOnCompleted);
                        return;
                    }
                case RecommendedEventType.SlimSimpleOne:
                    {
                        slimSimpleNotifyCollectionChangedEventSubject = new Subject<SlimSimpleNotifyCollectionChangedEvent<T>>();
                        subscribedOnError += slimSimpleNotifyCollectionChangedEventSubject.OnError;
                        subscribedOnCompleted += slimSimpleNotifyCollectionChangedEventSubject.OnCompleted;
                        CreateCurrentTaggedCollection();
                        subscriptions = source
                            .SlimSimpleInitialStateAndChanged
                            .UseObserver((observer, e) =>
                            {
                                try
                                {
                                    CurrentTaggedCollection.ApplySlimSimpleChangeEvent(e);
                                }
                                catch (InvalidInformationException<T> ex)
                                {
                                    observer.OnError(ex);
                                }

                                observer.OnNext(e);

                                if (e.Action == SlimSimpleNotifyCollectionChangedEventAction.InitialState)
                                {
                                    isInitialStateArrived = true;
                                    onInitialStateArrived();
                                }
                            })
                            .Subscribe(slimSimpleNotifyCollectionChangedEventSubject.OnNext, subscribedOnError, subscribedOnCompleted);
                        return;
                    }
                default:
                    {
                        notifyCollectionChangedEventSubject = new Subject<INotifyCollectionChangedEvent<T>>();
                        subscribedOnError += notifyCollectionChangedEventSubject.OnError;
                        subscribedOnCompleted += notifyCollectionChangedEventSubject.OnCompleted;
                        CreateCurrentCollection();
                        subscriptions = source
                            .InitialStateAndChanged
                            .UseObserver((observer, e) =>
                            {
                                try
                                {
                                    CurrentCollection.ApplyChangeEvent(e);
                                }
                                catch (InvalidInformationException<T> ex)
                                {
                                    observer.OnError(ex);
                                }

                                observer.OnNext(e);

                                if (e.Action == NotifyCollectionChangedEventAction.InitialState)
                                {
                                    isInitialStateArrived = true;
                                    onInitialStateArrived();
                                }
                            })
                            .Subscribe(notifyCollectionChangedEventSubject.OnNext, subscribedOnError, subscribedOnCompleted);
                        return;
                    }
            }
        }

        void CreateCurrentCollection()
        {
            currentCollection = collectionCreator();
            if (currentCollection == null || currentCollection.Count != 0)
            {
                throw new InvalidOperationException();
            }
        }

        void CreateCurrentTaggedCollection()
        {
            currentTaggedCollection = taggedCollectionCreator();
            if (currentTaggedCollection == null || currentTaggedCollection.Count != 0)
            {
                throw new InvalidOperationException();
            }
        }

        TCollection currentCollection;
        public TCollection CurrentCollection
        {
            get
            {
                ThrowExceptionIfDisposed();

                return currentCollection;
            }
        }

        TTaggedCollection currentTaggedCollection;
        public TTaggedCollection CurrentTaggedCollection
        {
            get
            {
                ThrowExceptionIfDisposed();

                return currentTaggedCollection;
            }
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                ThrowExceptionIfDisposed();

                if (notifyCollectionChangedEventSubject != null)
                {
                    return new RecommendedEvent(true, false, false, false);
                }
                if (slimNotifyCollectionChangedEventSubject != null)
                {
                    return new RecommendedEvent(false, true, false, false);
                }
                if (simpleNotifyCollectionChangedEventSubject != null)
                {
                    return new RecommendedEvent(false, false, true, false);
                }
                if (slimSimpleNotifyCollectionChangedEventSubject != null)
                {
                    return new RecommendedEvent(false, false, false, true);
                }

                throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            ThrowExceptionIfDisposed();

            if (notifyCollectionChangedEventSubject != null)
            {
                return ObtainNotifyCollectionChanged();
            }
            if (slimNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimNotifyCollectionChanged().ToStatuses().InitialStateAndChanged;
            }
            if (simpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSimpleNotifyCollectionChanged().ToStatuses().InitialStateAndChanged;
            }
            if (slimSimpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimSimpleNotifyCollectionChanged().ToStatuses().InitialStateAndChanged;
            }

            throw Exceptions.UnpredictableSwitchCasePattern;
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            ThrowExceptionIfDisposed();

            if (notifyCollectionChangedEventSubject != null)
            {
                return ObtainNotifyCollectionChanged().ToStatuses().SlimInitialStateAndChanged;
            }
            if (slimNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimNotifyCollectionChanged();
            }
            if (simpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSimpleNotifyCollectionChanged().ToStatuses().SlimInitialStateAndChanged;
            }
            if (slimSimpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimSimpleNotifyCollectionChanged().ToStatuses().SlimInitialStateAndChanged;
            }

            throw Exceptions.UnpredictableSwitchCasePattern;
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            ThrowExceptionIfDisposed();

            if (notifyCollectionChangedEventSubject != null)
            {
                return ObtainNotifyCollectionChanged().ToStatuses().SimpleInitialStateAndChanged;
            }
            if (slimNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimNotifyCollectionChanged().ToStatuses().SimpleInitialStateAndChanged;
            }
            if (simpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSimpleNotifyCollectionChanged();
            }
            if (slimSimpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimSimpleNotifyCollectionChanged().ToStatuses().SimpleInitialStateAndChanged;
            }

            throw Exceptions.UnpredictableSwitchCasePattern;
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            ThrowExceptionIfDisposed();

            if (notifyCollectionChangedEventSubject != null)
            {
                return ObtainNotifyCollectionChanged().ToStatuses().SlimSimpleInitialStateAndChanged;
            }
            if (slimNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimNotifyCollectionChanged().ToStatuses().SlimSimpleInitialStateAndChanged;
            }
            if (simpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSimpleNotifyCollectionChanged().ToStatuses().SlimSimpleInitialStateAndChanged;
            }
            if (slimSimpleNotifyCollectionChangedEventSubject != null)
            {
                return ObtainSlimSimpleNotifyCollectionChanged();
            }

            throw Exceptions.UnpredictableSwitchCasePattern;
        }

        IObservable<INotifyCollectionChangedEvent<T>> ObtainNotifyCollectionChanged()
        {
            Contract.Requires<ArgumentNullException>(notifyCollectionChangedEventSubject != null);
            Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<INotifyCollectionChangedEvent<T>>(observer =>
            {
                if (isInitialStateArrived)
                {
                    observer.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(CurrentCollection.ToArray().ToReadOnly()));
                }
                if (error != null)
                {
                    observer.OnError(error);
                }
                if (isCompleted)
                {
                    observer.OnCompleted();
                }
                if (error != null || isCompleted)
                {
                    return System.Reactive.Disposables.Disposable.Empty;
                }

                return notifyCollectionChangedEventSubject.Subscribe(observer);
            });
        }

        IObservable<SlimNotifyCollectionChangedEvent<T>> ObtainSlimNotifyCollectionChanged()
        {
            Contract.Requires<ArgumentNullException>(slimNotifyCollectionChangedEventSubject != null);
            Contract.Ensures(Contract.Result<IObservable<SlimNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SlimNotifyCollectionChangedEvent<T>>(observer =>
            {
                if (isInitialStateArrived)
                {
                    var initialState = new SlimInitialState<T>(CurrentCollection.ToArray().ToReadOnly());
                    observer.OnNext(new SlimNotifyCollectionChangedEvent<T>(initialState));
                }
                if (error != null)
                {
                    observer.OnError(error);
                }
                if (isCompleted)
                {
                    observer.OnCompleted();
                }
                if (error != null || isCompleted)
                {
                    return System.Reactive.Disposables.Disposable.Empty;
                }

                return slimNotifyCollectionChangedEventSubject.Subscribe(observer);
            });
        }

        IObservable<SimpleNotifyCollectionChangedEvent<T>> ObtainSimpleNotifyCollectionChanged()
        {
            Contract.Requires<ArgumentNullException>(simpleNotifyCollectionChangedEventSubject != null);
            Contract.Ensures(Contract.Result<IObservable<SimpleNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                if (isInitialStateArrived)
                {
                    observer.OnNext(SimpleNotifyCollectionChangedEvent<T>.CreateInitialState(CurrentTaggedCollection.ToArray().ToReadOnly()));
                }
                if (error != null)
                {
                    observer.OnError(error);
                }
                if (isCompleted)
                {
                    observer.OnCompleted();
                }
                if (error != null || isCompleted)
                {
                    return System.Reactive.Disposables.Disposable.Empty;
                }

                return simpleNotifyCollectionChangedEventSubject.Subscribe(observer);
            });
        }

        IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> ObtainSlimSimpleNotifyCollectionChanged()
        {
            Contract.Requires<ArgumentNullException>(slimSimpleNotifyCollectionChangedEventSubject != null);
            Contract.Ensures(Contract.Result<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>>() != null);

            return Observable.Create<SlimSimpleNotifyCollectionChangedEvent<T>>(observer =>
            {
                if (isInitialStateArrived)
                {
                    observer.OnNext(SlimSimpleNotifyCollectionChangedEvent<T>.CreateInitialState(CurrentTaggedCollection.ToArray().ToReadOnly()));
                }
                if (error != null)
                {
                    observer.OnError(error);
                }
                if (isCompleted)
                {
                    observer.OnCompleted();
                }
                if (error != null || isCompleted)
                {
                    return System.Reactive.Disposables.Disposable.Empty;
                }

                return slimSimpleNotifyCollectionChangedEventSubject.Subscribe(observer);
            });
        }

        #region IDispose implementation
        private bool IsDisposed
        {
            get;
            set;
        }

        private void ThrowExceptionIfDisposed()
        {
            lock (this)
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().FullName + " has been already disposed.");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static bool TryDispose<TDispose>(TDispose disposingValue)
        {
            if (disposingValue == null) return false;
            var d = disposingValue as IDisposable;
            if (d == null) return false;
            d.Dispose();
            return true;
        }

        private void Dispose(bool isDisposing)
        {
            lock (this)
            {
                if (IsDisposed) return;
                if (isDisposing) OnDisposingUnManagedResources();
                OnDisposingManagedResources();
                IsDisposed = true;
            }
        }

        private void OnDisposingManagedResources()
        {
            TryDispose(subscriptions);
            TryDispose(notifyCollectionChangedEventSubject);
            TryDispose(slimNotifyCollectionChangedEventSubject);
            TryDispose(simpleNotifyCollectionChangedEventSubject);
            TryDispose(slimSimpleNotifyCollectionChangedEventSubject);
        }

        private void OnDisposingUnManagedResources()
        {

        }
        #endregion

        ~DelegationCollectionStatuses()
        {
            Dispose(false);
        }
    }
}
