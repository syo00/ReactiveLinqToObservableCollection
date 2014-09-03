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
        public static IOrderedCollectionStatuses<T> OrderBy<T, TKey>(this ICollectionStatuses<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .CreateOrdered(keySelector, comparer ?? Comparer<TKey>.Default, false);
        }

        public static IOrderedCollectionStatuses<T> OrderByDecsending<T, TKey>(this ICollectionStatuses<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return source
                .ToInstance()
                .CreateOrdered(keySelector, comparer ?? Comparer<TKey>.Default, true);
        }
    }
}
