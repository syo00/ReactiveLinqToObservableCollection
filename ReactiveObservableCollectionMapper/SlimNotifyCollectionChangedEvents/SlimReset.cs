using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    class SlimReset<T>
    {
        public SlimReset(IReadOnlyList<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);

            this.items = newItems;
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
            return NotifyCollectionChangedEventAction.Reset.ToString();
        }
    }
}
