using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IAddedContract<>))]
    public interface IAdded<out T>
    {
        IReadOnlyList<T> Items { get; }
        int StartingIndex { get; }
    }

    [ContractClassFor(typeof(IAdded<>))]
    abstract class IAddedContract<T> : IAdded<T>
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
