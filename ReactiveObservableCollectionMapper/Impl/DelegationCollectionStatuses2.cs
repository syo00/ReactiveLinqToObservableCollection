using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System.Collections.ObjectModel;

namespace Kirinji.LinqToObservableCollection.Impl
{
    // イベントに応じて  MultiValuesObservableCollection の変更を任せるクラス
    sealed class DelegationCollectionStatuses<T> : CollectionStatuses<T>, IDisposable
    {
        IDisposable subscriptions;
        Exception error;
        bool isCompleted;
        bool isInitialStateArrived;
        Subject<INotifyCollectionChangedEvent<T>> notifyCollectionChangedEventSubject;
        Subject<SlimNotifyCollectionChangedEvent<T>> slimNotifyCollectionChangedEventSubject;

        public DelegationCollectionStatuses(CollectionStatuses<T> source, Func<MultiValuesObservableCollection<T>> collectionCreator, Action onInitialStateArrived, Action<Exception> onError, Action onCompleted)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(collectionCreator != null);
            Contract.Requires<ArgumentNullException>(onInitialStateArrived != null);
            Contract.Requires<ArgumentNullException>(onError != null);
            Contract.Requires<ArgumentNullException>(onCompleted != null);
            
            this.currentCollection = collectionCreator();
            if(this.currentCollection == null || this.currentCollection.Count != 0)
            {
                throw new InvalidOperationException();
            }

            Apply(source, this.currentCollection, onInitialStateArrived, onError, onCompleted);
        }

        void Apply(CollectionStatuses<T> source, MultiValuesObservableCollection<T> collection, Action onInitialStateArrived, Action<Exception> onError, Action onCompleted)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(collection != null);
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
                        subscriptions = source
                            .SlimInitialStateAndChanged
                            .UseObserver((observer, e) =>
                            {
                                try
                                {
                                    collection.ApplySlimChangeEvent(e);
                                }
                                catch (InvalidInformationException<T> ex)
                                {
                                    observer.OnError(ex);
                                }

                                observer.OnNext(e);

                                if(e.Action == NotifyCollectionChangedEventAction.InitialState)
                                {
                                    isInitialStateArrived = true;
                                    onInitialStateArrived();
                                }
                            })
                            .Subscribe(slimNotifyCollectionChangedEventSubject.OnNext, subscribedOnError, subscribedOnCompleted);
                        return;
                    }
                default:
                    {
                        notifyCollectionChangedEventSubject = new Subject<INotifyCollectionChangedEvent<T>>();
                        subscribedOnError += notifyCollectionChangedEventSubject.OnError;
                        subscribedOnCompleted += notifyCollectionChangedEventSubject.OnCompleted;
                        subscriptions = source
                            .InitialStateAndChanged
                            .UseObserver((observer, e) =>
                            {
                                try
                                {
                                    collection.ApplyChangeEvent(e);
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

        readonly MultiValuesObservableCollection<T> currentCollection;
        public MultiValuesObservableCollection<T> CurrentCollection
        {
            get
            {
                Contract.Ensures(Contract.Result<MultiValuesObservableCollection<T>>() != null);

                ThrowExceptionIfDisposed();

                return currentCollection;
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
