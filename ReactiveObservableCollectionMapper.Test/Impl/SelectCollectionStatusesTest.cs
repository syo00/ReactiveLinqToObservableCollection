using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using Kirinji.LinqToObservableCollection.Impl;
using System.Linq;
using System.Collections.Generic;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class SelectCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            Func<Wrapped<string>, bool> predicate = x => x.Value.Length >= 10;
            Func<string, Wrapped<string>> converter = x => new Wrapped<string> { Value = x };
            Func<Wrapped<string>, char> converter2 = x => x.Value[0];

            TestLogic.CollectionStatusesCheckAll(root => new SelectCollectionStatuses<string, Wrapped<string>>(root, converter),
                x => x.Select(converter));

            TestLogic.CollectionStatusesCheckAll(root => new SelectCollectionStatuses<string, Wrapped<string>>(root, converter).CreateSelect(converter2),
                x => x.Select(converter).Select(converter2));

            TestLogic.CollectionStatusesCheckAll(root => new SelectCollectionStatuses<string, Wrapped<string>>(root, converter).CreateWhere(predicate),
                x => x.Select(converter).Where(predicate));

            TestLogic.CollectionStatusesCheckAll(root => new SelectCollectionStatuses<string, Wrapped<string>>(root, converter).CreateOrdered(x => x.Value, Comparer<string>.Default, false),
                x => x.Select(converter).OrderBy(y => y.Value, Comparer<string>.Default));
        }
    }
}
