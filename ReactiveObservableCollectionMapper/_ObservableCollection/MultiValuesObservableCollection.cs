using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public class MultiValuesObservableCollection<T> : ObservableCollection<T>
    {
        public MultiValuesObservableCollection()
            : base()
        {
            
        }

        public MultiValuesObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
            Contract.Requires<ArgumentNullException>(collection != null);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {

        }

        public void AddRange(IEnumerable<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);

            InsertItems(Count, items);
        }

        public void Insert(int index, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(Count >= index);
            Contract.Requires<ArgumentNullException>(items != null);

            InsertItems(index, items);
        }

        public new T Move(int oldIndex, int newIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(oldIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(newIndex >= 0);

            return MoveItems(oldIndex, newIndex, 1).Single();
        }

        public IReadOnlyList<T> Move(int oldIndex, int newIndex, int movingItemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(oldIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(newIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(movingItemsCount >= 0);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            return MoveItems(oldIndex, newIndex, movingItemsCount);
        }

        public new T RemoveAt(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(Count - 1 >= index);
            
            return RemoveItems(index, 1).Single();
        }

        public IReadOnlyList<T> RemoveAt(int index, int removingCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(removingCount >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(Count - removingCount >= index);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            return RemoveItems(index, removingCount);
        }

        // 例えば this === [1, 2, 3], index == 1, items === [10, 20] のとき、this は
        // (1). [1, 10, 20, 3]
        // (2). [1, 10, 20]
        // のどちらになるべきか？本メソッドでは (1) としている
        //
        // 戻り値は削除された要素
        public IReadOnlyList<T> Set(int index, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(Count - 1 >= index);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            return ReplaceItems(index, 1, items);
        }

        // 戻り値は削除された要素
        public IReadOnlyList<T> Replace(int index, int removingItemsCount, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(removingItemsCount >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(Count - removingItemsCount >= index);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            return ReplaceItems(index, removingItemsCount, items);
        }

        public void Reset(IEnumerable<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);

            ResetItems(newItems);
        }

        protected virtual void InsertItems(int index, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(Count >= index);

            CheckReentrancy();

            var itemsList = new ReadOnlyCollection<T>(items.ToList());

            Items.InsertRange(index, itemsList);
            
            if (itemsList.Count >= 1)
            {
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsList, index));
            }
        }

        protected virtual IReadOnlyList<T> RemoveItems(int index, int removingItemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(removingItemsCount >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(Count - removingItemsCount >= index);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            CheckReentrancy();

            var removed = Items.RemoveAtRange(index, removingItemsCount);

            if (removed.Count >= 1)
            {
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
            }

            return removed;
        }

        // 戻り値は削除された要素
        protected virtual IReadOnlyList<T> ReplaceItems(int index, int removingItemsCount, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(removingItemsCount >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(Count - removingItemsCount >= index);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);
            
            CheckReentrancy();

            var itemsList = new ReadOnlyCollection<T>(items.ToList());
            var removed = Items.RemoveAtRange(index, removingItemsCount);
            Items.InsertRange(index, items);

            if (itemsList.Count != removed.Count)
            {
                OnPropertyChanged("Count");
            }

            if (itemsList.Count != 0 && removed.Count != 0)
            {
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, itemsList, removed, index));
            }
            else if (itemsList.Count == 0 && removed.Count != 0)
            {
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
            }
            else if (itemsList.Count != 0 && removed.Count == 0)
            {
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsList, index));
            }

            return removed;
        }

        protected virtual IReadOnlyList<T> MoveItems(int oldIndex, int newIndex, int movingItemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(oldIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(newIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(movingItemsCount >= 0);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            CheckReentrancy();

            var moved = Items.MoveRange(oldIndex, newIndex, movingItemsCount);

            if (oldIndex != newIndex && movingItemsCount != 0)
            {
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, moved, newIndex, oldIndex));
            }
            return moved;
        }

        protected virtual IReadOnlyList<T> ResetItems(IEnumerable<T> newItems)
        {
            Contract.Requires<ArgumentNullException>(newItems != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<T>>() != null);

            CheckReentrancy();

            Items.Clear();
            var result = newItems.ToArray().ToReadOnly();
            Items.AddRange(result);

            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            return result;
        }

        protected sealed override void InsertItem(int index, T item)
        {
            InsertItems(index, new[] { item });
        }

        protected sealed override void MoveItem(int oldIndex, int newIndex)
        {
            MoveItems(oldIndex, newIndex, 1);
        }

        protected sealed override void RemoveItem(int index)
        {
            RemoveItems(index, 1);
        }

        protected sealed override void SetItem(int index, T item)
        {
            ReplaceItems(index, 1, new[] { item });
        }

        void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
