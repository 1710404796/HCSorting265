using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HBYWCZP2024.ViewModel
{
    internal static class PalletCsvExportHelper
    {
        private const int CodesPerLine = 14;

        public static string FormatCodes(IEnumerable<string?> codes)
        {
            var list = codes
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim())
                .ToList();

            if (list.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new();
            for (int i = 0; i < list.Count; i += CodesPerLine)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(string.Join(",", list.Skip(i).Take(CodesPerLine)));
            }

            return sb.ToString();
        }

        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            return value.Contains(',') || value.Contains('\r') || value.Contains('\n')
                ? $"\"{value}\""
                : value;
        }
    }
}
