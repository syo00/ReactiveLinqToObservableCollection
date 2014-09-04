using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Test
{
    public partial class CollectionStatusesTest
    {
        [TestMethod]
        public void CastTest()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<object>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Cast<string>()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "0", "1" }));
                valuesHistory.Single().InitialState.Items.Is("0", "1");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "2" }, 2));
                valuesHistory.Single().Added.Items.Is("2");
                valuesHistory.Single().Added.StartingIndex.Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "2" }, 2));
                valuesHistory.Single().Removed.Items.Is("2");
                valuesHistory.Single().Removed.StartingIndex.Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "0" }, 0, 1));
                valuesHistory.Single().Moved.Items.Is("0");
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "0" }, new[] { "2" }, 1));
                valuesHistory.Single().Replaced.OldItems.Is("0");
                valuesHistory.Single().Replaced.NewItems.Is("2");
                valuesHistory.Single().Replaced.StartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new[] { "0", "1" }));
                valuesHistory.Single().Reset.Items.Is("0", "1");
                valuesHistory.Clear();

                completed.IsFalse();
                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<object>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Cast<string>()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "0", "1" }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }

            // failed casting test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<object>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Cast<string>()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "0", "1" }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new object[] { 2 }, 2));
                valuesHistory.Is();
                error.IsInstanceOf<InvalidCastException>();
                completed.IsFalse();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new object[] { 2 }, 2));
            }
        }

        [TestMethod]
        public void DistinctTest()
        {
            var subject = new Subject<INotifyCollectionChangedEvent<string>>();

            var convertedCollection =
                subject
                .ToStatuses(true)
                .Distinct((x, y) => x.ToLowerInvariant() == y.ToLowerInvariant())
                .ToObservableCollection();
            convertedCollection.IsInitialStateArrived.IsFalse();

            subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b", "b", "c", "C" }));
            convertedCollection
                .Select(x => x.ToLowerInvariant())
                .NonSequenceEqual(new[] { "a", "b", "c" })
                .IsTrue();

            subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "B", "d", "e", "D" }, 5));
            convertedCollection
                .Select(x => x.ToLowerInvariant())
                .NonSequenceEqual(new[] { "a", "b", "c", "d", "e" })
                .IsTrue();

            subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "B", "d", "e", "D" }, 5));
            convertedCollection
                .Select(x => x.ToLowerInvariant())
                .NonSequenceEqual(new[] { "a", "b", "c" })
                .IsTrue();

            subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "a", "b" }, new[] { "C", "d", "e", "D" }, 0)); // ["C", "d", "e", "D", "b", "c", "C"]
            convertedCollection
                .Select(x => x.ToLowerInvariant())
                .NonSequenceEqual(new[] { "b", "c", "d", "e" })
                .IsTrue();

            subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "d", "e", "D" }, 1, 3));
            convertedCollection
                .Select(x => x.ToLowerInvariant())
                .NonSequenceEqual(new[] { "b", "c", "d", "e" })
                .IsTrue();

            subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new[] { "a", "b", "b", "c", "C" }));
            convertedCollection
                .Select(x => x.ToLowerInvariant())
                .NonSequenceEqual(new[] { "a", "b", "c" })
                .IsTrue();

            subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent<string>(new[] { "" }));
            convertedCollection.RaisedError.IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
        }

        [TestMethod]
        public void DefaultIfEmptyTest()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .DefaultIfEmpty("default")
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[0]));
                valuesHistory.Single().InitialState.Items.Is("default");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "a", "b" }, 0));
                valuesHistory.Single().Replaced.OldItems.Is("default");
                valuesHistory.Single().Replaced.NewItems.Is("a", "b");
                valuesHistory.Single().Replaced.StartingIndex.Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "a" }, 0, 1));
                valuesHistory.Single().Moved.Items.Is("a");
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "b" }, new[] { "c" }, 0));
                valuesHistory.Single().Replaced.OldItems.Is("b");
                valuesHistory.Single().Replaced.NewItems.Is("c");
                valuesHistory.Single().Replaced.StartingIndex.Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "c" }, 0));
                valuesHistory.Single().Removed.Items.Is("c");
                valuesHistory.Single().Removed.StartingIndex.Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new string[0]));
                valuesHistory.Single().Reset.Items.Is("default");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "a", "b" }, 0));
                valuesHistory.Single().Replaced.OldItems.Is("default");
                valuesHistory.Single().Replaced.NewItems.Is("a", "b");
                valuesHistory.Single().Replaced.StartingIndex.Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "c", "d" }));
                valuesHistory.Single().Reset.Items.Is("c", "d");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent<string>(new[] { "c", "d" }, 0));
                valuesHistory.Single().Replaced.OldItems.Is("c", "d");
                valuesHistory.Single().Replaced.NewItems.Is("default");
                valuesHistory.Single().Replaced.StartingIndex.Is(0);
                valuesHistory.Clear();

                completed.IsFalse();
                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .DefaultIfEmpty("default")
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[0]));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void ExceptTest1()
        {
            var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
            var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();

            var convertedCollection =
                subject1
                .ToStatuses(true)
                .Except(subject2.ToStatuses(true), (i, s) => i == int.Parse(s), new SchedulingAndThreading[0])
                .ToObservableCollection();
            convertedCollection.IsInitialStateArrived.IsFalse();

            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "4", "5", "6" }));
            subject2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "6" }, 2));

            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 1, 2, 2, 3, 3, 3, 4, 4, 5 }));
            convertedCollection.NonSequenceEqual(new[] { 1, 2, 2, 3, 3, 3 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { 2, 3, 3, 3, 4 }, 2)); // 1: [1, 2, 4, 5]
            convertedCollection.NonSequenceEqual(new[] { 1, 2 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 2, 3, 3, 3, 4 }, 2)); // 1: [1, 2, 2, 3, 3, 3, 4, 4, 5]
            convertedCollection.NonSequenceEqual(new[] { 1, 2, 2, 3, 3, 3 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 3, 3, 4 }, 4, 6)); // 1: [1, 2, 2, 3, 4, 5, 3, 3, 4] => [1, 2, 2, 3, 3, 3, 4, 4, 5]
            convertedCollection.NonSequenceEqual(new[] { 1, 2, 2, 3, 3, 3 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 1, 2 }, new[] { 7, 6, 5, 5, 6 }, 0)); // 1: [7, 6, 5, 5, 6, 2, 3, 4, 5, 3, 3, 4] => [2, 3, 3, 3, 4, 4, 5, 5, 5, 6, 6, 7]
            convertedCollection.NonSequenceEqual(new[] { 2, 3, 3, 3, 6, 6, 7 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 1 }));
            convertedCollection.NonSequenceEqual(new int[] { 1 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 2, 2, 3, 3, 3, 4, 4, 4, 4 }, -1));
            convertedCollection.NonSequenceEqual(new int[] { 1, 2, 2, 3, 3, 3 }).IsTrue();

            subject2.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "3", "3" }, 2)); // 2: [4, 5, 3, 3]
            convertedCollection.NonSequenceEqual(new int[] { 1, 2, 2 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "3", "3" }, 2));
            convertedCollection.NonSequenceEqual(new int[] { 1, 2, 2, 3, 3, 3 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "3" }, -1));
            subject2.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "3" }, 2, 0)); // 2: [3, 4, 5]
            convertedCollection.NonSequenceEqual(new int[] { 1, 2, 2 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "4", "5" }, new[] { "6", "2" }, 1));
            convertedCollection.NonSequenceEqual(new int[] { 1, 4, 4, 4, 4 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "4" }));
            convertedCollection.NonSequenceEqual(new int[] { 1, 2, 2, 3, 3, 3 }).IsTrue();

            // subject2 error checking code is written in ExceptTest2
            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent<int>(new[] { 0 }));
            convertedCollection.RaisedError.IsInstanceOf<InvalidInformationException<int>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
        }

        [TestMethod]
        public void ExceptTest2()
        {
            var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
            var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();

            var convertedCollection =
                subject1
                .ToStatuses(true)
                .Except(subject2.ToStatuses(true), (i, s) => i == int.Parse(s), new SchedulingAndThreading[0])
                .ToObservableCollection();
            convertedCollection.IsInitialStateArrived.IsFalse();

            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 1, 2, 2, 3, 3, 3, 4, 4, 5 }));
            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "4", "5" }));

            convertedCollection.NonSequenceEqual(new[] { 1, 2, 2, 3, 3, 3 }).IsTrue();

            // subject1 error checking code is written in ExceptTest1
            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent<string>(new[] { "" }));
            convertedCollection.RaisedError.IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
        }

        [TestMethod]
        public void GroupByTest()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<IGroupedCollectionStatuses<string, string>>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .GroupBy(x => x, x => x.ToUpperInvariant(), EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "apple", "and", "bright" })); // ["apple", "and", "orange"]
                valuesHistory.Single().InitialState.Items.Select(g => g.Key).OrderBy(x => x).Is("apple", "bright");

                var sourceOfA = valuesHistory.Single().InitialState.Items[0];
                var sourceOfB = valuesHistory.Single().InitialState.Items[1];
                var valuesHistoryOfA = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistoryOfB = new List<INotifyCollectionChangedEvent<string>>();
                Exception errorOfA = null;
                Exception errorOfB = null;
                var completedOfA = false;
                var completedOfB = false;
                sourceOfA
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA.Add, ex => errorOfA = ex, () => completedOfA = true);
                sourceOfB
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfB.Add, ex => errorOfB = ex, () => completedOfB = true);

                valuesHistoryOfA.Single().InitialState.Items.Is("APPLE", "AND");
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Single().InitialState.Items.Is("BRIGHT");
                valuesHistoryOfB.Clear();

                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "book", "called" }, 3)); // ["apple", "and", "bright", "book", "called"]
                valuesHistory.Single().Added.Items.Select(g => g.Key).Is("called");
                valuesHistory.Single().Added.StartingIndex.Is(2);
                valuesHistory.Clear();

                valuesHistoryOfA.Is();
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Single().Added.Items.Is("BOOK");
                valuesHistoryOfB.Single().Added.StartingIndex.Is(1);
                valuesHistoryOfB.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] {"and", "bright", "book", "called" }, 1)); // ["apple"]
                valuesHistory.Single().Removed.Items.Select(g => g.Key).OrderBy(x => x).Is("bright", "called");
                valuesHistory.Single().Removed.StartingIndex.Is(1);
                valuesHistory.Clear();

                valuesHistoryOfA.Single().Removed.Items.Is("AND");
                valuesHistoryOfA.Single().Removed.StartingIndex.Is(1);
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Single().Removed.Items.Is("BRIGHT", "BOOK");
                valuesHistoryOfB.Single().Removed.StartingIndex.Is(0);
                valuesHistoryOfB.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "apple" }, new[] { "and", "bright", "book", "called" }, 0)); // ["and", "bright", "book", "called"]
                valuesHistory.Single().Added.Items.Select(g => g.Key).OrderBy(x => x).Is("bright", "called");
                valuesHistory.Single().Added.StartingIndex.Is(1);
                valuesHistory.Clear();

                valuesHistoryOfA.Single().Replaced.OldItems.Is("APPLE");
                valuesHistoryOfA.Single().Replaced.NewItems.Is("AND");
                valuesHistoryOfA.Single().Replaced.StartingIndex.Is(0);
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Single().Added.Items.Is("BRIGHT", "BOOK");
                valuesHistoryOfB.Single().Added.StartingIndex.Is(0);
                valuesHistoryOfB.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "and", "bright" }, 0, 2)); // ["book", "called", "and", "bright"]
                valuesHistory.Is();

                valuesHistoryOfA.Is();
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Single().Moved.Items.Is("BRIGHT");
                valuesHistoryOfB.Single().Moved.OldStartingIndex.Is(0);
                valuesHistoryOfB.Single().Moved.NewStartingIndex.Is(1);
                valuesHistoryOfB.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "ape", "cry", "dawn", "all" }));
                valuesHistory.Single().Reset.Items.Select(g => g.Key).OrderBy(x => x).Is("apple", "called", "dawn"); // IGroupedなんちゃらは前に作成されたものはいつまでも使えるので、Keyは更新されない
                valuesHistory.Clear();

                valuesHistoryOfA.Single().Reset.Items.Is("APE", "ALL");
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Single().Reset.Items.Is();
                valuesHistoryOfB.Clear();

                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();

                valuesHistoryOfA.Is();
                errorOfA.IsNull();
                completedOfA.IsTrue();
                valuesHistoryOfB.Is();
                errorOfB.IsNull();
                completedOfB.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<IGroupedCollectionStatuses<string, string>>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .GroupBy(x => x, x => x.ToUpperInvariant(), EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "apple", "and", "bright" }));
                
                var sourceOfA = valuesHistory.Single().InitialState.Items[0];
                var sourceOfB = valuesHistory.Single().InitialState.Items[1];
                var valuesHistoryOfA = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistoryOfB = new List<INotifyCollectionChangedEvent<string>>();
                Exception errorOfA = null;
                Exception errorOfB = null;
                var completedOfA = false;
                var completedOfB = false;
                sourceOfA
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA.Add, ex => errorOfA = ex, () => completedOfA = true);
                sourceOfB
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfB.Add, ex => errorOfB = ex, () => completedOfB = true);

                valuesHistory.Clear();
                valuesHistoryOfA.Clear();
                valuesHistoryOfB.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "xxx" }));
                valuesHistory.Is();
                error.IsInstanceOf<InvalidInformationException<string>>();
                completed.IsFalse();

                valuesHistoryOfA.Is();
                errorOfA.IsInstanceOf<InvalidInformationException<string>>();
                completedOfA.IsFalse();
                valuesHistoryOfB.Is();
                errorOfB.IsInstanceOf<InvalidInformationException<string>>();
                completedOfB.IsFalse();
            }

            // ひどい comparer が設定されたときのテスト
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<IGroupedCollectionStatuses<int, int>>>();
                Exception error = null;
                var completed = false;
                var random = new Random(123456);

                subject
                    .ToStatuses(true)
                    .GroupBy(x => x, x => x, EqualityComparer.Create<int>((x, y) => random.Next() % 2 == 0))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(Enumerable.Range(1, 10000).ToArray()));

                valuesHistory.Is();
                error.IsNotNull();
                completed.IsFalse();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(Enumerable.Range(1, 10000).ToArray(), 10000));
            }
        }

        [TestMethod]
        public void GroupJoinTest()
        {
            // OnNext & outer OnCompleted -> inner OnCompleted test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<Tuple<string, ICollectionStatuses<string>>>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .GroupJoin(innerSubject.ToStatuses(true), x => x, x => x, (outer, inners) => Tuple.Create(outer, inners), EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "apple", "and", "bright" })); // ["apple", "and", "orange"]
                valuesHistory.Single().InitialState.Items.Select(tuple => tuple.Item1).Is("apple", "and", "bright");

                var sourceOfA1 = valuesHistory.Single().InitialState.Items[0].Item2;
                var sourceOfA2 = valuesHistory.Single().InitialState.Items[1].Item2;
                var sourceOfB = valuesHistory.Single().InitialState.Items[2].Item2;
                var valuesHistoryOfA1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistoryOfA2 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistoryOfB = new List<INotifyCollectionChangedEvent<string>>();
                Exception errorOfA1 = null;
                Exception errorOfA2 = null;
                Exception errorOfB = null;
                var completedOfA1 = false;
                var completedOfA2 = false;
                var completedOfB = false;
                sourceOfA1
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA1.Add, ex => errorOfA1 = ex, () => completedOfA1 = true);
                sourceOfA2
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA2.Add, ex => errorOfA2 = ex, () => completedOfA2 = true);
                sourceOfB
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfB.Add, ex => errorOfB = ex, () => completedOfB = true);

                valuesHistoryOfA1.Is();
                valuesHistoryOfA2.Is();
                valuesHistoryOfB.Is();

                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "bright" }, new[] { "cold", "awful" }, 2)); // ["apple", "and", cold", "awful"]
                valuesHistory.Single().Replaced.OldItems.Select(tuple => tuple.Item1).Is("bright");
                valuesHistory.Single().Replaced.NewItems.Select(tuple => tuple.Item1).Is("cold", "awful");
                valuesHistory.Single().Replaced.StartingIndex.Is(2);
                valuesHistory.Clear();

                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "boy", "girl" })));
                valuesHistoryOfA1.Single().InitialState.Items.Is();
                valuesHistoryOfA1.Clear();
                valuesHistoryOfA2.Single().InitialState.Items.Is();
                valuesHistoryOfA2.Clear();
                valuesHistoryOfB.Single().InitialState.Items.Is("boy");
                valuesHistoryOfB.Clear();

                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "ant", "art", "blue", "cow" }, 2)));
                valuesHistoryOfA1.Single().Added.Items.Is("ant", "art");
                valuesHistoryOfA1.Single().Added.StartingIndex.Is(0);
                valuesHistoryOfA1.Clear();
                valuesHistoryOfA2.Single().Added.Items.Is("ant", "art");
                valuesHistoryOfA2.Single().Added.StartingIndex.Is(0);
                valuesHistoryOfA2.Clear();
                valuesHistoryOfB.Single().Added.Items.Is("blue");
                valuesHistoryOfB.Single().Added.StartingIndex.Is(1);
                valuesHistoryOfB.Clear();

                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "ant", "art", "blue", "cow" }, new[] { "cool", "bow" }, 2)));
                valuesHistoryOfA1.Single().Removed.Items.Is("ant", "art");
                valuesHistoryOfA1.Single().Removed.StartingIndex.Is(0);
                valuesHistoryOfA1.Clear();
                valuesHistoryOfA2.Single().Removed.Items.Is("ant", "art");
                valuesHistoryOfA2.Single().Removed.StartingIndex.Is(0);
                valuesHistoryOfA2.Clear();
                valuesHistoryOfB.Single().Replaced.OldItems.Is("blue");
                valuesHistoryOfB.Single().Replaced.NewItems.Is("bow");
                valuesHistoryOfB.Single().Replaced.StartingIndex.Is(1);
                valuesHistoryOfB.Clear();

                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "cool", "bow" }, 2)));
                valuesHistoryOfA1.Is();
                valuesHistoryOfA2.Is();
                valuesHistoryOfB.Single().Removed.Items.Is("bow");
                valuesHistoryOfB.Single().Removed.StartingIndex.Is(1);
                valuesHistoryOfB.Clear();

                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateResetEvent(new[] { "ant", "art", "blue", "cow" })));
                valuesHistoryOfA1.Single().Reset.Items.Is("ant", "art");
                valuesHistoryOfA1.Clear();
                valuesHistoryOfA2.Single().Reset.Items.Is("ant", "art");
                valuesHistoryOfA2.Clear();
                valuesHistoryOfB.Single().Reset.Items.Is("blue");
                valuesHistoryOfB.Clear();

                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "ant" }, 0, 2)));
                valuesHistoryOfA1.Single().Moved.OldStartingIndex.Is(0);
                valuesHistoryOfA1.Single().Moved.NewStartingIndex.Is(1);
                valuesHistoryOfA1.Single().Moved.Items.Is("ant");
                valuesHistoryOfA1.Clear();
                valuesHistoryOfA2.Single().Moved.OldStartingIndex.Is(0);
                valuesHistoryOfA2.Single().Moved.NewStartingIndex.Is(1);
                valuesHistoryOfA2.Single().Moved.Items.Is("ant");
                valuesHistoryOfA2.Clear();
                valuesHistoryOfB.Is();

                valuesHistory.Is();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "bug" }, 3));  // ["apple", "and", cold", "bug", "awful"]
                valuesHistory.Single().Added.Items.Select(tuple => tuple.Item1).Is("bug");
                valuesHistory.Single().Added.StartingIndex.Is(3);
                valuesHistory.Single().Added.Items.Single().Item2.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is("blue");
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "apple", "and" }, 0, 2)); // ["cold", "bug", "apple", "and", "awful"]
                valuesHistory.Single().Moved.Items.Select(tuple => tuple.Item1).Is("apple", "and");
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(2);
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "bug" }, 1)); // ["cold", "apple", "and", "awful"]
                valuesHistory.Single().Removed.Items.Select(tuple => tuple.Item1).Is("bug");
                valuesHistory.Single().Removed.StartingIndex.Is(1);
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "cold", "apple", "and" }, new[] { "bucket", "daze" }, 0)); // ["bucket", "daze", "awful"]
                valuesHistory.Single().Replaced.OldItems.Select(tuple => tuple.Item1).Is("cold", "apple", "and");
                valuesHistory.Single().Replaced.NewItems.Select(tuple => tuple.Item1).Is("bucket", "daze");
                valuesHistory.Single().Replaced.StartingIndex.Is(0);
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "cold", "apple", "and" }));
                valuesHistory.Single().Reset.Items.Select(tuple => tuple.Item1).Is("cold", "apple", "and");
                valuesHistory.Clear();

                outerSubject.OnCompleted();
                valuesHistory.Is();
                completed.IsTrue();
                completedOfA1.IsFalse();
                completedOfA2.IsFalse();
                completedOfB.IsFalse();
                error.IsNull();
                errorOfA1.IsNull();
                errorOfA2.IsNull();
                errorOfB.IsNull();

                innerSubject.OnCompleted();
                valuesHistory.Is();
                valuesHistoryOfA1.Is();
                valuesHistoryOfA2.Is();
                valuesHistoryOfB.Is();
                completedOfA1.IsTrue();
                completedOfA2.IsTrue();
                completedOfB.IsTrue();
                error.IsNull();
                errorOfA1.IsNull();
                errorOfA2.IsNull();
                errorOfB.IsNull();
            }

            // inner OnCompleted -> outer OnCompleted test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<Tuple<string, ICollectionStatuses<string>>>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .GroupJoin(innerSubject.ToStatuses(true), x => x, x => x, (outer, inners) => Tuple.Create(outer, inners), EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "apple", "and", "bright" })); // ["apple", "and", "orange"]
                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "boy", "girl" })));

                var sourceOfA = valuesHistory.Single().InitialState.Items[0].Item2;
                var valuesHistoryOfA = new List<INotifyCollectionChangedEvent<string>>();
                Exception errorOfA = null;
                var completedOfA = false;
                sourceOfA
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA.Add, ex => errorOfA = ex, () => completedOfA = true);

                valuesHistoryOfA.Clear();
                valuesHistory.Clear();

                innerSubject.OnCompleted();
                valuesHistory.Is();
                valuesHistoryOfA.Is();
                completed.IsFalse();
                completedOfA.IsTrue();
                error.IsNull();
                errorOfA.IsNull();

                outerSubject.OnCompleted();
                valuesHistory.Is();
                valuesHistoryOfA.Is();
                completed.IsTrue();
                error.IsNull();
                errorOfA.IsNull();
            }

            // outer OnError -> inner OnError test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<Tuple<string, ICollectionStatuses<string>>>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .GroupJoin(innerSubject.ToStatuses(true), x => x, x => x, (outer, inners) => Tuple.Create(outer, inners), EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "apple", "and", "bright" })); // ["apple", "and", "orange"]
                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "boy", "girl" })));

                var sourceOfA = valuesHistory.Single().InitialState.Items[0].Item2;
                var valuesHistoryOfA = new List<INotifyCollectionChangedEvent<string>>();
                Exception errorOfA = null;
                var completedOfA = false;
                sourceOfA
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA.Add, ex => errorOfA = ex, () => completedOfA = true);

                valuesHistoryOfA.Clear();
                valuesHistory.Clear();

                outerSubject.OnError(new DivideByZeroException());
                valuesHistory.Is();
                valuesHistoryOfA.Is();
                completed.IsFalse();
                completedOfA.IsFalse();
                error.IsInstanceOf<DivideByZeroException>();
                errorOfA.IsNull();

                innerSubject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                valuesHistoryOfA.Is();
                completed.IsFalse();
                completedOfA.IsFalse();
                errorOfA.IsInstanceOf<BadImageFormatException>();
            }

            // inner OnError -> outer OnError test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<Tuple<string, ICollectionStatuses<string>>>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .GroupJoin(innerSubject.ToStatuses(true), x => x, x => x, (outer, inners) => Tuple.Create(outer, inners), EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "apple", "and", "bright" })); // ["apple", "and", "orange"]
                innerSubject.OnNext((NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "boy", "girl" })));

                var sourceOfA = valuesHistory.Single().InitialState.Items[0].Item2;
                var valuesHistoryOfA = new List<INotifyCollectionChangedEvent<string>>();
                Exception errorOfA = null;
                var completedOfA = false;
                sourceOfA
                    .InitialStateAndChanged
                    .Subscribe(valuesHistoryOfA.Add, ex => errorOfA = ex, () => completedOfA = true);

                valuesHistoryOfA.Clear();
                valuesHistory.Clear();

                innerSubject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                valuesHistoryOfA.Is();
                completed.IsFalse();
                completedOfA.IsFalse();
                error.IsNull();
                errorOfA.IsInstanceOf<BadImageFormatException>();

                outerSubject.OnError(new DivideByZeroException());
                valuesHistory.Is();
                valuesHistoryOfA.Is();
                completed.IsFalse();
                completedOfA.IsFalse();
                error.IsInstanceOf<DivideByZeroException>();
            }

            // ひどい comparer が設定されたときのテスト
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<Tuple<int, ICollectionStatuses<int>>>>();
                Exception error = null;
                var completed = false;
                var random = new Random(123456);

                outerSubject
                    .ToStatuses(true)
                    .GroupJoin(innerSubject.ToStatuses(true), x => x, x => x, (outer, inners) => Tuple.Create(outer, inners), EqualityComparer.Create<int>((x, y) => random.Next() % 2 == 0))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(Enumerable.Range(0, 10000).ToArray()));
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(Enumerable.Range(0, 10000).ToArray()));
                error.IsNotNull();
                completed.IsFalse();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(Enumerable.Range(0, 10000).ToArray(), 0));
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(Enumerable.Range(0, 10000).ToArray(), 0));
                outerSubject.OnCompleted();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void IntersectTest1()
        {
            var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
            var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();

            var convertedCollection =
                subject1
                .ToStatuses(true)
                .Intersect(subject2.ToStatuses(true), (i, s) => i == int.Parse(s), new SchedulingAndThreading[0])
                .ToObservableCollection();
            convertedCollection.IsInitialStateArrived.IsFalse();

            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "4", "5", "6" }));
            subject2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "6" }, 2));

            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 1, 2, 2, 3, 3, 3, 4, 4, 5 }));
            convertedCollection.NonSequenceEqual(new[] { 4, 4, 5 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { 2, 3, 3, 3, 4 }, 2)); // 1: [1, 2, 4, 5]
            convertedCollection.NonSequenceEqual(new[] { 4, 5 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 2, 3, 3, 3, 4 }, 2)); // 1: [1, 2, 2, 3, 3, 3, 4, 4, 5]
            convertedCollection.NonSequenceEqual(new[] { 4, 4, 5 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 3, 3, 4 }, 4, 6)); // 1: [1, 2, 2, 3, 4, 5, 3, 3, 4] =(sort)=> [1, 2, 2, 3, 3, 3, 4, 4, 5]
            convertedCollection.NonSequenceEqual(new[] { 4, 4, 5 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 1, 2 }, new[] { 7, 6, 5, 5, 6 }, 0)); // 1: [7, 6, 5, 5, 6, 2, 3, 4, 5, 3, 3, 4] =(sort)=> [2, 3, 3, 3, 4, 4, 5, 5, 5, 6, 6, 7]
            convertedCollection.NonSequenceEqual(new[] { 4, 4, 5, 5, 5 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 3, 5, 5, 5 }));
            convertedCollection.NonSequenceEqual(new int[] { 5, 5, 5 }).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new int[0]));
            convertedCollection.NonSequenceEqual(new int[0]).IsTrue();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 1, 2, 2, 3, 3, 3, 4, 4, 4, 4 }, -1));
            convertedCollection.NonSequenceEqual(new int[] { 4, 4, 4, 4 }).IsTrue();

            subject2.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "3", "3" }, 2)); // 2: [4, 5, 3, 3]
            convertedCollection.NonSequenceEqual(new int[] { 3, 3, 3, 4, 4, 4, 4 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "3", "3" }, 2));
            convertedCollection.NonSequenceEqual(new int[] { 4, 4, 4, 4 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "3" }, -1));
            subject2.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "3" }, 2, 0)); // 2: [3, 4, 5]
            convertedCollection.NonSequenceEqual(new int[] { 3, 3, 3, 4, 4, 4, 4 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "4", "5" }, new[] { "6", "2" }, 1));
            convertedCollection.NonSequenceEqual(new int[] { 2, 2, 3, 3, 3 }).IsTrue();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new[] { "3", "6" }));
            convertedCollection.NonSequenceEqual(new int[] { 3, 3, 3 }).IsTrue();

            // subject2 error checking code is written in IntersectTest2
            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent<int>(new[] { 0 }));
            convertedCollection.RaisedError.IsInstanceOf<InvalidInformationException<int>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
        }

        [TestMethod]
        public void IntersectTest2()
        {
            var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
            var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();

            var convertedCollection =
                subject1
                .ToStatuses(true)
                .Intersect(subject2.ToStatuses(true), (i, s) => i == int.Parse(s), new SchedulingAndThreading[0])
                .ToObservableCollection();
            convertedCollection.IsInitialStateArrived.IsFalse();

            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 1, 2, 2, 3, 3, 3, 4, 4, 5 }));
            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "4", "5" }));

            convertedCollection.NonSequenceEqual(new[] { 4, 4, 5 }).IsTrue();

            // subject1 error checking code is written in IntersectTest1
            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent<string>(new[] { "" }));
            convertedCollection.RaisedError.IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
        }

        [TestMethod]
        public void JoinTest()
        {
            // OnNext & outer OnCompleted -> inner OnCompleted test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .Join(innerSubject.ToStatuses(true), x => x, x => x, (outer, inner) => outer + "+" + inner, EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "mind mapping", "i'm so happy", "bass 2 bass" })); // ["mind mapping", "i'm so happy", "bass 2 bass"]
                valuesHistory.Is();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "i'm so happy" }, new[] { "sakura storm", "sakura reflection" }, 1)); // ["mind mapping", "sakura storm", "sakura reflection", "bass 2 bass"]
                valuesHistory.Is();

                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "sigsig", "monkey business", "wuv u" })); // ["sigsig", "monkey business", "wuv u"]
                valuesHistory.Single().InitialState.Items.Is(new[] { "mind mapping+monkey business", "sakura storm+sigsig", "sakura reflection+sigsig" });
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "sakura sunrise", "532nm" }, 4)); // ["mind mapping", "sakura storm", "sakura reflection", "bass 2 bass", "sakura sunrise", "532nm"]
                valuesHistory.Single().Added.Items.Is(new[] { "sakura sunrise+sigsig" });
                valuesHistory.Single().Added.StartingIndex.Is(3);
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "sakura sunrise", "532nm" }, 4)); // ["mind mapping", "sakura storm", "sakura reflection", "bass 2 bass"], ["mind mapping+monkey business", "sakura storm+sigsig", "sakura reflection+sigsig"]
                valuesHistory.Single().Removed.Items.Is(new[] { "sakura sunrise+sigsig" });
                valuesHistory.Single().Removed.StartingIndex.Is(3);
                valuesHistory.Clear();

                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "bad maniacs", "d", "s!ck" }, 3)); // ["sigsig", "monkey business", "wuv u", "bad maniacs", "d", "s!ck"], ["mind mapping+monkey business", "sakura storm+sigsig", sakura storm+s!ck", "sakura reflection+sigsig", sakura reflection+s!ck", "bass 2 bass+bad maniacs"]
                valuesHistory[0].Added.Items.Is(new[] { "bass 2 bass+bad maniacs" });
                valuesHistory[0].Added.StartingIndex.Is(3);
                valuesHistory[1].Added.Items.Is(new[] { "sakura storm+s!ck" });
                valuesHistory[1].Added.StartingIndex.Is(2);
                valuesHistory[2].Added.Items.Is(new[] { "sakura reflection+s!ck"  });
                valuesHistory[2].Added.StartingIndex.Is(4);
                valuesHistory.Count.Is(3);
                valuesHistory.Clear();

                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "bad maniacs", "d" }, 3)); // ["sigsig", "monkey business", "wuv u", "s!ck"] -> ["mind mapping+monkey business", "sakura storm+sigsig", sakura storm+s!ck", "sakura reflection+sigsig", sakura reflection+s!ck" ]
                valuesHistory.Single().Removed.Items.Is(new[] { "bass 2 bass+bad maniacs" });
                valuesHistory.Single().Removed.StartingIndex.Is(5);
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "mind mapping", "sakura storm" }, 0, 2)); // ["sakura reflection", "bass 2 bass", "mind mapping", "sakura storm"], ["sakura reflection+sigsig", sakura reflection+s!ck", "mind mapping+monkey business", "sakura storm+sigsig", "sakura storm+s!ck"]
                valuesHistory.Single().Moved.Items.Is("mind mapping+monkey business", "sakura storm+sigsig", "sakura storm+s!ck");
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(2);
                valuesHistory.Clear();

                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "sigsig" }, 0, 3)); // ["monkey business", "wuv u", "s!ck", "sigsig"], ["sakura reflection+s!ck", "sakura reflection+sigsig", "mind mapping+monkey business", sakura storm+s!ck", "sakura storm+sigsig"]
                valuesHistory[0].Moved.Items.Is(new[] { "sakura storm+sigsig" });
                valuesHistory[0].Moved.OldStartingIndex.Is(3);
                valuesHistory[0].Moved.NewStartingIndex.Is(4);
                valuesHistory[1].Moved.Items.Is(new[] { "sakura reflection+sigsig" });
                valuesHistory[1].Moved.OldStartingIndex.Is(0);
                valuesHistory[1].Moved.NewStartingIndex.Is(1);
                // valuesHistory の順番が逆になっているが許容範囲
                valuesHistory.Count.Is(2);
                valuesHistory.Clear();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "sakura mirage", "400nm", "sakura luminance" })); // ["sakura mirage", "400nm", "sakura luminance"], ["sakura mirage+s!ck", "sakura mirage+sigsig", "sakura mirage+s!ck", "sakura mirage+sigsig"]
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "monkey business", "wuv u", "s!ck", "sigsig" })); // Reset により innerSubject の購読がいったん切れているので InitialState を流す
                valuesHistory.Single().Reset.Items.Is("sakura mirage+s!ck", "sakura mirage+sigsig", "sakura luminance+s!ck", "sakura luminance+sigsig");
                valuesHistory.Clear();

                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "kailua", "smooooch" })); // ["kailua", "smooooch"], ["sakura mirage+smooooch", "sakura luminance+smooooch"]
                // valuesHistory.Single().Reset.NewItems.Is("sakura mirage+smooooch", "sakura luminance+smooooch");
                valuesHistory[0].Replaced.OldItems.Is("sakura mirage+s!ck", "sakura mirage+sigsig");
                valuesHistory[0].Replaced.NewItems.Is("sakura mirage+smooooch");
                valuesHistory[1].Replaced.OldItems.Is("sakura luminance+s!ck", "sakura luminance+sigsig");
                valuesHistory[1].Replaced.NewItems.Is("sakura luminance+smooooch");
                valuesHistory.Count.Is(2);
                // Flatten の影響で 2 つの Replaced として流れてくる
                // 1 つの Replaced や Reset でも可
                valuesHistory.Clear();

                outerSubject.OnCompleted();
                completed.IsTrue();

                innerSubject.OnError(new DivideByZeroException());
                error.IsNull();
            }

            // inner OnCompleted -> outer OnCompleted test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .Join(innerSubject.ToStatuses(true), x => x, x => x, (outer, inner) => outer + "+" + inner, EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "mind mapping", "sakura storm", "sakura reflection", "bass 2 bass" }));
                valuesHistory.Is();
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "sigsig", "monkey business", "wuv u" }));

                innerSubject.OnCompleted();
                completed.IsFalse();

                outerSubject.OnCompleted();
                completed.IsTrue();
                error.IsNull();
            }

            // outer OnError -> inner OnError test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .Join(innerSubject.ToStatuses(true), x => x, x => x, (outer, inner) => outer + "+" + inner, EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "mind mapping", "sakura storm", "sakura reflection", "bass 2 bass" }));
                valuesHistory.Is();
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "sigsig", "monkey business", "wuv u" }));

                outerSubject.OnError(new BadImageFormatException());
                error.IsInstanceOf<BadImageFormatException>();

                innerSubject.OnError(new DivideByZeroException());
                completed.IsFalse();
            }

            // inner OnError -> outer OnError test
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                outerSubject
                    .ToStatuses(true)
                    .Join(innerSubject.ToStatuses(true), x => x, x => x, (outer, inner) => outer + "+" + inner, EqualityComparer.Create<string>((x, y) => x[0] == y[0]))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "mind mapping", "sakura storm", "sakura reflection", "bass 2 bass" }));
                valuesHistory.Is();
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "sigsig", "monkey business", "wuv u" }));

                innerSubject.OnError(new BadImageFormatException());
                error.IsInstanceOf<BadImageFormatException>();

                outerSubject.OnError(new DivideByZeroException());
                completed.IsFalse();
            }

            // ひどい comparer が設定されたときのテスト
            {
                var outerSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var innerSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<int>>();
                Exception error = null;
                var completed = false;
                var random = new Random(123456);

                outerSubject
                    .ToStatuses(true)
                    .Join(innerSubject.ToStatuses(true), x => x, x => x, (outer, inner) => outer + inner, EqualityComparer.Create<int>((x, y) => random.Next() % 2 == 0))
                    .InitialStateAndChanged
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(Enumerable.Range(0, 10000).ToArray()));
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(Enumerable.Range(0, 10000).ToArray()));
                error.IsNotNull();
                completed.IsFalse();

                outerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(Enumerable.Range(0, 10000).ToArray(), 0));
                innerSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(Enumerable.Range(0, 10000).ToArray(), 0));
                outerSubject.OnCompleted();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void DoWhenAddedOrRemovedItemTest()
        {
            var source = new Subject<INotifyCollectionChangedEvent<string>>();
            var addResult = new List<string>();
            var addResultUpper = new List<string>();
            var removeResult = new List<string>();
            var removeResultUpper = new List<string>();

            source
                .ToStatuses(true)
                .DoWhenAddedOrRemovedItem(addResult.Add, removeResult.Add)
                .Subscribe();
            source
                .ToStatuses(true)
                .DoWhenAddedOrRemovedItem(addResultUpper.Add, removeResultUpper.Add, (x, y) => x.ToUpperInvariant() == y.ToUpperInvariant())
                .Subscribe();
            addResult.Is();
            removeResult.Is();
            addResultUpper.Is();
            removeResultUpper.Is();

            source.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "A", "B" }));
            addResult.Is("A", "B");
            removeResult.Is();
            addResultUpper.Is("A", "B");
            removeResultUpper.Is();
            addResult.Clear();
            addResultUpper.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "C", "D" }, 2));
            addResult.Is("C", "D");
            removeResult.Is();
            addResultUpper.Is("C", "D");
            removeResultUpper.Is();
            addResult.Clear();
            addResultUpper.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "D" }, 3)); // ["A", "B", "C"]
            addResult.Is();
            removeResult.Is("D");
            addResultUpper.Is();
            removeResultUpper.Is("D");
            removeResult.Clear();
            removeResultUpper.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "B", "C" }, new[] { "c", "d" }, 1)); // ["A", "c", "d"]
            addResult.Is("c", "d");
            removeResult.Is("B", "C");
            addResultUpper.Is("d");
            removeResultUpper.Is("B");
            addResult.Clear();
            removeResult.Clear();
            addResultUpper.Clear();
            removeResultUpper.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "c" }, 1, 2)); // ["A", "d", "c"]
            addResult.Is();
            removeResult.Is();
            addResultUpper.Is();
            removeResultUpper.Is();

            source.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "D" }));
            addResult.Is("D");
            removeResult.Is("A", "d", "c");
            addResultUpper.Is();
            removeResultUpper.Is("A", "c");
            addResult.Clear();
            removeResult.Clear();
            addResultUpper.Clear();
            removeResultUpper.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new string[0]));
            addResult.Is();
            removeResult.Is("D");
            addResultUpper.Is();
            removeResultUpper.Is("D");

            AssertEx.Catch<InvalidInformationException<string>>(() => source.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[0]))).Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
        }

        [TestMethod]
        public void OfTypeTest()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<object>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .OfType<string>()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new object[] { "0", 1 })); // ["0", 1]
                valuesHistory.Single().InitialState.Items.Is("0");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new object[] { "2", 3 }, 2)); // ["0", 1, "2", 3]
                valuesHistory.Single().Added.Items.Is("2");
                valuesHistory.Single().Added.StartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new object[] { 4 }, 4)); // ["0", 1, "2", 3, 4]
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new object[] { "0", 1 }, 0, 1)); // ["2", "0", 1, 3]
                valuesHistory.Single().Moved.Items.Is("0");
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new object[] { "0", 1, 3 }, new object[] { "1", 2, 3 }, 1)); // ["2", "1", 2, 3]
                valuesHistory.Single().Replaced.OldItems.Is("0");
                valuesHistory.Single().Replaced.NewItems.Is("1");
                valuesHistory.Single().Replaced.StartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "2" }, 0));
                valuesHistory.Single().Removed.Items.Is("2");
                valuesHistory.Single().Removed.StartingIndex.Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new object[] { "0", 1 }));
                valuesHistory.Single().Reset.Items.Is("0");
                valuesHistory.Clear();

                completed.IsFalse();
                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<object>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .OfType<string>()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new object[] { "0", 1 }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void SelectTest_ObservableParameter()
        {
            var mainSubject = new Subject<INotifyCollectionChangedEvent<Subject<string>>>();
            var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
            var errorsHistory = new List<Exception>();
            var resultCollection = new List<string>();
            var completed = false;

            mainSubject
                .ToStatuses(true)
                .Select(s => s.AsObservable())
                .Subscribe(e =>
                {
                    valuesHistory.Add(e);
                    resultCollection.ApplyChangeEvent(e);
                }, errorsHistory.Add, () => completed = true);
            valuesHistory.Is();

            var subject1 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1 }));
            valuesHistory.Is();
            valuesHistory.Clear();

            subject1.OnNext("1-a");
            valuesHistory.Single().InitialState.Items.Is("1-a");
            valuesHistory.Clear();

            subject1.OnNext("1-b");
            valuesHistory.Single().Replaced.OldItems.Is("1-a");
            valuesHistory.Single().Replaced.NewItems.Is("1-b");
            valuesHistory.Single().Replaced.StartingIndex.Is(0);
            valuesHistory.Clear();

            var subject2 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject2 }, 1));
            valuesHistory.Is();

            subject1.OnNext("1-c");
            valuesHistory.Is();

            subject2.OnNext("2-a");
            valuesHistory.Single().Replaced.OldItems.Is("1-b");
            valuesHistory.Single().Replaced.NewItems.Is("1-c", "2-a");
            valuesHistory.Single().Replaced.StartingIndex.Is(0);
            valuesHistory.Clear();

            var subject3 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { subject2 }, new[] { subject3 }, 1));
            valuesHistory.Is();

            subject1.OnNext("1-d");
            valuesHistory.Is();

            subject2.OnNext("2-x");
            valuesHistory.Is();

            resultCollection.Is("1-c", "2-a");
            subject3.OnNext("3-a");
            resultCollection.Is("1-d", "3-a");
            valuesHistory.Clear();

            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { subject1 }, 0, 1));
            valuesHistory.Single().Moved.Items.Is("1-d");
            valuesHistory.Single().Moved.OldStartingIndex.Is(0);
            valuesHistory.Single().Moved.NewStartingIndex.Is(1);
            valuesHistory.Clear();

            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { subject1 }, 1, 0));
            valuesHistory.Single().Moved.Items.Is("1-d");
            valuesHistory.Single().Moved.OldStartingIndex.Is(1);
            valuesHistory.Single().Moved.NewStartingIndex.Is(0);
            valuesHistory.Clear();

            var subject4 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<Subject<string>>(new[] { subject4 }));
            valuesHistory.Is();

            subject1.OnNext("1-x");
            valuesHistory.Is();

            subject4.OnNext("4-a");
            valuesHistory.Single().Reset.Items.Is("4-a");
            valuesHistory.Clear();

            errorsHistory.Is();
            completed.IsFalse();
        }

        [TestMethod]
        public void SelectMany_FlattenTest()
        {
            {
                var masterSubject = new Subject<INotifyCollectionChangedEvent<IObservable<INotifyCollectionChangedEvent<string>>>>();

                var resultCollection =
                    masterSubject
                    .ToStatuses(true)
                    .SelectMany(x => x.ToStatuses(true))
                    .ToObservableCollection();
                resultCollection.Is();
                resultCollection.IsInitialStateArrived.IsFalse();

                var subject1 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject3 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2, subject3 }));
                resultCollection.Is();
                resultCollection.IsInitialStateArrived.IsFalse();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "1-a", "1-b", "1-c" }));
                resultCollection.Is();
                resultCollection.IsInitialStateArrived.IsFalse();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[0]));
                resultCollection.Is();
                resultCollection.IsInitialStateArrived.IsFalse();

                subject3.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "3-a", "3-b" }));
                resultCollection.Is("1-a", "1-b", "1-c", "3-a", "3-b");

                var subject4 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject5 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new IObservable<INotifyCollectionChangedEvent<string>>[] { subject4, subject5 }, 1));
                resultCollection.Is("1-a", "1-b", "1-c", "3-a", "3-b");

                subject4.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "4-a", "4-b", "4-c" }));
                resultCollection.Is("1-a", "1-b", "1-c", "3-a", "3-b");

                subject5.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "5-a", "5-b" }));
                resultCollection.Is("1-a", "1-b", "1-c", "4-a", "4-b", "4-c", "5-a", "5-b", "3-a", "3-b");

                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new IObservable<INotifyCollectionChangedEvent<string>>[] { subject4, subject5 }, 1));
                resultCollection.Is("1-a", "1-b", "1-c", "3-a", "3-b");

                var subject6 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject7 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { subject1, subject2 }, new IObservable<INotifyCollectionChangedEvent<string>>[] { subject6, subject7 }, 0));
                resultCollection.Is("1-a", "1-b", "1-c", "3-a", "3-b");

                subject6.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "6-a", "6-b", "6-c" }));
                resultCollection.Is("1-a", "1-b", "1-c", "3-a", "3-b");

                subject7.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "7-a", "7-b" }));
                resultCollection.Is("6-a", "6-b", "6-c", "7-a", "7-b", "3-a", "3-b");

                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new IObservable<INotifyCollectionChangedEvent<string>>[] { subject6, subject7 }, 0, 1));
                resultCollection.Is("3-a", "3-b", "6-a", "6-b", "6-c", "7-a", "7-b");

                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new IObservable<INotifyCollectionChangedEvent<string>>[] { subject6, subject7 }, 1, 0));
                resultCollection.Is("6-a", "6-b", "6-c", "7-a", "7-b", "3-a", "3-b");

                var subject8 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject9 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { subject8, subject9 }));
                resultCollection.Is("6-a", "6-b", "6-c", "7-a", "7-b", "3-a", "3-b");

                subject8.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "8-a", "8-b" }));
                resultCollection.Is("6-a", "6-b", "6-c", "7-a", "7-b", "3-a", "3-b");

                subject9.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "9-a", "9-b" }));
                resultCollection.Is("8-a", "8-b", "9-a", "9-b");

                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new IObservable<INotifyCollectionChangedEvent<string>>[0]));
                resultCollection.Is();
            }

            {
                var masterSubject = new Subject<INotifyCollectionChangedEvent<IObservable<INotifyCollectionChangedEvent<string>>>>();

                var resultCollection =
                    masterSubject
                    .ToStatuses(true)
                    .SelectMany(x => x.ToStatuses(true))
                    .ToObservableCollection();

                var subject1 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));

                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "2-a", "2-b" }));
                resultCollection.IsInitialStateArrived.IsFalse();

                var subject3 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject3 }, 2));
                resultCollection.IsInitialStateArrived.IsFalse();

                subject3.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "3-a", "3-b", "3-c" }));
                resultCollection.IsInitialStateArrived.IsFalse();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "1-a", "1-b", "1-c" }));
                resultCollection.Is("1-a", "1-b", "1-c", "2-a", "2-b", "3-a", "3-b", "3-c");
            }

            {
                var masterSubject = new Subject<INotifyCollectionChangedEvent<IObservable<INotifyCollectionChangedEvent<string>>>>();

                var resultCollection =
                    masterSubject
                    .ToStatuses(true)
                    .SelectMany(x => x.ToStatuses(true))
                    .ToObservableCollection();

                var subject1 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));

                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "2-a", "2-b" }));
                resultCollection.IsInitialStateArrived.IsFalse();

                var subject3 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject3 }, 2));
                resultCollection.IsInitialStateArrived.IsFalse();

                subject1.OnCompleted();
                resultCollection.IsInitialStateArrived.IsFalse();

                subject3.OnCompleted();
                resultCollection.IsInitialStateArrived.IsFalse();
            }

            {
                var masterSubject = new Subject<INotifyCollectionChangedEvent<IObservable<INotifyCollectionChangedEvent<string>>>>();

                var resultCollection =
                    masterSubject
                    .ToStatuses(true)
                    .SelectMany(x => x.ToStatuses(true))
                    .ToObservableCollection();

                var subject1 = new Subject<INotifyCollectionChangedEvent<string>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));

                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "2-a", "2-b" }));
                resultCollection.IsInitialStateArrived.IsFalse();

                var subject3 = new Subject<INotifyCollectionChangedEvent<string>>();
                masterSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject3 }, 2));
                resultCollection.IsInitialStateArrived.IsFalse();
                resultCollection.RaisedError.IsNull();

                subject1.OnError(new DivideByZeroException());
                resultCollection.RaisedError.IsInstanceOf<DivideByZeroException>();
            }
        }

        [TestMethod]
        public void WhereTest()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<int>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Where(i => i % 2 == 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 0, 1, 2, 3, 4 }));
                valuesHistory.Single().InitialState.Items.Is(0, 2, 4);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 5, 6, 7, 8 }, 5));
                valuesHistory.Single().Added.Items.Is(6, 8);
                valuesHistory.Single().Added.StartingIndex.Is(3);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { 5, 6, 7, 8 }, 5));
                valuesHistory.Single().Removed.Items.Is(6, 8);
                valuesHistory.Single().Removed.StartingIndex.Is(3);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 0, 1, 2 }, 0, 2));
                valuesHistory.Single().Moved.Items.Is(0, 2);
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 0, 1, 2, 3, 4, 5, 6 }));
                valuesHistory.Single().Reset.Items.Is(0, 2, 4, 6);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 2, 3, 4 }, new[] { 20, 30, 31, 40, 41, 42 }, 2));
                valuesHistory.Single().Replaced.OldItems.Is(2,4);
                valuesHistory.Single().Replaced.NewItems.Is(20, 30, 40, 42);
                valuesHistory.Single().Replaced.StartingIndex.Is(1);
                valuesHistory.Clear();

                subject.OnCompleted();
                valuesHistory.Is();
                completed.IsTrue();
                error.IsNull();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<INotifyCollectionChangedEvent<int>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Where(i => i % 2 == 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 0, 1, 2, 3, 4 }));
                valuesHistory.Clear();

                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void WhereTest_ObservableParameter()
        {
            var mainSubject = new Subject<INotifyCollectionChangedEvent<Subject<string>>>();
            var valuesHistory = new List<INotifyCollectionChangedEvent<Subject<string>>>();
            var errorsHistory = new List<Exception>();
            var completed = false;

            mainSubject
                .ToStatuses(true)
                .Where(s => s.Select(str =>
                    {
                        return str.Last() != '\'';
                    }))
                .Subscribe(valuesHistory.Add, errorsHistory.Add, () => completed = true);
            valuesHistory.Is();

            var subject1 = new Subject<string>();
            var subject2 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));
            valuesHistory.Is();

            subject1.OnNext("1-a'");
            valuesHistory.Is();

            subject2.OnNext("2-a");
            valuesHistory.Single().InitialState.Items.Is(subject2);
            valuesHistory.Clear();

            subject1.OnNext("1-b");
            valuesHistory.Single().Added.StartingIndex.Is(0);
            valuesHistory.Single().Added.Items.Is(subject1);
            valuesHistory.Clear();

            subject1.OnNext("1-c");
            valuesHistory.Is();

            subject2.OnNext("2-b'");
            valuesHistory.Single().Removed.StartingIndex.Is(1);
            valuesHistory.Single().Removed.Items.Is(subject2);
            valuesHistory.Clear();

            subject2.OnNext("2-c");
            valuesHistory.Single().Added.StartingIndex.Is(1);
            valuesHistory.Single().Added.Items.Is(subject2);
            valuesHistory.Clear();

            var subject3 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject3 }, 2));
            valuesHistory.Is();

            subject1.OnNext("1-d'");
            valuesHistory.Is();

            subject3.OnNext("3-a");
            valuesHistory[0].Removed.StartingIndex.Is(0);
            valuesHistory[0].Removed.Items.Is(subject1);
            valuesHistory[1].Added.StartingIndex.Is(1);
            valuesHistory[1].Added.Items.Is(subject3);
            valuesHistory.Count.Is(2);
            valuesHistory.Clear();

            var subject4 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { subject3 }, new[] { subject4 }, 2));
            valuesHistory.Is();

            subject4.OnNext("4-a");
            valuesHistory.Single().Replaced.StartingIndex.Is(1);
            valuesHistory.Single().Replaced.OldItems.Is(subject3);
            valuesHistory.Single().Replaced.NewItems.Is(subject4);
            valuesHistory.Clear();

            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { subject4 }, 2));
            valuesHistory.Single().Removed.StartingIndex.Is(1);
            valuesHistory.Single().Removed.Items.Is(subject4);
            valuesHistory.Clear();

            subject1.OnNext("1-e");
            valuesHistory.Single().Added.StartingIndex.Is(0);
            valuesHistory.Single().Added.Items.Is(subject1);
            valuesHistory.Clear();

            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { subject1 }, 0, 1));
            valuesHistory.Single().Moved.OldStartingIndex.Is(0);
            valuesHistory.Single().Moved.NewStartingIndex.Is(1);
            valuesHistory.Single().Moved.Items.Is(subject1);
            valuesHistory.Clear();

            var subject5 = new Subject<string>();
            var subject6 = new Subject<string>();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { subject5, subject6 }));
            valuesHistory.Is();

            subject1.OnNext("2-x");
            valuesHistory.Is();

            subject5.OnNext("5-a'");
            valuesHistory.Is();

            subject6.OnNext("6-a");
            valuesHistory.Single().Reset.Items.Is(subject6);
            valuesHistory.Clear();

            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new Subject<string>[0]));
            valuesHistory.Single().Reset.Items.Is();
            valuesHistory.Clear();

            errorsHistory.Is();
            completed.IsFalse();
        }
    }
}
