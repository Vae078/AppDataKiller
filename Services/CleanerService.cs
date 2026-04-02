using AppDataCleaner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AppDataCleaner.Services
{
    public interface ICleanerService
    {
        Task<CleanResult> CleanAsync(List<AppDataItem> items, IProgress<CleanProgress>? progress = null, CancellationToken cancellationToken = default);
    }

    public class CleanResult
    {
        public bool Success { get; set; }
        public long TotalFreed { get; set; }
        public int ItemsProcessed { get; set; }
        public int ItemsFailed { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CleanProgress
    {
        public string CurrentItem { get; set; } = string.Empty;
        public int ProcessedCount { get; set; }
        public int TotalCount { get; set; }
        public long FreedSoFar { get; set; }
    }

    public class CleanerService : ICleanerService
    {
        // Windows API 移入回收站
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public string pFrom;
            public string pTo;
            public ushort fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_SILENT = 0x0004;

        public async Task<CleanResult> CleanAsync(List<AppDataItem> items, IProgress<CleanProgress>? progress, CancellationToken cancellationToken)
        {
            var result = new CleanResult();
            int processed = 0;
            long freed = 0;

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                processed++;
                progress?.Report(new CleanProgress
                {
                    CurrentItem = item.Name,
                    ProcessedCount = processed,
                    TotalCount = items.Count,
                    FreedSoFar = freed
                });

                try
                {
                    if (await DeleteItemAsync(item, cancellationToken))
                    {
                        freed += item.Size;
                        result.ItemsProcessed++;
                    }
                    else
                    {
                        result.ItemsFailed++;
                    }
                }
                catch (Exception ex)
                {
                    result.ItemsFailed++;
                    result.Errors.Add($"{item.Name}: {ex.Message}");
                }
            }

            result.TotalFreed = freed;
            result.Success = result.ItemsFailed == 0;

            return result;
        }

        private async Task<bool> DeleteItemAsync(AppDataItem item, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // 使用Windows API移入回收站
                    var fileOp = new SHFILEOPSTRUCT
                    {
                        wFunc = FO_DELETE,
                        pFrom = item.Path + "\0\0", // 双null结尾
                        fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT
                    };

                    int result = SHFileOperation(ref fileOp);
                    return result == 0;
                }
                catch
                {
                    // 失败时尝试常规删除
                    try
                    {
                        if (item.IsDirectory)
                        {
                            Directory.Delete(item.Path, true);
                        }
                        else
                        {
                            File.Delete(item.Path);
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }, cancellationToken);
        }
    }
}
