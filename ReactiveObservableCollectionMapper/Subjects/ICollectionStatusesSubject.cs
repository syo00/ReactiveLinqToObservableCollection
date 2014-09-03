using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Subjects
{
    [ContractClass(typeof(ICollectionStatusesSubjectContract<>))]
    public interface ICollectionStatusesSubject<T> : ICollectionStatuses<T>, IObserver<INotifyCollectionChangedEvent<T>>
    {

    }

    [ContractClassFor(typeof(ICollectionStatusesSubject<>))]
    abstract class ICollectionStatusesSubjectContract<T> : ICollectionStatusesSubject<T>
    {
        public IObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(INotifyCollectionChangedEvent<T> value)
        {
            throw new NotImplementedException();
        }
    }
}
