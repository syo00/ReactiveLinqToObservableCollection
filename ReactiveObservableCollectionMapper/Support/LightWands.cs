//-----------------------------------------------------------------------
// <copyright file="LightWands.cs">
//    Copyright (c) 2013-2013, syo00.
//
//    Licensed under the MIT License (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.opensource.org/licenses/mit-license.php
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// <website>https://github.com/syo00/LightWands</website>
//-----------------------------------------------------------------------


// VERSION: 0.8.1


/***** public or internal *****/
// NOTE: uncomment the following line to make LightWands class internal.
#define USE_INTERNAL


/***** targeting projects *****/
// NOTE: select one number from (1), (2), or (3) by your project and uncomment its corresponding #define. You do not have to uncomment more than two #define lines in (1) to (3).

// (1) If you want to apply for below projects, uncomment #define TESTS and add reference to System.Runtime.Serialization.
//     * tests for .NET Framework 4.5
//#define TESTS

// (2) Else if you want to apply for below projects (including portable class libraries), uncomment #define NET45_WINRT45_WP8.
//     * .NET Framework 4.5
//     * Windows store application     
//     * Windows Phone 8
#define NET45_WINRT45_WP8

// (3) Else if you want to apply for below projects (including portable class libraries), uncomment #define NET40_SL5_WINRT45_WP8.
//     * .NET Framework 4.0
//     * Silverlight 5
//     * Windows store application
//     * Windows Phone 8
//#define NET40_SL5_WINRT45_WP8


/***** add implicit operators to Choice and ChoiceWithEmpty *****/
// NOTE: uncomment the following line to add implicit conversion operators to Choice and ChoiceWithEmpty class.
//#define USE_CHOICE_IMPLICIT



#if TESTS
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

#if NET45_WINRT45_WP8 || TESTS
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;


namespace Kirinji.LightWands
{
#if NET40_SL5_WINRT45_WP8 || NET45_WINRT45_WP8 || TESTS

    #region Disposable

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class Disposable
    {
        public static bool TryDisposeAndRelease<T>(ref T disposingValue) where T : class
        {
            if (disposingValue == null) return false;
            var d = disposingValue as IDisposable;
            disposingValue = null;
            if (d != null) d.Dispose();
            return true;
        }

        public static bool TryDispose<T>(T disposingValue)
        {
            if (disposingValue == null) return false;
            var d = disposingValue as IDisposable;
            if (d == null) return false;
            d.Dispose();
            return true;
        }
    }

    #endregion


    #region EnumerableEx

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class EnumerableEx
    {
        public static IEnumerable<T> Empty<T>()
        {
            return new T[] { };
        }

        public static IEnumerable<T> Return<T>(T value)
        {
            return new T[] { value };
        }
    }

    #endregion


    #region CollectionExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(collection != null);

            foreach (var r in collection) source.Add(r);
        }
    }

    #endregion


    #region DictionaryExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class DictionaryExtensions
    {
        public static IEnumerable<TKey> RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<TKey, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<IEnumerable<TKey>>() != null);

            var removingKeys = new List<TKey>();
            foreach (var pair in source)
            {
                if (predicate(pair.Key))
                {
                    removingKeys.Add(pair.Key);
                }
            }

            foreach (var removingKey in removingKeys)
            {
                source.Remove(removingKey);
            }

            return removingKeys;
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            TValue value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(TValue);
            }
        }
    }

    #endregion


    #region EnumerableExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class EnumerableExtensions
    {
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);

            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);

            int index = 0;
            foreach (var item in source)
            {
                action(item, index);
                index++;
                yield return item;
            }
        }

        public static IEnumerable<T> DoWhenDebug<T>(this IEnumerable<T> source, Action<T> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);

#if DEBUG
            return source.Do(action);
#else
            return source;
#endif
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);

            foreach (var item in source) action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);

            int index = 0;
            foreach (var item in source)
            {
                action(item, index);
                index++;
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> actionAsync)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(actionAsync != null);

            foreach (var item in source)
            {
                await actionAsync(item);
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> actionAsync)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(actionAsync != null);

            int index = 0;
            foreach (var item in source)
            {
                await actionAsync(item, index);
                index++;
            }
        }

        public static IEnumerable<T> Hide<T>(this IEnumerable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            foreach (var s in source) yield return s;
        }

        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IDictionary<TKey, TValue>>() != null);

            return source.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// 指定された条件に合致した、最後の要素のインデックスを返します。見つからなかった場合は null が返されます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int? FirstIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            for (int i = 0; i <= source.Count() - 1; i++)
            {
                if (predicate(source.ElementAt(i))) return i;
            }

            return null;
        }

        /// <summary>
        /// 指定された条件に合致した、最後の要素のインデックスを返します。見つからなかった場合は null が返されます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int? LastIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            for (int i = source.Count() - 1; i >= 0; i--)
            {
                if (predicate(source.ElementAt(i))) return i;
            }
            return null;
        }

        /// <summary>
        /// 指定された条件に合致した、唯一の要素のインデックスを返します。見つからなかった場合は null が返されます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int? SingleIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int? firstIndex = null;
            for (int i = 0; i <= source.Count() - 1; i++)
            {
                if (predicate(source.ElementAt(i)))
                {
                    if (firstIndex == null)
                    {
                        firstIndex = i;
                    }
                    else
                    {
                        throw new InvalidOperationException("要素が複数見つかりました。");
                    }
                }
            }
            return firstIndex;
        }

        public static IEnumerable<int> WhereIndexes<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            List<int> matchedIndexes = new List<int>();
            for (int i = 0; i <= source.Count() - 1; i++)
            {
                if (predicate(source.ElementAt(i))) matchedIndexes.Add(i);
            }
            return matchedIndexes;
        }

        /// <summary>
        /// 指定されたインデックスの要素を返します。存在しない場合は null が返されます。
        /// </summary>
        /// <typeparam name="T">T は値型である必要があります。</typeparam>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T? ElementAtOrNull<T>(this IEnumerable<T> source, int index) where T : struct
        {
            Contract.Requires<ArgumentNullException>(source != null);

            if (source.Count() - 1 >= index)
            {
                return source.ElementAt(index);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 最初の要素を返します。存在しない場合は null が返されます。
        /// </summary>
        /// <typeparam name="T">T は値型である必要があります。</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct
        {
            Contract.Requires<ArgumentNullException>(source != null);

            if (source.Any())
            {
                return source.ElementAt(0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 指定された条件に合致した最初の要素を返します。見つからなかった場合は null が返されます。
        /// </summary>
        /// <typeparam name="T">T は値型である必要があります。</typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int? index = FirstIndex(source, predicate);
            if (index.HasValue)
            {
                return source.ElementAt(index.Value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 最後の要素を返します。存在しない場合は null が返されます。
        /// </summary>
        /// <typeparam name="T">T は値型である必要があります。</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T? LastOrNull<T>(this IEnumerable<T> source) where T : struct
        {
            Contract.Requires<ArgumentNullException>(source != null);

            if (source.Any())
            {
                return source.ElementAt(source.Count() - 1);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 指定された条件に合致した最後の要素を返します。見つからなかった場合は null が返されます。
        /// </summary>
        /// <typeparam name="T">T は値型である必要があります。</typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T? LastOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int? index = LastIndex(source, predicate);
            if (index.HasValue)
            {
                return source.ElementAt(index.Value);
            }
            else
            {
                return null;
            }
        }

        // SingleOrNull<T> の predicate がないオーバーロードメソッドは、例外を返すかnullを返すかのシチュエーションがはっきりしないので未定義
        // まあ SingleOrDefault<T> を参考にすればいいんだけど実装しても使う場面がないし

        /// <summary>
        /// 指定された条件に合致した唯一の要素を返します。見つからなかった場合は null が返されます。
        /// </summary>
        /// <typeparam name="T">T は値型である必要があります。</typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T? SingleOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int? index = SingleIndex(source, predicate);
            if (index.HasValue)
            {
                return source.ElementAt(index.Value);
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<KeyValuePair<int, T>> Indexes<T>(this IEnumerable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<int, T>>>() != null);

            int count = 0;
            foreach (var e in source)
            {
                yield return new KeyValuePair<int, T>(count, e);
                count++;
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params T[] second)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);

            return source.Concat(second.AsEnumerable());
        }

        public static bool NonSequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> second)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);

            Func<T, T, bool> comparer = EqualityComparer<T>.Default.Equals;
            var sourceList = source.ToList();
            foreach (var sec in second)
            {
                if (!sourceList.RemoveFirst(t => comparer(t, sec))) return false;
            }
            return !sourceList.Any();
        }

        public static bool NonSequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> second, Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            var sourceList = source.ToList();
            foreach (var sec in second)
            {
                if (!sourceList.RemoveFirst(t => comparer(t, sec))) return false;
            }
            return !sourceList.Any();
        }
    }

    #endregion


    #region ListExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class ListExtensions
    {
        public static bool RemoveFirst<T>(this IList<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int? firstIndex = source.FirstIndex(predicate);
            if (firstIndex.HasValue)
            {
                source.RemoveAt(firstIndex.Value);
                return true;
            }
            else
                return false;
        }

        public static bool RemoveFirst<T>(this IList<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            if (source.Count == 0)
                return false;
            else
            {
                source.RemoveAt(0);
                return true;
            }
        }

        public static bool RemoveLast<T>(this IList<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            if (source.Count == 0)
                return false;
            else
            {
                source.RemoveAt(source.Count - 1);
                return true;
            }
        }

        public static bool RemoveLast<T>(this IList<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int? lastIndex = source.LastIndex(predicate);
            if (lastIndex.HasValue)
            {
                source.RemoveAt(lastIndex.Value);
                return true;
            }
            else
                return false;
        }

        public static IEnumerable<T> RemoveAll<T>(this IList<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var removedItems = new List<T>(source);
            source.Clear();
            return removedItems;
        }

        public static IEnumerable<T> RemoveAll<T>(this IList<T> source, T item)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return source.RemoveAll(t => Object.Equals(t, item));
        }

        public static IEnumerable<T> RemoveAll<T>(this IList<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            var removedItems = new List<T>();

            for (int i = source.Count - 1; i >= 0; i--)
            {
                var item = source[i];
                if (predicate(item))
                {
                    removedItems.Add(item);
                    source.RemoveAt(i);
                }
            }

            removedItems.Reverse();
            return removedItems;
        }

        public static T PopFirst<T>(this IList<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var firstItem = source.First();
            source.RemoveFirst();
            return firstItem;
        }

        public static IList<T> PopFirst<T>(this IList<T> source, int count)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            if (count <= -1) throw new ArgumentOutOfRangeException();

            var returnList = new List<T>();

            foreach (var i in Enumerable.Range(0, count))
            {
                returnList.Add(source.PopFirst());
            }

            return returnList;
        }

        public static T PopLast<T>(this IList<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var lastItem = source.Last();
            source.RemoveLast();
            return lastItem;
        }

        public static IList<T> PopLast<T>(this IList<T> source, int count)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(count >= 0);

            var returnList = new List<T>();

            foreach (var i in Enumerable.Range(0, count))
            {
                returnList.Add(source.PopLast());
            }

            returnList.Reverse();
            return returnList;
        }
    }

    #endregion


    #region StringExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class StringExtensions
    {
        /// <summary>
        /// 改行も Trim する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string TrimBreak(this string s)
        {
            if (s == null)
                return null;
            else if (s == "")
                return "";
            else
                return s.Trim(' ', '\r', '\n');
        }

        /// <summary>
        /// 改行も削除する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DeleteSpaces(this string s)
        {
            Contract.Requires<ArgumentNullException>(s != null);

            return s.Replace(" ", "").Replace("\r", "").Replace("\n", "");
        }

        private static IDictionary<string, string> WidthDicExcludingRegexSymbols = new Dictionary<string, string>
            {

                {"Ａ","A"},
                {"Ｂ","B"},
                {"Ｃ","C"},
                {"Ｄ","D"},
                {"Ｅ","E"},
                {"Ｆ","F"},
                {"Ｇ","G"},
                {"Ｈ","H"},
                {"Ｉ","I"},
                {"Ｊ","J"},
                {"Ｋ","K"},
                {"Ｌ","L"},
                {"Ｍ","M"},
                {"Ｎ","N"},
                {"Ｏ","O"},
                {"Ｐ","P"},
                {"Ｑ","Q"},
                {"Ｒ","R"},
                {"Ｓ","S"},
                {"Ｔ","T"},
                {"Ｕ","U"},
                {"Ｖ","V"},
                {"Ｗ","W"},
                {"Ｘ","X"},
                {"Ｙ","Y"},
                {"Ｚ","Z"},
                {"ａ","a"},
                {"ｂ","b"},
                {"ｃ","c"},
                {"ｄ","d"},
                {"ｅ","e"},
                {"ｆ","f"},
                {"ｇ","g"},
                {"ｈ","h"},
                {"ｉ","i"},
                {"ｊ","j"},
                {"ｋ","k"},
                {"ｌ","l"},
                {"ｍ","m"},
                {"ｎ","n"},
                {"ｏ","o"},
                {"ｐ","p"},
                {"ｑ","q"},
                {"ｒ","r"},
                {"ｓ","s"},
                {"ｔ","t"},
                {"ｕ","u"},
                {"ｖ","v"},
                {"ｗ","w"},
                {"ｘ","x"},
                {"ｙ","y"},
                {"ｚ","z"},
                {"ｶﾞ", "ガ"},
                {"ｷﾞ", "ギ"},
                {"ｸﾞ", "グ"},
                {"ｹﾞ", "ゲ"},
                {"ｺﾞ", "ゴ"},
                {"ｻﾞ", "ザ"},
                {"ｼﾞ", "ジ"},
                {"ｽﾞ", "ズ"},
                {"ｾﾞ", "ゼ"},
                {"ｿﾞ", "ゾ"},
                {"ﾀﾞ", "ダ"},
                {"ﾁﾞ", "ヂ"},
                {"ﾂﾞ", "ヅ"},
                {"ﾃﾞ", "デ"},
                {"ﾄﾞ", "ド"},
                {"ﾊﾞ", "バ"},
                {"ﾋﾞ", "ビ"},
                {"ﾌﾞ", "ブ"},
                {"ﾍﾞ", "ベ"},
                {"ﾎﾞ", "ボ"},
                {"ﾊﾟ", "パ"},
                {"ﾋﾟ", "ピ"},
                {"ﾌﾟ", "プ"},
                {"ﾍﾟ", "ペ"},
                {"ﾎﾟ", "ポ"},
                {"ｳﾞ", "ヴ"},
                {"ｱ", "ア"},
                {"ｲ", "イ"},
                {"ｳ", "ウ"},
                {"ｴ", "エ"},
                {"ｵ", "オ"},
                {"ｶ", "カ"},
                {"ｷ", "キ"},
                {"ｸ", "ク"},
                {"ｹ", "ケ"},
                {"ｺ", "コ"},
                {"ｻ", "サ"},
                {"ｼ", "シ"},
                {"ｽ", "ス"},
                {"ｾ", "セ"},
                {"ｿ", "ソ"},
                {"ﾀ", "タ"},
                {"ﾁ", "チ"},
                {"ﾂ", "ツ"},
                {"ﾃ", "テ"},
                {"ﾄ", "ト"},
                {"ﾅ", "ナ"},
                {"ﾆ", "ニ"},
                {"ﾇ", "ヌ"},
                {"ﾈ", "ネ"},
                {"ﾉ", "ノ"},
                {"ﾊ", "ハ"},
                {"ﾋ", "ヒ"},
                {"ﾌ", "フ"},
                {"ﾍ", "ヘ"},
                {"ﾎ", "ホ"},
                {"ﾏ", "マ"},
                {"ﾐ", "ミ"},
                {"ﾑ", "ム"},
                {"ﾒ", "メ"},
                {"ﾓ", "モ"},
                {"ﾔ", "ヤ"},
                {"ﾕ", "ユ"},
                {"ﾖ", "ヨ"},
                {"ﾗ", "ラ"},
                {"ﾘ", "リ"},
                {"ﾙ", "ル"},
                {"ﾚ", "レ"},
                {"ﾛ", "ロ"},
                {"ﾜ", "ワ"},
                {"ｦ", "ヲ"},
                {"ﾝ", "ン"},
                {"ｧ", "ァ"},
                {"ｨ", "ィ"},
                {"ｩ", "ゥ"},
                {"ｪ", "ェ"},
                {"ｫ", "ォ"},
                {"ｬ", "ャ"},
                {"ｭ", "ュ"},
                {"ｮ", "ョ"},
                {"ｯ", "ッ"},
                {"ﾞ", "゛"},
                {"ﾟ", "゜"},
                {"｢", "「"},
                {"｣", "」"},
                {"､", "、"},
                {"｡", "。"},
                {"ｰ", "ー"}
            };

        // ToDo: 不完全
        static IDictionary<string, string> WidthDicRegexSymbolsToRegexSymbols = new Dictionary<string, string>
        {
            {"￥",@"\\"},
            {"（",@"\("},
            {"）",@"\)"}
        };

        // ToDo: 不完全
        static IDictionary<string, string> WidthDicRegexSymbolsToString = new Dictionary<string, string>
        {
            {"￥",@"\"},
            {"（","("},
            {"）",")"}
        };

        /// <summary>
        /// 全角文字、半角文字を統一する（正規表現の文字列に対して）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RegexIgnoreWidth(this string s)
        {
            Contract.Requires<ArgumentNullException>(s != null);

            var str = s;

            foreach (var set in WidthDicExcludingRegexSymbols)
            {
                str = str.Replace(set.Key, set.Value);
            }

            foreach (var set in WidthDicRegexSymbolsToRegexSymbols)
            {
                str = str.Replace(set.Key, set.Value);
            }

            return str;
        }

        /// <summary>
        /// 全角文字、半角文字を統一する（正規表現でない文字列に対して）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StringIgnoreWidth(this string s)
        {
            Contract.Requires<ArgumentNullException>(s != null);

            var str = s;

            foreach (var set in WidthDicExcludingRegexSymbols)
            {
                str = str.Replace(set.Key, set.Value);
            }

            foreach (var set in WidthDicRegexSymbolsToString)
            {
                str = str.Replace(set.Key, set.Value);
            }

            return str;
        }


        static IDictionary<string, string> KanaDic = new Dictionary<string, string>
        {
            {"あ", "ア"},
            {"い", "イ"},
            {"う", "ウ"},
            {"え", "エ"},
            {"お", "オ"},
            {"か", "カ"},
            {"き", "キ"},
            {"く", "ク"},
            {"け", "ケ"},
            {"こ", "コ"},
            {"が", "ガ"},
            {"ぎ", "ギ"},
            {"ぐ", "グ"},
            {"げ", "ゲ"},
            {"ご", "ゴ"},
            {"さ", "サ"},
            {"し", "シ"},
            {"す", "ス"},
            {"せ", "セ"},
            {"そ", "ソ"},
            {"ざ", "ザ"},
            {"じ", "ジ"},
            {"ず", "ズ"},
            {"ぜ", "ゼ"},
            {"ぞ", "ゾ"},
            {"た", "タ"},
            {"ち", "チ"},
            {"つ", "ツ"},
            {"て", "テ"},
            {"と", "ト"},
            {"だ", "ダ"},
            {"ぢ", "ヂ"},
            {"づ", "ヅ"},
            {"で", "デ"},
            {"ど", "ド"},
            {"な", "ナ"},
            {"に", "ニ"},
            {"ぬ", "ヌ"},
            {"ね", "ネ"},
            {"の", "ノ"},
            {"は", "ハ"},
            {"ひ", "ヒ"},
            {"ふ", "フ"},
            {"へ", "ヘ"},
            {"ほ", "ホ"},
            {"ば", "バ"},
            {"び", "ビ"},
            {"ぶ", "ブ"},
            {"べ", "ベ"},
            {"ぼ", "ボ"},
            {"ぱ", "パ"},
            {"ぴ", "ピ"},
            {"ぷ", "プ"},
            {"ぺ", "ペ"},
            {"ぽ", "ポ"},
            {"ま", "マ"},
            {"み", "ミ"},
            {"む", "ム"},
            {"め", "メ"},
            {"も", "モ"},
            {"や", "ヤ"},
            {"ゆ", "ユ"},
            {"よ", "ヨ"},
            {"ら", "ラ"},
            {"り", "リ"},
            {"る", "ル"},
            {"れ", "レ"},
            {"ろ", "ロ"},
            {"わ", "ワ"},
            {"ゐ", "ヰ"},
            {"ゑ", "ヱ"},
            {"を", "ヲ"},
            {"ん", "ン"},
            {"ぁ", "ァ"},
            {"ぃ", "ィ"},
            {"ぅ", "ゥ"},
            {"ぇ", "ェ"},
            {"ぉ", "ォ"},
            {"ゕ", "ヵ"},
            {"ゖ", "ヶ"},
            {"ゔ", "ヴ"},
            {"ゝ", "ヽ"},
            //{"ゞ", "ヾ"} なぜかバグる
        };

        /// <summary>
        /// 全角文字、半角文字を統一する。対象文字列が通常の文字列でも Regex コンストラクタに渡す文字列でもどちらでも構わない
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string IgnoreKana(this string s)
        {
            Contract.Requires<ArgumentNullException>(s != null);

            string str = s;
            foreach (var set in KanaDic)
            {
                str = str.Replace(set.Key, set.Value);
            }

            return str;
        }

        public static byte? ByteParse(this string s)
        {
            byte outByte;
            if (byte.TryParse(s, out outByte))
                return outByte;
            else
                return null;
        }

        public static byte? ByteParse(this string s, NumberStyles numberStyles, IFormatProvider formatProvider)
        {
            byte outByte;
            if (byte.TryParse(s, numberStyles, formatProvider, out outByte))
                return outByte;
            else
                return null;
        }

        public static int? IntParse(this string s)
        {
            int outInt;
            if (int.TryParse(s, out outInt))
                return outInt;
            else
                return null;
        }

        public static int? IntParse(this string s, NumberStyles numberStyles, IFormatProvider formatProvider)
        {
            int outInt;
            if (int.TryParse(s, numberStyles, formatProvider, out outInt))
                return outInt;
            else
                return null;
        }

        public static long? LongParse(this string s)
        {
            long outLong;
            if (long.TryParse(s, out outLong))
                return outLong;
            else
                return null;
        }

        public static long? LongParse(this string s, NumberStyles numberStyles, IFormatProvider formatProvider)
        {
            long outLong;
            if (long.TryParse(s, numberStyles, formatProvider, out outLong))
                return outLong;
            else
                return null;
        }

        public static DateTime? DateTimeParse(this string s)
        {
            DateTime outDate;
            if (DateTime.TryParse(s, out outDate))
                return outDate;
            else
                return null;
        }

        public static DateTime? DateTimeParse(this string s, IFormatProvider formatProvider, DateTimeStyles dateTimeStyles)
        {
            DateTime outDate;
            if (DateTime.TryParse(s, formatProvider, dateTimeStyles, out outDate))
                return outDate;
            else
                return null;
        }

        public static DateTime? DateTimeParseExact(this string s, IFormatProvider formatProvider, DateTimeStyles dateTimeStyles, params string[] formats)
        {
            DateTime outDate;
            if (DateTime.TryParseExact(s, formats, formatProvider, dateTimeStyles, out outDate))
                return outDate;
            else
                return null;
        }

        public static bool? BoolParse(this string s)
        {
            bool outBool;
            if (bool.TryParse(s, out outBool))
                return outBool;
            else
                return null;
        }
    }

    #endregion


    #region EqualityComparer

    /// <summary>Creates <c>EqualityComparer&lt;T&gt;</c> by delegates.</summary>
    class AnonymousEqualityComparer<T> : EqualityComparer<T>
    {
        private Func<T, T, bool> comparerDelegate;
        private Func<T, int> getHashCodeDelegate;

        /// <remarks>Not recommended to use this constructor because GetHashCode always returns same value and it makes programs slow.</remarks>
        public AnonymousEqualityComparer(Func<T, T, bool> comparerDelegate)
            : this(comparerDelegate, _ => 1)
        {
            Contract.Requires<ArgumentNullException>(comparerDelegate != null);
        }

        public AnonymousEqualityComparer(Func<T, T, bool> comparerDelegate, Func<T, int> getHashCodeDelegate)
        {
            Contract.Requires<ArgumentNullException>(comparerDelegate != null);
            Contract.Requires<ArgumentNullException>(getHashCodeDelegate != null);

            this.comparerDelegate = comparerDelegate;
            this.getHashCodeDelegate = getHashCodeDelegate;
        }

        public override bool Equals(T x, T y)
        {
            return this.comparerDelegate(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return this.getHashCodeDelegate(obj);
        }
    }

    /// <summary>Supports creating <c>EqualityComparer&lt;T&gt;</c>.</summary>
#if USE_INTERNAL
    internal
#else
    public
#endif
 static class EqualityComparer
    {
        /// <remarks>Not recommended to use this method because GetHashCode always returns same value and it makes programs slow.</remarks>
        public static EqualityComparer<T> Create<T>(Func<T, T, bool> comparer)
        {
            Contract.Requires<ArgumentNullException>(comparer != null);

            return new AnonymousEqualityComparer<T>(comparer);
        }

        public static EqualityComparer<T> Create<T>(Func<T, T, bool> comparer, Func<T, int> hashCodeCreator)
        {
            Contract.Requires<ArgumentNullException>(comparer != null);
            Contract.Requires<ArgumentNullException>(hashCodeCreator != null);

            return new AnonymousEqualityComparer<T>(comparer, hashCodeCreator);
        }

        /// <summary>Creates <c>EqualityComparer&lt;T&gt;</c> by specifying parameters or methods.</summary>
        public static EqualityComparer<T> Create<T>(IEnumerable<Func<T, object>> comparingParameters)
        {
            Contract.Requires<ArgumentNullException>(comparingParameters != null);

            Func<T, T, bool> comparer = (x, y) => comparingParameters.All(f => Object.Equals(f(x), f(y)));
            Func<T, int> hashCodeCreator = t => comparingParameters
                .Select(f => f(t))
                .Select(p => p == null ? 0 : p.GetHashCode())
                .Aggregate((l, r) => l ^ r);
            return new AnonymousEqualityComparer<T>(comparer, hashCodeCreator);
        }

        /// <summary>Creates <c>EqualityComparer&lt;T&gt;</c> by specifying parameters or methods.</summary>
        public static EqualityComparer<T> Create<T>(params Func<T, object>[] comparingParameters)
        {
            Contract.Requires<ArgumentNullException>(comparingParameters != null);

            return Create(comparingParameters.AsEnumerable());
        }

        /// <summary>Creates <c>EqualityComparer&lt;T&gt;</c> of using references of objects.</summary>
        /// <remarks>Be careful not using boxed objects.</remarks>
        public static EqualityComparer<T> ReferenceEquals<T>() where T : class
        {
            Func<T, T, bool> comparer = (x, y) => Object.ReferenceEquals(x, y);
            Func<T, int> hashCodeCreator = t => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(t);

            return new AnonymousEqualityComparer<T>(comparer, hashCodeCreator);
        }

        /// <summary>Creates <c>EqualityComparer&lt;IEnumerable&lt;T&gt;&gt;</c> to compare sequentially.</summary>
        public static EqualityComparer<IEnumerable<T>> EnumerableOf<T>()
        {
            return EnumerableOfInner<T>(false, Comparer<T>.Default);
        }

        /// <summary>Creates <c>EqualityComparer&lt;IEnumerable&lt;T&gt;&gt;</c> to compare the number of each values.</summary>
        public static EqualityComparer<IEnumerable<T>> EnumerableOfUnordered<T>()
        {
            return EnumerableOfInner<T>(true, Comparer<T>.Default);
        }

        /// <summary>Creates <c>EqualityComparer&lt;IEnumerable&lt;T&gt;&gt;</c> to compare the number of each values.</summary>
        public static EqualityComparer<IEnumerable<T>> EnumerableOfUnordered<T>(IComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(comparer != null);

            return EnumerableOfInner<T>(true, comparer);
        }

        // ignoreOrder = true のとき、順序がバラバラでも要素の個数が合っていれば Equal となる
        private static EqualityComparer<IEnumerable<T>> EnumerableOfInner<T>(bool ignoreOrder, IComparer<T> orderingComparer)
        {
            Contract.Requires<ArgumentNullException>(orderingComparer != null);

            Func<IEnumerable<T>, int> hashCodeCreator = e =>
                e.Count() == 0
                ? 1
                : e
                    .Select(p => p == null ? 0 : p.GetHashCode())
                    .Aggregate((l, r) => l ^ r);
            if (ignoreOrder)
            {
                return EqualityComparer.Create<IEnumerable<T>>(
                       (e1, e2) =>
                       {
                           if (e1 == null || e2 == null) return e1 == e2;
                           return e1
                               .OrderBy(p => p, orderingComparer)
                               .SequenceEqual(e2.OrderBy(p => p, orderingComparer));
                       },
                       hashCodeCreator);
            }
            else
            {
                return EqualityComparer.Create<IEnumerable<T>>(
                    (e1, e2) =>
                    {
                        if (e1 == null || e2 == null) return e1 == e2;
                        return e1.SequenceEqual(e2);
                    },
                    hashCodeCreator);
            }
        }
    }

    #endregion


    #region ObjectEx

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class ObjectEx
    {
        /// <summary>Null-safe method for GetHashCode.</summary>
        public static int GetHashCode<T>(T value)
        {
            if (value == null) return 0;
            return value.GetHashCode();
        }

        /// <summary>Null-safe method for ToString.</summary>
        public static string ToString<T>(T value)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return ToString(value, "Null");
        }

        /// <summary>
        /// Null-safe method for ToString.
        /// </summary>
        /// <param name="nullString">Returning string value when vakue is null.</param>
        public static string ToString<T>(T value, string nullString)
        {
            Contract.Requires<ArgumentNullException>(nullString != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (value == null) return nullString;
            return value.ToString();
        }

        /// <summary>Null-safe method for GetType. If value is null, returns typeof(T).</summary>
        public static Type GetType<T>(T value)
        {
            Contract.Ensures(Contract.Result<Type>() != null);

            return value == null ? typeof(T) : value.GetType();
        }

        /// <summary>Null-safe method for GetType. If value is null, returns null.</summary>
        public static Type GetTypeOrDefault<T>(T value)
        {
            return value == null ? null : value.GetType();
        }
    }

    #endregion


    #region Choice

    /// <summary>F# の判別共用体を再現した機能を提供します。</summary>
#if USE_INTERNAL
    internal
#else
    public
#endif
 sealed class Choice<T1, T2> : IEquatable<IChoice>, IChoice
    {
        readonly ChoiceWithEmpty<T1, T2> coreChoice;

        public Choice(T1 value1)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2>(value1);
        }

        public Choice(T2 value2)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2>(value2);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(coreChoice != null);
        }

        public T Match<T>(Func<T1, T> convert1, Func<T2, T> convert2)
        {
            return coreChoice.Match(convert1, convert2, null);
        }

        public void Action(Action<T1> action1, Action<T2> action2)
        {
            coreChoice.Action(action1, action2, null);
        }

        public object AsObject()
        {
            return Match<object>(x => x, x => x);
        }

        public override bool Equals(object obj)
        {
            return Choice.Equals(this, obj);
        }

        public bool Equals(IChoice other)
        {
            return Choice.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Match(ObjectEx.GetHashCode, ObjectEx.GetHashCode);
        }

        public override string ToString()
        {
            return Match(ObjectEx.ToString, ObjectEx.ToString);
        }

#if USE_CHOICE_IMPLICIT
        public static implicit operator Choice<T1, T2>(T1 value) { return new Choice<T1, T2>(value); }
        public static implicit operator Choice<T1, T2>(T2 value) { return new Choice<T1, T2>(value); }
#endif
    }

    /// <summary>F# の判別共用体を再現した機能を提供します。</summary>
#if USE_INTERNAL
    internal
#else
    public
#endif
 sealed class Choice<T1, T2, T3> : IEquatable<IChoice>, IChoice
    {
        readonly ChoiceWithEmpty<T1, T2, T3> coreChoice;

        public Choice(T1 value1)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3>(value1);
        }

        public Choice(T2 value2)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3>(value2);
        }

        public Choice(T3 value3)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3>(value3);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(coreChoice != null);
        }

        public T Match<T>(Func<T1, T> convert1, Func<T2, T> convert2, Func<T3, T> convert3)
        {
            return coreChoice.Match(convert1, convert2, convert3, null);
        }

        public void Action(Action<T1> action1, Action<T2> action2, Action<T3> action3)
        {
            coreChoice.Action(action1, action2, action3, null);
        }

        public object AsObject()
        {
            return Match<object>(x => x, x => x, x => x);
        }

        public override bool Equals(object obj)
        {
            return Choice.Equals(this, obj);
        }

        public bool Equals(IChoice other)
        {
            return Choice.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Match(ObjectEx.GetHashCode, ObjectEx.GetHashCode, ObjectEx.GetHashCode);
        }

        public override string ToString()
        {
            return Match(ObjectEx.ToString, ObjectEx.ToString, ObjectEx.ToString);
        }

#if USE_CHOICE_IMPLICIT
        public static implicit operator Choice<T1, T2, T3>(T1 value) { return new Choice<T1, T2, T3>(value); }
        public static implicit operator Choice<T1, T2, T3>(T2 value) { return new Choice<T1, T2, T3>(value); }
        public static implicit operator Choice<T1, T2, T3>(T3 value) { return new Choice<T1, T2, T3>(value); }
#endif
    }

    /// <summary>F# の判別共用体を再現した機能を提供します。</summary>
#if USE_INTERNAL
    internal
#else
    public
#endif
 sealed class Choice<T1, T2, T3, T4> : IEquatable<IChoice>, IChoice
    {
        readonly ChoiceWithEmpty<T1, T2, T3, T4> coreChoice;

        public Choice(T1 value1)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3, T4>(value1);
        }

        public Choice(T2 value2)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3, T4>(value2);
        }

        public Choice(T3 value3)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3, T4>(value3);
        }

        public Choice(T4 value4)
        {
            this.coreChoice = new ChoiceWithEmpty<T1, T2, T3, T4>(value4);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(coreChoice != null);
        }

        public T Match<T>(Func<T1, T> convert1, Func<T2, T> convert2, Func<T3, T> convert3, Func<T4, T> convert4)
        {
            return coreChoice.Match(convert1, convert2, convert3, convert4, null);
        }

        public void Action(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4)
        {
            coreChoice.Action(action1, action2, action3, action4, null);
        }

        public object AsObject()
        {
            return Match<object>(x => x, x => x, x => x, x => x);
        }

        public override bool Equals(object obj)
        {
            return Choice.Equals(this, obj);
        }

        public bool Equals(IChoice other)
        {
            return Choice.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Match(ObjectEx.GetHashCode, ObjectEx.GetHashCode, ObjectEx.GetHashCode, ObjectEx.GetHashCode);
        }

        public override string ToString()
        {
            return Match(ObjectEx.ToString, ObjectEx.ToString, ObjectEx.ToString, ObjectEx.ToString);
        }

#if USE_CHOICE_IMPLICIT
        public static implicit operator Choice<T1, T2, T3, T4>(T1 value) { return new Choice<T1, T2, T3, T4>(value); }
        public static implicit operator Choice<T1, T2, T3, T4>(T2 value) { return new Choice<T1, T2, T3, T4>(value); }
        public static implicit operator Choice<T1, T2, T3, T4>(T3 value) { return new Choice<T1, T2, T3, T4>(value); }
        public static implicit operator Choice<T1, T2, T3, T4>(T4 value) { return new Choice<T1, T2, T3, T4>(value); }
#endif
    }

    #endregion


    #region ChoiceWithEmpty

    [DataContract]
#if USE_INTERNAL
    internal
#else
    public
#endif
 sealed class ChoiceWithEmpty<T1, T2> : IEquatable<IChoice>, IChoice
    {
        [DataMember]
        readonly int? valueIndex;

        [DataMember]
        readonly T1 value1;

        [DataMember]
        readonly T2 value2;

        /// <summary>空の値を示すインスタンスを作成します。</summary>
        public ChoiceWithEmpty()
        {

        }

        public ChoiceWithEmpty(T1 value1)
        {
            this.value1 = value1;
            this.valueIndex = 0;
        }

        public ChoiceWithEmpty(T2 value2)
        {
            this.value2 = value2;
            this.valueIndex = 1;
        }

        public T Match<T>(Func<T1, T> convert1, Func<T2, T> convert2, Func<T> convertEmpty)
        {
            switch (valueIndex)
            {
                case 0:
                    if (convert1 == null) throw new InvalidOperationException();
                    return convert1(value1);
                case 1:
                    if (convert2 == null) throw new InvalidOperationException();
                    return convert2(value2);
                default:
                    if (convertEmpty == null) throw new InvalidOperationException();
                    return convertEmpty();
            }
        }

        public void Action(Action<T1> action1, Action<T2> action2, Action actionEmpty)
        {
            switch (valueIndex)
            {
                case 0:
                    if (action1 == null) throw new InvalidOperationException();
                    action1(value1);
                    return;
                case 1:
                    if (action2 == null) throw new InvalidOperationException();
                    action2(value2);
                    return;
                default:
                    if (actionEmpty == null) throw new InvalidOperationException();
                    actionEmpty();
                    return;
            }
        }

        public object AsObject()
        {
            return Match<object>(x => x, x => x, () => Unit.Default);
        }

        public override bool Equals(object obj)
        {
            return Choice.Equals(this, obj);
        }

        public bool Equals(IChoice other)
        {
            return Choice.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Match(ObjectEx.GetHashCode, ObjectEx.GetHashCode, () => 0);
        }

        public override string ToString()
        {
            return Match(ObjectEx.ToString, ObjectEx.ToString, () => "Empty");
        }

#if USE_CHOICE_IMPLICIT
        public static implicit operator ChoiceWithEmpty<T1, T2>(T1 value) { return new ChoiceWithEmpty<T1, T2>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2>(T2 value) { return new ChoiceWithEmpty<T1, T2>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2>(Unit empty) { return new ChoiceWithEmpty<T1, T2>(); }
#endif
    }

    [DataContract]
#if USE_INTERNAL
    internal
#else
    public
#endif
 sealed class ChoiceWithEmpty<T1, T2, T3> : IEquatable<IChoice>, IChoice
    {
        [DataMember]
        readonly int? valueIndex;

        [DataMember]
        readonly T1 value1;

        [DataMember]
        readonly T2 value2;

        [DataMember]
        readonly T3 value3;

        /// <summary>空の値を示すインスタンスを作成します。</summary>
        public ChoiceWithEmpty()
        {

        }

        public ChoiceWithEmpty(T1 value1)
        {
            this.value1 = value1;
            this.valueIndex = 0;
        }

        public ChoiceWithEmpty(T2 value2)
        {
            this.value2 = value2;
            this.valueIndex = 1;
        }

        public ChoiceWithEmpty(T3 value3)
        {
            this.value3 = value3;
            this.valueIndex = 2;
        }

        public T Match<T>(Func<T1, T> convert1, Func<T2, T> convert2, Func<T3, T> convert3, Func<T> convertEmpty)
        {
            switch (valueIndex)
            {
                case 0:
                    if (convert1 == null) throw new InvalidOperationException();
                    return convert1(value1);
                case 1:
                    if (convert2 == null) throw new InvalidOperationException();
                    return convert2(value2);
                case 2:
                    if (convert3 == null) throw new InvalidOperationException();
                    return convert3(value3);
                default:
                    if (convertEmpty == null) throw new InvalidOperationException();
                    return convertEmpty();
            }
        }

        public void Action(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action actionEmpty)
        {
            switch (valueIndex)
            {
                case 0:
                    if (action1 == null) throw new InvalidOperationException();
                    action1(value1);
                    return;
                case 1:
                    if (action2 == null) throw new InvalidOperationException();
                    action2(value2);
                    return;
                case 2:
                    if (action3 == null) throw new InvalidOperationException();
                    action3(value3);
                    return;
                default:
                    if (actionEmpty == null) throw new InvalidOperationException();
                    actionEmpty();
                    return;
            }
        }

        public object AsObject()
        {
            return Match<object>(x => x, x => x, x => x, () => Unit.Default);
        }

        public override bool Equals(object obj)
        {
            return Choice.Equals(this, obj);
        }

        public bool Equals(IChoice other)
        {
            return Choice.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Match(ObjectEx.GetHashCode, ObjectEx.GetHashCode, ObjectEx.GetHashCode, () => 0);
        }

        public override string ToString()
        {
            return Match(ObjectEx.ToString, ObjectEx.ToString, ObjectEx.ToString, () => "Empty");
        }

#if USE_CHOICE_IMPLICIT
        public static implicit operator ChoiceWithEmpty<T1, T2, T3>(T1 value) { return new ChoiceWithEmpty<T1, T2, T3>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3>(T2 value) { return new ChoiceWithEmpty<T1, T2, T3>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3>(T3 value) { return new ChoiceWithEmpty<T1, T2, T3>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3>(Unit empty) { return new ChoiceWithEmpty<T1, T2, T3>(); }
#endif
    }

    [DataContract]
#if USE_INTERNAL
    internal
#else
    public
#endif
 sealed class ChoiceWithEmpty<T1, T2, T3, T4> : IEquatable<IChoice>, IChoice
    {
        [DataMember]
        readonly int? valueIndex;

        [DataMember]
        readonly T1 value1;

        [DataMember]
        readonly T2 value2;

        [DataMember]
        readonly T3 value3;

        [DataMember]
        readonly T4 value4;

        /// <summary>空の値を示すインスタンスを作成します。</summary>
        public ChoiceWithEmpty()
        {

        }

        public ChoiceWithEmpty(T1 value1)
        {
            this.value1 = value1;
            this.valueIndex = 0;
        }

        public ChoiceWithEmpty(T2 value2)
        {
            this.value2 = value2;
            this.valueIndex = 1;
        }

        public ChoiceWithEmpty(T3 value3)
        {
            this.value3 = value3;
            this.valueIndex = 2;
        }

        public ChoiceWithEmpty(T4 value4)
        {
            this.value4 = value4;
            this.valueIndex = 3;
        }

        public T Match<T>(Func<T1, T> convert1, Func<T2, T> convert2, Func<T3, T> convert3, Func<T4, T> convert4, Func<T> convertEmpty)
        {
            switch (valueIndex)
            {
                case 0:
                    if (convert1 == null) throw new InvalidOperationException();
                    return convert1(value1);
                case 1:
                    if (convert2 == null) throw new InvalidOperationException();
                    return convert2(value2);
                case 2:
                    if (convert3 == null) throw new InvalidOperationException();
                    return convert3(value3);
                case 3:
                    if (convert4 == null) throw new InvalidOperationException();
                    return convert4(value4);
                default:
                    if (convertEmpty == null) throw new InvalidOperationException();
                    return convertEmpty();
            }
        }

        public void Action(Action<T1> action1, Action<T2> action2, Action<T3> action3, Action<T4> action4, Action actionEmpty)
        {
            switch (valueIndex)
            {
                case 0:
                    if (action1 == null) throw new InvalidOperationException();
                    action1(value1);
                    return;
                case 1:
                    if (action2 == null) throw new InvalidOperationException();
                    action2(value2);
                    return;
                case 2:
                    if (action3 == null) throw new InvalidOperationException();
                    action3(value3);
                    return;
                case 3:
                    if (action4 == null) throw new InvalidOperationException();
                    action4(value4);
                    return;
                default:
                    if (actionEmpty == null) throw new InvalidOperationException();
                    actionEmpty();
                    return;
            }
        }

        public object AsObject()
        {
            return Match<object>(x => x, x => x, x => x, x => x, () => Unit.Default);
        }

        public override bool Equals(object obj)
        {
            return Choice.Equals(this, obj);
        }

        public bool Equals(IChoice other)
        {
            return Choice.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Match(ObjectEx.GetHashCode, ObjectEx.GetHashCode, ObjectEx.GetHashCode, ObjectEx.GetHashCode, () => 0);
        }

        public override string ToString()
        {
            return Match(ObjectEx.ToString, ObjectEx.ToString, ObjectEx.ToString, ObjectEx.ToString, () => "Empty");
        }

#if USE_CHOICE_IMPLICIT
        public static implicit operator ChoiceWithEmpty<T1, T2, T3, T4>(T1 value) { return new ChoiceWithEmpty<T1, T2, T3, T4>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3, T4>(T2 value) { return new ChoiceWithEmpty<T1, T2, T3, T4>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3, T4>(T3 value) { return new ChoiceWithEmpty<T1, T2, T3, T4>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3, T4>(T4 value) { return new ChoiceWithEmpty<T1, T2, T3, T4>(value); }
        public static implicit operator ChoiceWithEmpty<T1, T2, T3, T4>(Unit empty) { return new ChoiceWithEmpty<T1, T2, T3, T4>(); }
#endif
    }

    #endregion


    #region IChoice

#if USE_INTERNAL
    internal
#else
    public
#endif
 interface IChoice
    {
        object AsObject();
    }

    #endregion


    #region Choice (static)

    internal static class Choice
    {
        public static bool Equals(IChoice x, IChoice y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return Object.Equals(x.AsObject(), y.AsObject());
        }

        public static bool Equals(IChoice x, object y)
        {
            Contract.Requires<ArgumentNullException>(x != null);

            var ichoice = y as IChoice;
            if (ichoice != null) return Equals(x, ichoice);

            return Object.Equals(x.AsObject(), y);
        }
    }


    #region Unit


#if NET40_SL5_WINRT45_WP8
    public struct Unit : IEquatable<Unit>
    {
        public static Unit Default
        {
            get
            {
                return new Unit();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Unit;
        }

        public bool Equals(Unit other)
        {
            return true;
        }

        public static bool operator ==(Unit first, Unit second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Unit first, Unit second)
        {
            return !(first == second);
        }

        public override int GetHashCode()
        {
            return 123456;
        }

        public override string ToString()
        {
            return "()";
        }
    }
#endif

    #endregion

    #endregion

#endif

#if NET45_WINRT45_WP8 || TESTS

    #region ArrayExtensions

    public static class ArrayExtensions
    {
        public static IReadOnlyList<T> ToReadOnly<T>(this T[] source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IReadOnlyCollection<T>>() != null);

            return new ReadOnlyArray<T>(source);
        }

        [DataContract]
        class ReadOnlyArray<T> : IReadOnlyList<T>, IList<T>
        {
            [DataMember]
            readonly T[] coreArray;

            public ReadOnlyArray(T[] array)
            {
                Contract.Requires<ArgumentNullException>(array != null);

                this.coreArray = array;
            }

            public int Count
            {
                get { return coreArray == null ? 0 : coreArray.Length; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return (coreArray ?? new T[0]).AsEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public T this[int index]
            {
                get { return (coreArray ?? new T[0])[index]; /* 例外は丸投げ */ }
            }

            int IList<T>.IndexOf(T item)
            {
                return coreArray == null ? -1 : Array.IndexOf(coreArray, item);
            }

            void IList<T>.Insert(int index, T item)
            {
                throw new NotSupportedException();
            }

            void IList<T>.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            T IList<T>.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            void ICollection<T>.Add(T item)
            {
                throw new NotSupportedException();
            }

            void ICollection<T>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<T>.Contains(T item)
            {
                return coreArray == null ? false : coreArray.Any(elem => Object.Equals(elem, item));
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                (coreArray ?? new T[0]).CopyTo(array, arrayIndex);
            }

            bool ICollection<T>.IsReadOnly
            {
                get { return true; }
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotSupportedException();
            }
        }
    }

    #endregion


    #region ReadOnlyDictionaryExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class ReadOnlyDictionaryExtensions
    {
        public static TValue ValueOrDefaultByReadOnly<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            TValue value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(TValue);
            }
        }
    }

    #endregion


    #region ObservableCollectionExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class ObservableCollectionExtensions
    {
        public static int ReplaceAll<T>(this ObservableCollection<T> source, T newItem, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(newItem != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            return source.ReplaceAll(t => newItem, predicate);
        }

        public static int ReplaceAll<T>(this ObservableCollection<T> source, Func<T, T> newItem, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(newItem != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            int matchedCount = 0;
            for (int i = 0; i <= source.Count - 1; i++)
            {
                var item = source[i];
                if (predicate(item))
                {
                    source[i] = newItem(item);
                    matchedCount++;
                }
            }
            return matchedCount;
        }

        public static ReadOnlyObservableCollection<T> ToReadOnly<T>(this ObservableCollection<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return new ReadOnlyObservableCollection<T>(source);
        }

        public static void Update<T>(this ObservableCollection<T> source, IEnumerable<T> updateCollection)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(updateCollection != null);

            source.Update(updateCollection, EqualityComparer<T>.Default);
        }

        /// <summary>ObservableCollection の内容をコレクションと比較し、それぞれの要素数を合わせます。順序は保存されません。</summary>
        public static void Update<T>(this ObservableCollection<T> source, IEnumerable<T> updateCollection, IEqualityComparer<T> comparer)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(updateCollection != null);
            Contract.Requires<ArgumentNullException>(comparer != null);

            // updateCollection の数 - source の数
            IDictionary<T, int> 要素の増減量 = new Dictionary<T, int>();

            foreach (var s in source)
            {
                if (要素の増減量.ContainsKey(s))
                {
                    要素の増減量[s]--;
                }
                else
                {
                    要素の増減量[s] = -1;
                }
            }

            foreach (var u in updateCollection)
            {
                if (要素の増減量.ContainsKey(u))
                {
                    要素の増減量[u]++;
                }
                else
                {
                    要素の増減量[u] = 1;
                }
            }

            foreach (var i in 要素の増減量.Where(p => p.Value < 0))
            {
                source.Remove(i.Key);
            }

            foreach (var i in 要素の増減量.Where(p => p.Value > 0))
            {
                source.Add(i.Key);
            }
        }
    }

    #endregion


    #region ObservableEx

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class ObservableEx
    {
        public static IObservable<T> Empty<T>()
        {
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var emptyObservable = Observable.Empty<T>();
            Contract.Assert(emptyObservable != null);
            return emptyObservable;
        }

        public static IObservable<T> Empty<T>(IScheduler scheduler)
        {
            Contract.Requires<ArgumentNullException>(scheduler != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var emptyObservable = Observable.Empty<T>(scheduler);
            Contract.Assert(emptyObservable != null);
            return emptyObservable;
        }

        public static IObservable<T> Never<T>()
        {
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var neverObservable = Observable.Never<T>();
            Contract.Assert(neverObservable != null);
            return neverObservable;
        }

        public static IObservable<T> Return<T>(T value)
        {
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var returnObservable = Observable.Return<T>(value);
            Contract.Assert(returnObservable != null);
            return returnObservable;
        }

        public static IObservable<T> Return<T>(T value, IScheduler scheduler)
        {
            Contract.Requires<ArgumentNullException>(scheduler != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var returnObservable = Observable.Return<T>(value, scheduler);
            Contract.Assert(returnObservable != null);
            return returnObservable;
        }
    }

    #endregion


    #region ObservableExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class ObservableExtensions
    {
        public static T MostRecentValue<T>(this IObservable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var value = source.MostRecent(default(T)).First();
            Contract.Assume(value != null);
            return value;
        }

        public static T MostRecentValue<T>(this IObservable<T> source, T missingValue)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var value = source.MostRecent(missingValue).First();
            Contract.Assume(value != null);
            return value;
        }

        /// <summary>Invokes actions when subscriptions count changes 0 to 1 or 1 to 0.</summary>
        public static IObservable<T> OnSubscriptionChanged<T>(this IObservable<T> source, Action onStarted, Action onPaused)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onStarted != null);
            Contract.Requires<ArgumentNullException>(onPaused != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var refCount = 0;

            var value = Observable.Create<T>(obs =>
            {
                if (refCount == 0) onStarted();
                refCount++;

                return source
                    .Finally(() =>
                    {
                        refCount--;
                        if (refCount == 0) onPaused();
                    })
                    .Subscribe(obs);
            });
            Contract.Assert(value != null);
            return value;
        }

        /// <summary>Passes values when subscribed.</summary>
        public static IObservable<T> WhenSubscribed<T>(this IObservable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var refCount = 0;

            var value = Observable.Create<T>(obs =>
            {
                refCount++;

                return source
                    .Finally(() => refCount--)
                    .Where(_ => refCount >= 1)
                    .Subscribe(obs);
            });
            Contract.Assert(value != null);
            return value;
        }

        /// <summary>Switches multiple sequences.</summary>
        /// <remarks>sources should implements IList&gt;T&lt;.</remarks>
        public static IObservable<SelectorResult<T>> Selector<T>(this IObservable<IEnumerable<int>> selector, IEnumerable<IObservable<T>> sources)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Requires<ArgumentNullException>(sources != null);
            Contract.Ensures(Contract.Result<IObservable<SelectorResult<T>>>() != null);

            CompositeDisposable disposable = new CompositeDisposable();

            var value = Observable.Create<SelectorResult<T>>(observer =>
            {
                return selector.Subscribe(ary =>
                {
                    disposable.Dispose();
                    disposable = new CompositeDisposable();
                    if (ary == null)
                    {
                        observer.OnError(new NullReferenceException());
                        return;
                    }

                    foreach (var i in ary)
                    {
                        var selected = sources.ElementAtOrDefault(i);
                        if (selected == null) return;
                        disposable.Add(selected.Subscribe(
                            x => observer.OnNext(SelectorResult<T>.OnNext(x, selected, i)),
                            ex => observer.OnNext(SelectorResult<T>.OnError(ex, selected, i)),
                            () => observer.OnNext(SelectorResult<T>.OnCompleted(selected, i))
                            ));
                    }
                },
                observer.OnError,
                observer.OnCompleted);
            });
            Contract.Assert(value != null);
            return value;
        }

        public static IObservable<SelectorResult<T>> Selector<T>(this IObservable<IEnumerable<int>> selector, params IObservable<T>[] sources)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Requires<ArgumentNullException>(sources != null);
            Contract.Ensures(Contract.Result<IObservable<SelectorResult<T>>>() != null);

            return selector.Selector(sources.AsEnumerable());
        }

        public static IObservable<SelectorResult<T>> Selector<T>(this IObservable<int> selector, IEnumerable<IObservable<T>> sources)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Requires<ArgumentNullException>(sources != null);
            Contract.Ensures(Contract.Result<IObservable<SelectorResult<T>>>() != null);

            return selector.Select(i => new[] { i }).Selector(sources);
        }

        public static IObservable<SelectorResult<T>> Selector<T>(this IObservable<int> selector, params IObservable<T>[] sources)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Requires<ArgumentNullException>(sources != null);
            Contract.Ensures(Contract.Result<IObservable<SelectorResult<T>>>() != null);

            var parameter = sources.AsEnumerable();
            Contract.Assume(parameter != null);
            return selector.Selector(parameter);
        }

        public static IObservable<IValueOrError<T>> TakeError<T>(this IObservable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<IValueOrError<T>>>() != null);

            return source
                .TakeError<T, Exception>()
                .Select(x => !x.IsError ? ValueOrError.CreateValue(x.Value) : ValueOrError.CreateError<T>(x.Error));
        }

        public static IObservable<IValueOrError<TValue, TException>> TakeError<TValue, TException>(this IObservable<TValue> source) where TException : Exception
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<IValueOrError<TValue, TException>>>() != null);

            var value = source
                .Select(v => ValueOrError.CreateValue<TValue, TException>(v)) // Catch では射影できないので、まずここで通常の値を変換
                .Catch((TException ex) => Observable.Return(ValueOrError.CreateError<TValue, TException>(ex))); // そしてここでエラーを変換
            Contract.Assert(value != null);
            return value;
        }

        public static IObservable<T> ExtractError<T>(this IObservable<IValueOrError<T>> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            return source.Cast<IValueOrError<T, Exception>>().ExtractError();
        }

        public static IObservable<TValue> ExtractError<TValue, TException>(this IObservable<IValueOrError<TValue, TException>> source) where TException : Exception
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<TValue>>() != null);

            var value = Observable.Create<TValue>(observer =>
            {
                var s = source
                    .Where(x => x != null)
                    .Subscribe(x =>
                    {
                        if (x.IsError)
                        {
                            observer.OnError(x.Error);
                        }
                        else
                        {
                            observer.OnNext(x.Value);
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return s;
            });
            Contract.Assert(value != null);
            return value;
        }

        public static IObservable<T> DoWhenDebug<T>(this IObservable<T> source, Action<T> action)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(action != null);

#if DEBUG
            return source.Do(action);
#else
            return source;
#endif
        }

        public static IObservable<T> UseObserver<T>(this IObservable<T> source, Action<IObserver<T>, T, int> onNextSelector, Action<Exception, IObserver<T>> onErrorSelector, Action<IObserver<T>> onCompletedSelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            Action<IObserver<T>, T, int> actualOnNextSelector = onNextSelector ?? ((observer, x, i) => observer.OnNext(x));
            Action<Exception, IObserver<T>> actualOnErrorSelector = onErrorSelector ?? ((error, observer) => observer.OnError(error));
            Action<IObserver<T>> actualOnCompletedSelector = onCompletedSelector ?? ((observer) => observer.OnCompleted());

            int itemsCount = 0;
            return Observable.Create<T>(observer =>
            {
                return source.Subscribe(x =>
                {
                    actualOnNextSelector(observer, x, itemsCount);
                    Interlocked.Increment(ref itemsCount);
                },
                    ex => actualOnErrorSelector(ex, observer),
                    () => actualOnCompletedSelector(observer));
            });
        }

        public static IObservable<T> UseObserver<T>(this IObservable<T> source, Action<IObserver<T>, T, int> onNextSelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            return source
                .UseObserver(onNextSelector, null, null);
        }

        public static IObservable<T> UseObserver<T>(this IObservable<T> source, Action<IObserver<T>, T> onNextSelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(onNextSelector != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            return source
                .UseObserver((observer, x, i) => onNextSelector(observer, x), null, null);
        }
    }

    #endregion


    #region SelectorResult

#if USE_INTERNAL
    internal
#else
    public
#endif
 class SelectorResult<T>
    {
        public static SelectorResult<T> OnNext(T value, IObservable<T> source, int sourceIndex)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<SelectorResult<T>>() != null);

            var r = new SelectorResult<T>();
            r.Kind = NotificationKind.OnNext;
            r.Value = value;
            r.Source = source;
            r.SourceIndex = sourceIndex;
            return r;
        }

        public static SelectorResult<T> OnError(Exception exception, IObservable<T> source, int sourceIndex)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<SelectorResult<T>>() != null);

            var r = new SelectorResult<T>();
            r.Kind = NotificationKind.OnError;
            r.Exception = exception;
            r.Source = source;
            r.SourceIndex = sourceIndex;
            return r;
        }

        public static SelectorResult<T> OnCompleted(IObservable<T> source, int sourceIndex)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<SelectorResult<T>>() != null);

            var r = new SelectorResult<T>();
            r.Kind = NotificationKind.OnCompleted;
            r.Source = source;
            r.SourceIndex = sourceIndex;
            return r;
        }

        public T Value { get; private set; }
        public Exception Exception { get; private set; }
        public NotificationKind Kind { get; private set; }
        public IObservable<T> Source { get; private set; }
        public int SourceIndex { get; private set; }
    }

    #endregion


    #region ValueOrError

    [ContractClass(typeof(IValueOrErrorContract<,>))]
#if USE_INTERNAL
    internal
#else
    public
#endif
 interface IValueOrError<out TValue, out TException> where TException : Exception
    {
        bool IsError { get; }
        TValue Value { get; }
        TException Error { get; }
    }

    [ContractClassFor(typeof(IValueOrError<,>))]
    abstract class IValueOrErrorContract<TValue, TException> : IValueOrError<TValue, TException> where TException : Exception
    {
        public bool IsError
        {
            get { throw new NotImplementedException(); }
        }

        public TValue Value
        {
            get { throw new NotImplementedException(); }
        }

        public TException Error
        {
            get
            {
                Contract.Ensures(!IsError || Contract.Result<TException>() != null);

                throw new NotImplementedException();
            }
        }
    }

#if USE_INTERNAL
    internal
#else
    public
#endif
 interface IValueOrError<out T> : IValueOrError<T, Exception>
    {

    }

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class ValueOrError
    {
        public static IValueOrError<T> CreateValue<T>(T value)
        {
            Contract.Ensures(Contract.Result<IValueOrError<T>>() != null);

            return new ValueOrError<T>(value);
        }

        public static IValueOrError<T, TException> CreateValue<T, TException>(T value) where TException : Exception
        {
            Contract.Ensures(Contract.Result<IValueOrError<T, TException>>() != null);

            return new ValueOrError<T, TException>(value);
        }

        public static IValueOrError<T> CreateError<T>(Exception error)
        {
            Contract.Requires<ArgumentNullException>(error != null);
            Contract.Ensures(Contract.Result<IValueOrError<T>>() != null);

            return new ValueOrError<T>(error);
        }

        public static IValueOrError<T, TException> CreateError<T, TException>(TException error) where TException : Exception
        {
            Contract.Requires<ArgumentNullException>(error != null);
            Contract.Ensures(Contract.Result<IValueOrError<T, TException>>() != null);

            return new ValueOrError<T, TException>(error);
        }
    }

    class ValueOrError<TValue, TException> : IValueOrError<TValue, TException>, IEquatable<ValueOrError<TValue, TException>> where TException : Exception
    {
        public ValueOrError(TValue value)
        {
            this.Value = value;
        }

        public ValueOrError(TException error)
        {
            Contract.Requires<ArgumentNullException>(error != null);

            this.IsError = true;
            this.Error = error;
        }

        public bool IsError { get; private set; }
        public TValue Value { get; private set; }
        public TException Error { get; private set; }

        public Choice<TValue, TException> ToChoice()
        {
            Contract.Ensures(Contract.Result<Choice<TValue, TException>>() != null);

            if (IsError)
            {
                return new Choice<TValue, TException>(Error);
            }
            else
            {
                return new Choice<TValue, TException>(Value);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var casted = obj as ValueOrError<TValue, TException>;
            return Equals(casted);
        }

        public override int GetHashCode()
        {
            if (this.IsError)
            {
                return this.Error.GetHashCode();
            }
            else
            {
                return this.Value == null ? 0 : this.Value.GetHashCode();
            }
        }

        public bool Equals(ValueOrError<TValue, TException> other)
        {
            if (other == null) return false;
            if (this.IsError && other.IsError)
            {
                return Object.Equals(this.Error, other.Error);
            }
            else if (!this.IsError && !other.IsError)
            {
                return Object.Equals(this.Value, other.Value);
            }
            else
            {
                return false;
            }
        }
    }

    class ValueOrError<T> : ValueOrError<T, Exception>, IValueOrError<T>, IEquatable<ValueOrError<T>>
    {
        public ValueOrError(T value)
            : base(value)
        {

        }

        public ValueOrError(Exception error)
            : base(error)
        {
            Contract.Requires<ArgumentNullException>(error != null);
        }

        public bool Equals(ValueOrError<T> other)
        {
            return Equals((ValueOrError<T, Exception>)other);
        }
    }

    #endregion


    #region CashedReplaySubject

#if USE_INTERNAL
    internal
#else
    public
#endif
 class CashedReplaySubject<T> : ISubject<T>, IDisposable
    {
        IList<Tuple<ItemType, T, Exception>> m_cache;
        IScheduler m_scheduler;
        readonly Subject<T> m_source = new Subject<T>();
        bool m_isCompleted;

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.m_cache != null);
            Contract.Invariant(this.m_source != null);
        }

        public CashedReplaySubject(IScheduler scheduler = null)
        {
            this.m_cache = new List<Tuple<ItemType, T, Exception>>();
            this.m_scheduler = scheduler;
        }

        public CashedReplaySubject(int bufferSize, IScheduler scheduler = null)
        {
            this.m_cache = new List<Tuple<ItemType, T, Exception>>(bufferSize);
            this.m_scheduler = scheduler;
        }

        public void OnCompleted()
        {
            ThrowExceptionIfDisposed();

            if (!this.m_isCompleted)
            {
                this.m_isCompleted = true;
                this.m_source.OnCompleted();
                this.m_cache = null;
            }
        }

        public void OnError(Exception error)
        {
            ThrowExceptionIfDisposed();

            if (!this.m_isCompleted)
            {
                this.m_source.OnError(error);
                this.m_cache.Add(new Tuple<ItemType, T, Exception>(ItemType.OnErrorValue, default(T), error));
            }
        }

        public void OnNext(T value)
        {
            ThrowExceptionIfDisposed();

            if (!this.m_isCompleted)
            {
                this.m_source.OnNext(value);
                this.m_cache.Add(new Tuple<ItemType, T, Exception>(ItemType.OnNextValue, value, null));
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            ThrowExceptionIfDisposed();

            return Observable.Merge(this.ReplayCache(), this.m_source).Subscribe(observer);
        }

        public IObservable<T> ReplayCache()
        {
            ThrowExceptionIfDisposed();

            ReplaySubject<T> returnSubject;
            if (this.m_scheduler != null)
            {
                returnSubject = new System.Reactive.Subjects.ReplaySubject<T>(this.m_scheduler);
            }
            else
            {
                returnSubject = new System.Reactive.Subjects.ReplaySubject<T>();
            }
            foreach (var t in this.m_cache)
            {
                switch (t.Item1)
                {
                    case ItemType.OnNextValue:
                        returnSubject.OnNext(t.Item2);
                        break;
                    case ItemType.OnErrorValue:
                        returnSubject.OnError(t.Item3);
                        break;
                }
            }
            returnSubject.OnCompleted();
            return returnSubject.AsObservable();
        }

        enum ItemType
        {
            OnNextValue = 0,
            OnErrorValue = 1,
        }

        #region IDispose implementation
        private bool IsDisposed
        {
            get;
            set;
        }

        private void ThrowExceptionIfDisposed()
        {
            lock (this)
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().FullName + " has been already disposed.");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            lock (this)
            {
                if (IsDisposed) return;
                if (isDisposing) OnDisposingUnManagedResources();
                OnDisposingManagedResources();
                IsDisposed = true;
            }
        }

        protected virtual void OnDisposingManagedResources()
        {
            m_source.Dispose();
        }

        protected virtual void OnDisposingUnManagedResources()
        {

        }
        #endregion

        ~CashedReplaySubject()
        {
            Dispose(false);
        }
    }

    #endregion


    #region ObservableDictionary

#if USE_INTERNAL
    internal
#else
    public
#endif
 class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        private readonly ObservableCollection<TKey> keys = new ObservableCollection<TKey>();
        private readonly ObservableCollection<TValue> values = new ObservableCollection<TValue>();
        private readonly ObservableCollection<KeyValuePair<TKey, TValue>> keyValuePairs = new ObservableCollection<KeyValuePair<TKey, TValue>>();
        private readonly ReadOnlyObservableCollection<TKey> readOnlyKeys;
        private readonly ReadOnlyObservableCollection<TValue> readOnlyValues;
        private readonly ReadOnlyObservableCollection<KeyValuePair<TKey, TValue>> readOnlyKeyValuePairs;

        public ObservableDictionary()
        {
            this.readOnlyKeys = this.keys.ToReadOnly();
            this.readOnlyValues = this.values.ToReadOnly();
            this.readOnlyKeyValuePairs = this.keyValuePairs.ToReadOnly();
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.keys != null);
            Contract.Invariant(this.values != null);
            Contract.Invariant(this.keyValuePairs != null);
            Contract.Invariant(this.readOnlyKeys != null);
            Contract.Invariant(this.readOnlyValues != null);
            Contract.Invariant(this.readOnlyKeyValuePairs != null);

            // 6つのコレクションの個数は常に全部等しい
            Contract.Invariant(this.keys.Count == this.values.Count && this.values.Count == this.keyValuePairs.Count);
            Contract.Invariant(this.readOnlyKeys.Count == this.readOnlyValues.Count && this.readOnlyValues.Count == this.readOnlyKeyValuePairs.Count);
            Contract.Invariant(this.keyValuePairs.Count == this.readOnlyKeyValuePairs.Count);
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key)) throw new ArgumentException();
            this.keys.Add(key);
            this.values.Add(value);
            this.keyValuePairs.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return this.keys.Contains(key);
        }

        public ReadOnlyObservableCollection<TKey> Keys
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyObservableCollection<TKey>>() != null);

                return this.readOnlyKeys;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return this.Keys;
            }
        }

        public bool Remove(TKey key)
        {
            var index = this.keys.IndexOf(key);
            if (index == -1) return false;
            this.keys.RemoveAt(index);
            this.values.RemoveAt(index);
            this.keyValuePairs.RemoveAt(index);
            Contract.Assume(this.keyValuePairs.Count == this.readOnlyKeyValuePairs.Count);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = this.keys.IndexOf(key);
            if (index <= -1)
            {
                value = default(TValue);
                Contract.Assume(!ContainsKey(key));
                return false;
            }
            value = this.values[index];
            Contract.Assume(ContainsKey(key));
            return true;
        }

        public ReadOnlyObservableCollection<TValue> Values
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyObservableCollection<TValue>>() != null);

                return this.readOnlyValues;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return this.Values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var index = this.keys.IndexOf(key);
                if (index == -1) throw new KeyNotFoundException();
                return this.values[index];
            }
            set
            {
                var index = this.keys.IndexOf(key);
                if (index == -1)
                {
                    Add(key, value);
                    return;
                }
                this.values[index] = value;
                this.keyValuePairs[index] = new KeyValuePair<TKey, TValue>(this.keys[index], value);
            }
        }

        public ReadOnlyObservableCollection<KeyValuePair<TKey, TValue>> KeyValuePairs
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyObservableCollection<KeyValuePair<TKey, TValue>>>() != null);

                return this.readOnlyKeyValuePairs;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.keys.Clear();
            this.values.Clear();
            this.keyValuePairs.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.keyValuePairs.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.keyValuePairs.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.keyValuePairs.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var index = this.keyValuePairs.IndexOf(item);
            if (index == -1) return false;
            this.keys.RemoveAt(index);
            this.values.RemoveAt(index);
            this.keyValuePairs.RemoveAt(index);
            Contract.Assume(this.keyValuePairs.Count == this.readOnlyKeyValuePairs.Count);
            return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.keyValuePairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                keyValuePairs.CollectionChanged += value;
            }
            remove
            {
                keyValuePairs.CollectionChanged -= value;
            }
        }
    }

    #endregion


    #region DictionaryExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class DictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IReadOnlyDictionary<TKey, TValue>>() != null);

            return new ReadOnlyDictionary<TKey, TValue>(source);
        }
    }

    #endregion


    #region ListExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
 static partial class ListExtensions
    {
        public static IReadOnlyList<T> ToReadOnly<T>(this IList<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IReadOnlyCollection<T>>() != null);

            return new ReadOnlyCollection<T>(source);
        }
    }

    #endregion


    #region TaskEx

#if USE_INTERNAL
    internal
#else
    public
#endif
 static class TaskEx
    {
        /// <summary>Obtains an empty Task object.</summary>
        public static Task Empty()
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            return Task.FromResult(0);
        }
    }

    #endregion

#endif
}

#if TESTS
namespace Kirinji.LightWands.Tests
{

#region EnumerableExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
    static partial class EnumerableExtensions
    {
        public static void IsSequenceEqual<T>(this IEnumerable<T> source, params T[] second)
        {
            source.IsSequenceEqual(second.AsEnumerable());
        }

        public static void IsSequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> second, string message = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (second == null) throw new ArgumentNullException("second");

            var actual = source.SequenceEqual(second);
            if (message == null)
            {
                Assert.AreEqual(true, actual);
            }
            else
            {
                Assert.AreEqual(true, actual, message);
            }
        }

        public static void IsNonSequenceEqual<T>(this IEnumerable<T> source, params T[] second)
        {
            source.IsNonSequenceEqual(second.AsEnumerable());
        }

        public static void IsNonSequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> second, string message = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (second == null) throw new ArgumentNullException("second");

            var actual = source.NonSequenceEqual(second);
            if (message == null)
            {
                Assert.AreEqual(true, actual);
            }
            else
            {
                Assert.AreEqual(true, actual, message);
            }
        }
    }

#endregion


#region ObservableExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
    static partial class ObservableExtensions
    {
        /// <summary>Starts subscribing and cache pushed values.</summary>
        public static History<T> SubscribeHistory<T>(this IObservable<T> source)
        {
            return new History<T>(source);
        }
    }

    /// <summary>Indicates pushed values.</summary>
#if USE_INTERNAL
    internal
#else
    public
#endif
        class History<T> : IEnumerable<Notification<T>>
    {
        private readonly IList<Notification<T>> notifications = new List<Notification<T>>();

        /// <summary>Creates instance and starts subscribing.</summary>
        public History(IObservable<T> observable)
        {
            if (observable == null) throw new ArgumentNullException("IObservable<T> is null.");
            observable
                .Synchronize()
                .Subscribe(
                t => notifications.Add(Notification.CreateOnNext(t)),
                ex => notifications.Add(Notification.CreateOnError<T>(ex)),
                () => notifications.Add(Notification.CreateOnCompleted<T>())
                );
        }

        /// <summary>Gets values history.</summary>
        public IReadOnlyList<T> Values
        {
            get
            {
                return this.notifications
                    .Where(n => n.Kind == NotificationKind.OnNext)
                    .Select(n => n.Value)
                    .ToList();
            }
        }

        /// <summary>Gets exceptions history.</summary>
        public IReadOnlyList<Exception> Exceptions
        {
            get
            {
                return this.notifications
                    .Where(n => n.Kind == NotificationKind.OnError)
                    .Select(n => n.Exception)
                    .ToList();
            }
        }

        /// <summary>Gets all notifications.</summary>
        public IReadOnlyList<Notification<T>> Notifications
        {
            get
            {
                return notifications.ToList();
            }
        }

        /// <summary>Indicates called OnCompleted.</summary>
        public bool IsCompleted
        {
            get
            {
                return this.notifications
                    .Any(n => n.Kind == NotificationKind.OnCompleted);
            }
        }

        public void Clear()
        {
            this.notifications.Clear();
        }

        [Obsolete]
        public IEnumerator<Notification<T>> GetEnumerator()
        {
            return this.notifications.GetEnumerator();
        }

        [Obsolete]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

#endregion


#region PrivateObjectExtensions

#if USE_INTERNAL
    internal
#else
    public
#endif
    static class PrivateObjectExtensions
    {
        public static object Invoke<T>(this PrivateObject source, string name, T param)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(name != null);

            return source.Invoke(name, new Type[] { typeof(T) }, new object[] { param });
        }

        public static object Invoke<T1, T2>(this PrivateObject source, string name, T1 param1, T2 param2)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(name != null);

            return source.Invoke(name, new Type[] { typeof(T1), typeof(T2) }, new object[] { param1, param2 });
        }

        public static object Invoke<T1, T2, T3>(this PrivateObject source, string name, T1 param1, T2 param2, T3 param3)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(name != null);

            return source.Invoke(name, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, new object[] { param1, param2, param3 });
        }

        public static object Invoke<T1, T2, T3, T4>(this PrivateObject source, string name, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(name != null);

            return source.Invoke(name, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, new object[] { param1, param2, param3, param4 });
        }

        public static object Invoke<T1, T2, T3, T4, T5>(this PrivateObject source, string name, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(name != null);

            return source.Invoke(name, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, new object[] { param1, param2, param3, param4, param5 });
        }

        public static object Invoke<T1, T2, T3, T4, T5, T6>(this PrivateObject source, string name, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(name != null);

            return source.Invoke(name, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, new object[] { param1, param2, param3, param4, param5, param6 });
        }
    }

#endregion

}
#endif