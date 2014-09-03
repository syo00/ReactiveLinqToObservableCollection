using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public interface IGroupedCollectionStatuses<out TKey, out TElement> : ICollectionStatuses<TElement>
    {
        TKey Key { get; }
    }
}
