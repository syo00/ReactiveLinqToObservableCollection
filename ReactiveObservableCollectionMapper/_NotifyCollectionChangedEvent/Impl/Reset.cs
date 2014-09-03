using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    sealed class Reset<T> : IReset<T>
    {
        public Reset(IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);

            this.Items = newItems;
        }

        public IReadOnlyList<T> Items { get; private set; }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Reset.ToString()
                + " (items: "
                + Converters.ListToString(Items, 4)
                + ")";
        }
    }
}
