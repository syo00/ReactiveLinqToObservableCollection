using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Subjects
{
    class NotifyCollectionChangedEventSubject<T> : ISubject<INotifyCollectionChangedEvent<T>>
    {
        readonly ICollectionStatusesSubject<T> source;

        public NotifyCollectionChangedEventSubject(ICollectionStatusesSubject<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
        }

        public void OnCompleted()
        {
            source.OnCompleted();
        }

        public void OnError(Exception error)
        {
            source.OnError(error);
        }

        public void OnNext(INotifyCollectionChangedEvent<T> value)
        {
            source.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<INotifyCollectionChangedEvent<T>> observer)
        {
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            return source.InitialStateAndChanged.Subscribe(observer);
        }
    }
}
