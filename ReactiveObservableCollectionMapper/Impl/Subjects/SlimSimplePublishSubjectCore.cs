using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    sealed class SlimSimplePublishSubjectCore<T> : ISubject<SlimSimpleNotifyCollectionChangedEvent<T>>
    {
        Subject<SlimSimpleNotifyCollectionChangedEvent<T>> stream = new Subject<SlimSimpleNotifyCollectionChangedEvent<T>>();
        DelegationCollectionStatuses<T, List<T>, List<Tagged<T>>> core;

        public SlimSimplePublishSubjectCore()
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

        public void OnNext(SlimSimpleNotifyCollectionChangedEvent<T> value)
        {
            stream.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<SlimSimpleNotifyCollectionChangedEvent<T>> observer)
        {
            return core.SlimSimpleInitialStateAndChanged.Subscribe(observer);
        }

        public IReadOnlyList<Tagged<T>> CurrentItems
        {
            get
            {
                return core.CurrentTaggedCollection;
            }
        }
    }
}
