using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(ICollectionStatusesAttachedContract<>))]
    public interface ICollectionStatusesAttached<out T>
    {
        ICollectionStatuses<T> Statuses { get; }
    }

    [ContractClassFor(typeof(ICollectionStatusesAttached<>))]
    abstract class ICollectionStatusesAttachedContract<T> : ICollectionStatusesAttached<T>
    {
        public ICollectionStatuses<T> Statuses
        {
            get
            {
                Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

                throw new NotImplementedException();
            }
        }
    }
}
