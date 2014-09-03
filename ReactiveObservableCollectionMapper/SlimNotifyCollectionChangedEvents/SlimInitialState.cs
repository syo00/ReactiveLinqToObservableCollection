using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents
{
    class SlimInitialState<T>
    {
        public SlimInitialState(IReadOnlyList<T> items)
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
