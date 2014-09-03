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
        public static ICollectionStatuses<T> Merge<T>(this IEnumerable<ICollectionStatuses<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(source, x => x != null));
            Contract.Ensures(Contract.Result<ICollectionStatuses<T>>() != null);

            return CollectionStatuses.Defer(
                () => CollectionStatuses.Return<ICollectionStatuses<T>>(source.ToArray())
                )
                .Flatten();
        }
    }
}
