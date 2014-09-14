using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    class ReadOnlyTaggedCollection<T> : IList<T>, IList<Tagged<T>>, IReadOnlyList<T>, IReadOnlyList<Tagged<T>>
    {
        readonly TaggedCollection<T> source;

        public ReadOnlyTaggedCollection(TaggedCollection<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            this.source = source;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
        }

        public int IndexOf(Tagged<T> item)
        {
            return source.IndexOf(item);
        }

        public void Insert(int index, Tagged<T> item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            source.RemoveAt(index);
        }

        public Tagged<T> this[int index]
        {
            get
            {
                return source[index];
            }
        }

        Tagged<T> IList<Tagged<T>>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        void ICollection<Tagged<T>>.Add(Tagged<T> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<Tagged<T>>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(Tagged<T> item)
        {
            return source.Contains(item);
        }

        public void CopyTo(Tagged<T>[] array, int arrayIndex)
        {
            source.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return source.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<Tagged<T>>.Remove(Tagged<T> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<Tagged<T>> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return source.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        T IReadOnlyList<T>.this[int index]
        {
            get
            {
                IList<T> temp = source;
                return temp[index];
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                IList<T> temp = source;
                return temp[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return source.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            source.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IList<T> temp = source;
            return temp.GetEnumerator();
        }
    }
}
