using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal class SynchronizationChecker
    {
        int isWorking;

        public void Start()
        {
            var result = Interlocked.Exchange(ref isWorking, 1);

            if (result != 0)
            {
                throw new InvalidOperationException("Not synchronized");
            }
        }

        public void End()
        {
            isWorking = 0;
        }
    }
}
