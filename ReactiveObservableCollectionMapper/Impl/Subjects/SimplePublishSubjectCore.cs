using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    class SimplePublishSubjectCore<T> : ISubject<SimpleNotifyCollectionChangedEvent<T>>
    {
        Subject<SimpleNotifyCollectionChangedEvent<T>> stream = new Subject<SimpleNotifyCollectionChangedEvent<T>>();
        DelegationCollectionStatuses<T> core;

        public SimplePublishSubjectCore()
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

        public void OnNext(SimpleNotifyCollectionChangedEvent<T> value)
        {
            stream.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<SimpleNotifyCollectionChangedEvent<T>> observer)
        {
            return core.SimpleInitialStateAndChanged.Subscribe(observer);
        }

        public ReadOnlyTaggedCollection<T> CurrentItems
        {
            get
            {
                return core.CurrentCollection;
            }
        }
    }
}
