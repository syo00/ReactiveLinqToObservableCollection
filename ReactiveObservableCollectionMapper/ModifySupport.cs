using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Modification
{
    internal static class ModifySupport
    {
        /// <summary>prev を next に近づけるための prev に対応した Moved を返します。Moved を適用した場合、prev にあって next にない要素は最後に移動します。</summary>
        static IEnumerable<IMoved<T>> CheckMoves<T>(IReadOnlyCollection<T> prev, IReadOnlyCollection<T> next, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(prev != null);
            Contract.Requires<ArgumentNullException>(next != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            var indexedPrev = IndexesCount(prev, comparer).ToList();
            var indexedNext = IndexesCount(next, comparer);
            var moveCount = 0;

            foreach (var n in indexedNext)
            {
                var matchedIndexInPrev = indexedPrev.FirstIndex(pair => pair.Value == n.Value && comparer(pair.Key, n.Key));

                if(matchedIndexInPrev != null)
                {
                    var moved = NotifyCollectionChangedEvent.CreateMoved(new[] { indexedPrev[matchedIndexInPrev.Value].Key }.ToReadOnly(), matchedIndexInPrev.Value, moveCount);
                    moveCount++;
                    if (moved.OldStartingIndex != moved.NewStartingIndex)
                    {
                        indexedPrev.MoveRange(moved.OldStartingIndex, moved.NewStartingIndex, 1);
                        yield return moved;
                    }
                }
            }
        }

        /// <summary>prev を next に近づけるための prev に対応した Added を返します。</summary>
        static IEnumerable<IAdded<T>> CheckAdds<T>(IReadOnlyCollection<T> prev, IReadOnlyCollection<T> next, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(prev != null);
            Contract.Requires<ArgumentNullException>(next != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            using (var nextIterator = next.GetEnumerator())
            {
                var prevIndex = 0;
                var addedCount = 0;
                foreach (var prevItem in prev)
                {
                    while (true)
                    {
                        if (!nextIterator.MoveNext())
                        {
                            throw new InvalidOperationException("prev.Intersect(next).SequenceEqual(prev) == true でなければなりません。");
                        }

                        if (comparer(nextIterator.Current, prevItem))
                        {
                            break;
                        }
                        else
                        {
                            yield return NotifyCollectionChangedEvent.CreateAdded(new[] { nextIterator.Current }, prevIndex + addedCount);
                            addedCount++;
                        }
                    }

                    prevIndex++;
                }

                while (nextIterator.MoveNext())
                {
                    yield return NotifyCollectionChangedEvent.CreateAdded(new[] { nextIterator.Current }, prevIndex + addedCount);
                    addedCount++;
                }
            }
        }

        /// <summary>prev を next に近づけるための prev に対応した Removed を返します。</summary>
        static IEnumerable<IRemoved<T>> CheckRemoves<T>(IReadOnlyCollection<T> prev, IReadOnlyCollection<T> next, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(prev != null);
            Contract.Requires<ArgumentNullException>(next != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            var indexedNext = IndexesCount(next, comparer);
            var i = 0;
            foreach (var p in IndexesCount(prev, comparer))
            {
                var matchedIndexedNext = indexedNext.Any(pair => pair.Value == p.Value && comparer(pair.Key, p.Key));

                if (!matchedIndexedNext)
                {
                    yield return NotifyCollectionChangedEvent.CreateRemoved(new[] { p.Key }, i);
                }
                else
                {
                    i++;
                }
            }
        }

        static IReadOnlyList<KeyValuePair<T, int>> IndexesCount<T>(IEnumerable<T> source, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<T, int>>>() != null);

            var result = new List<KeyValuePair<T, int>>();
            foreach (var s in source)
            {
                var matched = result.LastOrNull(pair => comparer(s, pair.Key)); // 重い
                if (matched != null)
                {
                    result.Add(new KeyValuePair<T, int>(s, matched.Value.Value + 1));
                }
                else
                {
                    result.Add(new KeyValuePair<T, int>(s, 1));
                }
            }
            return result;
        }

        /// <summary>一つ目のコレクションを二つ目のコレクションと一致させるのに必要な操作を NotifyCollectionChangedEvent の形で返します。</summary>
        public static IReadOnlyList<INotifyCollectionChangedEvent<T>> CreateEventsToGetEquality<T>(IEnumerable<T> compared, IReadOnlyCollection<T> comparing, IEqualityComparer<T> equalityComparer = null)
        {
            Contract.Requires<ArgumentNullException>(compared != null);
            Contract.Requires<ArgumentNullException>(comparing != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<INotifyCollectionChangedEvent<T>>>() != null);

            var equalityComparerNotNull = equalityComparer ?? EqualityComparer<T>.Default;

            var comparedCopy = new List<T>(compared);

            var removes = CheckRemoves(comparedCopy, comparing, equalityComparerNotNull.Equals).ToArray();
            foreach (var r in removes)
            {
                if (r != null)
                {
                    comparedCopy.RemoveAtRange(r.StartingIndex, r.Items.Count);
                }
            }

            var moves = CheckMoves(comparedCopy, comparing, equalityComparerNotNull.Equals).ToArray();
            foreach (var m in moves)
            {
                if (m != null)
                {
                    comparedCopy.MoveRange(m.OldStartingIndex, m.NewStartingIndex, m.Items.Count);
                }
            }

            var adds = CheckAdds(comparedCopy, comparing, equalityComparerNotNull.Equals).ToArray();

            return removes.Select(r => NotifyCollectionChangedEvent.ToEvent(r))
                .Concat(moves.Select(m => NotifyCollectionChangedEvent.ToEvent(m)))
                .Concat(adds.Select(a => NotifyCollectionChangedEvent.ToEvent(a)))
                .ToArray()
                .ToReadOnly();
        }
    }
}
