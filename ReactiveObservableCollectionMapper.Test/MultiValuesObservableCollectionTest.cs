using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Test
{
    [TestClass]
    public class MultiValuesObservableCollectionTest
    {
        [TestMethod]
        public void AddAndAddRangeTest()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.AddRange(new int[0]);
            tested.Is(1, 2, 3);
            collectionChanged.Is();

            tested.Add(4);
            tested.Is(1, 2, 3, 4);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(i => i == 3 || i <= -1);
            collectionChanged.Single().NewItems.Cast<int>().Is(4);
            collectionChanged.Clear();

            tested.AddRange(new[] { 5, 6 });
            tested.Is(1, 2, 3, 4, 5, 6);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(i => i == 4 || i <= -1);
            collectionChanged.Single().NewItems.Cast<int>().Is(5, 6);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void InsertTest1()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.Insert(0, new int[0]);
            tested.Is(1, 2, 3);
            collectionChanged.Is();

            tested.Insert(0, 0);
            tested.Is(0, 1, 2, 3);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(0);
            collectionChanged.Single().NewItems.Cast<int>().Is(0);
            collectionChanged.Clear();

            tested.Insert(0, new[] { -2, -1 });
            tested.Is(-2, -1, 0, 1, 2, 3);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(0);
            collectionChanged.Single().NewItems.Cast<int>().Is(-2, -1);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void InsertTest2()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.Insert(3, new int[0]);
            tested.Is(1, 2, 3);
            collectionChanged.Is();

            tested.Insert(3, 4);
            tested.Is(1, 2, 3, 4);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(i => i == 3 || i <= -1);
            collectionChanged.Single().NewItems.Cast<int>().Is(4);
            collectionChanged.Clear();

            tested.Insert(4, new[] { 5, 6 });
            tested.Is(1, 2, 3, 4, 5, 6);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(i => i == 4 || i <= -1);
            collectionChanged.Single().NewItems.Cast<int>().Is(5, 6);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void InsertTest3()
        {
            var tested = new MultiValuesObservableCollection<int> { 10, 20, 30 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.Insert(1, new int[0]);
            tested.Is(10, 20, 30);
            collectionChanged.Is();

            tested.Insert(1, 15);
            tested.Is(10, 15, 20, 30);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(1);
            collectionChanged.Single().NewItems.Cast<int>().Is(15);
            collectionChanged.Clear();

            tested.Insert(3, new[] { 21, 22 });
            tested.Is(10, 15, 20, 21, 22, 30);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(3);
            collectionChanged.Single().NewItems.Cast<int>().Is(21, 22);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void RemoveAtTest1()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3, 4, 5 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.RemoveAt(0, 0).Is();
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Is();

            tested.RemoveAt(0, 1).Is(1);
            tested.Is(2, 3, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(0);
            collectionChanged.Single().OldItems.Cast<int>().Is(1);
            collectionChanged.Clear();

            tested.RemoveAt(0, 2).Is(2, 3);
            tested.Is(4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(0);
            collectionChanged.Single().OldItems.Cast<int>().Is(2, 3);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void RemoveAtTest2()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3, 4, 5 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.RemoveAt(4, 0).Is();
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Is();

            tested.RemoveAt(4, 1).Is(5);
            tested.Is(1, 2, 3, 4);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(4);
            collectionChanged.Single().OldItems.Cast<int>().Is(5);
            collectionChanged.Clear();

            tested.RemoveAt(2, 2).Is(3, 4);
            tested.Is(1, 2);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(2);
            collectionChanged.Single().OldItems.Cast<int>().Is(3, 4);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void RemoveAtRangeTest3()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3, 4, 5 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.RemoveAt(1, 0).Is();
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Is();

            tested.RemoveAt(1, 1).Is(2);
            tested.Is(1, 3, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(1);
            collectionChanged.Single().OldItems.Cast<int>().Is(2);
            collectionChanged.Clear();

            tested.RemoveAt(1, 2).Is(3, 4);
            tested.Is(1, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(1);
            collectionChanged.Single().OldItems.Cast<int>().Is(3, 4);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void RemoveTest()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 3, 2, 3, 2 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.Remove(1).IsTrue();
            tested.Is(3, 2, 3, 2);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(0);
            collectionChanged.Single().OldItems.Cast<int>().Is(1);
            collectionChanged.Clear();

            tested.Remove(2).IsTrue();
            tested.Is(3, 3, 2);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(1);
            collectionChanged.Single().OldItems.Cast<int>().Is(2);
            collectionChanged.Clear();

            tested.Remove(-1).IsFalse();
            tested.Is(3, 3, 2);
            collectionChanged.Is();
        }

        [TestMethod]
        public void MoveTest()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3, 4, 5 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.Move(1, 1).Is(2);
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Is();

            tested.Move(1, 2).Is(2);
            tested.Is(1, 3, 2, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Move);
            collectionChanged.Single().OldStartingIndex.Is(1);
            collectionChanged.Single().NewStartingIndex.Is(2);
            collectionChanged.Single().OldItems.Cast<int>().Is(2);
            collectionChanged.Single().NewItems.Cast<int>().Is(2);
            collectionChanged.Clear();

            tested.Move(2, 1).Is(2);
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Move);
            collectionChanged.Single().OldStartingIndex.Is(2);
            collectionChanged.Single().NewStartingIndex.Is(1);
            collectionChanged.Single().OldItems.Cast<int>().Is(2);
            collectionChanged.Single().NewItems.Cast<int>().Is(2);
            collectionChanged.Clear();

            tested.Move(1, 2, 0).Is();
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Is();

            tested.Move(0, 3, 2).Is(1, 2);
            tested.Is(3, 4, 5, 1, 2);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Move);
            collectionChanged.Single().OldStartingIndex.Is(0);
            collectionChanged.Single().NewStartingIndex.Is(3);
            collectionChanged.Single().OldItems.Cast<int>().Is(1, 2);
            collectionChanged.Single().NewItems.Cast<int>().Is(1, 2);
            collectionChanged.Clear();

            tested.Move(3, 0, 2).Is(1, 2);
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Move);
            collectionChanged.Single().OldStartingIndex.Is(3);
            collectionChanged.Single().NewStartingIndex.Is(0);
            collectionChanged.Single().OldItems.Cast<int>().Is(1, 2);
            collectionChanged.Single().NewItems.Cast<int>().Is(1, 2);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void ReplaceTest()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3, 4, 5 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested.Replace(3, 2, new[] { 6, 7, 8 }).Is(4, 5);
            tested.Is(1, 2, 3, 6, 7, 8);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Replace);
            collectionChanged.Single().OldStartingIndex.Is(3);
            collectionChanged.Single().NewStartingIndex.Is(3);
            collectionChanged.Single().OldItems.Cast<int>().Is(4, 5);
            collectionChanged.Single().NewItems.Cast<int>().Is(6, 7, 8);
            collectionChanged.Clear();

            tested.Replace(3, 0, new[] { 4, 5 }).Is();
            tested.Is(1, 2, 3, 4, 5, 6, 7, 8);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Add);
            collectionChanged.Single().NewStartingIndex.Is(3);
            collectionChanged.Single().NewItems.Cast<int>().Is(4, 5);
            collectionChanged.Clear();

            tested.Replace(5, 3, new int[0]).Is(6, 7, 8);
            tested.Is(1, 2, 3, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(5);
            collectionChanged.Single().OldItems.Cast<int>().Is(6, 7, 8);
            collectionChanged.Clear();
        }

        [TestMethod]
        public void SetTest()
        {
            var tested = new MultiValuesObservableCollection<int> { 1, 2, 3, 4, 5 };
            var collectionChanged = new List<NotifyCollectionChangedEventArgs>();
            tested.CollectionChanged += (s, e) => collectionChanged.Add(e);

            tested[2] = 30;
            tested.Is(1, 2, 30, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Replace);
            collectionChanged.Single().OldStartingIndex.Is(2);
            collectionChanged.Single().NewStartingIndex.Is(2);
            collectionChanged.Single().OldItems.Cast<int>().Is(3);
            collectionChanged.Single().NewItems.Cast<int>().Is(30);
            collectionChanged.Clear();

            tested.Set(2, new int[0]).Is(30);
            tested.Is(1, 2, 4, 5);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Remove);
            collectionChanged.Single().OldStartingIndex.Is(2);
            collectionChanged.Single().OldItems.Cast<int>().Is(30);
            collectionChanged.Clear();

            tested.Set(3, new int[] { 6, 7, 8 }).Is(5);
            tested.Is(1, 2, 4, 6, 7, 8);
            collectionChanged.Single().Action.Is(NotifyCollectionChangedAction.Replace);
            collectionChanged.Single().OldStartingIndex.Is(3);
            collectionChanged.Single().NewStartingIndex.Is(3);
            collectionChanged.Single().OldItems.Cast<int>().Is(5);
            collectionChanged.Single().NewItems.Cast<int>().Is(6, 7, 8);
            collectionChanged.Clear();
        }
    }
}
