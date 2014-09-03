using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using System.Diagnostics.Contracts;
using Kirinji.LinqToObservableCollection.Support;

namespace Kirinji.LinqToObservableCollection
{
    public class InvalidInformationException<T> : InvalidInformationException
    {
        public InvalidInformationException(InvalidInformationExceptionType type)
            : base(type)
        {

        }

        public InvalidInformationException(InvalidInformationExceptionType type, string message)
            : base(type, message)
        {

        }

        public InvalidInformationException(InvalidInformationExceptionType type, INotifyCollectionChangedEvent<T> sourceEvent)
            : base(type)
        {
            this.SourceEvents = new[] { sourceEvent }.ToReadOnly();
        }

        public InvalidInformationException(InvalidInformationExceptionType type, IReadOnlyList<INotifyCollectionChangedEvent<T>> sourceEvents)
            : base(type)
        {
            Contract.Requires<ArgumentNullException>(sourceEvents != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(sourceEvents, x => x != null));

            this.SourceEvents = sourceEvents;
        }

        public InvalidInformationException(InvalidInformationExceptionType type, string message, INotifyCollectionChangedEvent<T> sourceEvent)
            : base(type, message)
        {
            Contract.Requires<ArgumentNullException>(sourceEvent != null);

            this.SourceEvents = new[] { sourceEvent }.ToReadOnly();
        }

        public InvalidInformationException(InvalidInformationExceptionType type, string message, IReadOnlyList<INotifyCollectionChangedEvent<T>> sourceEvents)
            : base(type, message)
        {
            Contract.Requires<ArgumentNullException>(sourceEvents != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(sourceEvents, x => x != null));

            this.SourceEvents = sourceEvents;
        }

        internal InvalidInformationException(InvalidInformationExceptionType type, SimpleNotifyCollectionChangedEvent<T> sourceEvent)
            : base(type)
        {
            Contract.Requires<ArgumentNullException>(sourceEvent != null);

            this.SourceSimpleEvents = new[] { sourceEvent }.ToReadOnly();
            this.SourceEvents = sourceEvent.Extract();

        }

        internal InvalidInformationException(InvalidInformationExceptionType type, IReadOnlyCollection<SimpleNotifyCollectionChangedEvent<T>> sourceEvents)
            : base(type)
        {
            Contract.Requires<ArgumentNullException>(sourceEvents != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(sourceEvents, x => x != null));

            this.SourceSimpleEvents = sourceEvents;
            this.SourceEvents = sourceEvents.SelectMany(x => x.Extract()).ToArray().ToReadOnly();
        }

        internal InvalidInformationException(InvalidInformationExceptionType type, string message, SimpleNotifyCollectionChangedEvent<T> sourceEvent)
            : base(type, message)
        {
            Contract.Requires<ArgumentNullException>(sourceEvent != null);

            this.SourceSimpleEvents = new[] { sourceEvent }.ToReadOnly();
            this.SourceEvents = sourceEvent.Extract();
        }


        internal InvalidInformationException(InvalidInformationExceptionType type, string message, IReadOnlyCollection<SimpleNotifyCollectionChangedEvent<T>> sourceEvents)
            : base(type, message)
        {
            Contract.Requires<ArgumentNullException>(sourceEvents != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(sourceEvents, x => x != null));

            this.SourceSimpleEvents = sourceEvents;
            this.SourceEvents = sourceEvents.SelectMany(x => x.Extract()).ToArray().ToReadOnly();
        }

        public IReadOnlyList<INotifyCollectionChangedEvent<T>> SourceEvents { get; private set; }
        internal IReadOnlyCollection<SimpleNotifyCollectionChangedEvent<T>> SourceSimpleEvents { get; private set; }
    }
}
