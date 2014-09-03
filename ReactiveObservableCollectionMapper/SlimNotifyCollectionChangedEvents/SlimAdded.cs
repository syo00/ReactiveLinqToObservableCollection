using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    sealed class SlimAdded<T>
    {
        public SlimAdded(IReadOnlyList<T> items, int startingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentException>(startingIndex >= 0);

            this.items = items;
            this.startingIndex = startingIndex;
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

        readonly IReadOnlyList<T> items;
        public IReadOnlyList<T> Items
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

                return items;
            }
        }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Add.ToString()
                + " (index: " + StartingIndex
                + ", count: "
                + Converters.ListToString(Items, 3)
                + ")";
        }
    }
}
