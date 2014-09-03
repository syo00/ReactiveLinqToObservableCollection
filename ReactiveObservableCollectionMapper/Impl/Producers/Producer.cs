using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    /// <summary>Provides easier way to create new observable sequences and receive OnError and OnComleted events.</summary>
    internal abstract class Producer<T>
    {
        protected abstract IDisposable SubscribeCore(ProducerObserver<T> observer);

        protected virtual void OnNext(T value)
        {
            
        }

        protected virtual void OnError(Exception e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
        }

        protected virtual void OnCompleted()
        {
            
        }

        // can't call this method more than 2 times
        public IDisposable Subscribe(IObserver<T> observer)
        {
            Contract.Requires<InvalidOperationException>(!IsOnceSubscribed);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            IsOnceSubscribed = true;

            var resultSubscription = new System.Reactive.Disposables.SerialDisposable();
            resultSubscription.Disposable = Observable.Create<T>(innerObserver =>
                 {
                     var newObserver = Observer.Create<T>(x =>
                     {
                         OnNext(x);
                         innerObserver.OnNext(x);
                     }, ex =>
                     {
                         try
                         {
                             OnError(ex);
                         }
                         finally
                         {
                             try
                             {
                                 innerObserver.OnError(ex);
                             }
                             finally
                             {
                                 resultSubscription.Dispose();
                             }
                         }
                     }, () =>
                     {
                         try
                         {
                             OnCompleted();
                         }
                         finally
                         {
                             try
                             {
                                 innerObserver.OnCompleted();
                             }
                             finally
                             {
                                 resultSubscription.Dispose();
                             }
                         }
                     });

                     var producerObserver = new ProducerObserver<T>(newObserver);
                     return SubscribeCore(producerObserver);
                 })
                 .Subscribe(observer);
            return resultSubscription;
        }

        public bool IsOnceSubscribed { get; private set; }
    }
}
