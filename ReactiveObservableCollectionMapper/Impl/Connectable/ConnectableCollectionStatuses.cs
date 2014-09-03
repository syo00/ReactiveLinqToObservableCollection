using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Connectable
{
    internal abstract class ConnectableCollectionStatuses<T> : CollectionStatuses<T>, IConnectableCollectionStatuses<T>
    {
        public IConnectableObservable<INotifyCollectionChangedEvent<T>> ConnectableInitialStateAndChanged
        {
            get
            {
                return new AnonymousConnectableObservable<INotifyCollectionChangedEvent<T>>(InitialStateAndChanged.Subscribe, Connect);
            }
        }

        public IConnectableObservable<SlimNotifyCollectionChangedEvent<T>> ConnectableSlimInitialStateAndChanged
        {
            get
            {
                return new AnonymousConnectableObservable<SlimNotifyCollectionChangedEvent<T>>(SlimInitialStateAndChanged.Subscribe, Connect);
            }
        }

        public IConnectableObservable<SimpleNotifyCollectionChangedEvent<T>> ConnectableSimpleInitialStateAndChanged
        {
            get
            {
                return new AnonymousConnectableObservable<SimpleNotifyCollectionChangedEvent<T>>(SimpleInitialStateAndChanged.Subscribe, Connect);
            }
        }

        public IConnectableObservable<SlimSimpleNotifyCollectionChangedEvent<T>> ConnectableSlimSimpleInitialStateAndChanged
        {
            get
            {
                return new AnonymousConnectableObservable<SlimSimpleNotifyCollectionChangedEvent<T>>(SlimSimpleInitialStateAndChanged.Subscribe, Connect);
            }
        }

        public abstract IDisposable Connect();
    }
}
