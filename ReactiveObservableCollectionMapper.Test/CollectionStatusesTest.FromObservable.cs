using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Kirinji.LinqToObservableCollection.Test
{
    public partial class CollectionStatusesTest
    {
        [TestMethod]
        public void SwitchTest()
        {
            var mainSubject = new Subject<Subject<INotifyCollectionChangedEvent<string>>>();
            var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
            var errorsHistory = new List<Exception>();
            var completed = false;
            int switchCount = 0;

            var subscription = mainSubject
                .Select(x => x.ToStatuses(true))
                .Switch((observer, initialState) =>
                    {
                        initialState.Items.Is("x", "y", "z");
                        if (switchCount == 0)
                        {
                            observer.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new string[0]));
                        }
                        else
                        {
                            observer.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "" }, 100000));
                        }
                        switchCount++;
                    })
                .Subscribe(valuesHistory.Add, errorsHistory.Add, () => completed = true);

            var subject1 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(subject1);
            subject1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
            valuesHistory.Single().InitialState.Items.Is("a", "b");
            valuesHistory.Clear();
            subject1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c" }, 2));
            valuesHistory.Single().Added.Items.Is("c");
            valuesHistory.Single().Added.StartingIndex.Is(2);
            valuesHistory.Clear();

            var subject2 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(subject2);
            valuesHistory.Is();
            subject2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "x", "y", "z" }));
            valuesHistory.Single().Reset.IsNotNull();
            valuesHistory.Clear();

            var subject3 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(subject3);
            valuesHistory.Is();
            subject3.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "x", "y", "z" }));
            valuesHistory.Is();
            errorsHistory.Single().IsInstanceOf<InvalidInformationException<string>>();
            completed.IsFalse();
        }


        [TestMethod]
        public void SwitchWithResetTest()
        {
            var mainSubject = new Subject<Subject<INotifyCollectionChangedEvent<string>>>();

            var collection = mainSubject
                .Select(x => x.ToStatuses(true))
                .SwitchWithReset()
                .ToObservableCollection();

            var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
            Exception error = null;
            var isCompleted = false;
            collection.Statuses.Subscribe(valuesHistory.Add, ex => error = ex, () => isCompleted = true);

            collection.IsInitialStateArrived.IsFalse();

            var events1 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(events1);
            collection.IsInitialStateArrived.IsFalse();

            events1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
            collection.Is("a", "b");

            events1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c", "d" }, 2));
            collection.Is("a", "b", "c", "d");

            valuesHistory.Clear();
            var events2 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(events2);
            collection.Is("a", "b", "c", "d");
            valuesHistory.Is();

            events2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "E", "B", "A" }));
            collection.Is("E", "B", "A");
            valuesHistory.Single().Reset.Items.Is("E", "B", "A");

            events1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new string[0]));
            collection.Is("E", "B", "A");

            events1.OnError(new Exception());
            collection.Is("E", "B", "A");
            collection.RaisedError.IsNull();
            error.IsNull();

            events2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "z" }, 1000));
            collection.RaisedError.IsInstanceOf<InvalidInformationException<string>>();
            error.IsInstanceOf<InvalidInformationException<string>>();
            isCompleted.IsFalse();
        }

        [TestMethod]
        public void SwitchWithMoveTest()
        {
            var mainSubject = new Subject<Subject<INotifyCollectionChangedEvent<string>>>();

            var collection = mainSubject
                .Select(x => x.ToStatuses(true))
                .SwitchWithMove(EqualityComparer.Create<string>((x, y) => x.ToUpperInvariant() == y.ToUpperInvariant()))
                .ToObservableCollection();

            var valuesHistory = new List<INotifyCollectionChangedEvent<string>>();
            Exception error = null;
            var isCompleted = false;
            collection.Statuses.Subscribe(valuesHistory.Add, ex => error = ex, () => isCompleted = true);

            collection.IsInitialStateArrived.IsFalse();

            var events1 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(events1);
            collection.IsInitialStateArrived.IsFalse();

            events1.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "a", "b" }));
            collection.Is("a", "b");

            events1.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "c", "d" }, 2));
            collection.Is("a", "b", "c", "d");

            var events2 = new Subject<INotifyCollectionChangedEvent<string>>();
            mainSubject.OnNext(events2);
            collection.Is("a", "b", "c", "d");

            valuesHistory.Clear();
            events2.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new[] { "E", "B", "A" }));
            collection.Is("E", "b", "a");
            valuesHistory.All(e => e.Action != NotifyCollectionChangedEventAction.Reset).IsTrue();

            events1.OnNext(NotifyCollectionChangedEvent.CreateResetEvent<string>(new string[0]));
            collection.Is("E", "b", "a");

            events1.OnError(new Exception());
            collection.Is("E", "b", "a");
            collection.RaisedError.IsNull();
            error.IsNull();

            events2.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "z" }, 1000));
            collection.RaisedError.IsInstanceOf<InvalidInformationException<string>>();
            error.IsInstanceOf<InvalidInformationException<string>>();
            isCompleted.IsFalse();
        }
    }
}

