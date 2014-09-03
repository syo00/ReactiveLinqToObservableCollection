using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.Collections.ObjectModel;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal static partial class Extensions
    {
        public static void InsertRange<T>(this IList<T> source, int index, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(source.Count >= index);
            Contract.Requires<ArgumentNullException>(items != null);

            if (index > source.Count)
            {
                throw new InvalidOperationException("Insert のインデックスが範囲外です。");
            }
            else
            {
                foreach (var i in items)
                {
                    source.Insert(index, i);
                    index++;
                }
            }
        }

        public static ReadOnlyCollection<T> RemoveAtRange<T>(this IList<T> source, int index, int count)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(source.Count - count >= index);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>() != null);

            var result = new List<T>();
            foreach (var i in Enumerable.Range(0, count))
            {
                result.Add(source[index]);
                source.RemoveAt(index);
            }

            return new ReadOnlyCollection<T>(result);
        }

        public static ReadOnlyCollection<T> MoveRange<T>(this IList<T> source, int oldIndex, int newIndex, int count)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(oldIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(newIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(source.Count - count >= oldIndex);
            Contract.Requires<ArgumentOutOfRangeException>(source.Count - count >= newIndex);
            Contract.Requires<ArgumentOutOfRangeException>(count >= 0);

            var removed = RemoveAtRange(source, oldIndex, count);
            InsertRange(source, newIndex, removed);
            return new ReadOnlyCollection<T>(removed);
        }
    }
}
