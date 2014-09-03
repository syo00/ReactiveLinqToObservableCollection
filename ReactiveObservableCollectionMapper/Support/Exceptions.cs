using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Support
{
    internal static class Exceptions
    {
        public static Exception UnpredictableSwitchCasePattern
        {
            get
            {
                return new Exception("動作が定義されていません。");
            }
        }

        public static InvalidOperationException TryToOverrideName
        {
            get
            {
                return new InvalidOperationException("既に登録されている名前です。");
            }
        }
    }
}
