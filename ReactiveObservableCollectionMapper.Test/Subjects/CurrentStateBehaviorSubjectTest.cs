using Kirinji.LinqToObservableCollection;
using Kirinji.LinqToObservableCollection.Subjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Subjects
{
    [TestClass]
    public class CurrentStateBehaviorSubjectTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CurrentStateBehaviorSubjectTest_OnNext()
        {
            var subject = new CurrentStateBehaviorSubject<string>();
            var firstSubscriptionHistory = new List<INotifyCollectionChangedEvent<string>>();
            var secondSubscriptionHistory = new List<INotifyCollectionChangedEvent<string>>();
            var thirdSubscriptionHistory = new List<INotifyCollectionChangedEvent<string>>();

            var firstSubscription = subject.InitialStateAndChanged.Subscribe(firstSubscriptionHistory.Add);
            firstSubscriptionHistory.Is();
            subject.IsInitialStateArrived.IsFalse();


            subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));

            subject.IsInitialStateArrived.IsTrue();
            subject.CurrentState.Is("a", "b");

            firstSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.InitialState);
            firstSubscriptionHistory.Single().InitialState.Items.Is("a", "b");
            firstSubscriptionHistory.Clear();

            subject.InitialStateAndChanged.Subscribe(secondSubscriptionHistory.Add);
            secondSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.InitialState);
            secondSubscriptionHistory.Single().InitialState.Items.Is("a", "b");
            secondSubscriptionHistory.Clear();


            subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));

            subject.IsInitialStateArrived.IsTrue();
            subject.CurrentState.Is("a", "b", "c");

            firstSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.Add);
            firstSubscriptionHistory.Single().Added.Items.Is("c");
            firstSubscriptionHistory.Single().Added.StartingIndex.Is(2);
            firstSubscriptionHistory.Clear();

            secondSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.Add);
            secondSubscriptionHistory.Single().Added.Items.Is("c");
            secondSubscriptionHistory.Single().Added.StartingIndex.Is(2);
            secondSubscriptionHistory.Clear();

            subject.InitialStateAndChanged.Subscribe(thirdSubscriptionHistory.Add);
            thirdSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.InitialState);
            thirdSubscriptionHistory.Single().InitialState.Items.Is("a", "b", "c");
            thirdSubscriptionHistory.Clear();


            firstSubscription.Dispose();
            subject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "c" }, 2));

            subject.IsInitialStateArrived.IsTrue();
            subject.CurrentState.Is("a", "b");

            firstSubscriptionHistory.Is();

            secondSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.Remove);
            secondSubscriptionHistory.Single().Removed.Items.Is("c");
            secondSubscriptionHistory.Single().Removed.StartingIndex.Is(2);
            secondSubscriptionHistory.Clear();

            thirdSubscriptionHistory.Single().Action.Is(NotifyCollectionChangedEventAction.Remove);
            thirdSubscriptionHistory.Single().Removed.Items.Is("c");
            thirdSubscriptionHistory.Single().Removed.StartingIndex.Is(2);
            thirdSubscriptionHistory.Clear();


            subject.Dispose();
            AssertEx.Catch<ObjectDisposedException>(() => subject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new[] { "" })));
        }

        [TestMethod]
        public void CurrentStateBehaviorSubjectTest_OnError()
        {
            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var completed = false;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completed = true);
                subject.OnError(new DivideByZeroException());
                subject.OnError(new BadImageFormatException());
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completed = true);

                valuesHistory1.Is();
                errorsHistory1.Single().IsInstanceOf<DivideByZeroException>();
                valuesHistory2.Is();
                errorsHistory2.Single().IsInstanceOf<DivideByZeroException>();
                completed.IsFalse();
            }

            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory3 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var errorsHistory3 = new List<Exception>();
                var completed = false;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "a", "b" }, 0));
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completed = true);
                subject.OnError(new DivideByZeroException());
                subject.Subscribe(valuesHistory3.Add, errorsHistory3.Add, () => completed = true);

                subject.IsInitialStateArrived.IsFalse();
                subject.CurrentState.Is();
                valuesHistory1.Is();
                errorsHistory1.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
                valuesHistory2.Is();
                errorsHistory2.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
                valuesHistory3.Is();
                errorsHistory3.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
                completed.IsFalse();
            }

            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory3 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var errorsHistory3 = new List<Exception>();
                var completed = false;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completed = true);
                subject.OnError(new DivideByZeroException());
                subject.OnError(new BadImageFormatException());
                subject.Subscribe(valuesHistory3.Add, errorsHistory3.Add, () => completed = true);

                subject.IsInitialStateArrived.IsTrue();
                subject.CurrentState.Is("a", "b");
                valuesHistory1.Single().InitialState.Items.Is("a", "b");
                errorsHistory1.Single().IsInstanceOf<DivideByZeroException>();
                valuesHistory2.Single().InitialState.Items.Is("a", "b");
                errorsHistory2.Single().IsInstanceOf<DivideByZeroException>();
                valuesHistory3.Single().InitialState.Items.Is("a", "b");
                errorsHistory3.Single().IsInstanceOf<DivideByZeroException>();
                completed.IsFalse();
            }

            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory3 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var errorsHistory3 = new List<Exception>();
                var completed = false;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completed = true);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "INVALID EVENT" }));
                subject.OnError(new BadImageFormatException());
                subject.Subscribe(valuesHistory3.Add, errorsHistory3.Add, () => completed = true);

                subject.IsInitialStateArrived.IsTrue();
                subject.CurrentState.Is("a", "b", "c");
                valuesHistory1.Count.Is(2);
                errorsHistory1.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
                valuesHistory2.Single().InitialState.Items.Is("a", "b", "c");
                errorsHistory2.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
                valuesHistory3.Single().InitialState.Items.Is("a", "b", "c");
                errorsHistory3.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
                completed.IsFalse();
            }
        }

        [TestMethod]
        public void CurrentStateBehaviorSubjectTest_OnCompleted()
        {
            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var completedCount1 = 0;
                var completedCount2 = 0;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completedCount1++);
                subject.OnCompleted();
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
                subject.OnError(new DivideByZeroException());
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completedCount2++);

                subject.IsInitialStateArrived.IsFalse();
                subject.CurrentState.Is();
                valuesHistory1.Is();
                errorsHistory1.Is();
                completedCount1.Is(1);
                valuesHistory2.Is();
                errorsHistory2.Is();
                completedCount2.Is(1);
            }

            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory3 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var errorsHistory3 = new List<Exception>();
                var completedCount1 = 0;
                var completedCount2 = 0;
                var completedCount3 = 0;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completedCount1++);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completedCount2++);
                subject.OnCompleted();
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
                subject.OnError(new DivideByZeroException());
                subject.Subscribe(valuesHistory3.Add, errorsHistory3.Add, () => completedCount3++);

                subject.IsInitialStateArrived.IsTrue();
                subject.CurrentState.Is("a", "b");
                valuesHistory1.Single().InitialState.Items.Is("a", "b");
                errorsHistory1.Is();
                completedCount1.Is(1);
                valuesHistory2.Single().InitialState.Items.Is("a", "b");
                errorsHistory2.Is();
                completedCount2.Is(1);
                valuesHistory3.Single().InitialState.Items.Is("a", "b");
                errorsHistory3.Is();
                completedCount3.Is(1);
            }

            {
                var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
                var valuesHistory3 = new List<INotifyCollectionChangedEvent<string>>();
                var errorsHistory1 = new List<Exception>();
                var errorsHistory2 = new List<Exception>();
                var errorsHistory3 = new List<Exception>();
                var completedCount1 = 0;
                var completedCount2 = 0;
                var completedCount3 = 0;

                var subject = new CurrentStateBehaviorSubject<string>();
                subject.Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => completedCount1++);
                subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
                subject.Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => completedCount2++);
                subject.OnCompleted();
                subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "d" }, 3));
                subject.OnError(new DivideByZeroException());
                subject.Subscribe(valuesHistory3.Add, errorsHistory3.Add, () => completedCount3++);

                subject.IsInitialStateArrived.IsTrue();
                subject.CurrentState.Is("a", "b", "c");
                valuesHistory1.Count.Is(2);
                errorsHistory1.Is();
                completedCount1.Is(1);
                valuesHistory2.Single().InitialState.Items.Is("a", "b", "c");
                errorsHistory2.Is();
                completedCount2.Is(1);
                valuesHistory3.Single().InitialState.Items.Is("a", "b", "c");
                errorsHistory3.Is();
                completedCount3.Is(1);
            }
        }
    }
}
