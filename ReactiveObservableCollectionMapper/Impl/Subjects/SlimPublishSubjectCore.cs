using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    class SlimPublishSubjectCore<T> : ISubject<SlimNotifyCollectionChangedEvent<T>>
    {
        Subject<SlimNotifyCollectionChangedEvent<T>> stream = new Subject<SlimNotifyCollectionChangedEvent<T>>();
        DelegationCollectionStatuses<T> core;

        public SlimPublishSubjectCore()
        {
            this.core = new DelegationCollectionStatuses<T>(stream.ToStatuses());
        }

        public void OnCompleted()
        {
            stream.OnCompleted();
        }

        public void OnError(Exception error)
        {
            stream.OnError(error);
        }

        public void OnNext(SlimNotifyCollectionChangedEvent<T> value)
        {
            stream.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<SlimNotifyCollectionChangedEvent<T>> observer)
        {
            return core.SlimInitialStateAndChanged.Subscribe(observer);
        }
    }
}
