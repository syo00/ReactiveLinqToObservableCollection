using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.Impl;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Kirinji.LinqToObservableCollection.Test
{
    [TestClass]
    public partial class CollectionStatusesTest
    {
        [TestMethod]
        public void Aggregate1Test()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<ValueOrEmpty<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Aggregate((x, y) => x + y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "c", "o", "w" }));
                valuesHistory.Single().Value.Is("cow");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "b", "o", "y" }, 3));
                valuesHistory.Single().Value.Is("cowboy");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "b", "o", "y" }, 3));
                valuesHistory.Single().Value.Is("cow");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "c" }, new[] { "r" }, 0));
                valuesHistory.Single().Value.Is("row");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "r" }, 0, 1));
                valuesHistory.Single().Value.Is("orw");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new string[0]));
                valuesHistory.Single().HasValue.IsFalse();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "r", "e", "s", "e", "t" }));
                valuesHistory.Single().Value.Is("reset");
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
                var valuesHistory = new List<ValueOrEmpty<string>>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Aggregate((x, y) => x + y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "c", "o", "w" }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void Aggregate2Test()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<string>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Aggregate("+", (x, y) => x + y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "c", "o", "w" }));
                valuesHistory.Single().Is("+cow");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "b", "o", "y" }, 3));
                valuesHistory.Single().Is("+cowboy");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "b", "o", "y" }, 3));
                valuesHistory.Single().Is("+cow");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "c" }, new[] { "r" }, 0));
                valuesHistory.Single().Is("+row");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "r" }, 0, 1));
                valuesHistory.Single().Is("+orw");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new string[0]));
                valuesHistory.Single().Is("+");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "r", "e", "s", "e", "t" }));
                valuesHistory.Single().Is("+reset");
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
                var valuesHistory = new List<string>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .Aggregate("-", (x, y) => x + y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "c", "o", "w" }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void AllTest()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .All(i => i >= 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 0, 1, 2 }));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { -1 }, 3));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { -1 }, 3));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 0 }, new[] { -1 }, 0));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { -1 }, 0, 1));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new int[] { -1 }));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new int[0]));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                completed.IsFalse();
                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                var completed = false;

                subject
                    .ToStatuses(true)
                    .All(i => i >= 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 0, 1, 2 }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void Any1Test()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Any()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[] { }));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "b", "c" }, 0));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "b" }, 0, 1));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "c" }, new[] { "C" }, 0));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "C", "b" }, 0));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "b", "c" }, 0));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new string[0]));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "" }));
                valuesHistory.Single().Is(true);
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
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Any()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[] { }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void Any2Test()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Any(i => i >= 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { -1, 1 }, 0));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { -1 }, 0, 1));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 1 }, new[] { -2 }, 0));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { -1 }, 1));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { -2, 1 }, 0));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new int[0]));
                valuesHistory.Single().Is(false);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { -1, 1 }));
                valuesHistory.Single().Is(true);
                valuesHistory.Clear();

                completed.IsFalse();
                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Any(i => i >= 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void CombineLatestTest()
        {
            {
                var subject = new Subject<INotifyCollectionChangedEvent<IObservable<string>>>();
                var valuesHistory = new List<IReadOnlyList<string>>();
                Exception error = null;
                var completed = false;

                var subscription = subject
                    .ToStatuses(true)
                    .CombineLatest()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                var subject1 = new Subject<string>();
                var subject2 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));
                valuesHistory.Is();

                subject1.OnNext("1-a");
                valuesHistory.Is();

                subject2.OnNext("2-a");
                valuesHistory.Single().Is("1-a", "2-a");
                valuesHistory.Clear();

                var subject3 = new Subject<string>();
                var subject4 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject3, subject4 }, 2));
                valuesHistory.Is();

                subject3.OnNext("3-a");
                valuesHistory.Is();

                subject4.OnNext("4-a");
                valuesHistory.Single().Is("1-a", "2-a", "3-a", "4-a");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { subject2, subject3 }, 1, 2));
                valuesHistory.Single().Is("1-a", "4-a", "2-a", "3-a");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { subject4 }, 1));
                valuesHistory.Single().Is("1-a", "2-a", "3-a");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { subject3 }, new[] { subject3, subject4 }, 2));
                valuesHistory.Is();

                subject4.OnNext("4-a");
                valuesHistory.Is();

                subject3.OnNext("3-a");
                valuesHistory.Single().Is("1-a", "2-a", "3-a", "4-a");
                valuesHistory.Clear();

                var subject5 = new Subject<string>();
                var subject6 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { subject5, subject6 }));
                valuesHistory.Is();

                subject5.OnNext("5-a");
                valuesHistory.Is();

                subject6.OnNext("6-a");
                valuesHistory.Single().Is("5-a", "6-a");
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new Subject<string>[0]));
                valuesHistory.Single().Is();
                valuesHistory.Clear();

                subject.OnCompleted();
                valuesHistory.Is();
                error = null;
                completed.IsTrue();
            }


            {
                var subject = new Subject<INotifyCollectionChangedEvent<IObservable<string>>>();
                var valuesHistory = new List<IReadOnlyList<string>>();
                Exception error = null;
                var completed = false;

                var subscription = subject
                    .ToStatuses(true)
                    .CombineLatest()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                var subject1 = new Subject<string>();
                var subject2 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));
                subject1.OnNext("1-a");
                subject2.OnNext("2-a");
                valuesHistory.Clear();

                subject.OnError(new DivideByZeroException());
                valuesHistory.Is();
                error.IsInstanceOf<DivideByZeroException>();
                completed.IsFalse();
            }


            {
                var subject = new Subject<INotifyCollectionChangedEvent<IObservable<string>>>();
                var valuesHistory = new List<IReadOnlyList<string>>();
                Exception error = null;
                var completed = false;

                var subscription = subject
                    .ToStatuses(true)
                    .CombineLatest()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                var subject1 = new Subject<string>();
                var subject2 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));
                subject1.OnNext("1-a");
                subject2.OnNext("2-a");
                valuesHistory.Clear();

                subject1.OnError(new DivideByZeroException());
                error.IsInstanceOf<DivideByZeroException>();
                completed.IsFalse();
            }


            {
                var subject = new Subject<INotifyCollectionChangedEvent<IObservable<string>>>();
                var valuesHistory = new List<IReadOnlyList<string>>();
                Exception error = null;
                var completed = false;

                var subscription = subject
                    .ToStatuses(true)
                    .CombineLatest()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                var subject1 = new Subject<string>();
                var subject2 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { subject1, subject2 }));
                subject1.OnNext("1-a");
                subject2.OnNext("2-a");
                valuesHistory.Clear();

                subject1.OnCompleted();
                subject2.OnNext("2-b");
                valuesHistory.Single().Is("1-a", "2-b");
                valuesHistory.Clear();

                subject2.OnCompleted();
                valuesHistory.Is();

                var subject3 = new Subject<string>();
                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { subject2 }, new[] { subject3 }, 1));
                valuesHistory.Is();

                subject3.OnNext("3-a");
                valuesHistory.Single().Is("1-a", "3-a");

                error.IsNull();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void Count1Test()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<string>>();
                var valuesHistory = new List<int>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Count()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new string[] { }));
                valuesHistory.Single().Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "b", "c" }, 0));
                valuesHistory.Single().Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "b" }, 0, 1));
                valuesHistory.Single().Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "c" }, new[] { "C", "D" }, 0));
                valuesHistory.Single().Is(3);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "C" }, 0));
                valuesHistory.Single().Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { "d" }));
                valuesHistory.Single().Is(1);
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
                var valuesHistory = new List<int>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Count()
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void Count2Test()
        {
            // OnNext & OnCompleted test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<int>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Count(i => i >= 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                valuesHistory.Single().Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { -1, 1, 2 }, 0));
                valuesHistory.Single().Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { -1 }, 0, 1));
                valuesHistory.Single().Is(2);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 1 }, new[] { -2 }, 0));
                valuesHistory.Single().Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { 2 }, 2));
                valuesHistory.Single().Is(0);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { -2, 1 }, 0));
                valuesHistory.Single().Is(1);
                valuesHistory.Clear();

                subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { -1, 1, 2 }));
                valuesHistory.Single().Is(2);
                valuesHistory.Clear();

                completed.IsFalse();
                subject.OnCompleted();
                valuesHistory.Is();
                error.IsNull();
                completed.IsTrue();
            }

            // OnError test
            {
                var subject = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<int>();
                Exception error = null;
                bool completed = false;

                subject
                    .ToStatuses(true)
                    .Count(i => i >= 0)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                valuesHistory.Clear();

                error.IsNull();
                subject.OnError(new BadImageFormatException());
                valuesHistory.Is();
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void MergeTest()
        {
            var source = new Subject<INotifyCollectionChangedEvent<IObservable<string>>>();
            var subject1 = new Subject<string>();
            var subject2 = new BehaviorSubject<string>("2-0");
            var subject3 = new Subject<string>();

            var result = new List<string>();
            source
                .ToStatuses(true)
                .Merge()
                .Subscribe(result.Add);
            result.Is();

            source.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new IObservable<string>[] { subject1, subject2 }));
            result.Is("2-0");
            result.Clear();

            subject1.OnNext("1-1");
            subject2.OnNext("2-1");
            result.Is("1-1", "2-1");
            result.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { subject3 }, -2));
            subject1.OnNext("1-2");
            subject2.OnNext("2-2");
            subject3.OnNext("3-2");
            result.Is("1-2", "2-2", "3-2");
            result.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { subject3 }, 2));
            subject1.OnNext("1-3");
            subject2.OnNext("2-3");
            subject3.OnNext("3-3");
            result.Is("1-3", "2-3");
            result.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new IObservable<string>[] { subject2 }, new IObservable<string>[] { subject3 }, 1));
            subject1.OnNext("1-4");
            subject2.OnNext("2-4");
            subject3.OnNext("3-4");
            result.Is("1-4", "3-4");
            result.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new IObservable<string>[] { subject3 }, 1, 0));
            subject1.OnNext("1-5");
            subject2.OnNext("2-5");
            subject3.OnNext("3-5");
            result.Is("1-5", "3-5");
            result.Clear();

            source.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { subject1 }));
            subject1.OnNext("1-6");
            subject2.OnNext("2-6");
            subject3.OnNext("3-6");
            result.Is("1-6");
            result.Clear();

            AssertEx.Catch<Exception>(() => source.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new IObservable<string>[] { subject1 })));
        }

        [TestMethod]
        public void SequenceEqualTest()
        {
            // OnNext & OnCompleted(subject1 -> subject2) test
            {
                var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject1
                    .ToStatuses(true)
                    .SequenceEqual(subject2.ToStatuses(true), (x, y) => x == y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                valuesHistory.Is();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 1, 2 }, 0));
                valuesHistory.Is();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { 1, 2 }));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 3, 4 }, 2));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { 3, 4 }, 2));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 1 }, 0, 1));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 1 }, 1, 0));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 1 }, new[] { 2 }, 0));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 2 }, new[] { 1 }, 0));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 1 }));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 1, 2 }));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { 3, 4 }, 2));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { 3, 4 }, 2));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 1 }, 0, 1));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 1 }, 1, 0));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 1 }, new[] { 2 }, 0));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 2 }, new[] { 1 }, 0));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 1 }));
                valuesHistory.Single().IsFalse();
                valuesHistory.Clear();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 1, 2 }));
                valuesHistory.Single().IsTrue();
                valuesHistory.Clear();

                subject1.OnCompleted();
                completed.IsFalse();

                subject2.OnCompleted();
                completed.IsTrue();
                error.IsNull();
            }

            // OnNext & OnCompleted(subject2 -> subject1) test
            {
                var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject1
                    .ToStatuses(true)
                    .SequenceEqual(subject2.ToStatuses(true), (x, y) => x == y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));

                subject2.OnCompleted();
                completed.IsFalse();

                subject1.OnCompleted();
                completed.IsTrue();
                error.IsNull();
            }

            // OnError test(subject1)
            {
                var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject1
                    .ToStatuses(true)
                    .SequenceEqual(subject2.ToStatuses(true), (x, y) => x == y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));

                subject1.OnError(new BadImageFormatException());
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();

                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
            }

            // OnError test(subject2)
            {
                var subject1 = new Subject<INotifyCollectionChangedEvent<int>>();
                var subject2 = new Subject<INotifyCollectionChangedEvent<int>>();
                var valuesHistory = new List<bool>();
                Exception error = null;
                bool completed = false;

                subject1
                    .ToStatuses(true)
                    .SequenceEqual(subject2.ToStatuses(true), (x, y) => x == y)
                    .Subscribe(valuesHistory.Add, ex => error = ex, () => completed = true);
                valuesHistory.Is();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
                subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));

                subject2.OnError(new BadImageFormatException());
                error.IsInstanceOf<BadImageFormatException>();
                completed.IsFalse();

                subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { }));
            }
        }
    }
}
