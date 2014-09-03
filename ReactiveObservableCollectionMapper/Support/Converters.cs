using Kirinji.LightWands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal static class Converters
    {
        public static string ListToString<T>(IReadOnlyList<T> source, int maxItemsCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return ListToString(source, maxItemsCount, ObjectEx.ToString);
        }

        public static string ListToString<T>(IReadOnlyList<T> source, int maxItemsCount, Func<T, string> toString)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(toString != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var result = new StringBuilder();

            if (source.Count == 0)
            {
                return "[]";
            }
            else
            {
                if (source.Count > maxItemsCount)
                {
                    return source.Count + " items";
                }

                result.Append('[');
                bool isFirstLoop = true;
                foreach (var s in source)
                {
                    if (!isFirstLoop)
                    {
                        result.Append(", ");
                    }
                    else
                    {
                        isFirstLoop = false;
                    }

                    result.Append(toString(s));
                }
                result.Append(']');
                return result.ToString();
            }
        }
    }
}
