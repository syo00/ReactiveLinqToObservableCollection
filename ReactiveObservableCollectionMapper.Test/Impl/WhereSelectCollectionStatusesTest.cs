using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using Kirinji.LinqToObservableCollection.Impl;
using System.Linq;
using Kirinji.LightWands;
using System.Collections.Generic;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class WhereSelectCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            Func<string, bool> predicate = x => x.Length >= 10;
            Func<Wrapped<string>, bool> predicate2 = x => x.Value.Length <= 15;
            Func<string, Wrapped<string>> converter = x => new Wrapped<string> { Value = x };
            Func<Wrapped<string>, char> converter2 = x => x.Value[0];

            TestLogic.CollectionStatusesCheckAll(root => new WhereSelectCollectionStatuses<string, Wrapped<string>>(root, predicate, converter),
                x => x.Where(predicate).Select(converter));

            TestLogic.CollectionStatusesCheckAll(root => new WhereSelectCollectionStatuses<string, Wrapped<string>>(root, predicate, converter).CreateWhere(predicate2),
                x => x.Where(predicate).Select(converter).Where(predicate2));

            TestLogic.CollectionStatusesCheckAll(root => new WhereSelectCollectionStatuses<string, Wrapped<string>>(root, predicate, converter).CreateSelect(converter2),
                x => x.Where(predicate).Select(converter).Select(converter2));

            TestLogic.CollectionStatusesCheckAll(root => new WhereSelectCollectionStatuses<string, Wrapped<string>>(root, predicate, converter).CreateOrdered(x => x.Value, Comparer<string>.Default, false),
                x => x.Where(predicate).Select(converter).OrderBy(y => y.Value, Comparer<string>.Default));
        }
    }
}
