using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class ReverseCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            TestLogic.CollectionStatusesCheckAll(root => new ReverseCollectionStatuses<string>(root),
                x => x.Reverse());

            TestLogic.CollectionStatusesCheckAll(root => new ReverseCollectionStatuses<string>(root).CreateReverse(), x => x);
        }
    }
}
