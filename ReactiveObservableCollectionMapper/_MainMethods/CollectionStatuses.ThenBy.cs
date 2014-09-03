using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static IOrderedCollectionStatuses<T> ThenBy<T, TKey>(this IOrderedCollectionStatuses<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Ensures(Contract.Result<IOrderedCollectionStatuses<T>>() != null);

            return source
                .ToOrderedInstance()
                .CreateThenBy(new OrderingComparer<T, TKey>(keySelector, comparer ?? Comparer<TKey>.Default, false));
        }

        public static IOrderedCollectionStatuses<T> ThenByDescending<T, TKey>(this IOrderedCollectionStatuses<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Ensures(Contract.Result<IOrderedCollectionStatuses<T>>() != null);

            return source
                .ToOrderedInstance()
                .CreateThenBy(new OrderingComparer<T, TKey>(keySelector, comparer ?? Comparer<TKey>.Default, true));
        }
    }
}
