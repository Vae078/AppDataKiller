using AppDataCleaner.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace AppDataCleaner.ViewModels
{
    public partial class AppDataItemViewModel : ObservableObject
    {
        private readonly AppDataItem _item;

        public AppDataItemViewModel(AppDataItem item)
        {
            _item = item;
            Children = new ObservableCollection<AppDataItemViewModel>(
                item.Children.Select(c => new AppDataItemViewModel(c)));

            // 默认勾选Safe项目
            _isSelected = item.RiskLevel == RiskLevel.Safe;
        }

        public string Name => _item.Name;
        public string Path => _item.Path;
        public long Size => _item.Size;
        public string FormattedSize => _item.FormattedSize;
        public RiskLevel RiskLevel => _item.RiskLevel;
        public bool IsDirectory => _item.IsDirectory;
        public ObservableCollection<AppDataItemViewModel> Children { get; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isExpanded;

        public long GetSelectedSize()
        {
            if (!IsSelected) return 0;

            long size = Size;
            foreach (var child in Children)
            {
                size += child.GetSelectedSize();
            }
            return size;
        }
    }
}
