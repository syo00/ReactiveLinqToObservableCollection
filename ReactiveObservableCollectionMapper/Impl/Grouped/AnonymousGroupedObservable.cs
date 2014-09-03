using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Grouped
{
    class AnonymousGroupedObservable<TKey, TElement> : IGroupedObservable<TKey, TElement>
    {
        readonly Func<IObserver<TElement>, IDisposable> subscribe;

        public AnonymousGroupedObservable(Func<IObserver<TElement>, IDisposable> subscribe, TKey key)
        {
            this.subscribe = subscribe;
            this.Key = key;
        }

        public TKey Key { get; private set; }

        public IDisposable Subscribe(IObserver<TElement> observer)
        {
            return subscribe(observer);
        }
    }
}
