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
    sealed class Moved<T> : IMoved<T>
    {
        public Moved(IReadOnlyList<T> items, int oldStartingIndex, int newStartingIndex)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(newStartingIndex >= 0);

            this.Items = items;
            this.oldStartingIndex = oldStartingIndex;
            this.newStartingIndex = newStartingIndex;
        }

        public IReadOnlyList<T> Items { get; private set; }

        // 不明な場合は -1 を返す
        readonly int oldStartingIndex;
        public int OldStartingIndex
        {
            get
            {
                return oldStartingIndex;
            }
        }

        readonly int newStartingIndex;
        public int NewStartingIndex
        {
            get
            {
                return newStartingIndex;
            }
        }

        public override string ToString()
        {
            return NotifyCollectionChangedEventAction.Move.ToString()
                + " (index: " + OldStartingIndex
                + " -> "
                + NewStartingIndex
                + ", items: "
                + Converters.ListToString(Items, 3)
                + ")";
        }
    }
}
