﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl.Producers
{
    class Box<T>
    {
        public bool HasValue { get; set; }
        public T Value { get; set; }
    }
}
