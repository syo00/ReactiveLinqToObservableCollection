using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    sealed class ZipFilledProducer<TSource1, TSource2> : CombineProducer<TSource1, TSource2, INotifyCollectionChangedEvent<Zipped<TSource1, TSource2>>>
    {
        IReadOnlyList<TSource1> prevItems1;
        IReadOnlyList<TSource2> prevItems2;

        public ZipFilledProducer(CollectionStatuses<TSource1> source1, CollectionStatuses<TSource2> source2)
            : base(source1.ConvertToSlimInitialStateAndChanged(), source2.ConvertToSlimInitialStateAndChanged(), new SchedulingAndThreading[0])
        {
            Contract.Requires<ArgumentNullException>(source1 != null);
            Contract.Requires<ArgumentNullException>(source2 != null);
        }

        protected override IEnumerable<INotifyCollectionChangedEvent<Zipped<TSource1, TSource2>>> ConvertInitialState(IReadOnlyList<TSource1> initialLeftCollection, IReadOnlyList<TSource2> initialRightCollection)
        {
            SavePrevItems();
            var items = ZipFilled(initialLeftCollection, initialRightCollection, 0);
            yield return NotifyCollectionChangedEvent.CreateInitialStateEvent(items);
        }

        protected override IEnumerable<INotifyCollectionChangedEvent<Zipped<TSource1, TSource2>>> ConvertLeftChanged(NotifyCollectionChangedEventObject<TSource1> leftEvent)
        {
            switch(leftEvent.SlimOne.Action)
            {
                case NotifyCollectionChangedEventAction.Add:
                    {
                        var index = leftEvent.SlimOne.Added.StartingIndex;
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        var index = leftEvent.SlimOne.Removed.StartingIndex;
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        var index = leftEvent.SlimOne.Replaced.StartingIndex;
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        var index = Min(leftEvent.SlimOne.Moved.OldStartingIndex, leftEvent.SlimOne.Moved.NewStartingIndex);
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }

            SavePrevItems();
        }

        protected override IEnumerable<INotifyCollectionChangedEvent<Zipped<TSource1, TSource2>>> ConvertRightChanged(NotifyCollectionChangedEventObject<TSource2> rightEvent)
        {
            switch (rightEvent.SlimOne.Action)
            {
                case NotifyCollectionChangedEventAction.Add:
                    {
                        var index = rightEvent.SlimOne.Added.StartingIndex;
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        var index = rightEvent.SlimOne.Removed.StartingIndex;
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        var index = rightEvent.SlimOne.Replaced.StartingIndex;
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        var index = Min(rightEvent.SlimOne.Moved.OldStartingIndex, rightEvent.SlimOne.Moved.NewStartingIndex);
                        var oldItems = ZipFilled(prevItems1, prevItems2, index);
                        var newItems = ZipFilled(LeftCollection, RightCollection, index);
                        yield return NotifyCollectionChangedEvent.CreateReplacedEvent(oldItems, newItems, index);
                        break;
                    }
                default:
                    throw Exceptions.UnpredictableSwitchCasePattern;
            }

            SavePrevItems();
        }

        protected override IEnumerable<INotifyCollectionChangedEvent<Zipped<TSource1, TSource2>>> ConvertLeftReset(IReadOnlyList<TSource1> leftReset)
        {
            SavePrevItems();
            var items = ZipFilled(LeftCollection, RightCollection, 0);
            yield return NotifyCollectionChangedEvent.CreateResetEvent(items);
        }

        protected override IEnumerable<INotifyCollectionChangedEvent<Zipped<TSource1, TSource2>>> ConvertRightReset(IReadOnlyList<TSource2> rightReset)
        {
            SavePrevItems();
            var items = ZipFilled(LeftCollection, RightCollection, 0);
            yield return NotifyCollectionChangedEvent.CreateResetEvent(items);
        }

        private void SavePrevItems()
        {
            prevItems1 = LeftCollection.ToArray<TSource1>();
            prevItems2 = RightCollection.ToArray<TSource2>();
        }

        private static int Max(int i, int j)
        {
            return new[] { i, j }.Max();
        }

        private static int Min(int i, int j)
        {
            return new[] { i, j }.Min();
        }

        private static IReadOnlyList<Zipped<TSource1, TSource2>> ZipFilled(IReadOnlyList<TSource1> collection1, IReadOnlyList<TSource2> collection2, int skipCount)
        {
            Contract.Requires<ArgumentNullException>(collection1 != null);
            Contract.Requires<ArgumentNullException>(collection2 != null);
            Contract.Requires<ArgumentOutOfRangeException>(skipCount >= 0);
            Contract.Ensures(Contract.Result<IReadOnlyList<Zipped<TSource1, TSource2>>>() != null);

            var result = new List<Zipped<TSource1, TSource2>>();
            var loopCount = Max(collection1.Count, collection2.Count) - skipCount;
            if (loopCount <= 0)
            {
                return result.ToReadOnly();
            }
            foreach (var index in Enumerable.Range(skipCount, loopCount))
            {
                var a = collection1.Count > index ? new ValueOrEmpty<TSource1>(collection1[index]) : new ValueOrEmpty<TSource1>();
                var b = collection2.Count > index ? new ValueOrEmpty<TSource2>(collection2[index]) : new ValueOrEmpty<TSource2>();
                result.Add(new Zipped<TSource1, TSource2>(a, b));
            }

            return result.ToReadOnly();
        }
    }

    internal sealed class Zipped<TSource1, TSource2> : IEquatable<Zipped<TSource1, TSource2>>
    {
        public Zipped(TSource1 value1)
        {
            this.Value1 = new ValueOrEmpty<TSource1>(value1);
        }

        public Zipped(TSource2 value2)
        {
            this.Value2 = new ValueOrEmpty<TSource2>(value2);
        }

        public Zipped(TSource1 value1, TSource2 value2)
        {
            this.Value1 = new ValueOrEmpty<TSource1>(value1);
            this.Value2 = new ValueOrEmpty<TSource2>(value2);
        }

        public Zipped(ValueOrEmpty<TSource1> value1, ValueOrEmpty<TSource2> value2)
        {
            this.Value1 = value1;
            this.Value2 = value2;
        }

        public ValueOrEmpty<TSource1> Value1 { get; private set; }
        public ValueOrEmpty<TSource2> Value2 { get; private set; }

        public bool Equals(Zipped<TSource1, TSource2> other)
        {
            if(other == null)
            {
                return false;
            }

            return Value1.Equals(other.Value1) && Value2.Equals(other.Value2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Zipped<TSource1, TSource2>);
        }

        public override int GetHashCode()
        {
            return Value1.GetHashCode() ^ Value2.GetHashCode();
        }
    }
}
