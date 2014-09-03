using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.OrderingComparers
{
    [ContractClass(typeof(IIOrderingComparer<>))]
    internal interface IOrderingComparer<T>
    {
        int Compare(T left, T right);
        IOrderedEnumerable<TSource> Order<TSource>(IEnumerable<TSource> mapped, Func<TSource, T> keySelector);
        IOrderedEnumerable<TSource> Order<TSource>(IOrderedEnumerable<TSource> mapped, Func<TSource, T> keySelector);
    }

    #region IOrderingComparerOrder contract
    [ContractClassFor(typeof(IOrderingComparer<>))]
    abstract class IIOrderingComparer<T> : IOrderingComparer<T>
    {
        public int Compare(T left, T right)
        {
            throw new NotImplementedException();
        }

        public int Compare<TSource>(TSource left, TSource right, Func<TSource, T> keySelector)
        {
            Contract.Requires<ArgumentNullException>(keySelector != null);

            throw new NotImplementedException();
        }

        public IOrderedEnumerable<T> MapCurrent(IEnumerable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }

        public IOrderedEnumerable<TSource> Order<TSource>(IEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Ensures(Contract.Result<IOrderedEnumerable<TSource>>() != null);

            throw new NotImplementedException();
        }

        public IOrderedEnumerable<T> MapCurrent(IOrderedEnumerable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            throw new NotImplementedException();
        }

        public IOrderedEnumerable<TSource> Order<TSource>(IOrderedEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Ensures(Contract.Result<IOrderedEnumerable<TSource>>() != null);

            throw new NotImplementedException();
        }
    }
    #endregion

    internal static class OrderingComparer
    {
        public static int Compare<T, TSource>(this IOrderingComparer<T> source, TSource left, TSource right, Func<TSource, T> keySelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);

            return source.Compare(keySelector(left), keySelector(right));
        }

        public static IOrderedEnumerable<T> Order<T>(this IOrderingComparer<T> source, IEnumerable<T> mapped)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(mapped != null);
            Contract.Ensures(Contract.Result<IOrderedEnumerable<T>>() != null);

            return source.Order(mapped, x => x);
        }

        public static IOrderedEnumerable<T> Order<T>(this IOrderingComparer<T> source, IOrderedEnumerable<T> mapped)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(mapped != null);
            Contract.Ensures(Contract.Result<IOrderedEnumerable<T>>() != null);

            return source.Order(mapped, x => x);
        }
    }
}
