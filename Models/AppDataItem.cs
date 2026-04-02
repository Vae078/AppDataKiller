using System;
using System.Collections.Generic;
using System.IO;

namespace AppDataCleaner.Models
{
    public class AppDataItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string? Description { get; set; }
        public List<AppDataItem> Children { get; set; } = new();
        public bool IsDirectory { get; set; }
        public string? ParentApplication { get; set; }

        public string FormattedSize => FormatBytes(Size);

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0 B";
            int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
            return $"{bytes / Math.Pow(1024, i):F2} {sizes[i]}";
        }
    }
}
