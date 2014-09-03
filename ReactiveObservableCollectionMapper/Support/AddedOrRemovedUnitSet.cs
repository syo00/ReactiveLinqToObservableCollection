using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    // IList を実装するとAddなどで契約違反が起こるので実装しない
    internal class AddedOrRemovedUnitSet<T> : IEnumerable<AddedOrRemovedUnit<T>>
    {
        readonly List<AddOrRemoveUnitInfo> items = new List<AddOrRemoveUnitInfo>();

        public AddedOrRemovedUnitSet()
        {

        }

        public AddedOrRemovedUnitSet(IEnumerable<AddedOrRemovedUnit<T>> events)
        {
            Contract.Requires<ArgumentNullException>(events != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(events, x => x != null));

            foreach (var e in events)
            {
                this.Add(e);
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(items != null);
            Contract.Invariant(Contract.ForAll(items, info => info != null));
            Contract.Invariant(ItemsFollowAddAndRemoveRuleContract());
            Contract.Invariant(IsItemsCountMinimizedContract());
        }

        [Pure]
        private bool IsItemsCountMinimizedContract()
        {
            return items
                .ToLookup(info => info.Item)
                .Select(group =>
                {
                    if (group.Count() >= 3) return false;
                    if(group.Count() == 2)
                    {
                        return group.ElementAt(0).Type == AddOrRemoveUnitType.Remove
                            && group.ElementAt(1).Type == AddOrRemoveUnitType.Add;
                    }
                    return true;

                })
                .Aggregate(true, (x, y) => x && y);
        }

        [Pure]
        private bool ItemsFollowAddAndRemoveRuleContract()
        {
            try
            {
                foreach (var _ in ObtainPseudoOutput())
                {

                }
            }
            catch(InvalidOperationException)
            {
                return false;
            }
            catch(ArgumentException)
            {
                return false;
            }

            return true;
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

        [Pure]
        private IEnumerable<IReadOnlyDictionary<int, Tagged<T>>> ObtainPseudoOutput()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IReadOnlyDictionary<int, Tagged<T>>>>() != null);
            //Contract.Ensures(Contract.Result<IEnumerable<IReadOnlyDictionary<int, Tagged<T>>>>() == items.Count() + 1);

            var result = new Dictionary<int, Tagged<T>>();

            yield return result.ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var i in items)
            {
                if (i.Type == AddOrRemoveUnitType.Add)
                {
                    if(result.Any(r => r.Value.Equals(i.Item)))
                    {
                        throw ObtainNotFollowingRuleException("tried to add existing value!");
                    }

                    var newResult = result.Select(pair => pair.Key >= i.Index ? new KeyValuePair<int, Tagged<T>>(pair.Key + 1, pair.Value) : pair).Concat(new KeyValuePair<int, Tagged<T>>(i.Index, i.Item));

                    result = newResult.ToDictionary(pair => pair.Key, pair => pair.Value);
                    yield return newResult.ToDictionary(pair => pair.Key, pair => pair.Value);
                }
                else
                {
                    if(result.ContainsKey(i.Index))
                    {
                        if(result[i.Index].Equals(i.Item))
                        {
                            throw ObtainNotFollowingRuleException("tried to remove different value!");
                        }
                        result.Remove(i.Index);
                    }
                    var newResult = result.Select(pair => pair.Key > i.Index ? new KeyValuePair<int, Tagged<T>>(pair.Key - 1, pair.Value) : pair);

                    result = newResult.ToDictionary(pair => pair.Key, pair => pair.Value);
                    yield return newResult.ToDictionary(pair => pair.Key, pair => pair.Value);
                }
            }
        }

        public int IndexOf(AddedOrRemovedUnit<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            return items.IndexOf(AddOrRemoveUnitInfo.Convert(item));
        }

        public void Move(int oldIndex, int newIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(oldIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(newIndex >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(oldIndex < Count);
            Contract.Requires<ArgumentOutOfRangeException>(newIndex < Count);

            if (oldIndex == newIndex) return;

            var itemsBackup = items.ToArray();

            try
            {
                var movingItem = this[oldIndex];
                RemoveAt(oldIndex);
                Insert(newIndex, movingItem);
            }
            catch (IndexOutOfRangeException)
            {
                items.Clear();
                items.AddRange(itemsBackup);
            }
            catch (InvalidOperationException)
            {
                items.Clear();
                items.AddRange(itemsBackup);
            }
        }

        public void Insert(int index, AddedOrRemovedUnit<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var outputs = ObtainPseudoOutput().ToArray();
            Contract.Assume(outputs != null);

            if (item.Type == AddOrRemoveUnitType.Add)
            {
                if(items.Any(i => i.Item.Equals(item.Item) && i.Type == AddOrRemoveUnitType.Add))
                {
                    throw ObtainNotFollowingRuleException("同じ要素を追加する、または後から追加させるような状態にすることはできません。");
                }

                var removingItemIndex = items.Skip(index).FirstIndex(i => i.Item.Equals(item.Item) && item.Type == AddOrRemoveUnitType.Remove);
                if (removingItemIndex != null)
                {
                    var itemsBackup = items.ToList();
                    InsertCore(index, item);

                    try
                    {
                        if (ObtainPseudoOutput().ElementAt(removingItemIndex.Value + 1)[item.Index].Equals(item.Item))
                        {
                            items.Clear();
                            items.AddRange(itemsBackup);
                            RemoveAtCore(removingItemIndex.Value);
                        }
                        else
                        {
                            throw new KeyNotFoundException(); // catch 節に移動するための例外
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        items.Clear();
                        items.AddRange(itemsBackup);

                        throw ObtainNotFollowingRuleException("後の削除イベントのインデックスに矛盾が生じます。");
                    }
                }
                else
                {
                    InsertCore(index, item);
                }
            }
            else
            {
                if (outputs[index].Where(pair => pair.Key != item.Index).Any(pair => pair.Value.Equals(item.Item)))
                {
                    throw ObtainNotFollowingRuleException("インデックスが誤っています。");
                }
                if(items.Skip(index).Any(i => i.Item.Equals(item.Item) && i.Type == AddOrRemoveUnitType.Remove))
                {
                    throw ObtainNotFollowingRuleException("同じ要素を後から削除させるような状態にすることはできません。");
                }

                var addEventIndex = items.Take(index).FirstIndex(i => i.Item.Equals(item.Item) && i.Type == AddOrRemoveUnitType.Add);

                if (addEventIndex != null)
                {
                    RemoveAtCore(addEventIndex.Value);
                }
                else
                {
                    InsertCore(index, item);
                }
            }
        }

        // Add、Removeの個数の最適化を行わずにInsert する
        private void InsertCore(int index, AddedOrRemovedUnit<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Contract.Requires<ArgumentOutOfRangeException>(0 <= index && index <= Count);

            int insertingItemIndex = item.Index;

            if (item.Type == AddOrRemoveUnitType.Add)
            {
                foreach (var i in items.Skip(index))
                {
                    if (i.Index > insertingItemIndex)
                    {
                        i.Index++;
                    }
                    else
                    {
                        if (i.Type == AddOrRemoveUnitType.Add)
                        {
                            insertingItemIndex++;
                        }
                        else
                        {
                            insertingItemIndex--;
                        }
                    }
                }
            }
            else
            {
                foreach (var i in items.Skip(index))
                {
                    if (i.Index > insertingItemIndex)
                    {
                        i.Index--;
                    }
                    else if (i.Index == insertingItemIndex && i.Type == AddOrRemoveUnitType.Remove)
                    {
                        if(!i.Item.Equals(item.Item))
                        {
                            throw ObtainNotFollowingRuleException("アイテムが一致しません。");
                        }

                        items.Remove(i);
                        return;
                    }
                    else
                    {
                        if (i.Type == AddOrRemoveUnitType.Add)
                        {
                            insertingItemIndex++;
                        }
                        else
                        {
                            insertingItemIndex--;
                        }
                    }
                }
            }

            items.Insert(index, AddOrRemoveUnitInfo.Convert(item));
        }

        public void RemoveAt(int index)
        {
            var outputs = ObtainPseudoOutput().ToArray();
            var removingItem = items[index];
            if (removingItem.Type == AddOrRemoveUnitType.Add)
            {
                if(items.Skip(index + 1).Any(i => i.Item.Equals(removingItem.Item) && i.Type == AddOrRemoveUnitType.Remove))
                {
                    throw ObtainNotFollowingRuleException("指定した要素を削除するイベントが後に存在します。");
                }
            }
            else
            {
                if (items.Skip(index + 1).Any(i => i.Item.Equals(removingItem.Item) && i.Type == AddOrRemoveUnitType.Add))
                {
                    throw ObtainNotFollowingRuleException("指定した要素を追加するイベントが後に存在します。");
                }
            }

            RemoveAtCore(index);
        }

        // Add、Removeの個数の最適化を行わずにRemove する
        private void RemoveAtCore(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(0 <= index && index < Count);

            var removing = items[index];

            int removingItemIndex = removing.Index;
            if (removing.Type == AddOrRemoveUnitType.Add)
            {
                foreach (var i in items.Skip(index + 1))
                {
                    if (i.Index > removingItemIndex)
                    {
                        i.Index--;
                    }
                    else
                    {
                        if (i.Type == AddOrRemoveUnitType.Add)
                        {
                            removingItemIndex++;
                        }
                        else
                        {
                            removingItemIndex--;
                        }
                    }
                }
            }
            else
            {
                foreach (var i in items.Skip(index + 1))
                {
                    if (i.Index > removingItemIndex)
                    {
                        i.Index++;
                    }
                    else if(i.Index == removingItemIndex)
                    {
                        throw ObtainNotFollowingRuleException("Remove -> Add のセットで前者のみが削除されようとしました。");
                    }
                    else
                    {
                        if (i.Type == AddOrRemoveUnitType.Add)
                        {
                            removingItemIndex++;
                        }
                        else
                        {
                            removingItemIndex--;
                        }
                    }
                }
            }

            items.RemoveAt(index);
        }

        public AddedOrRemovedUnit<T> this[int index]
        {
            get
            {
                return items[index].Extract();
            }
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public void Add(AddedOrRemovedUnit<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            
            Insert(Count, item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(AddedOrRemovedUnit<T> item)
        {
            return items.Contains(AddOrRemoveUnitInfo.Convert(item));
        }

        public void CopyTo(AddedOrRemovedUnit<T>[] array, int arrayIndex)
        {
            items
                .Select(x => x.Extract())
                .ToList()
                .CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() == items.Count);

                return items.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(AddedOrRemovedUnit<T> item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        public IEnumerator<AddedOrRemovedUnit<T>> GetEnumerator()
        {
            return items
                .Select(x => x.Extract())
                .GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class AddOrRemoveUnitInfo : IEquatable<AddOrRemoveUnitInfo>
        {
            public AddOrRemoveUnitType Type { get; set; }
            public Tagged<T> Item { get; set; }
            public int Index { get; set; }

            public override bool Equals(object obj)
            {
                if (obj != null && obj is AddOrRemoveUnitInfo)
                {
                    return Equals((AddOrRemoveUnitInfo)obj);
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(AddOrRemoveUnitInfo other)
            {
                return Type == other.Type
                    && Index == other.Index
                    && Object.Equals(Item, other.Item);
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode()
                    ^ (Index.GetHashCode() * 8)
                    ^ ObjectEx.GetHashCode(Item);
            }

            public override string ToString()
            {
                return Type.ToString()
                + " (index: " + Index
                + ", item: "
                + Item.ToString()
                + ")";
            }

            public AddedOrRemovedUnit<T> Extract()
            {
                if (Item == null) return null;

                return new AddedOrRemovedUnit<T>(Type, Item, Index);
            }

            public static AddOrRemoveUnitInfo Convert(AddedOrRemovedUnit<T> source)
            {
                Contract.Requires<ArgumentNullException>(source != null);
                Contract.Ensures(Contract.Result<AddOrRemoveUnitInfo>() != null);

                return new AddOrRemoveUnitInfo { Type = source.Type, Index = source.Index, Item = source.Item };
            }
        }
    }
}
