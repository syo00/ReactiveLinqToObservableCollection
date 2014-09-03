using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    sealed class SlimReplaced<T>
    {
        public SlimReplaced(int startingIndex, int oldItemsCount, IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentException>(startingIndex >= 0);
            Contract.Requires<ArgumentException>(oldItemsCount >= 1);
            Contract.Requires<ArgumentNullException>(newItems != null);

            this.newItems = newItems;
            this.startingIndex = startingIndex;
            this.oldItemsCount = oldItemsCount;
        }

        readonly int startingIndex;
        public int StartingIndex
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return startingIndex;
            }
        }

        readonly IReadOnlyList<T> newItems;
        public IReadOnlyList<T> NewItems
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

                return newItems;
            }
        }

        readonly int oldItemsCount;
        public int OldItemsCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 1);

                return oldItemsCount;
            }
        }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Replace.ToString()
                + " (index: " + StartingIndex
                + ", items: "
                + OldItemsCount
                + " items -> "
                + Converters.ListToString(NewItems, 2)
                + ")";
        }
    }
}
