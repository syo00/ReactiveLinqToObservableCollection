using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents
{
    internal enum SimpleNotifyCollectionChangedEventAction
    {
        InitialState,
        AddOrRemove,
        Reset,
    }
}
