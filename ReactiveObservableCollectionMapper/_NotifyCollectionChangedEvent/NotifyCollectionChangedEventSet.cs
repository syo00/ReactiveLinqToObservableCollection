using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection
{
    // IList を実装するとAddなどで契約違反が起こるので実装しない
    // 作ったはいいがまだ使っていない。CombineでMovedに変換するのに使う予定
    class NotifyCollectionChangedEventSet<T> : IEnumerable<INotifyCollectionChangedEvent<T>>
    {
        readonly List<Reference<INotifyCollectionChangedEvent<T>>> items = new List<Reference<INotifyCollectionChangedEvent<T>>>();

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(items != null);
            Contract.Invariant(Contract.ForAll(items, reference => reference != null));
        }

        private static InvalidOperationException ObtainNotFollowingRuleException(string message)
        {
            if (message == null)
            {
                return new InvalidOperationException();
            }
            else
            {
                return new InvalidOperationException(message);
            }
        }

        public int IndexOf(INotifyCollectionChangedEvent<T> item)
        {
            return items.FirstIndex(reference => reference.Value == item) ?? -1;
        }

        public void Insert(int index, INotifyCollectionChangedEvent<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            if (item.Action == NotifyCollectionChangedEventAction.InitialState)
            {
                if (index != 0 || items.Any(reference => reference.Value.Action == NotifyCollectionChangedEventAction.InitialState))
                {
                    throw ObtainNotFollowingRuleException("InitialState location error");
                }
            }
            else
            {
                if (index == 0 && items.Count != 0 && items[0].Value.Action == NotifyCollectionChangedEventAction.InitialState)
                {
                    throw ObtainNotFollowingRuleException("InitialState location error");
                }
            }

            switch(item.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        UpdateIndexAsIfInsertEvent(0, 0, item.InitialState.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        UpdateIndexAsIfInsertEvent(index, item.Added.StartingIndex, item.Added.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        UpdateIndexAsIfInsertEvent(index, item.Removed.StartingIndex, -1 * item.Removed.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        UpdateIndexAsIfInsertEvent(index, item.Moved.OldStartingIndex, -1 * item.Moved.Items.Count);
                        UpdateIndexAsIfInsertEvent(index, item.Moved.NewStartingIndex, item.Moved.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        UpdateIndexAsIfInsertEvent(index, item.Replaced.StartingIndex, -1 * item.Replaced.OldItems.Count);
                        UpdateIndexAsIfInsertEvent(index, item.Replaced.StartingIndex, item.Replaced.NewItems.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        throw new NotSupportedException();
                    }
            }

            items.Insert(index, new Reference<INotifyCollectionChangedEvent<T>>(item));
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            
            switch (item.Action)
            {
                case NotifyCollectionChangedEventAction.InitialState:
                    {
                        UpdateIndexAsIfRemoveEvent(0, 0, item.InitialState.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Add:
                    {
                        UpdateIndexAsIfRemoveEvent(index, item.Added.StartingIndex, item.Added.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Remove:
                    {
                        UpdateIndexAsIfRemoveEvent(index, item.Removed.StartingIndex, -1 * item.Removed.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Move:
                    {
                        UpdateIndexAsIfRemoveEvent(index, item.Moved.OldStartingIndex, -1 * item.Moved.Items.Count);
                        UpdateIndexAsIfRemoveEvent(index, item.Moved.NewStartingIndex, item.Moved.Items.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Replace:
                    {
                        UpdateIndexAsIfRemoveEvent(index, item.Replaced.StartingIndex, -1 * item.Replaced.OldItems.Count);
                        UpdateIndexAsIfRemoveEvent(index, item.Replaced.StartingIndex, item.Replaced.NewItems.Count);
                        break;
                    }
                case NotifyCollectionChangedEventAction.Reset:
                    {
                        throw new NotSupportedException();
                    }
            }

            items.RemoveAt(index);
        }

        void UpdateIndexAsIfInsertEvent(int index, int insertingEventStartingIndex, int addingItemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(insertingEventStartingIndex >= 0);

            foreach (var reference in items.Skip(index))
            {
                switch (reference.Value.Action)
                {
                    case NotifyCollectionChangedEventAction.InitialState:
                        {
                            throw new NotSupportedException("Use IAdded instead");
                        }
                    case NotifyCollectionChangedEventAction.Add:
                        {
                            if (reference.Value.Added.StartingIndex >= insertingEventStartingIndex)
                            {
                                var newEvent = NotifyCollectionChangedEvent.CreateAddedEvent(reference.Value.Added.Items, reference.Value.Added.StartingIndex + addingItemsCount);
                                reference.Value = newEvent;
                            }
                            else
                            {
                                insertingEventStartingIndex += reference.Value.Added.Items.Count;
                            }

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Remove:
                        {
                            if (reference.Value.Removed.StartingIndex >= insertingEventStartingIndex)
                            {
                                var newEvent = NotifyCollectionChangedEvent.CreateRemovedEvent(reference.Value.Removed.Items, reference.Value.Removed.StartingIndex + addingItemsCount);
                                reference.Value = newEvent;
                            }
                            else
                            {
                                insertingEventStartingIndex -= reference.Value.Added.Items.Count;
                            }

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Move:
                        {
                            int oldIndex;
                            if (reference.Value.Moved.OldStartingIndex >= insertingEventStartingIndex)
                            {
                                oldIndex = reference.Value.Moved.OldStartingIndex + addingItemsCount;
                            }
                            else
                            {
                                insertingEventStartingIndex -= reference.Value.Moved.Items.Count;
                                oldIndex = reference.Value.Moved.OldStartingIndex;
                            }

                            int newIndex;
                            if (reference.Value.Moved.NewStartingIndex >= insertingEventStartingIndex)
                            {
                                newIndex = reference.Value.Moved.NewStartingIndex + addingItemsCount;
                            }
                            else
                            {
                                insertingEventStartingIndex += reference.Value.Moved.Items.Count;
                                newIndex = reference.Value.Moved.NewStartingIndex;
                            }

                            reference.Value = NotifyCollectionChangedEvent.CreateMovedEvent(reference.Value.Moved.Items, oldIndex, newIndex);

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Replace:
                        {
                            if (reference.Value.Replaced.StartingIndex >= insertingEventStartingIndex)
                            {
                                var newEvent = NotifyCollectionChangedEvent.CreateReplacedEvent(reference.Value.Replaced.OldItems, reference.Value.Replaced.NewItems, reference.Value.Replaced.StartingIndex + addingItemsCount);
                                reference.Value = newEvent;
                            }
                            else
                            {
                                insertingEventStartingIndex =
                                    insertingEventStartingIndex
                                    - reference.Value.Replaced.OldItems.Count
                                    + reference.Value.Replaced.NewItems.Count;
                            }

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Reset:
                        {
                            return;
                        }
                }
            }
        }

        void UpdateIndexAsIfRemoveEvent(int index, int removingEventStartingIndex, int addingItemsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(removingEventStartingIndex >= 0);

            foreach (var reference in items.Skip(index))
            {
                switch (reference.Value.Action)
                {
                    case NotifyCollectionChangedEventAction.InitialState:
                        {
                            throw new NotSupportedException("Use IAdded instead");
                        }
                    case NotifyCollectionChangedEventAction.Add:
                        {
                            if (reference.Value.Added.StartingIndex >= removingEventStartingIndex)
                            {
                                var newEvent = NotifyCollectionChangedEvent.CreateAddedEvent(reference.Value.Added.Items, reference.Value.Added.StartingIndex - addingItemsCount);
                                reference.Value = newEvent;
                            }
                            else
                            {
                                removingEventStartingIndex += reference.Value.Added.Items.Count;
                            }

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Remove:
                        {
                            if (reference.Value.Removed.StartingIndex >= removingEventStartingIndex)
                            {
                                var newEvent = NotifyCollectionChangedEvent.CreateRemovedEvent(reference.Value.Removed.Items, reference.Value.Removed.StartingIndex - addingItemsCount);
                                reference.Value = newEvent;
                            }
                            else
                            {
                                removingEventStartingIndex -= reference.Value.Added.Items.Count;
                            }

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Move:
                        {
                            int oldIndex;
                            if (reference.Value.Moved.OldStartingIndex >= removingEventStartingIndex)
                            {
                                oldIndex = reference.Value.Moved.OldStartingIndex - addingItemsCount;
                            }
                            else
                            {
                                removingEventStartingIndex -= reference.Value.Moved.Items.Count;
                                oldIndex = reference.Value.Moved.OldStartingIndex;
                            }

                            int newIndex;
                            if (reference.Value.Moved.NewStartingIndex >= removingEventStartingIndex)
                            {
                                newIndex = reference.Value.Moved.NewStartingIndex - addingItemsCount;
                            }
                            else
                            {
                                removingEventStartingIndex += reference.Value.Moved.Items.Count;
                                newIndex = reference.Value.Moved.NewStartingIndex;
                            }

                            reference.Value = NotifyCollectionChangedEvent.CreateMovedEvent(reference.Value.Moved.Items, oldIndex, newIndex);

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Replace:
                        {
                            if (reference.Value.Replaced.StartingIndex >= removingEventStartingIndex)
                            {
                                var newEvent = NotifyCollectionChangedEvent.CreateReplacedEvent(reference.Value.Replaced.OldItems, reference.Value.Replaced.NewItems, reference.Value.Replaced.StartingIndex - addingItemsCount);
                                reference.Value = newEvent;
                            }
                            else
                            {
                                removingEventStartingIndex =
                                    removingEventStartingIndex
                                    - reference.Value.Replaced.OldItems.Count
                                    + reference.Value.Replaced.NewItems.Count;
                            }

                            break;
                        }
                    case NotifyCollectionChangedEventAction.Reset:
                        {
                            return;
                        }
                }
            }
        }

        public INotifyCollectionChangedEvent<T> this[int index]
        {
            get
            {
                return items[index].Value;
            }
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public void Add(INotifyCollectionChangedEvent<T> item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(INotifyCollectionChangedEvent<T> item)
        {
            return items.Any(reference => Object.Equals(reference.Value, item));
        }

        public int Count
        {
            get { return items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(INotifyCollectionChangedEvent<T> item)
        {
            var removingIndex = IndexOf(item);
            if (removingIndex < 0) return false;

            RemoveAt(removingIndex);
            return true;
        }

        public IEnumerator<INotifyCollectionChangedEvent<T>> GetEnumerator()
        {
            return items.Select(reference => reference.Value).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
