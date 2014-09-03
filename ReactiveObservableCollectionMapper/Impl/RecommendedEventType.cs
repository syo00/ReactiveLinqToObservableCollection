using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    // Flags に後から対応可能
    internal enum RecommendedEventType
    {
        None = 0,
        DefaultOne = 1,
        SlimOne = 2,
        SimpleOne = 1024,
        SlimSimpleOne = 2048,
    }
}
