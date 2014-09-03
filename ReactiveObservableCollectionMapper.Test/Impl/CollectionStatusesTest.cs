using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.Impl;
using System.Linq;
using System.Reactive.Linq;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    // AnonymousCollectionStatusesTest との兼用
    [TestClass]
    public class CollectionStatusesTest
    {
        [TestMethod]
        public void AddedTest()
        {
            var obs = new[]
                {
                    NotifyCollectionChangedEvent.CreateInitialStateEvent(new [] { 10, 20, 30, 40 } ),
                    NotifyCollectionChangedEvent.CreateAddedEvent(new[]{ 50, 60 }, 4),
                    NotifyCollectionChangedEvent.CreateAddedEvent(new[]{ 70, 80 }, -1),
                    NotifyCollectionChangedEvent.CreateAddedEvent(new[]{ -20, -10, 0 }, 0),
                    NotifyCollectionChangedEvent.CreateAddedEvent(new[]{ 31, 32 }, 6),
                };

            CollectionStatuses.Create(obs.Take(1).ToObservable())
                .ToObservableCollection()
                .Is(10, 20, 30, 40);

            CollectionStatuses.Create(obs.Take(2).ToObservable())
               .ToObservableCollection()
               .Is(10, 20, 30, 40, 50, 60);

            CollectionStatuses.Create(obs.Take(3).ToObservable())
               .ToObservableCollection()
               .Is(10, 20, 30, 40, 50, 60, 70, 80);

            CollectionStatuses.Create(obs.Take(4).ToObservable())
               .ToObservableCollection()
               .Is(-20, -10, 0, 10, 20, 30, 40, 50, 60, 70, 80);

            CollectionStatuses.Create(obs.ToObservable())
               .ToObservableCollection()
               .Is(-20, -10, 0, 10, 20, 30, 31, 32, 40, 50, 60, 70, 80);
        }

        [TestMethod]
        public void RemovedTest()
        {
            var obs = new[]
                {
                    NotifyCollectionChangedEvent.CreateInitialStateEvent(new [] { 0, 10, 20, 30, 40, 50, 60, 70, 80 } ),
                    NotifyCollectionChangedEvent.CreateRemovedEvent(new[]{ 0, 10 }, 0),
                    NotifyCollectionChangedEvent.CreateRemovedEvent(new[]{ 70, 80 }, 5),
                    NotifyCollectionChangedEvent.CreateRemovedEvent(new[]{ 30, 40, 50 }, 1),
                };

            CollectionStatuses.Create(obs.Take(1).ToObservable())
                .ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);

            CollectionStatuses.Create(obs.Take(2).ToObservable())
                .ToObservableCollection()
                .Is(20, 30, 40, 50, 60, 70, 80);

            CollectionStatuses.Create(obs.Take(3).ToObservable())
               .ToObservableCollection()
               .Is(20, 30, 40, 50, 60);

            CollectionStatuses.Create(obs.ToObservable())
               .ToObservableCollection()
               .Is(20, 60);
        }

        [TestMethod]
        public void MovedTest()
        {
            var obs = new[]
                {
                    NotifyCollectionChangedEvent.CreateInitialStateEvent(new [] { 0, 10, 20, 30, 40, 50, 60, 70, 80 } ),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 0, 10 }, 0, 2),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 40, 50 }, 4, 7),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 40, 50 }, 7, 4),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 0, 10 }, 2, 0),

                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 10, 20, 30 }, 1, 3),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 10, 20, 30 }, 3, 1),

                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 10, 20, 30, 40 }, 1, 3),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 10, 20, 30, 40 }, 3, 1),

                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 20, 30, 40 }, 2, 4),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 20, 30, 40 }, 4, 2),

                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 10, 20, 30, 40 }, 1, 4),
                    NotifyCollectionChangedEvent.CreateMovedEvent(new[]{ 10, 20, 30, 40 }, 4, 1),
                };

            var a1 = CollectionStatuses.Create(obs.Take(1).ToObservable());
            a1.ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);

            var a2 = CollectionStatuses.Create(obs.Take(2).ToObservable());
            a2.ToObservableCollection()
                .Is(20, 30, 0, 10, 40, 50, 60, 70, 80);

            var a3 = CollectionStatuses.Create(obs.Take(3).ToObservable());
            a3.ToObservableCollection()
                .Is(20, 30, 0, 10, 60, 70, 80, 40, 50);

            var a4 = CollectionStatuses.Create(obs.Take(4).ToObservable());
            a4.ToObservableCollection()
                .Is(20, 30, 0, 10, 40, 50, 60, 70, 80);

            var a5 = CollectionStatuses.Create(obs.Take(5).ToObservable());
            a5.ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);


            var a6 = CollectionStatuses.Create(obs.Take(6).ToObservable());
            a6.ToObservableCollection()
                .Is(0, 40, 50, 10, 20, 30, 60, 70, 80);

            var a7 = CollectionStatuses.Create(obs.Take(7).ToObservable());
            a7.ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);


            var a8 = CollectionStatuses.Create(obs.Take(8).ToObservable());
            a8.ToObservableCollection()
                .Is(0, 50, 60, 10, 20, 30, 40, 70, 80);

            var a9 = CollectionStatuses.Create(obs.Take(9).ToObservable());
            a9.ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);


            var a10 = CollectionStatuses.Create(obs.Take(10).ToObservable());
            a10.ToObservableCollection()
                .Is(0, 10, 50, 60, 20, 30, 40, 70, 80);

            var a11 = CollectionStatuses.Create(obs.Take(11).ToObservable());
            a11.ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);


            var a12 = CollectionStatuses.Create(obs.Take(12).ToObservable());
            a12.ToObservableCollection()
                .Is(0, 50, 60, 70, 10, 20, 30, 40, 80);

            var a13 = CollectionStatuses.Create(obs.Take(13).ToObservable());
            a13.ToObservableCollection()
                .Is(0, 10, 20, 30, 40, 50, 60, 70, 80);
        }
    }
}
