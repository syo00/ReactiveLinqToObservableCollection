using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;

namespace Kirinji.LinqToObservableCollection
{
    public class InvalidInformationException : Exception
    {
        internal InvalidInformationException(InvalidInformationExceptionType type)
            : base()
        {
            this.Type = type;
        }

        internal InvalidInformationException(InvalidInformationExceptionType type, string message)
            : base(message)
        {
            this.Type = type;
        }

        public InvalidInformationExceptionType Type { get; private set; }
    }
}
