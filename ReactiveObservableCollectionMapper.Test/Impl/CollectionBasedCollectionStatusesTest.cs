using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using Kirinji.LinqToObservableCollection.Impl;
using Kirinji.LightWands;
using System.Reactive.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    [TestClass]
    public class CollectionBasedCollectionStatusesTest
    {
        [TestMethod]
        public void ObservableCollectionTest()
        {
            var source = new ObservableCollection<string> { "a", "b", "c" };
            var r = new CollectionBasedCollectionStatuses<string>(source);
            var first = r.ToObservableCollection();

            r.InitialStateAndChanged.Subscribe();
            r.InitialStateAndChanged.Subscribe();

            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "c");
            r.ToObservableCollection().Is("a", "b", "c");

            source.Add("d");
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "c", "d");
            r.ToObservableCollection().Is("a", "b", "c", "d");

            source[3] = "mart";
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "c", "mart");
            r.ToObservableCollection().Is("a", "b", "c", "mart");

            source.RemoveAt(2);
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "mart");
            r.ToObservableCollection().Is("a", "b", "mart");

            first.Is("a", "b", "mart");

            source.Clear();
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is();
            r.ToObservableCollection().Is();

            first.Is();
        }

        [TestMethod]
        public void ReadOnlyObservableCollectionTest()
        {
            var source = new ObservableCollection<string> { "a", "b", "c" };
            var r = new CollectionBasedCollectionStatuses<string>(source.ToReadOnly());
            var first = r.ToObservableCollection();

            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "c");
            r.ToObservableCollection().Is("a", "b", "c");

            source.Add("d");
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "c", "d");
            r.ToObservableCollection().Is("a", "b", "c", "d");

            source[3] = "mart";
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "c", "mart");
            r.ToObservableCollection().Is("a", "b", "c", "mart");

            source.RemoveAt(2);
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("a", "b", "mart");
            r.ToObservableCollection().Is("a", "b", "mart");

            first.Is("a", "b", "mart");

            source.Clear();
            r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is();
            r.ToObservableCollection().Is();

            first.Is();
        }
    }
}
