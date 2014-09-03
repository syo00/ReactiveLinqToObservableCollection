using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public enum InvalidInformationExceptionType
    {
        /// <summary>Unknown type. Usually, this is not indicated by InvalidInformationException.</summary>
        Unknown = 0,


        /// <summary>Not received InitialState first, or received InitialState twice.</summary>
        NotFollowingEventSequenceRule = 1,

        /// <summary>Received <c>INotifyCollectionChangedEvent&lt;T&gt;</c> is null.</summary>
        EventIsNull = 2,

        /// <summary>Received <c>INotifyCollectionChangedEvent&lt;T&gt;</c> index is not correct.</summary>
        InvalidIndex = 3,

        /// <summary>Received <c>INotifyCollectionChangedEvent&lt;T&gt;</c> item is not correct, especially in case of missing removing, moving, or replacing items equality.</summary>
        InvalidItem = 4,


        NotSupportedIndex = 20,

        NotSupportedItem = 21,
    }
}
