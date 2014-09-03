using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System.Collections.Generic;
using System.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Impl.OrderingComparers
{
    [TestClass]
    public class CompositeOrderingComparerTest
    {
        [TestMethod]
        public void AllTest()
        {
            var order = new OrderingComparer<string, int>(x => x.Length, Comparer<int>.Default, false);
            var order2 = new OrderingComparer<string, char>(x => x.First(), Comparer<char>.Default, false);

            var orders = new CompositeOrderingComparer<string>(new IOrderingComparer<string>[] { order, order2 });

            orders.Compare("AA", "B").Is(i => i > 0);
            orders.Compare("AA", "BB").Is(i => i < 0);
            orders.Compare("B", "AA").Is(i => i < 0);
            orders.Compare("BB", "AA").Is(i => i > 0);
            orders.Compare("BB", "BA").Is(0);

            orders.Order(new[] { "hD", "uBB", "tCC", "kA" }, x => new string(x.Skip(1).ToArray())).Is("kA", "hD", "uBB", "tCC");

            var orderedEnumerable = new[] { "hD_1", "uBB_1", "tCC_1", "kA_1" }.OrderBy(x => x.Last());
            orderedEnumerable.Is("hD_1", "uBB_1", "tCC_1", "kA_1");
            orders.Order(orderedEnumerable, x => new string(x.Skip(1).ToArray())).Is("kA_1", "hD_1", "uBB_1", "tCC_1");
        }
    }
}
