using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IResetContract<>))]
    public partial interface IReset<out T>
    {
        IReadOnlyList<T> Items { get; }
    }

    [ContractClassFor(typeof(IReset<>))]
    abstract class IResetContract<T> : IReset<T>
    {
        public IReadOnlyList<T> Items
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

                throw new NotImplementedException();
            }
        }
    }
}
