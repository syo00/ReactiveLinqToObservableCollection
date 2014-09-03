using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Kirinji.LinqToObservableCollection
{
    [ContractClass(typeof(IInitialStateContract<>))]
    public interface IInitialState<out T>
    {
        IReadOnlyList<T> Items { get; }
    }

    [ContractClassFor(typeof(IInitialState<>))]
    abstract class IInitialStateContract<T> : IInitialState<T>
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
