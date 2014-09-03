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
    sealed class Replaced<T> : IReplaced<T>
    {
        public Replaced(IReadOnlyList<T> oldItems, IReadOnlyList<T> newItems, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(oldItems != null);
            Contract.Requires<ArgumentNullException>(newItems != null);

            this.oldItems = oldItems;
            this.newItems = newItems;
            this.StartingIndex = startingIndex;
        }

        readonly IReadOnlyList<T> oldItems;
        public IReadOnlyList<T> OldItems
        {
            get
            {
                return oldItems;
            }
        }

        readonly IReadOnlyList<T> newItems;
        public IReadOnlyList<T> NewItems
        {
            get
            {
                return newItems;
            }
        }

        // 不明な場合は -1 を返す
        public int StartingIndex { get; private set; }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Replace.ToString()
                + " (index: " + StartingIndex
                + ", items: "
                + Converters.ListToString(OldItems, 2)
                + " -> "
                + Converters.ListToString(NewItems, 2)
                + ")";
        }
    }
}
