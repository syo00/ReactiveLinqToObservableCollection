using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class GroupJoinProducer<TOuter, TInner, TKey, TResult> : Producer<SlimNotifyCollectionChangedEvent<TResult>>
    {
        IEqualityComparer<TKey> comparer;
        Func<TOuter, TKey> outerKeySelector;
        Func<TInner, TKey> innerKeySelector;
        Func<TOuter, ICollectionStatuses<TInner>, TResult> resultSelector;
        Dictionary<TKey, SubjectItem> cache;
        ICollectionStatuses<KeyValuePair<TKey, TInner>> allValues;
        ICollectionStatuses<TOuter> outerStatuses;

        public GroupJoinProducer(CollectionStatuses<TOuter> outer, ICollectionStatuses<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, ICollectionStatuses<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(outer != null);
            Contract.Requires<ArgumentNullException>(inner != null);
            Contract.Requires<ArgumentNullException>(outerKeySelector != null);
            Contract.Requires<ArgumentNullException>(innerKeySelector != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.outerStatuses = outer;
            this.allValues = inner.Select(x => new KeyValuePair<TKey, TInner>(innerKeySelector(x), x)).Publish().RefCount();
            this.outerKeySelector = outerKeySelector;
            this.innerKeySelector = innerKeySelector;
            this.resultSelector = resultSelector;
            this.comparer = comparer;

            this.cache = new Dictionary<TKey, SubjectItem>(comparer);
        }

        private ICollectionStatuses<TInner> ObtainStatuses(TKey key)
        {
            try
            {
                if (cache.ContainsKey(key))
                {
                    return cache[key].Result;
                }
                var newValue = new SubjectItem(allValues, key, comparer);
                cache.Add(key, newValue);
                return newValue.Result;
            }
            catch (ArgumentException e)
            {
                throw new Abort.AbortException(e);
            }
            catch (KeyNotFoundException e)
            {
                throw new Abort.AbortException(e);
            }
        }

        protected override IDisposable SubscribeCore(ProducerObserver<SlimNotifyCollectionChangedEvent<TResult>> observer)
        {
            return outerStatuses
                .Select(x => new { Key = outerKeySelector(x), Value = x })
                .Select(a => new { Key = a.Key, Statuses = ObtainStatuses(a.Key), Value = a.Value })
                .Select(a => resultSelector(a.Value, a.Statuses))
                .ToInstance()
                .SlimInitialStateAndChanged
                .Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
        }

        class SubjectItem
        {
            public SubjectItem(ICollectionStatuses<KeyValuePair<TKey, TInner>> allValues, TKey key, IEqualityComparer<TKey> comparer)
            {
                this.result = allValues
                    .Where(pair => comparer.Equals(pair.Key, key))
                    .Select(pair => pair.Value);
            }

            readonly ICollectionStatuses<TInner> result;
            public ICollectionStatuses<TInner> Result
            {
                get
                {
                    return result;
                }
            }
        }
    }
}
