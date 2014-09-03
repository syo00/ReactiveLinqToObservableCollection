using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Subjects
{
    sealed class SlimSimplePublishSubject<T> : SubjectBase<SlimSimpleNotifyCollectionChangedEvent<T>>
    {
        protected override ISubject<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSubject()
        {
            return new SlimSimplePublishSubjectCore<T>();
        }

        SlimSimplePublishSubjectCore<T> CoreSubject
        {
            get
            {
                return (SlimSimplePublishSubjectCore<T>)base.CurrentSubject;
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
