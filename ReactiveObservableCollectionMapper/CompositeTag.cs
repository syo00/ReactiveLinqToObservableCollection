using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirinji.LightWands;

namespace Kirinji.LinqToObservableCollection
{
    internal sealed class CompositeTag : IEquatable<CompositeTag>
    {
        readonly IReadOnlyList<object> tags;
        public CompositeTag(object x, object y)
        {
            this.tags = new[] { x, y };
        }
        

        public override bool Equals(object obj)
        {
            return Equals(obj as CompositeTag);
        }

        public bool Equals(CompositeTag other)
        {
            if (other == null) return false;
            if (tags.Count != other.tags.Count) return false;
            return tags.SequenceEqual(other.tags);
        }

        public override int GetHashCode()
        {
            return tags.Select(tag => ObjectEx.GetHashCode(tag)).Aggregate((x, y) => x ^ y);
        }

        public override string ToString()
        {
            return Support.Converters.ListToString(tags, 2);
        }
    }
}
