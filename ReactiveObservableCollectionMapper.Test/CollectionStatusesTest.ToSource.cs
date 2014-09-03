using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace Kirinji.LinqToObservableCollection.Test
{
    public partial class CollectionStatusesTest
    {
        [TestMethod]
        public void ToObservableCollectionWithMoveTest()
        {
            var s = new Subject<IEnumerable<int>>();
            var oc = s
                .ToStatusesWithMove(EqualityComparer<int>.Default)
                .ToObservableCollection();
            var source = new List<INotifyCollectionChangedEvent<int>>();
            oc.Statuses.InitialStateAndChanged.Subscribe(source.Add);

            s.OnNext(new int[] { 1, 2, 3 });
            oc.Is(1, 2, 3);
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Add || e.Action == NotifyCollectionChangedEventAction.InitialState).IsTrue();
            source.Clear();

            s.OnNext(new int[] { 1, 2, 3, 4 });
            oc.Is(1, 2, 3, 4);
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Add).IsTrue();
            source.Clear();

            s.OnNext(new int[] { 4, 2, 3, 1 });
            oc.Is(4, 2, 3, 1);
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Move).IsTrue();
            source.Clear();

            s.OnNext(new int[] { });
            oc.Is();
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Remove || e.Action == NotifyCollectionChangedEventAction.Reset).IsTrue();
            source.Clear();
        }

        [TestMethod]
        public void ToObservableCollectionWithResetTest()
        {
            var s = new Subject<IEnumerable<int>>();
            var oc = s
                .ToStatusesWithReset()
                .ToObservableCollection();
            var source = new List<INotifyCollectionChangedEvent<int>>();
            oc.Statuses.InitialStateAndChanged.Subscribe(source.Add);

            s.OnNext(new int[] { 1, 2, 3 });
            oc.Is(1, 2, 3);
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Add || e.Action == NotifyCollectionChangedEventAction.InitialState).IsTrue();
            source.Clear();

            s.OnNext(new int[] { 1, 2, 3, 4 });
            oc.Is(1, 2, 3, 4);
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Add || e.Action == NotifyCollectionChangedEventAction.Reset).IsTrue();
            source.Clear();

            s.OnNext(new int[] { 4, 2, 3, 1 });
            oc.Is(4, 2, 3, 1);
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Add || e.Action == NotifyCollectionChangedEventAction.Reset).IsTrue();
            source.Clear();

            s.OnNext(new int[] { });
            oc.Is();
            source.All(e => e.Action == NotifyCollectionChangedEventAction.Remove || e.Action == NotifyCollectionChangedEventAction.Reset).IsTrue();
            source.Clear();
        }

        [TestMethod]
        public void ToSingleItemStatusesTest()
        {
            // -> "a" -> "b" -> void -> void -> "c"
            {
                var mainSubject = new Subject<ValueOrEmpty<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory = new List<Exception>();
                var completed = false;

                mainSubject
                    .ToSingleItemStatuses()
                    .Subscribe(valuesHistory.Add, errorsHistory.Add, () => completed = true);
                valuesHistory.Is();

                mainSubject.OnNext(new ValueOrEmpty<string>("a"));
                valuesHistory.Single().InitialState.Items.Is("a");
                valuesHistory.Clear();

                mainSubject.OnNext(new ValueOrEmpty<string>("b"));
                valuesHistory.Single().Replaced.OldItems.Is("a");
                valuesHistory.Single().Replaced.NewItems.Is("b");
                valuesHistory.Single().Replaced.StartingIndex.Is(0);
                valuesHistory.Clear();

                mainSubject.OnNext(new ValueOrEmpty<string>());
                valuesHistory.Single().Removed.Items.Is("b");
                valuesHistory.Single().Removed.StartingIndex.Is(0);
                valuesHistory.Clear();

                mainSubject.OnNext(new ValueOrEmpty<string>());
                valuesHistory.Is();

                mainSubject.OnNext(new ValueOrEmpty<string>("c"));
                valuesHistory.Single().Added.Items.Is("c");
                valuesHistory.Single().Added.StartingIndex.Is(0);
                valuesHistory.Clear();

                errorsHistory.Is();
                completed.IsFalse();
            }

            // -> void
            {
                var mainSubject = new Subject<ValueOrEmpty<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory = new List<Exception>();
                var completed = false;

                mainSubject
                    .ToSingleItemStatuses()
                    .Subscribe(valuesHistory.Add, errorsHistory.Add, () => completed = true);

                mainSubject.OnNext(new ValueOrEmpty<string>());
                valuesHistory.Single().InitialState.Items.Is();

                errorsHistory.Is();
                completed.IsFalse();
            }
        }
    }
}
