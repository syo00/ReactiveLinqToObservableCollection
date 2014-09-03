using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    class PublishSubjectCore<T> : ISubject<INotifyCollectionChangedEvent<T>>
    {
        Subject<INotifyCollectionChangedEvent<T>> stream = new Subject<INotifyCollectionChangedEvent<T>>();
        DelegationCollectionStatuses<T, List<T>, List<Tagged<T>>> core;

        public PublishSubjectCore()
        {
            this.core = new DelegationCollectionStatuses<T, List<T>, List<Tagged<T>>>(stream.ToStatuses(), () => new List<T>(), () => new List<Tagged<T>>());
        }

        public void OnCompleted()
        {
            stream.OnCompleted();
        }

        public void OnError(Exception error)
        {
            stream.OnError(error);
        }

        public void OnNext(INotifyCollectionChangedEvent<T> value)
        {
            stream.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            return core.InitialStateAndChanged.Subscribe(observer);
        }
    }
}
