using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IMovedContract<>))]
    public interface IMoved<out T>
    {
        IReadOnlyList<T> Items { get; }
        int OldStartingIndex { get; }
        int NewStartingIndex { get; }
    }

    [ContractClassFor(typeof(IMoved<>))]
    abstract class IMovedContract<T> : IMoved<T>
    {
        public IReadOnlyList<T> Items
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

                throw new NotImplementedException();
            }
        }

        public int OldStartingIndex
        {
            get { throw new NotImplementedException(); }
        }

        public int NewStartingIndex
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                throw new NotImplementedException();
            }
        }
    }
}
