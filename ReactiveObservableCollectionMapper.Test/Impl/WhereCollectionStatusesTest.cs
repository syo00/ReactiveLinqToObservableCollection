using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using Kirinji.LinqToObservableCollection.Impl;
using System.Linq;
using System.Collections.Generic;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class WhereCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            Func<string, bool> predicate = x => x.Length >= 10;
            Func<string, bool> predicate2 = x => x.Length <= 15;
            Func<string, char> converter = x => x[0];

            TestLogic.CollectionStatusesCheckAll(root => new WhereCollectionStatuses<string>(root, predicate),
                x => x.Where(predicate));

            TestLogic.CollectionStatusesCheckAll(root => new WhereCollectionStatuses<string>(root, predicate).CreateWhere(predicate2),
                x => x.Where(e => predicate(e) && predicate2(e)));

            TestLogic.CollectionStatusesCheckAll(root => new WhereCollectionStatuses<string>(root, predicate).CreateSelect(converter),
                x => x.Where(predicate).Select(converter));

            TestLogic.CollectionStatusesCheckAll(root => new WhereCollectionStatuses<string>(root, predicate).CreateOrdered(x => x, Comparer<string>.Default, false),
                x => x.Where(predicate).OrderBy(y => y, Comparer<string>.Default));
        }
    }
}
