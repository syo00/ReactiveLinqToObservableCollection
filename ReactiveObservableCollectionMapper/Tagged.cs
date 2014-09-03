using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection
{
    internal sealed class Tagged<T> : IEquatable<Tagged<T>>
    {
        readonly object tag;

        public Tagged(T item)
        {
            this.tag = new object();
            this.Item = item;
        }

        public Tagged(T item, object tag)
        {
            Contract.Requires<ArgumentNullException>(tag != null);

            this.tag = tag;
            this.Item = item;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(tag != null);
        }

        public T Item { get; private set; }
        public object Tag
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);

                return tag;
            }
        }

        public Tagged<TTo> Select<TTo>(Func<T, TTo> converter)
        {
            Contract.Requires<ArgumentNullException>(converter != null);
            Contract.Ensures(Contract.Result<Tagged<TTo>>() != null);

            return new Tagged<TTo>(converter(Item), Tag);
        }

        public override bool Equals(object obj)
        {
            var casted = obj as Tagged<T>;
            if (casted == null) return false;
            return Equals(casted);
        }

        public bool Equals(Tagged<T> other)
        {
            return Object.Equals(tag, other.tag);
        }

        public override int GetHashCode()
        {
            return tag.GetHashCode();
        }

        public override string ToString()
        {
            return "Tag: " + tag.GetHashCode() + " (" + ObjectEx.ToString(Item) + ")";
        }
    }

}
