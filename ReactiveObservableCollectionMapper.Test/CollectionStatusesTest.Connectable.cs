using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Moq;
using System.Reactive.Disposables;
using System.Reactive;

namespace Kirinji.LinqToObservableCollection.Test
{
    public partial class CollectionStatusesTest
    {
        [TestMethod]
        public void PublishTest()
        {
            // 現時点では CurrentStateBehaviorSubject に大きく依存している実装なので、このテストコードはサボり気味

            var mainSubject = new Subject<INotifyCollectionChangedEvent<string>>();
            var history1 = new List<INotifyCollectionChangedEvent<KeyValuePair<object, string>>>();
            var history2 = new List<INotifyCollectionChangedEvent<KeyValuePair<object, string>>>();
            var history3 = new List<INotifyCollectionChangedEvent<KeyValuePair<object, string>>>();

            var testingObservable = mainSubject
                .ToStatuses(true)
                .Select(x => new KeyValuePair<object, string>(new object(), x))
                .Publish();

            testingObservable.Subscribe(history1.Add);
            var collection1 = testingObservable.ToObservableCollection();
            testingObservable.Connect();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
            testingObservable.Subscribe(history2.Add);
            var collection2 = testingObservable.ToObservableCollection();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
            testingObservable.Subscribe(history3.Add);
            var collection3 = testingObservable.ToObservableCollection();

            history1[0].Action.Is(NotifyCollectionChangedEventAction.InitialState);
            history1[1].Action.Is(NotifyCollectionChangedEventAction.Add);
            history2[0].Action.Is(NotifyCollectionChangedEventAction.InitialState);
            history2[1].Action.Is(NotifyCollectionChangedEventAction.Add);
            history3.Single().Action.Is(NotifyCollectionChangedEventAction.InitialState);
            collection1.Is(collection2.AsEnumerable());
            collection1.Is(collection3.AsEnumerable());
        }

        [TestMethod]
        public void ReplayTest()
        {
            // 現時点では ReplaySubject に大きく依存している実装なので、このテストコードはサボり気味

            var mainSubject = new Subject<INotifyCollectionChangedEvent<string>>();
            var history1 = new List<INotifyCollectionChangedEvent<KeyValuePair<object, string>>>();
            var history2 = new List<INotifyCollectionChangedEvent<KeyValuePair<object, string>>>();
            var history3 = new List<INotifyCollectionChangedEvent<KeyValuePair<object, string>>>();

            var testingObservable = mainSubject
                .ToStatuses(true)
                .Select(x => new KeyValuePair<object, string>(new object(), x))
                .Replay();

            testingObservable.Subscribe(history1.Add);
            var collection1 = testingObservable.ToObservableCollection();
            testingObservable.Connect();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
            testingObservable.Subscribe(history2.Add);
            var collection2 = testingObservable.ToObservableCollection();
            mainSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
            testingObservable.Subscribe(history3.Add);
            var collection3 = testingObservable.ToObservableCollection();

            history1[0].Action.Is(NotifyCollectionChangedEventAction.InitialState);
            history1[1].Action.Is(NotifyCollectionChangedEventAction.Add);
            history2[0].Action.Is(NotifyCollectionChangedEventAction.InitialState);
            history2[1].Action.Is(NotifyCollectionChangedEventAction.Add);
            history3[0].Action.Is(NotifyCollectionChangedEventAction.InitialState);
            history3[1].Action.Is(NotifyCollectionChangedEventAction.Add);
            collection1.Is(collection2.AsEnumerable());
            collection1.Is(collection3.AsEnumerable());
        }

        [TestMethod]
        public void RefCountTest()
        {
            bool? disposed = null;
            var subject = new Subject<INotifyCollectionChangedEvent<string>>();
            
            var valuesHistory1 = new List<INotifyCollectionChangedEvent<string>>();
            var errorsHistory1 = new List<Exception>();
            var onCompletedHistory1 = new List<Unit>();

            var valuesHistory2 = new List<INotifyCollectionChangedEvent<string>>();
            var errorsHistory2 = new List<Exception>();
            var onCompletedHistory2 = new List<Unit>();

            var valuesHistory3 = new List<INotifyCollectionChangedEvent<string>>();
            var errorsHistory3 = new List<Exception>();
            var onCompletedHistory3 = new List<Unit>();

            var mock = new Mock<IConnectableCollectionStatuses<string>>();
            mock.Setup(x => x.Connect()).Returns(() => 
                {
                    disposed = false;
                    return Disposable.Create(() => disposed = true);
                });
            var returns =
                subject
                .ToStatuses(true)
                .InitialStateAndChanged;
            mock.SetupGet(x => x.InitialStateAndChanged).Returns(returns);

            // call RefCount
            var observable = mock.Object.RefCount();
            disposed.IsNull();

            // call Subscribe (1)
            var subscription1 = observable
                .Subscribe(valuesHistory1.Add, errorsHistory1.Add, () => onCompletedHistory1.Add(Unit.Default));
            valuesHistory1.Is();
            disposed.Value.IsFalse();

            // call Subscribe (2)
            var subscription2 = observable
                .Subscribe(valuesHistory2.Add, errorsHistory2.Add, () => onCompletedHistory2.Add(Unit.Default));
            valuesHistory1.Is();
            valuesHistory2.Is();

            // push InitialState
            subject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
            valuesHistory1.Single().InitialState.Items.Is("a", "b");
            valuesHistory2.Single().InitialState.Items.Is("a", "b");
            valuesHistory1.Clear();
            valuesHistory2.Clear();

            // call Subscribe (3)
            var subscription3 = observable
                .Subscribe(valuesHistory3.Add, errorsHistory3.Add, () => onCompletedHistory3.Add(Unit.Default));
            valuesHistory1.Is();
            valuesHistory2.Is();
            valuesHistory3.Is();

            // push Added
            subject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
            valuesHistory1.Single().Added.Items.Is("c");
            valuesHistory1.Single().Added.StartingIndex.Is(2);
            valuesHistory2.Single().Added.Items.Is("c");
            valuesHistory2.Single().Added.StartingIndex.Is(2);
            valuesHistory3.Is();
            errorsHistory3.Single().IsInstanceOf<InvalidInformationException<string>>().Type.Is(InvalidInformationExceptionType.NotFollowingEventSequenceRule);
            valuesHistory1.Clear();
            valuesHistory2.Clear();
            errorsHistory3.Clear();

            // unsubscribe subscription2 and subscription3
            subscription2.Dispose();
            subscription3.Dispose();
            valuesHistory1.Is();
            valuesHistory2.Is();
            valuesHistory3.Is();

            // unsubscribe subscription1
            subscription1.Dispose();
            valuesHistory1.Is();
            valuesHistory2.Is();
            valuesHistory3.Is();
            errorsHistory1.Is();
            errorsHistory2.Is();
            errorsHistory3.Is();
            disposed.Value.IsTrue();
        }
    }
}
