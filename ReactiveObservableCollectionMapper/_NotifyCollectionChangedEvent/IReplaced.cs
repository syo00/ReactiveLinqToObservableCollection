using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IReplacedContract<>))]
    public interface IReplaced<out T>
    {
        IReadOnlyList<T> NewItems { get; }
        IReadOnlyList<T> OldItems { get; }
        int StartingIndex { get; }
    }

    [ContractClassFor(typeof(IReplaced<>))]
    abstract class IReplacedContract<T> : IReplaced<T>
    {

        public IReadOnlyList<T> NewItems
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

                throw new NotImplementedException();
            }
        }

        public IReadOnlyList<T> OldItems
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

                throw new NotImplementedException();
            }
        }

        public int StartingIndex
        {
            get { throw new NotImplementedException(); }
        }
    }
}
