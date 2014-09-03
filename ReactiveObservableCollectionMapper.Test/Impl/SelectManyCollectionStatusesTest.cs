using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using Kirinji.LinqToObservableCollection.Impl;
using System.Linq;
using System.Collections.Generic;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class SelectManyCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            Func<Wrapped<string>, bool> predicate = x => x.Value.Length >= 10;
            Func<string, string[]> converter1 = x => new []{x , x + " (2)"};
            Func<string, char[]> converter2 = x => x.ToArray();

            TestLogic.CollectionStatusesCheckAll(root => new SelectManyCollectionStatuses<string, string>(root, converter1),
                x => x.SelectMany(converter1));

            TestLogic.CollectionStatusesCheckAll(root => new SelectManyCollectionStatuses<string, string>(root, converter1).CreateSelectMany(converter2),
                x => x.SelectMany(converter1).SelectMany(converter2));
        }
    }
}
