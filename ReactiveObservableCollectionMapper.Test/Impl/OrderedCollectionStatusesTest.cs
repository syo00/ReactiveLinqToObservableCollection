using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.Impl.OrderingComparers;
using System.Collections.Generic;
using System.Linq;
using Kirinji.LinqToObservableCollection.Impl;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class OrderedCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            Func<string, bool> predicate = x => x.Length >= 10;
            Func<string, Wrapped<string>> converter = x => new Wrapped<string> { Value = x };

            var order = new OrderingComparer<string, string>(x => x, Comparer<string>.Default, false);
            TestLogic.CollectionStatusesCheckAll(root => new OrderedCollectionStatuses<string>(root, order),
                x => x.OrderBy(y => y, Comparer<string>.Default));

            var order2 = new OrderingComparer<string, string>(x => x, Comparer<string>.Default, true);
            TestLogic.CollectionStatusesCheckAll(root => new OrderedCollectionStatuses<string>(root, order2),
                x => x.OrderByDescending(y => y, Comparer<string>.Default));

            TestLogic.CollectionStatusesCheckAll(root => new OrderedCollectionStatuses<string>(root, order).CreateSelect(converter),
                x => x.OrderBy(y => y, Comparer<string>.Default).Select(converter));

            TestLogic.CollectionStatusesCheckAll(root => new OrderedCollectionStatuses<string>(root, order).CreateWhere(predicate),
                x => x.OrderBy(y => y, Comparer<string>.Default).Where(predicate));

            var order3 = new OrderingComparer<string, int>(x => x.Length, Comparer<int>.Default, false);
            TestLogic.CollectionStatusesCheckAll(root => new OrderedCollectionStatuses<string>(root, order3).CreateThenBy(order2),
                x => x.OrderBy(y => y.Length, Comparer<int>.Default).ThenByDescending(y => y, Comparer<string>.Default));
        }
    }
}
