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
    sealed class Removed<T> : IRemoved<T>
    {
        public Removed(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);

            this.items = items;
            this.StartingIndex = startingIndex;
        }

        readonly IReadOnlyList<T> items;
        public IReadOnlyList<T> Items
        {
            get
            {
                return items;
            }
        }

        // 不明な場合は -1 を返す
        public int StartingIndex { get; private set; }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Remove.ToString()
                + " (index: " + StartingIndex
                + ", items: "
                + Converters.ListToString(Items, 3)
                + ")";
        }
    }
}
