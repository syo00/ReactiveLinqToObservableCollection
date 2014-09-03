using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public enum NotifyCollectionChangedEventAction
    {
        /// <summary>最初の購読元のコレクションの状態を示します。</summary>
        /// <remarks>これが指定されたイベントは原則として購読した瞬間のみに1回通知されます。</remarks>
        InitialState = 0,

        Add = 1,

        Remove = 2,

        Replace = 3,

        Move = 4,

        Reset = 5,
    }
}
