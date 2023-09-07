using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeSwitch
{
    public static class ExtensionsOfException
    {
        public static string FullStackTrace(this Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(ex.MessageWithStackTrace());
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"Inner exception - {innerException.MessageWithStackTrace()}");
                innerException = innerException.InnerException;
            }
            return sb.ToString();
        }

        private static string MessageWithStackTrace(this Exception ex)
        {
            return string.Concat($"{ ex.GetType().FullName}: { ex.Message}", Environment.NewLine, Environment.NewLine, ex.StackTrace);
        }

    }
}
