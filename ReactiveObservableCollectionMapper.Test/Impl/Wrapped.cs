using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Test.Impl
{
    internal sealed class Wrapped<T> : IEquatable<Wrapped<T>>
    {
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Wrapped<T>;
            if (other == null) return false;

            return Equals(other);
        }

        public bool Equals(Wrapped<T> other)
        {
            if (other == null) return false;
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return ObjectEx.GetHashCode(Value);
        }

        public override string ToString()
        {
            return ObjectEx.ToString(Value);
        }
    }
}
