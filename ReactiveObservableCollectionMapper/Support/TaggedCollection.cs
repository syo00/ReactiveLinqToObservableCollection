using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal class TaggedCollection<T> : IList<T>, IList<Tagged<T>>, IReadOnlyList<T>, IReadOnlyList<Tagged<T>>
    {
        private readonly IList<ListItem> core = new List<ListItem>();

        public int IndexOf(Tagged<T> item)
        {
            return core.FirstIndex(elem => Object.Equals(elem.Tag, item.Tag) && Object.Equals(elem.Item, item.Item)) ?? -1;
        }

        public void Insert(int index, Tagged<T> item)
        {
            core.Insert(index, ListItem.Create(item));
        }

        public void RemoveAt(int index)
        {
            core.RemoveAt(index);
        }

        public Tagged<T> this[int index]
        {
            get
            {
                return core[index].ToTagged();
            }
            set
            {
                core[index] = ListItem.Create(value);
            }
        }

        Tagged<T> IReadOnlyList<Tagged<T>>.this[int index]
        {
            get
            {
                return core[index].ToTagged();
            }
        }

        public void Add(Tagged<T> item)
        {
            core.Add(ListItem.Create(item));
        }

        public void Clear()
        {
            core.Clear();
        }

        public bool Contains(Tagged<T> item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(Tagged<T>[] array, int arrayIndex)
        {
            core.Select(elem => elem.ToTagged()).ToArray().CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return core.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Tagged<T> item)
        {
            var removingIndex = IndexOf(item);
            if (removingIndex < 0) return false;
            RemoveAt(removingIndex);
            return true;
        }

        public IEnumerator<Tagged<T>> GetEnumerator()
        {
            return core.Select(elem => elem.ToTagged()).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return core.FirstIndex(elem => Object.Equals(elem.Item, item)) ?? -1;
        }

        public void Insert(int index, T item)
        {
            core.Insert(index, new ListItem { Item = item });
        }

        T IList<T>.this[int index]
        {
            get
            {
                return core[index].Item;
            }
            set
            {
                core[index] = new ListItem { Item = value };
            }
        }

        T IReadOnlyList<T>.this[int index]
        {
            get
            {
                return core[index].Item;
            }
        }

        public void Add(T item)
        {
            core.Add(new ListItem { Item = item });
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            core.Select(elem => elem.Item).ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var removingIndex = IndexOf(item);
            if (removingIndex < 0) return false;
            RemoveAt(removingIndex);
            return true;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return core.Select(elem => elem.Item).GetEnumerator();
        }

        public void ClearTags()
        {
            core.ForEach(elem => elem.Tag = null);
        }

        public ReadOnlyTaggedCollection<T> ToReadOnly()
        {
            Contract.Ensures(Contract.Result<ReadOnlyTaggedCollection<T>>() != null);

            return new ReadOnlyTaggedCollection<T>(this);
        }

        private class ListItem
        {
            public object Tag { get; set; }

            public T Item { get; set; }

            public static ListItem Create(Tagged<T> tagged)
            {
                Contract.Requires<ArgumentNullException>(tagged != null);
                Contract.Ensures(Contract.Result<ListItem>() != null);

                return new ListItem { Item = tagged.Item, Tag = tagged.Tag };
            }

            public Tagged<T> ToTagged()
            {
                Contract.Ensures(Contract.Result<Tagged<T>>() != null);

                if (Tag == null)
                {
                    Tag = new object();
                }
                return new Tagged<T>(Item, Tag);
            }
        }
    }
}