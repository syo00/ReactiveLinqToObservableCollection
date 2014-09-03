using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Kirinji.LinqToObservableCollection
{
    /* ---ICollectionStatuses<T> の規則---
     * 
     * 1. 1 つ目に流される値は NotifyCollectionChangedEventAction.InitialState
     * 2. 2 つ目以降に流される値は NotifyCollectionChangedEventAction.InitialState 以外
     * 
     * -はっきりしていないこと
     * 1. 流される値の個数が 0 というのは許容される？
     * 2. NotifyCollectionChangedEventAction.InitialState の値は、必ず購読した瞬間に cold で流れてくる？
     *      -購読した瞬間に cold で流れてくる、ということを保証する(そのような IObservable に変換できる)コードが書けるのか？
     *          ↑今のところ購読した瞬間に cold で流さなければならない必然性が見当たらない。であれば InitialState が遅れて流れてきたり、非同期で流れてくるのもサポートすればより便利になると思われる。
     * 
     */

    [ContractClass(typeof(ICollectionStatuses_Contract<>))]
    public interface ICollectionStatuses<out T>
    {
        IObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged { get; }
    }

    #region IObservableEnumerable contract
    [ContractClassFor(typeof(ICollectionStatuses<>))]
    abstract class ICollectionStatuses_Contract<T> : ICollectionStatuses<T>
    {
        public IObservable<INotifyCollectionChangedEvent<T>> InitialStateAndChanged
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<INotifyCollectionChangedEvent<T>>>() != null);

                throw new NotImplementedException();
            }
        }
    }
    #endregion

}
