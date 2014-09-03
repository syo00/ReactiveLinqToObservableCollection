using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    class PublishSubject<T> : SubjectBase<INotifyCollectionChangedEvent<T>>
    {
        protected override ISubject<INotifyCollectionChangedEvent<T>> CreateSubject()
        {
            return new PublishSubjectCore<T>();
        }
    }
}
