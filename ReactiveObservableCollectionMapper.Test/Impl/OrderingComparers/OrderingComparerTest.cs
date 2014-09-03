using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System.Collections.Generic;
using System.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Impl.OrderingComparers
{
    // OrderingComparer<T, TKey> と OrderingComparer の拡張メソッドの両方のテストを行っている
    [TestClass]
    public class OrderingComparerTest
    {
        [TestMethod]
        public void AscendingTest()
        {
            var order = new OrderingComparer<string, string>(x => x, Comparer<string>.Default, false);

            order.Compare("a", "z").Is(i => i < 0);
            order.Compare("a", "a").Is(0);
            order.Compare("z", "a").Is(i => i > 0);

            order.Compare("za", "az", x => x.Last().ToString()).Is(i => i < 0);
            order.Compare("aa", "za", x => x.Last().ToString()).Is(0);
            order.Compare("az", "za", x => x.Last().ToString()).Is(i => i > 0);

            order.Order(new[] { "a", "c", "b" }).Is("a", "b", "c");
            order.Order(new[] { "az", "cy", "bx" }, x => x.Last().ToString()).Is("bx", "cy", "az");

            var orderedEnumerable = new[] { "ab", "ghij_1", "cdef_2", "k" }.OrderBy(x => x.Length);
            orderedEnumerable.Is("k", "ab", "ghij_1", "cdef_2");
            order.Order(orderedEnumerable).Is("k", "ab", "cdef_2", "ghij_1");
            order.Order(orderedEnumerable, x => x.Last().ToString()).Is("k", "ab", "ghij_1", "cdef_2");
        }

        [TestMethod]
        public void DescendingTest()
        {
            var order = new OrderingComparer<string, string>(x => x, Comparer<string>.Default, true);

            order.Compare("a", "z").Is(i => i > 0);
            order.Compare("a", "a").Is(0);
            order.Compare("z", "a").Is(i => i < 0);

            order.Compare("za", "az", x => x.Last().ToString()).Is(i => i > 0);
            order.Compare("aa", "za", x => x.Last().ToString()).Is(0);
            order.Compare("az", "za", x => x.Last().ToString()).Is(i => i < 0);

            order.Order(new[] { "a", "c", "b" }).Is("c", "b", "a");
            order.Order(new[] { "az", "cy", "bx" }, x => x.Last().ToString()).Is("az", "cy", "bx");

            var orderedEnumerable = new[] { "ab", "ghij_1", "cdef_2", "k" }.OrderBy(x => x.Length);
            orderedEnumerable.Is("k", "ab", "ghij_1", "cdef_2");
            order.Order(orderedEnumerable).Is("k", "ab", "ghij_1", "cdef_2");
            order.Order(orderedEnumerable, x => x.Last().ToString()).Is("k", "ab", "cdef_2", "ghij_1");
        }
    }
}
