using AppDataCleaner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppDataCleaner.Services
{
    public static class SafetyEvaluator
    {
        // 高置信度缓存标识 - Safe
        private static readonly HashSet<string> CacheFolderPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            "Cache", "Caches", "CachedData", "Cached",
            "Code Cache", "GPUCache", "ShaderCache",
            "logs", "Logs", "LOG", "log",
            "Crash Reports", "crashReports", "CrashReports",
            "Service Worker", "ServiceWorker", "ServiceWorkers",
            "blob_storage", "File System", "Storage",
            "Temp", "temp", "TEMP", "tmp",
            "Sessions", "session", "Backups", "backup",
            "WebKit", "GPUCache", "Local Storage", "IndexedDB"
        };

        private static readonly HashSet<string> CacheFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".tmp", ".temp", ".log", ".bak", ".old", ".cache"
        };

        // 黑名单 - 绝不触碰
        private static readonly HashSet<string> BlackListFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft", "Windows", "System", "SysWOW64",
            "Packages", "ProgramData", "Program Files", "Program Files (x86)",
            "Intel", "NVIDIA", "AMD", "Realtek",
            "dotnet", "NuGet", "pip", "npm", "yarn"
        };

        // 可能包含配置 - Caution
        private static readonly HashSet<string> ConfigFolderPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            "Config", "Configuration", "Settings", "Preferences",
            "Data", "Database", "db", "Profiles", "Profile"
        };

        // 危险标识 - Danger
        private static readonly HashSet<string> DangerPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            "Registry", "System", "Drivers", "Sys"
        };

        public static RiskLevel EvaluateRisk(string path, bool isDirectory)
        {
            var name = System.IO.Path.GetFileName(path);
            var parent = System.IO.Path.GetDirectoryName(path);
            var parentName = parent != null ? System.IO.Path.GetFileName(parent) : "";

            // 1. 检查黑名单
            if (IsBlacklisted(name) || IsBlacklisted(parentName))
            {
                return RiskLevel.Danger;
            }

            // 2. 检查危险标识
            if (DangerPatterns.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                return RiskLevel.Danger;
            }

            // 3. 检查明确的缓存标识 - Safe
            if (CacheFolderPatterns.Contains(name))
            {
                return RiskLevel.Safe;
            }

            if (!isDirectory)
            {
                var ext = System.IO.Path.GetExtension(name);
                if (CacheFileExtensions.Contains(ext))
                {
                    return RiskLevel.Safe;
                }
            }

            // 4. 检查可能包含配置的 - Caution
            if (ConfigFolderPatterns.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                return RiskLevel.Caution;
            }

            // 5. 应用根目录通常是Caution
            if (IsApplicationRoot(name, parent))
            {
                return RiskLevel.Caution;
            }

            // 默认Safe（如果里面有缓存文件会被识别）
            return RiskLevel.Safe;
        }

        private static bool IsBlacklisted(string name)
        {
            return BlackListFolders.Contains(name);
        }

        private static bool IsApplicationRoot(string name, string? parent)
        {
            if (parent == null) return false;

            var parentName = System.IO.Path.GetFileName(parent);
            // 如果在Local或Roaming下直接是应用文件夹
            return parentName.Equals("Local", StringComparison.OrdinalIgnoreCase)
                || parentName.Equals("LocalLow", StringComparison.OrdinalIgnoreCase)
                || parentName.Equals("Roaming", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ShouldScanDirectory(string path)
        {
            var name = System.IO.Path.GetFileName(path);
            return !IsBlacklisted(name);
        }
    }
}
