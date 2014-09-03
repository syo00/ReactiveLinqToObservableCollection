using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    public struct ValueOrEmpty<T> : IEquatable<ValueOrEmpty<T>>
    {
        public ValueOrEmpty(T value)
            : this()
        {
            this.HasValue = true;
            this.Value = value;
        }

        public ValueOrEmpty<TTo> Select<TTo>(Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(converter != null);

            return HasValue ? new ValueOrEmpty<TTo>(converter(Value)) : new ValueOrEmpty<TTo>();
        }

        public bool HasValue { get; private set; }
        public T Value { get; private set; }

        public override string ToString()
        {
            return HasValue ? ObjectEx.ToString(Value) : "(no value)";
        }

        public bool Equals(ValueOrEmpty<T> other)
        {
            if (!this.HasValue && !other.HasValue)
            {
                return true;
            }

            if (this.HasValue != other.HasValue)
            {
                return false;
            }

            return Object.Equals(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is ValueOrEmpty<T>)
            {
                return Equals((ValueOrEmpty<T>)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (HasValue.GetHashCode() * 1024) ^ ObjectEx.GetHashCode(Value);
        }
    }
}
