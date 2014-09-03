using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IRemovedContract<>))]
    public interface IRemoved<out T>
    {
        IReadOnlyList<T> Items { get; }
        int StartingIndex { get; }
    }

    [ContractClassFor(typeof(IRemoved<>))]
    abstract class IRemovedContract<T> : IRemoved<T>
    {
        public IReadOnlyList<T> Items
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
