using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    sealed class InitialState<T> : IInitialState<T>
    {
        public InitialState(IReadOnlyList<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);

            this.Items = items;
        }

        public IReadOnlyList<T> Items { get; private set; }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.InitialState.ToString()
                + " (items: "
                + Converters.ListToString(Items, 4)
                + ")";
        }
    }
}
