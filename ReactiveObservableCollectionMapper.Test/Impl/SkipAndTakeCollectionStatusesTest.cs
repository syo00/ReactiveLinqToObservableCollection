using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.Impl;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class SkipAndTakeCollectionStatusesTest
    {
        [TestMethod]
        public void AllTest()
        {
            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 1, null),
                x => x.Skip(1));

            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 1, null).CreateSkip(1),
                x => x.Skip(2));

            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 1, null).CreateTake(3),
                x => x.Skip(1).Take(3));

            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 1, null).CreateTake(4).CreateSkip(1),
                x => x.Skip(1).Take(4).Skip(1));


            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 0, 3),
                x => x.Take(3));

            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 0, 5).CreateSkip(2),
                x => x.Take(5).Skip(2));

            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 0, 5).CreateTake(3),
                x => x.Take(5).Take(3));

            TestLogic.CollectionStatusesCheckAll(root => new SkipAndTakeCollectionStatuses<string>(root, 0, 7).CreateSkip(2).CreateTake(3),
                x => x.Take(7).Skip(2).Take(3));
        }
    }
}
