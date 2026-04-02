using AppDataCleaner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDataCleaner.Services
{
    public class ScanProgress
    {
        public string Message { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public string CurrentItem { get; set; } = string.Empty;
    }

    public interface IScannerService
    {
        Task<List<AppDataItem>> ScanAsync(IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default);
    }

    public class ScannerService : IScannerService
    {
        private readonly List<string> _scanPaths;

        public ScannerService()
        {
            _scanPaths = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Path.GetTempPath()
            };
        }

        public async Task<List<AppDataItem>> ScanAsync(IProgress<ScanProgress>? progress, CancellationToken cancellationToken)
        {
            var results = new List<AppDataItem>();
            int totalPaths = _scanPaths.Count;
            int currentPathIndex = 0;

            foreach (var basePath in _scanPaths)
            {
                if (!Directory.Exists(basePath))
                {
                    currentPathIndex++;
                    continue;
                }

                int pathProgressStart = (currentPathIndex * 100) / totalPaths;
                int pathProgressEnd = ((currentPathIndex + 1) * 100) / totalPaths;

                progress?.Report(new ScanProgress
                {
                    Message = $"正在扫描: {Path.GetFileName(basePath)}",
                    ProgressPercent = pathProgressStart,
                    CurrentItem = basePath
                });

                try
                {
                    var items = await ScanDirectoryAsync(basePath, progress, pathProgressStart, pathProgressEnd, cancellationToken);
                    results.AddRange(items);
                }
                catch (Exception ex)
                {
                    progress?.Report(new ScanProgress
                    {
                        Message = $"扫描失败 {basePath}: {ex.Message}",
                        ProgressPercent = pathProgressEnd
                    });
                }

                currentPathIndex++;
            }

            progress?.Report(new ScanProgress
            {
                Message = $"扫描完成，发现 {results.Count} 个项目",
                ProgressPercent = 100
            });

            return results
                .Where(r => r.Size > 0)
                .OrderByDescending(r => r.Size)
                .ToList();
        }

        private async Task<List<AppDataItem>> ScanDirectoryAsync(string path, IProgress<ScanProgress>? progress,
            int progressStart, int progressEnd, CancellationToken cancellationToken)
        {
            var results = new List<AppDataItem>();

            try
            {
                // 获取所有子目录
                var subDirs = Directory.GetDirectories(path)
                    .Where(SafetyEvaluator.ShouldScanDirectory)
                    .ToArray();

                int totalDirs = subDirs.Length;
                int processedDirs = 0;

                foreach (var dir in subDirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var item = await AnalyzeDirectoryAsync(dir, cancellationToken);
                        if (item.Size > 1024 * 1024) // 只显示 > 1MB
                        {
                            results.Add(item);
                        }

                        // 报告进度
                        processedDirs++;
                        int currentProgress = progressStart + ((progressEnd - progressStart) * processedDirs / Math.Max(1, totalDirs));

                        progress?.Report(new ScanProgress
                        {
                            Message = $"正在分析: {item.Name}",
                            ProgressPercent = currentProgress,
                            CurrentItem = item.Name
                        });
                    }
                    catch (Exception ex)
                    {
                        progress?.Report(new ScanProgress
                        {
                            Message = $"分析失败 {dir}: {ex.Message}",
                            ProgressPercent = progressStart + ((progressEnd - progressStart) * processedDirs / Math.Max(1, totalDirs))
                        });
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限访问的目录
            }

            return results;
        }

        private async Task<AppDataItem> AnalyzeDirectoryAsync(string path, CancellationToken cancellationToken)
        {
            var item = new AppDataItem
            {
                Name = Path.GetFileName(path),
                Path = path,
                IsDirectory = true,
                RiskLevel = SafetyEvaluator.EvaluateRisk(path, true)
            };

            // 递归计算大小并查找子项
            var (size, children) = await CalculateDirectorySizeAsync(path, cancellationToken);
            item.Size = size;
            item.Children = children;

            // 如果子项有Safe的缓存，提升整体安全等级
            if (children.Any(c => c.RiskLevel == RiskLevel.Safe && c.Size > 10 * 1024 * 1024))
            {
                item.RiskLevel = RiskLevel.Safe;
            }

            return item;
        }

        private async Task<(long, List<AppDataItem>)> CalculateDirectorySizeAsync(string path, CancellationToken cancellationToken)
        {
            long size = 0;
            var children = new List<AppDataItem>();

            try
            {
                // 计算文件
                foreach (var file in Directory.GetFiles(path))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Exists)
                        {
                            size += fileInfo.Length;

                            // 如果是缓存文件，单独记录
                            if (IsCacheFile(file))
                            {
                                children.Add(new AppDataItem
                                {
                                    Name = Path.GetFileName(file),
                                    Path = file,
                                    Size = fileInfo.Length,
                                    IsDirectory = false,
                                    RiskLevel = RiskLevel.Safe
                                });
                            }
                        }
                    }
                    catch { }
                }

                // 递归子目录
                foreach (var dir in Directory.GetDirectories(path))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var dirName = Path.GetFileName(dir);
                        var risk = SafetyEvaluator.EvaluateRisk(dir, true);

                        // 如果是缓存目录，快速计算大小
                        if (risk == RiskLevel.Safe || IsCacheFolder(dirName))
                        {
                            var dirSize = await FastCalculateSizeAsync(dir, cancellationToken);
                            size += dirSize;

                            children.Add(new AppDataItem
                            {
                                Name = dirName,
                                Path = dir,
                                Size = dirSize,
                                IsDirectory = true,
                                RiskLevel = risk
                            });
                        }
                        else
                        {
                            // 非缓存目录，递归分析
                            var (subSize, subChildren) = await CalculateDirectorySizeAsync(dir, cancellationToken);
                            size += subSize;
                            children.AddRange(subChildren);
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (UnauthorizedAccessException) { }

            return (size, children.OrderByDescending(c => c.Size).ToList());
        }

        private async Task<long> FastCalculateSizeAsync(string path, CancellationToken cancellationToken)
        {
            long size = 0;

            try
            {
                await Task.Run(() =>
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            size += new FileInfo(file).Length;
                        }
                        catch { }
                    }
                }, cancellationToken);
            }
            catch { }

            return size;
        }

        private bool IsCacheFile(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".tmp" or ".temp" or ".log" or ".cache" or ".bak" or ".old";
        }

        private bool IsCacheFolder(string name)
        {
            var cacheNames = new[] { "cache", "logs", "temp", "tmp", "crash reports" };
            return cacheNames.Any(c => name.Contains(c, StringComparison.OrdinalIgnoreCase));
        }
    }
}
