using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(Exception error)
        {
            Contract.Requires<ArgumentNullException>(error != null);

            this.error = error;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(error != null);
        }

        readonly Exception error;
        public Exception Error
        {
            get
            {
                Contract.Ensures(Contract.Result<Exception>() != null);

                return error;
            }
        }
    }
}
