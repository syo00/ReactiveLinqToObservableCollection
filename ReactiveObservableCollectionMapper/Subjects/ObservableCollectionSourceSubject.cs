using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Subjects
{
    public static class CollectionStatusesSubject
    {
        public static ISubject<INotifyCollectionChangedEvent<T>> ToSubject<T>(this ICollectionStatusesSubject<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return new NotifyCollectionChangedEventSubject<T>(source);
        }
    }
}
