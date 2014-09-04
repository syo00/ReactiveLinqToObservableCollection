using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Debug
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DebugSetting
    {
        public static string Version = "1";
        public static CheckSynchronizationType CheckSynchronization;
        public static bool UseCastToConvertEvent;

        public static string SettingInfo
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("Version: ");
                sb.Append(Version);
                sb.AppendLine();
                sb.Append("CheckSynchronization: ");
                sb.Append(CheckSynchronization);
                sb.AppendLine();
                sb.Append("UseCastToConvertEvent: ");
                sb.Append(UseCastToConvertEvent);

                return sb.ToString();
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum CheckSynchronizationType
    {
        CheckSynchronization,
        Synchronization,
        Disable,
    }
}
