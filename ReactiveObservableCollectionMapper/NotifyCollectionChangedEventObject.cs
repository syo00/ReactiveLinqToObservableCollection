using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    internal class NotifyCollectionChangedEventObject<T>
    {
        public NotifyCollectionChangedEventObject(INotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);

            this.defaultOne = e;
        }

        public NotifyCollectionChangedEventObject(SimpleNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);

            this.simpleOne = e;
        }

        public NotifyCollectionChangedEventObject(SlimNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);

            this.slimOne = e;
        }

        public NotifyCollectionChangedEventObject(SlimSimpleNotifyCollectionChangedEvent<T> e)
        {
            Contract.Requires<ArgumentNullException>(e != null);

            this.slimSimpleOne = e;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(NotNullEventPropertyCountContract());
        }

        [Pure]
        bool NotNullEventPropertyCountContract()
        {
            var notNullEventPropertyCount = 0;
            if (defaultOne != null) notNullEventPropertyCount++;
            if (simpleOne != null) notNullEventPropertyCount++;
            if (slimOne != null) notNullEventPropertyCount++;
            if (slimSimpleOne != null) notNullEventPropertyCount++;

            return notNullEventPropertyCount == 1;
        }

        public NotifyCollectionChangedEventType EventType
        {
            get
            {
                if (defaultOne != null) return NotifyCollectionChangedEventType.DefaultOne;
                if (simpleOne != null) return NotifyCollectionChangedEventType.SimpleOne;
                if (slimOne != null) return NotifyCollectionChangedEventType.SlimOne;
                return NotifyCollectionChangedEventType.SlimSimpleOne;
            }
        }

        readonly INotifyCollectionChangedEvent<T> defaultOne;
        public INotifyCollectionChangedEvent<T> DefaultOne
        {
            get
            {
                return defaultOne;
            }
        }

        readonly SimpleNotifyCollectionChangedEvent<T> simpleOne;
        public SimpleNotifyCollectionChangedEvent<T> SimpleOne
        {
            get
            {
                return simpleOne;
            }
        }

        readonly SlimNotifyCollectionChangedEvent<T> slimOne;
        public SlimNotifyCollectionChangedEvent<T> SlimOne
        {
            get
            {
                return slimOne;
            }
        }

        readonly SlimSimpleNotifyCollectionChangedEvent<T> slimSimpleOne;
        public SlimSimpleNotifyCollectionChangedEvent<T> SlimSimpleOne
        {
            get
            {
                return slimSimpleOne;
            }
        }
    }
}
