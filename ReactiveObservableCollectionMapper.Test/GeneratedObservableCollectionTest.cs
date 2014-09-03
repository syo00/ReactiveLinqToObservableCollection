using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Kirinji.LinqToObservableCollection.Test
{
    [TestClass]
    public class GeneratedObservableCollectionTest
    {
        [TestMethod]
        public void GeneratedObservableCollectionAllTest()
        {
            {
                var coreSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var collection = coreSubject.ToStatuses(true).ToObservableCollection();
                var valuesHistory = new List<INotifyCollectionChangedEvent<int>>();
                Exception error = null;
                var isCompleted = false;
                collection.Statuses.Subscribe(valuesHistory.Add, ex => error = ex, () => isCompleted = true);

                collection.Count.Is(0);
                collection.IsInitialStateArrived.IsFalse();
                valuesHistory.Is();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { 1, 2, 3 }));
                collection.Is(1, 2, 3);
                collection.IsInitialStateArrived.IsTrue();
                valuesHistory.Single().InitialState.Items.Is(1, 2, 3);
                valuesHistory.Clear();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new int[] { 4 }, 3));
                collection.Is(1, 2, 3, 4);
                valuesHistory.Single().Added.Items.Is(4);
                valuesHistory.Single().Added.StartingIndex.Is(3);
                valuesHistory.Clear();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateRemovedEvent(new int[] { 4 }, 3));
                collection.Is(1, 2, 3);
                valuesHistory.Single().Removed.Items.Is(4);
                valuesHistory.Single().Removed.StartingIndex.Is(3);
                valuesHistory.Clear();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { 3 }, new[] { 4 }, 2));
                collection.Is(1, 2, 4);
                valuesHistory.Single().Replaced.OldItems.Is(3);
                valuesHistory.Single().Replaced.NewItems.Is(4);
                valuesHistory.Single().Replaced.StartingIndex.Is(2);
                valuesHistory.Clear();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { 1 }, 0, 1));
                collection.Is(2, 1, 4);
                valuesHistory.Single().Moved.Items.Is(1);
                valuesHistory.Single().Moved.OldStartingIndex.Is(0);
                valuesHistory.Single().Moved.NewStartingIndex.Is(1);
                valuesHistory.Clear();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 1, 2 }));
                collection.Is(1, 2);
                valuesHistory.Single().Reset.Items.Is(1, 2);
                valuesHistory.Clear();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { -1 , -2 }));
                collection.IsInitialStateArrived.IsTrue();
                collection.RaisedError.IsInstanceOf<InvalidInformationException<int>>();
                collection.IsCompleted.IsFalse();
                error.IsInstanceOf<InvalidInformationException<int>>();
                isCompleted.IsFalse();
            }

            {
                var coreSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var collection = coreSubject.ToStatuses(true).ToObservableCollection();
                var valuesHistory = new List<INotifyCollectionChangedEvent<int>>();
                Exception error = null;
                var isCompleted = false;
                collection.Statuses.Subscribe(valuesHistory.Add, ex => error = ex, () => isCompleted = true);

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateInitialStateEvent(new int[] { 1, 2, 3 }));
                coreSubject.OnCompleted();
                collection.IsCompleted.IsTrue();
                collection.RaisedError.IsNull();
                error.IsNull();
                isCompleted.IsTrue();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new int[] { -1 }, 1000));
                collection.IsCompleted.IsTrue();
                collection.RaisedError.IsNull();
                error.IsNull();
            }

            {
                var coreSubject = new Subject<INotifyCollectionChangedEvent<int>>();
                var collection = coreSubject.ToStatuses(true).ToObservableCollection();
                var valuesHistory = new List<INotifyCollectionChangedEvent<int>>();
                Exception error = null;
                var isCompleted = false;
                collection.Statuses.Subscribe(valuesHistory.Add, ex => error = ex, () => isCompleted = true);

                coreSubject.OnCompleted();
                collection.IsInitialStateArrived.IsFalse();
                collection.IsCompleted.IsTrue();
                collection.RaisedError.IsNull();
                error.IsNull();
                isCompleted.IsTrue();

                coreSubject.OnNext(NotifyCollectionChangedEvent.CreateAddedEvent(new int[] { -1 }, 1000));
                collection.IsInitialStateArrived.IsFalse();
                collection.IsCompleted.IsTrue();
                collection.RaisedError.IsNull();
                error.IsNull();
            }
        }
    }
}
