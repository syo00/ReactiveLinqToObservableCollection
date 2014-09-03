using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    sealed class SlimMoved
    {
        public SlimMoved(int oldStartingIndex, int newStartingIndex, int itemsCount)
        {
            Contract.Requires<ArgumentException>(oldStartingIndex >= 0);
            Contract.Requires<ArgumentException>(newStartingIndex >= 0);
            Contract.Requires<ArgumentException>(itemsCount >= 1);

            this.oldStartingIndex = oldStartingIndex;
            this.newStartingIndex = newStartingIndex;
            this.itemsCount = itemsCount;
        }

        readonly int oldStartingIndex;
        public int OldStartingIndex
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return oldStartingIndex;
            }
        }

        readonly int newStartingIndex;
        public int NewStartingIndex
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return newStartingIndex;
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
            return NotifyCollectionChangedEventAction.Move.ToString()
                + " (index: " + OldStartingIndex
                + " -> "
                + NewStartingIndex
                + ", items count: "
                + ItemsCount
                + ")";
        }
    }
}
