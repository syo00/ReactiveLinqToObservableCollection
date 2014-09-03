using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading;

namespace Kirinji.LinqToObservableCollection
{
    public static partial class CollectionStatuses
    {
        public static GeneratedObservableCollection<T> ToObservableCollection<T>(this ICollectionStatuses<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<GeneratedObservableCollection<T>>() != null);

            return source
                .ToInstance()
                .ToObservableCollection();
        }
    }
}
