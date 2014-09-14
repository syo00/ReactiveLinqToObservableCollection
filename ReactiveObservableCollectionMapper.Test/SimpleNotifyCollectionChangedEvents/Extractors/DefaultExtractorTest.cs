using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LinqToObservableCollection.Support.Extractors;

namespace Kirinji.LinqToObservableCollection.Test.SimpleNotifyCollectionChangedEvents.Extractors
{
    [TestClass]
    public class DefaultExtractorTest
    {
        [TestMethod]
        public void ExtractTest()
        {
            var tag = Enumerable.Range(0, 100).Select(i => new Tagged<int>(i)).ToArray();

            var extractor = new DefaultEventExtractor<int>();
            
            var result = new MultiValuesObservableCollection<int>();
            var events = new Subject<SimpleNotifyCollectionChangedEvent<int>>();
            events.Select(x => extractor.Extract(x)).SelectMany(x => x).Subscribe(result.ApplyChangeEvent);
            result.Count.Is(0);

            var initialState = new[] { 1, 2, 3 };
            events.OnNext(SimpleNotifyCollectionChangedEvent<int>.CreateInitialState(initialState));
            result.Is(1, 2, 3);

            var event1 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[4], 3));
            events.OnNext(event1);
            result.Is(1, 2, 3, 4);

            var event2 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[4], 3));
            events.OnNext(event2);
            result.Is(1, 2, 3);

            var event3_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[1], 0);
            var event3_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[1], 2);
            var event3 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event3_1, event3_2 });
            events.OnNext(event3);
            result.Is(2, 3, 1);

            var event4_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[1], 2);
            var event4_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[1], 0);
            var event4 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event4_1, event4_2 });
            events.OnNext(event4);
            result.Is(1, 2, 3);

            var event5_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[1], 0);
            var event5_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[2], 0);
            var event5_3 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[1], 1);
            var event5_4 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[2], 2);
            var event5 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event5_1, event5_2, event5_3, event5_4 });
            events.OnNext(event5);
            result.Is(3, 1, 2);

            var event6_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[1], 1);
            var event6_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[2], 1);
            var event6_3 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[1], 0);
            var event6_4 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[2], 1);
            var event6 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event6_1, event6_2, event6_3, event6_4 });
            events.OnNext(event6);
            result.Is(1, 2, 3);

            var event7_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[1], 0); // => [2, 3]
            var event7_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[4], 2); // => [2, 3, 4]
            var event7_3 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[2], 0); // => [3, 4]
            var event7_4 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[5], 2); // => [3, 4, 5]
            var event7_5 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[5], 2); // => [3, 4]
            var event7_6 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[6], 0); // => [6, 3, 4]
            var event7_7 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[4], 2); // => [6, 3]
            var event7_8 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[7], 1); // => [6, 7, 3]
            var event7 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event7_1, event7_2, event7_3, event7_4, event7_5, event7_6, event7_7, event7_8 });
            events.OnNext(event7);
            result.Is(6, 7, 3);

            var event8_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[7], 1); // => [6, 3]
            var event8_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[4], 2); // => [6, 3, 4]
            var event8_3 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[6], 0); // => [3, 4]
            var event8_4 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[5], 2); // => [3, 4, 5]
            var event8_5 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[5], 2); // => [3, 4]
            var event8_6 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[2], 0); // => [2, 3, 4]
            var event8_7 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[4], 2); // => [2, 3]
            var event8_8 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[1], 0); // => [1, 2, 3]
            var event8 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event8_1, event8_2, event8_3, event8_4, event8_5, event8_6, event8_7, event8_8 });
            events.OnNext(event8);
            result.Is(1, 2, 3);
            
            var event9_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[4], 3); // => [1, 2, 3, 4]
            var event9_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[1], 0); // => [2, 3, 4]
            var event9_3 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[5], 3); // => [2, 3, 4, 5]
            var event9_4 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[2], 0); // => [3, 4, 5]
            var event9_5 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[6], 3); // => [3, 4, 5, 6]
            var event9_6 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[6], 3); // => [3, 4, 5]
            var event9_7 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[7], 1); // => [3, 7, 4, 5]
            var event9_8 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[5], 3); // => [3, 7, 4]
            var event9_9 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[8], 2); // => [3, 7, 8, 4]
            var event9 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event9_1, event9_2, event9_3, event9_4, event9_5, event9_6, event9_7, event9_8, event9_9 });
            events.OnNext(event9);
            result.Is(3, 7, 8, 4);

            var event10_1 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[7], 1); // => [3, 8, 4]
            var event10_2 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[6], 1); // => [3, 6, 8, 4]
            var event10_3 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Remove, tag[6], 1); // => [3, 8, 4]
            var event10_4 = new AddedOrRemovedUnit<int>(AddOrRemoveUnitType.Add, tag[5], 1); // => [3, 5, 8, 4]
            var event10 = SimpleNotifyCollectionChangedEvent<int>.CreateAddOrRemove(new[] { event10_1, event10_2, event10_3, event10_4 });
            events.OnNext(event10);
            result.Is(3, 5, 8, 4);
        }
    }
}
