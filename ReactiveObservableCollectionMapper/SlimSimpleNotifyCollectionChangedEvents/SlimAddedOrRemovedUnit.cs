using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents
{
    internal class SlimAddedOrRemovedUnit<T>
    {
        /// <summary>Added</summary>
        public SlimAddedOrRemovedUnit(Tagged<T> item, int index)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            this.Type = SlimAddOrRemoveUnitType.Add;
            this.item = item;
            this.index = index;
        }

        /// <summary>Removed</summary>
        public SlimAddedOrRemovedUnit(int index)
        {
            this.Type = SlimAddOrRemoveUnitType.Add;
            this.index = index;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(item != null);
        }

        public SlimAddOrRemoveUnitType Type { get; private set; }

        readonly Tagged<T> item;
        public Tagged<T> Item
        {
            get
            {
                Contract.Ensures(Contract.Result<Tagged<T>>() != null || Type == SlimAddOrRemoveUnitType.Remove);

                return item;
            }
        }

        readonly int index;
        public int Index
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return index;
            }
        }

        public override string ToString()
        {
            return Type.ToString() 
                + " (index: " + Index
                + ", item: "
                + ObjectEx.ToString(Item)
                + ")";
        }
    }

    internal enum SlimAddOrRemoveUnitType
    {
        Add,
        Remove,
    }
}
