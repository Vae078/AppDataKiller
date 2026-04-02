using AppDataCleaner.Models;
using AppDataCleaner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AppDataCleaner.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IScannerService _scanner;
        private readonly ICleanerService _cleaner;
        private CancellationTokenSource? _scanCts;
        private CancellationTokenSource? _cleanCts;

        public MainViewModel()
        {
            _scanner = new ScannerService();
            _cleaner = new CleanerService();
            Items = new ObservableCollection<AppDataItemViewModel>();
            StatusText = "就绪，点击\"开始扫描\"查找可清理文件";
        }

        [ObservableProperty]
        private ObservableCollection<AppDataItemViewModel> _items;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private bool _isCleaning;

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private string _totalSizeText = "0 MB";

        [ObservableProperty]
        private string _selectedSizeText = "0 MB";

        public bool CanScan => !IsScanning && !IsCleaning;
        public bool CanClean => !IsScanning && !IsCleaning && Items.Any(i => i.IsSelected);

        [RelayCommand]
        private async Task ScanAsync()
        {
            IsScanning = true;
            StatusText = "正在扫描...";
            Items.Clear();
            ProgressValue = 0;

            _scanCts = new CancellationTokenSource();

            try
            {
                var progress = new Progress<ScanProgress>(p =>
                {
                    StatusText = p.Message;
                    ProgressValue = p.ProgressPercent;
                });
                var results = await _scanner.ScanAsync(progress, _scanCts.Token);

                foreach (var result in results)
                {
                    var vm = new AppDataItemViewModel(result);
                    vm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AppDataItemViewModel.IsSelected))
                        {
                            OnPropertyChanged(nameof(CanClean));
                            UpdateSelectedSize();
                        }
                    };
                    Items.Add(vm);
                }

                UpdateTotalSize();
                StatusText = $"扫描完成，发现 {results.Count} 个项目，共 {TotalSizeText}";
            }
            catch (OperationCanceledException)
            {
                StatusText = "扫描已取消";
            }
            catch (Exception ex)
            {
                StatusText = $"扫描出错: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }

        [RelayCommand]
        private void CancelScan()
        {
            _scanCts?.Cancel();
            StatusText = "正在取消...";
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var item in Items)
            {
                if (item.RiskLevel != RiskLevel.Danger)
                {
                    item.IsSelected = true;
                }
            }
            UpdateSelectedSize();
        }

        [RelayCommand]
        private void DeselectAll()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
            UpdateSelectedSize();
        }

        [RelayCommand]
        private async Task CleanAsync()
        {
            var selectedItems = GetSelectedItems();
            if (!selectedItems.Any())
            {
                MessageBox.Show("请先选择要清理的项目", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var totalSize = selectedItems.Sum(i => i.Size);
            var confirm = MessageBox.Show(
                $"确定要将以下 {selectedItems.Count} 个项目移入回收站吗？\n" +
                $"总计: {FormatBytes(totalSize)}\n\n" +
                $"注意：删除的文件可以在回收站中恢复。",
                "确认清理",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            IsCleaning = true;
            StatusText = "正在清理...";
            _cleanCts = new CancellationTokenSource();

            try
            {
                var progress = new Progress<CleanProgress>(p =>
                {
                    StatusText = $"正在清理: {p.CurrentItem} ({p.ProcessedCount}/{p.TotalCount})";
                    ProgressValue = p.TotalCount > 0 ? (int)((double)p.ProcessedCount / p.TotalCount * 100) : 0;
                });

                var result = await _cleaner.CleanAsync(selectedItems, progress, _cleanCts.Token);

                // 移除已清理的项
                foreach (var item in selectedItems)
                {
                    var vm = Items.FirstOrDefault(i => i.Path == item.Path);
                    if (vm != null) Items.Remove(vm);
                }

                StatusText = $"清理完成！释放了 {FormatBytes(result.TotalFreed)}";
                MessageBox.Show(
                    $"成功清理 {result.ItemsProcessed} 个项目\n" +
                    $"释放空间: {FormatBytes(result.TotalFreed)}\n" +
                    $"失败: {result.ItemsFailed} 个",
                    "清理完成",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                StatusText = "清理已取消";
            }
            catch (Exception ex)
            {
                StatusText = $"清理出错: {ex.Message}";
            }
            finally
            {
                IsCleaning = false;
                UpdateTotalSize();
            }
        }

        [RelayCommand]
        private void CancelClean()
        {
            _cleanCts?.Cancel();
            StatusText = "正在取消...";
        }

        private List<AppDataItem> GetSelectedItems()
        {
            var result = new List<AppDataItem>();
            foreach (var item in Items)
            {
                if (item.IsSelected)
                {
                    result.Add(new AppDataItem
                    {
                        Name = item.Name,
                        Path = item.Path,
                        Size = item.Size,
                        IsDirectory = item.IsDirectory,
                        RiskLevel = item.RiskLevel
                    });
                }
            }
            return result;
        }

        private void UpdateTotalSize()
        {
            var total = Items.Sum(i => i.Size);
            TotalSizeText = FormatBytes(total);
        }

        private void UpdateSelectedSize()
        {
            var selected = Items.Where(i => i.IsSelected).Sum(i => i.Size);
            SelectedSizeText = FormatBytes(selected);
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0 B";
            int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
            return $"{bytes / Math.Pow(1024, i):F2} {sizes[i]}";
        }

        partial void OnItemsChanged(ObservableCollection<AppDataItemViewModel> value)
        {
            UpdateTotalSize();
        }
    }
}
