using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.OrderingComparers
{
    internal class OrderingComparer<T, TKey> : IOrderingComparer<T>
    {
        public OrderingComparer(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.keySelector = keySelector;
            this.comparer = comparer;
            this.Descending = descending;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(keySelector != null);
            Contract.Invariant(comparer != null);
        }


        Func<T, TKey> keySelector;
        public Func<T, TKey> KeySelector
        {
            get
            {
                Contract.Ensures(Contract.Result<Func<T, TKey>>() != null);

                return keySelector;
            }
        }

        IComparer<TKey> comparer;
        public IComparer<TKey> Comparer
        {
            get
            {
                Contract.Ensures(Contract.Result<IComparer<TKey>>() != null);

                return comparer;
            }
        }

        public bool Descending { get; private set; }

        public int Compare(T left, T right)
        {
            var ascendingResult = Comparer.Compare(KeySelector(left), KeySelector(right));
            return Descending ? (ascendingResult * -1) : ascendingResult;
        }

        public IOrderedEnumerable<TSource> Order<TSource>(IEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            if (Descending)
            {
                return source.OrderByDescending(x => KeySelector(keySelector(x)), Comparer);
            }
            else
            {
                return source.OrderBy(x => KeySelector(keySelector(x)), Comparer);
            }
        }

        public IOrderedEnumerable<TSource> Order<TSource>(IOrderedEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            if (Descending)
            {
                return source.ThenByDescending(x => KeySelector(keySelector(x)), Comparer);
            }
            else
            {
                return source.ThenBy(x => KeySelector(keySelector(x)), Comparer);
            }
        }
    }
}
