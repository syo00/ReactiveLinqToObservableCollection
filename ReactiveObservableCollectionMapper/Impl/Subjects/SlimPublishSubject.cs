using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    class SlimPublishSubject<T> : SubjectBase<SlimNotifyCollectionChangedEvent<T>>
    {
        protected override ISubject<SlimNotifyCollectionChangedEvent<T>> CreateSubject()
        {
            return new SlimPublishSubjectCore<T>();
        }
    }
}
