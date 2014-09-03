using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public class SchedulingAndThreading
    {
        public SchedulingAndThreadingType Type { get; private set; }
        public IScheduler ObserveOnScheduler { get; private set; }
        public object SynchronizationObject { get; private set; }

        public static SchedulingAndThreading ObserveOn(IScheduler scheduler)
        {
            Contract.Requires<ArgumentNullException>(scheduler != null);
            Contract.Ensures(Contract.Result<SchedulingAndThreading>() != null);

            return new SchedulingAndThreading { Type = SchedulingAndThreadingType.ObserveOn, ObserveOnScheduler = scheduler };
        }

        public static SchedulingAndThreading Synchronize()
        {
            Contract.Ensures(Contract.Result<SchedulingAndThreading>() != null);

            return new SchedulingAndThreading { Type = SchedulingAndThreadingType.Synchronize };
        }

        public static SchedulingAndThreading Synchronize(object syncObject)
        {
            Contract.Ensures(Contract.Result<SchedulingAndThreading>() != null);

            return new SchedulingAndThreading { Type = SchedulingAndThreadingType.Synchronize, SynchronizationObject = syncObject };
        }
    }

    public enum SchedulingAndThreadingType
    {
        None,
        Synchronize,
        ObserveOn,
    }
}
