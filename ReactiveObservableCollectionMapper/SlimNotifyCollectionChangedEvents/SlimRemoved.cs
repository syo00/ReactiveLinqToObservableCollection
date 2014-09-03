using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    sealed class SlimRemoved
    {
        public SlimRemoved(int startingIndex, int itemsCount)
        {
            Contract.Requires<ArgumentException>(startingIndex >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);

            this.startingIndex = startingIndex;
            this.itemsCount = itemsCount;
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

        readonly int itemsCount;
        public int ItemsCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 1);

                return itemsCount;
            }
        }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Remove.ToString()
                + " (index: " + StartingIndex
                + ", items count: "
                + ItemsCount
                + ")";
        }
    }
}
