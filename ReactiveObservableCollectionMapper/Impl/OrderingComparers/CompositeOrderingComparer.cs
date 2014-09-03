using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;

namespace Kirinji.LinqToObservableCollection.Impl.OrderingComparers
{
    internal class CompositeOrderingComparer<T> : IOrderingComparer<T>
    {
        readonly IReadOnlyList<IOrderingComparer<T>> rawOrders;
        readonly bool reverse;
        readonly Lazy<IReadOnlyList<IOrderingComparer<T>>> orders;

        public CompositeOrderingComparer(IReadOnlyList<IOrderingComparer<T>> orders)
            : this(orders, false)
        {
            Contract.Requires<ArgumentNullException>(orders != null);
            Contract.Requires<ArgumentException>(orders.Count >= 1);
            Contract.Requires<ArgumentException>(Contract.ForAll(orders, order => order != null));
        }

        private CompositeOrderingComparer(IReadOnlyList<IOrderingComparer<T>> orders, bool reverse)
        {
            Contract.Requires<ArgumentNullException>(orders != null);
            Contract.Requires<ArgumentException>(orders.Count >= 1);
            Contract.Requires<ArgumentException>(Contract.ForAll(orders, order => order != null));

            this.rawOrders = orders;
            this.reverse = reverse;
            this.orders = new Lazy<IReadOnlyList<IOrderingComparer<T>>>(() => reverse ? orders.Reverse().ToArray() : orders);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(orders != null);
            Contract.Invariant(orders.Value != null);
            Contract.Invariant(Contract.ForAll(orders.Value, order => order != null));
        }

        public CompositeOrderingComparer<T> CombineToFirst(params IOrderingComparer<T>[] orders)
        {
            Contract.Requires<ArgumentNullException>(orders != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(orders, o => o != null));
            Contract.Ensures(Contract.Result<CompositeOrderingComparer<T>>() != null);

            return new CompositeOrderingComparer<T>(orders.Concat(Orders).ToArray());
        }

        public CompositeOrderingComparer<T> CombineToLast(params IOrderingComparer<T>[] orders)
        {
            Contract.Requires<ArgumentNullException>(orders != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(orders, o => o != null));
            Contract.Ensures(Contract.Result<CompositeOrderingComparer<T>>() != null);

            return new CompositeOrderingComparer<T>(Orders.Concat(orders).ToArray());
        }

        public CompositeOrderingComparer<T> Reverse()
        {
            Contract.Ensures(Contract.Result<CompositeOrderingComparer<T>>() != null);

            return new CompositeOrderingComparer<T>(rawOrders, !reverse);
        }

        private IReadOnlyList<IOrderingComparer<T>> Orders
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<IOrderingComparer<T>>>() != null);

                return orders.Value;
            }
        }

        public int Compare(T left, T right)
        {
            foreach (var order in Orders)
            {
                var result = order.Compare(left, right);
                if (result != 0) return result;
            }
            return 0;
        }

        public IOrderedEnumerable<TSource> Order<TSource>(IEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            var result = Orders[0].Order(source, keySelector);

            foreach (var order in Orders.Skip(1))
            {
                result = order.Order(result, keySelector);
            }

            return result;
        }

        public IOrderedEnumerable<TSource> Order<TSource>(IOrderedEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            var result = source;

            foreach (var order in Orders)
            {
                result = order.Order(result, keySelector);
            }

            return result;
        }
    }
}
