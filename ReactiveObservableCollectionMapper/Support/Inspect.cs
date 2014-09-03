using Kirinji.LightWands;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    public static class Inspect
    {
        public static IObservable<string> CollectionChanged(INotifyCollectionChanged source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<string>>() != null);

            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => new NotifyCollectionChangedEventHandler(h),
                h => source.CollectionChanged += h,
                h => source.CollectionChanged -= h)
                .Select(pattern => NotifyCollectionChangedEventArgs(pattern.EventArgs));
        }

        public static string NotifyCollectionChangedEventArgs(NotifyCollectionChangedEventArgs e)
        {
            Contract.Requires<ArgumentNullException>(e != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                return "Reset";
            }
            
            return NotifyCollectionChangedEvent.Convert<object>(e, () => null).ToString();
        }

        public static string Enumerable<T>(IEnumerable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return Converters.ListToString(source.ToArray(), 10);
        }
    }
}
