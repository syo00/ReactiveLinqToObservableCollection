using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    sealed class SequenceEqualProducer<TSource1, TSource2> : CombineProducer<TSource1, TSource2, bool>
    {
        readonly Func<TSource1, TSource2, bool> comparer;

        public SequenceEqualProducer(CollectionStatuses<TSource1> source1, CollectionStatuses<TSource2> source2, Func<TSource1, TSource2, bool> comparer)
            : base(source1, source2, new SchedulingAndThreading[0])
        {
            Contract.Requires<ArgumentNullException>(source1 != null);
            Contract.Requires<ArgumentNullException>(source2 != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.comparer = comparer;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(comparer != null);
        }

        protected override IEnumerable<bool> ConvertInitialState(IReadOnlyList<TSource1> initialLeftCollection, IReadOnlyList<TSource2> initialRightCollection)
        {
            yield return Check();
        }

        protected override IEnumerable<bool> ConvertLeftChanged(NotifyCollectionChangedEventObject<TSource1> leftEvent)
        {
            yield return Check();
        }

        protected override IEnumerable<bool> ConvertRightChanged(NotifyCollectionChangedEventObject<TSource2> rightEvent)
        {
            yield return Check();
        }

        protected override IEnumerable<bool> ConvertLeftReset(IReadOnlyList<TSource1> leftReset)
        {
            yield return Check();
        }

        protected override IEnumerable<bool> ConvertRightReset(IReadOnlyList<TSource2> rightReset)
        {
            yield return Check();
        }

        bool Check()
        {
            if (LeftCollection.Count != RightCollection.Count)
            {
                return false;
            }

            foreach (var index in Enumerable.Range(0, LeftCollection.Count))
            {
                if (!comparer(LeftCollection[index].Item, RightCollection[index].Item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
