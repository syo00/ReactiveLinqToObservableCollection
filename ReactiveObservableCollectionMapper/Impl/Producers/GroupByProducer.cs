using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LinqToObservableCollection.Support;
using System.Reactive.Subjects;
using Kirinji.LinqToObservableCollection.Impl.Subjects;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Impl.Grouped;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class GroupByProducer<T, TKey, TElement> : Producer<SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>>
    {
        readonly CollectionStatuses<T> source;
        readonly Func<T, TKey> keySelector;
        readonly Func<T, TElement> valueSelector;
        readonly IEqualityComparer<TKey> comparer;
        readonly Dictionary<TKey, SubjectItem> cache;
        readonly List<KeyValuePair<TKey, Reference<int>>> matchedKeysCount = new List<KeyValuePair<TKey, Reference<int>>>();
        readonly PublishSubject<KeyValuePair<TKey, TElement>> allValues = new PublishSubject<KeyValuePair<TKey, TElement>>();

        public GroupByProducer(CollectionStatuses<T> source, Func<T, TKey> keySelector, Func<T, TElement> valueSelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(keySelector != null);
            Contract.Requires<ArgumentNullException>(valueSelector != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.source = source;
            this.keySelector = keySelector;
            this.valueSelector = valueSelector;
            this.comparer = comparer;

            this.cache = new Dictionary<TKey, SubjectItem>(comparer);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(source != null);
            Contract.Invariant(keySelector != null);
            Contract.Invariant(valueSelector != null);
            Contract.Invariant(comparer != null);
            Contract.Invariant(cache != null);
            Contract.Invariant(matchedKeysCount != null);
            Contract.Invariant(allValues != null);
        }


        protected override IDisposable SubscribeCore(ProducerObserver<SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>> observer)
        {
            return source
                .Select(x => new KeyValuePair<TKey, TElement>(keySelector(x), valueSelector(x)))
                .ToInstance()
                .InitialStateAndChanged
                .Subscribe(e =>
                {
                    allValues.OnNext(e);

                    try
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedEventAction.InitialState:
                                {
                                    var newItems = CreatedAddedUnits(e.InitialState.Items)
                                        .Select(unit => unit.Item.Item)
                                        .ToArray()
                                        .ToReadOnly();
                                    var newEvent = SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>.CreateInitialState(newItems);
                                    observer.OnNext(newEvent);

                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Add:
                                {
                                    var newItems = CreatedAddedUnits(e.Added.Items);
                                    var newEvent = SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>.CreateAddOrRemove(newItems);
                                    observer.OnNext(newEvent);

                                    return;
                                }

                            case NotifyCollectionChangedEventAction.Remove:
                                {
                                    var newItems = CreatedRemovedUnits(e.Removed.Items);
                                    var newEvent = SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>.CreateAddOrRemove(newItems);
                                    observer.OnNext(newEvent);

                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Replace:
                                {
                                    var removed = CreatedRemovedUnits(e.Replaced.OldItems);
                                    var added = CreatedAddedUnits(e.Replaced.NewItems);
                                    var replaced = CleanDuplicatedValues(removed, added);
                                    var newEvent = SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>.CreateAddOrRemove(replaced);
                                    observer.OnNext(newEvent);

                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Move:
                                {
                                    return;
                                }
                            case NotifyCollectionChangedEventAction.Reset:
                                {
                                    ClearKeys();

                                    var newItems = CreatedAddedUnits(e.Reset.Items)
                                        .Select(unit => unit.Item.Item)
                                        .ToArray()
                                        .ToReadOnly();
                                    var newEvent = SimpleNotifyCollectionChangedEvent<IGroupedCollectionStatuses<TKey, TElement>>.CreateReset(newItems);
                                    observer.OnNext(newEvent);

                                    return;
                                }
                        }
                    }
                    catch (Abort.AbortException ex)
                    {
                        observer.OnError(ex);
                    }
                }, ex =>
                {
                    allValues.OnError(ex);
                    observer.OnError(ex);
                }, () =>
                {
                    allValues.OnCompleted();
                    observer.OnCompleted();
                });
        }

        private IReadOnlyList<AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>> CreatedAddedUnits(IReadOnlyList<KeyValuePair<TKey, TElement>> items)
        {
            var newItems = items
                .Select(pair => new { Index = AddKey(pair.Key), Key = pair.Key })
                .Where(a => a.Index != null)
                .Select(a => new { SubjectItem = ObtainSubjectItem(a.Key), Index = a.Index.Value })
                .Select(a =>
                {
                    var tagged = new Tagged<IGroupedCollectionStatuses<TKey, TElement>>(a.SubjectItem.Result);

                    return new AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>(AddOrRemoveUnitType.Add, tagged, a.Index);
                })
                .ToArray()
                .ToReadOnly();
            return newItems;
        }

        private IReadOnlyList<AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>> CreatedRemovedUnits(IReadOnlyList<KeyValuePair<TKey, TElement>> items)
        {
            var newItems = items
                .Select(pair => new { Index = RemoveKey(pair.Key), Key = pair.Key })
                .Where(a => a.Index != null)
                .Select(a => new { SubjectItem = ObtainSubjectItem(a.Key), Index = a.Index.Value })
                .Select(a =>
                {
                    var tagged = new Tagged<IGroupedCollectionStatuses<TKey, TElement>>(a.SubjectItem.Result);

                    return new AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>(AddOrRemoveUnitType.Remove, tagged, a.Index);
                })
                .ToArray()
                .ToReadOnly();
            return newItems;
        }


        private IReadOnlyList<AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>> CleanDuplicatedValues(IReadOnlyList<AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>> removing, IReadOnlyList<AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>> adding)
        {
            Contract.Requires<ArgumentNullException>(removing != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(removing, item => item.Type == AddOrRemoveUnitType.Remove));
            Contract.Requires<ArgumentNullException>(adding != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(adding, item => item.Type == AddOrRemoveUnitType.Add));
            Contract.Ensures(Contract.Result<IReadOnlyList<AddedOrRemovedUnit<IGroupedCollectionStatuses<TKey, TElement>>>>() != null);

            var result = new AddedOrRemovedUnitSet<IGroupedCollectionStatuses<TKey, TElement>>(removing);

            foreach (var a in adding)
            {
                var matchedIndex = result.FirstIndex(unit => unit.Type == AddOrRemoveUnitType.Remove && comparer.Equals(unit.Item.Item.Key, a.Item.Item.Key));
                if (matchedIndex == null)
                {
                    result.Add(a);
                }
                else
                {
                    result.RemoveAt(matchedIndex.Value);
                }
            }

            return result.ToArray().ToReadOnly();
        }

        // 追加されたら追加されたインデックス、存在していなくて新しく追加されたなら null を返す
        private int? AddKey(TKey key)
        {
            var matchedIndex = matchedKeysCount.FirstIndex(pair => comparer.Equals(key, pair.Key));
            if(matchedIndex == null)
            {
                matchedKeysCount.Add(new KeyValuePair<TKey, Reference<int>>(key, new Reference<int> { Value = 1 }));
                return matchedKeysCount.Count - 1;
            }
            else
            {
                matchedKeysCount[matchedIndex.Value].Value.Value++;
                return null;
            }
        }

        // 削除されたら削除されたインデックス、1つでも残っているなら null を返す
        private int? RemoveKey(TKey key)
        {
            var matchedIndex = matchedKeysCount.FirstIndex(pair => comparer.Equals(key, pair.Key));
            if (matchedIndex == null)
            {
                // comparer が、同じ値ならばどの実行タイミングでも同じ値を返す作りならばここには普通来ない
                return null;
            }
            else
            {
                matchedKeysCount[matchedIndex.Value].Value.Value--;
                if (matchedKeysCount[matchedIndex.Value].Value.Value == 0)
                {
                    matchedKeysCount.RemoveAt(matchedIndex.Value);
                    return matchedIndex.Value;
                }
                return null;
            }
        }

        private void ClearKeys()
        {
            matchedKeysCount.Clear();
        }

        private SubjectItem ObtainSubjectItem(TKey key)
        {
            var matched = cache.ValueOrDefault(key);
            if(matched != null)
            {
                return matched;
            }

            var newValue = new SubjectItem(allValues, comparer, key);
            try
            {
                cache.Add(key, newValue);
            }
            catch(ArgumentException e)
            {
                throw new Abort.AbortException(e);
            }
            return newValue;
        }

        class SubjectItem
        {
            public SubjectItem(PublishSubject<KeyValuePair<TKey, TElement>> allValues, IEqualityComparer<TKey> comparer, TKey key)
            {
                Contract.Requires<ArgumentNullException>(allValues != null);
                Contract.Requires<ArgumentNullException>(comparer != null);

                this.key = key;

                var core = allValues
                    .ToStatuses()
                    .Where(pair => comparer.Equals(pair.Key, Key))
                    .Select(pair => pair.Value)
                    .ToInstance();
                this.result = new AnonymousGroupedCollectionStatuses<TKey, TElement>(core, Key);
            }

            IGroupedCollectionStatuses<TKey, TElement> result;
            public IGroupedCollectionStatuses<TKey, TElement> Result
            {
                get
                {
                    return result;
                }
            }

            readonly TKey key;
            public TKey Key
            {
                get
                {
                    return key;
                }
            }
        }
    }
}
