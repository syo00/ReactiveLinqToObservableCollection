using Kirinji.LinqToObservableCollection.Impl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    public static class TestLogic
    {
        internal static void CollectionStatusesCheckAll<T>(Func<CollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            CollectionStatusesCheck1(testedFactory, map);
            CollectionStatusesCheck2(testedFactory, map);
            CollectionStatusesCheckAdded(testedFactory, map);
            CollectionStatusesCheckRemoved(testedFactory, map);
            CollectionStatusesCheckMoved(testedFactory, map);
            CollectionStatusesCheckReplaced(testedFactory, map);
            CollectionStatusesCheckReset(testedFactory, map);
        }

        // 文字列の長さを用いた Where を意識したチェック。
        internal static void CollectionStatusesCheck1<T>(Func<CollectionBasedCollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var source = new ObservableCollection<string> { "beatmania", "pop'n music", "jubeat" };
            var root = new CollectionBasedCollectionStatuses<string>(source);
            var r = testedFactory(root);
            var first = r.ToObservableCollection();

            Action check = () =>
            {
                var mapped = map(source).ToArray();

                r
                    .InitialStateAndChanged
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(10))
                    .Wait()
                    .InitialState
                    .Items
                    .Is(mapped);
                r
                    .ToObservableCollection()
                    .Is(mapped);
                first.Is(mapped);
            };

            source.Is("beatmania", "pop'n music", "jubeat");
            check();

            source.Add("REFLEC BEAT");
            source.Is("beatmania", "pop'n music", "jubeat", "REFLEC BEAT");
            check();

            source.Add("GITADORA");
            source.Is("beatmania", "pop'n music", "jubeat", "REFLEC BEAT", "GITADORA");
            check();

            source[0] = "beatmania IIDX";
            source.Is("beatmania IIDX", "pop'n music", "jubeat", "REFLEC BEAT", "GITADORA");
            check();

            source.Move(0, 2);
            source.Is("pop'n music", "jubeat", "beatmania IIDX", "REFLEC BEAT", "GITADORA");
            check();

            source.Move(1, 3);
            source.Is("pop'n music", "beatmania IIDX", "REFLEC BEAT", "jubeat", "GITADORA");
            check();

            source.Move(1, 2);
            source.Is("pop'n music", "REFLEC BEAT", "beatmania IIDX", "jubeat", "GITADORA");
            check();

            source.Move(0, 3);
            source.Is("REFLEC BEAT", "beatmania IIDX", "jubeat", "pop'n music", "GITADORA");
            check();

            source.Move(1, 1);
            source.Is("REFLEC BEAT", "beatmania IIDX", "jubeat", "pop'n music", "GITADORA");
            check();

            source.Move(3, 0);
            source.Is("pop'n music", "REFLEC BEAT", "beatmania IIDX", "jubeat", "GITADORA");
            check();

            source.Move(2, 1);
            source.Is("pop'n music", "beatmania IIDX", "REFLEC BEAT", "jubeat", "GITADORA");
            check();

            source.Move(3, 1);
            source.Is("pop'n music", "jubeat", "beatmania IIDX", "REFLEC BEAT", "GITADORA");
            check();

            source.Move(2, 0);
            source.Is("beatmania IIDX", "pop'n music", "jubeat", "REFLEC BEAT", "GITADORA");
            check();

            source.RemoveAt(1);
            source.Is("beatmania IIDX", "jubeat", "REFLEC BEAT", "GITADORA");
            check();

            source.Add("SOUND VOLTEX");
            source.RemoveAt(3);
            source.Is("beatmania IIDX", "jubeat", "REFLEC BEAT", "SOUND VOLTEX");
            check();

            source.RemoveAt(2);
            source.Is("beatmania IIDX", "jubeat", "SOUND VOLTEX");
            check();

            source.RemoveAt(0);
            source.Is("jubeat", "SOUND VOLTEX");
            check();

            source.RemoveAt(1);
            source.Is("jubeat");
            check();

            source.Clear();
            source.Is();
            check();
        }

        // OrderBy を意識したチェック。
        internal static void CollectionStatusesCheck2<T>(Func<CollectionBasedCollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var source = new ObservableCollection<string> { "name: Saitoh", "name: Yamada", "name: Ono" };
            var root = new CollectionBasedCollectionStatuses<string>(source);
            var r = testedFactory(root);
            var first = r.ToObservableCollection();

            Action check = () =>
            {
                r.InitialStateAndChanged.FirstAsync().Wait().InitialState.Items.Is(map(source));
                r.ToObservableCollection().Is(map(source));
                first.Is(map(source));
            };

            source.Is("name: Saitoh", "name: Yamada", "name: Ono");
            check();

            source.Add("name: Tanaka");
            source.Is("name: Saitoh", "name: Yamada", "name: Ono", "name: Tanaka");
            check();

            source.Add("name: Konaka");
            source.Is("name: Saitoh", "name: Yamada", "name: Ono", "name: Tanaka", "name: Konaka");
            check();

            source[0] = "name: Satoh";
            source.Is("name: Satoh", "name: Yamada", "name: Ono", "name: Tanaka", "name: Konaka");
            check();

            source[1] = "name: Hamada";
            source.Is("name: Satoh", "name: Hamada", "name: Ono", "name: Tanaka", "name: Konaka");
            check();

            source[0] = "name: Saitoh";
            source[1] = "name: Yamada";
            source.Is("name: Saitoh", "name: Yamada", "name: Ono", "name: Tanaka", "name: Konaka");
            check();

            source.Move(0, 2);
            source.Is("name: Yamada", "name: Ono", "name: Saitoh", "name: Tanaka", "name: Konaka");
            check();

            source.Move(2, 4);
            source.Is("name: Yamada", "name: Ono", "name: Tanaka", "name: Konaka", "name: Saitoh");
            check();

            source.Move(0, 4);
            source.Is("name: Ono", "name: Tanaka", "name: Konaka", "name: Saitoh", "name: Yamada");
            check();

            source.Move(1, 3);
            source.Is("name: Ono", "name: Konaka", "name: Saitoh", "name: Tanaka", "name: Yamada");
            check();

            source.Move(3, 1);
            source.Is("name: Ono", "name: Tanaka", "name: Konaka", "name: Saitoh", "name: Yamada");
            check();

            source.Move(4, 0);
            source.Is("name: Yamada", "name: Ono", "name: Tanaka", "name: Konaka", "name: Saitoh");
            check();

            source.Move(4, 2);
            source.Is("name: Yamada", "name: Ono", "name: Saitoh", "name: Tanaka", "name: Konaka");
            check();

            source.Move(2, 0);
            source.Is("name: Saitoh", "name: Yamada", "name: Ono", "name: Tanaka", "name: Konaka");
            check();

            source.RemoveAt(4);
            source.Is("name: Saitoh", "name: Yamada", "name: Ono", "name: Tanaka");
            check();

            source.RemoveAt(0);
            source.Is("name: Yamada", "name: Ono", "name: Tanaka");
            check();

            source.RemoveAt(1);
            source.Is("name: Yamada", "name: Tanaka");
            check();

            source.Clear();
            source.Is();
            check();
        }

        internal static void CollectionStatusesCheckAdded<T>(Func<CollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var events = new List<INotifyCollectionChangedEvent<string>>();

            var items = new string[0];

            CollectionStatuses<string> source;
            ICollectionStatuses<T> converted;

            events.Add(NotifyCollectionChangedEvent.CreateInitialStateEvent(items));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "IMMORAL", "Abyss" }, -1));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { "IMMORAL", "Abyss" };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "blossomdays", "birthday eve" }, 2));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "砂の城 -The Castle of Sand", "アナタだけのAngel☆" }, 0));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆",
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateAddedEvent(new[] { "常識!バトラー行進曲", "Face of Fact" }, 1));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "砂の城 -The Castle of Sand", 
                "常識!バトラー行進曲",  
                "Face of Fact",
                "アナタだけのAngel☆",
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));
        }

        internal static void CollectionStatusesCheckRemoved<T>(Func<CollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var events = new List<INotifyCollectionChangedEvent<string>>();

            var items = new[] { 
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };

            CollectionStatuses<string> source;
            ICollectionStatuses<T> converted;

            events.Add(NotifyCollectionChangedEvent.CreateInitialStateEvent(items));
            source = new AnonymousCollectionStatuses<string>(events.ToObservable()).ToInstance();
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "常識!バトラー行進曲", "Face of Fact" }, 6));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "IMMORAL", "Abyss", "blossomdays" }, 0));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "Leaf Ticket", "SHIFT -世代の向こう" }, -1));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "For our days",
                "Collective",
                "Princess Brave!",
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateRemovedEvent(new[] { "Collective", "Princess Brave!" }, 4));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] { 
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "For our days",
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));
        }

        internal static void CollectionStatusesCheckMoved<T>(Func<CollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var events = new List<INotifyCollectionChangedEvent<string>>();

            var items = new[] { 
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };

            CollectionStatuses<string> source;
            ICollectionStatuses<T> converted;

            events.Add(NotifyCollectionChangedEvent.CreateInitialStateEvent(items));
            source = new AnonymousCollectionStatuses<string>(events.ToObservable());
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "砂の城 -The Castle of Sand", "アナタだけのAngel☆" }, 4, 7));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss",
                "blossomdays",
                "birthday eve",
                "常識!バトラー行進曲",
                "Face of Fact",
                "For our days",
                "砂の城 -The Castle of Sand",
                "アナタだけのAngel☆",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "砂の城 -The Castle of Sand", "アナタだけのAngel☆" }, 7, 4));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "砂の城 -The Castle of Sand", "アナタだけのAngel☆" }, 4, 5));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "常識!バトラー行進曲", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "砂の城 -The Castle of Sand", "アナタだけのAngel☆" }, 5, 4));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "IMMORAL", "Abyss" }, 0, 3));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "IMMORAL",
                "Abyss", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "IMMORAL", "Abyss" }, 3, 0));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "Face of Fact", "For our days" }, 7, 11));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう",
                "Face of Fact", 
                "For our days"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateMovedEvent(new[] { "Face of Fact", "For our days" }, 11, 7));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
                "Face of Fact", 
                "For our days",
                "Collective",
                "Princess Brave!",
                "Leaf Ticket",
                "SHIFT -世代の向こう"
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));
        }

        internal static void CollectionStatusesCheckReplaced<T>(Func<CollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var events = new List<INotifyCollectionChangedEvent<string>>();

            var items = new[] { 
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
            };

            CollectionStatuses<string> source;
            ICollectionStatuses<T> converted;

            events.Add(NotifyCollectionChangedEvent.CreateInitialStateEvent(items));
            source = new AnonymousCollectionStatuses<string>(events.ToObservable()).ToInstance();
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "Abyss", "blossomdays", "birthday eve" }, new[] { "Face of Fact", "For our days" }, 1));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "IMMORAL",
                "Face of Fact", 
                "For our days",
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "IMMORAL", "Face of Fact" }, new[] { "Collective", "Princess Brave!" }, 0));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "Collective",
                "Princess Brave!", 
                "For our days",
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateReplacedEvent(new[] { "アナタだけのAngel☆", "常識!バトラー行進曲" }, new[] { "Leaf Ticket", "SHIFT -世代の向こう", "Abyss" }, 4));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "Collective",
                "Princess Brave!", 
                "For our days",
                "砂の城 -The Castle of Sand", 
                "Leaf Ticket", 
                "SHIFT -世代の向こう",
                "Abyss" 
            };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));
        }


        internal static void CollectionStatusesCheckReset<T>(Func<CollectionStatuses<string>, ICollectionStatuses<T>> testedFactory, Func<IEnumerable<string>, IEnumerable<T>> map)
        {
            Contract.Requires<ArgumentNullException>(testedFactory != null);
            Contract.Requires<ArgumentNullException>(map != null);

            var events = new List<INotifyCollectionChangedEvent<string>>();

            var items = new[] { 
                "IMMORAL",
                "Abyss", 
                "blossomdays",
                "birthday eve", 
                "砂の城 -The Castle of Sand", 
                "アナタだけのAngel☆", 
                "常識!バトラー行進曲", 
            };

            CollectionStatuses<string> source;
            ICollectionStatuses<T> converted;

            events.Add(NotifyCollectionChangedEvent.CreateInitialStateEvent(items));
            source = new AnonymousCollectionStatuses<string>(events.ToObservable()).ToInstance();
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateResetEvent(new[] { 
                "Collective",
                "Princess Brave!", 
                "For our days",
                "砂の城 -The Castle of Sand", 
                "Leaf Ticket", 
                "SHIFT -世代の向こう",
                "Abyss"  }));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new[] {
                "Collective",
                "Princess Brave!", 
                "For our days",
                "砂の城 -The Castle of Sand", 
                "Leaf Ticket", 
                "SHIFT -世代の向こう",
                "Abyss" };
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));

            events.Add(NotifyCollectionChangedEvent.CreateResetEvent(new string[0]));
            source = CollectionStatuses.Create(events.ToObservable()).ToInstance();
            items = new string[0];
            converted = testedFactory(source);
            converted.ToObservableCollection().Is(map(items));
        }
    }
}
