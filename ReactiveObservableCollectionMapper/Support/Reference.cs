using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    class Reference<T>
    {
        public Reference()
        {

        }

        public Reference(T value)
        {
            this.Value = value;
        }

        public T Value { get; set; }
    }
}
