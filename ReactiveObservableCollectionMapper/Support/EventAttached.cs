using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal class EventAttached<TValue, TEvent>
    {
        public EventAttached(TValue value, INotifyCollectionChangedEvent<TEvent> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);

            this.Value = value;
            this.e = e;
        }

        public TValue Value { get; private set; }

        private INotifyCollectionChangedEvent<TEvent> e;
        public INotifyCollectionChangedEvent<TEvent> Event
        {
            get
            {
                Contract.Ensures(Contract.Result<INotifyCollectionChangedEvent<TEvent>>() != null);

                return e;
            }
        }
    }

    internal class EventAttached
    {
        public static EventAttached<TValue, TEvent> Create<TValue, TEvent>(TValue value, INotifyCollectionChangedEvent<TEvent> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<EventAttached<TValue, TEvent>>() != null);

            return new EventAttached<TValue, TEvent>(value, e);
        }
    }
}
