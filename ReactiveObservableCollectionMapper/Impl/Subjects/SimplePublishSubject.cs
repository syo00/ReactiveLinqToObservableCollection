using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    class SimplePublishSubject<T> : SubjectBase<SimpleNotifyCollectionChangedEvent<T>>
    {
        protected override ISubject<SimpleNotifyCollectionChangedEvent<T>> CreateSubject()
        {
            return new SimplePublishSubjectCore<T>();
        }

        SimplePublishSubjectCore<T> CoreSubject
        {
            get
            {
                return (SimplePublishSubjectCore<T>)base.CurrentSubject;
            }
        }

        public IReadOnlyList<Tagged<T>> CurrentItems
        {
            get
            {
                return CoreSubject.CurrentItems;
            }
        }
    }
}
