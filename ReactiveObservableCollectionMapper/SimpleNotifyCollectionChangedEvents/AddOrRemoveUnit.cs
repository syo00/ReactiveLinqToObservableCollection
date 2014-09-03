using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents
{
    internal class AddedOrRemovedUnit<T>
    {
        public AddedOrRemovedUnit(AddOrRemoveUnitType type, Tagged<T> item, int index)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            this.Type = type;
            this.item = item;
            this.Index = index;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(item != null);
        }

        public AddOrRemoveUnitType Type { get; private set; }

        readonly Tagged<T> item;
        public Tagged<T> Item
        {
            get
            {
                Contract.Ensures(Contract.Result<Tagged<T>>() != null);

                return item;
            }
        }

        public int Index { get; private set; }

        public override string ToString()
        {
            return Type.ToString() 
                + " (index: " + Index
                + ", item: "
                + ObjectEx.ToString(Item)
                + ")";
        }
    }

    internal enum AddOrRemoveUnitType
    {
        Add,
        Remove,
    }
}
