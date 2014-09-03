using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using Kirinji.LinqToObservableCollection.Support;
using Kirinji.LightWands;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class JoinProducer<TOuter, TInner, TKey, TResult> : Producer<INotifyCollectionChangedEvent<TResult>>
    {
        readonly CollectionStatuses<TOuter> outer;
        readonly CollectionStatuses<TInner> inner;
        IEqualityComparer<TKey> comparer;
        Func<TOuter, TKey> outerKeySelector;
        Func<TInner, TKey> innerKeySelector;
        Func<TOuter, TInner, TResult> resultSelector;

        public JoinProducer(CollectionStatuses<TOuter> outer, CollectionStatuses<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires<ArgumentNullException>(outer != null);
            Contract.Requires<ArgumentNullException>(inner != null);
            Contract.Requires<ArgumentNullException>(outerKeySelector != null);
            Contract.Requires<ArgumentNullException>(innerKeySelector != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            this.outer = outer;
            this.inner = inner;
            this.outerKeySelector = outerKeySelector;
            this.innerKeySelector = innerKeySelector;
            this.resultSelector = resultSelector;
            this.comparer = comparer;
        }

        protected override IDisposable SubscribeCore(ProducerObserver<INotifyCollectionChangedEvent<TResult>> observer)
        {
            var subject = new Subject<Unit>();
            var innerSource = inner
                .SlimInitialStateAndChanged
                .UseObserver((obs, e) =>
                {
                    obs.OnNext(e);
                    subject.OnNext(Unit.Default);
                })
                .ToStatuses();
            var outerSource = outer
                .SlimInitialStateAndChanged
                .UseObserver((obs, e) =>
                {
                    obs.OnNext(e);
                    subject.OnNext(Unit.Default);
                })
                .ToStatuses();

            return outerSource.GroupJoin(innerSource, outerKeySelector, innerKeySelector, (o, i) => i.Select(x => resultSelector(o, x)), comparer)
                .ToInstance()
                .Flatten()
                .InitialStateAndChanged
                .Buffer(subject)
                .SelectMany(events =>
                    { 
                        return EventsConverter.Combine(events.ToReadOnly());
                    })
                .Subscribe(observer);
        }
    }
}
