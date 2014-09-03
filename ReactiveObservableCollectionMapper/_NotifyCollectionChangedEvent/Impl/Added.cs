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
    sealed class Added<T> : IAdded<T>
    {
        public Added(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);

            this.Items = items;
            this.StartingIndex = startingIndex;
        }

        public IReadOnlyList<T> Items { get; private set; }

        // 不明な場合は -1 を返す
        public int StartingIndex { get; private set; }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Add.ToString()
                + " (index: " + StartingIndex
                + ", items: "
                + Converters.ListToString(Items, 3)
                + ")";
        }
    }
}
