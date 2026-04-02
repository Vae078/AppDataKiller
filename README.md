# AppData 清理器

一个简洁高效的 Windows 桌面应用，帮助用户清理 AppData 目录下的应用程序缓存，释放 C 盘空间。

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows%20Desktop-purple)

## 功能特性

- **智能扫描** - 自动扫描 Local、LocalLow、Roaming 目录，识别可清理的缓存文件
- **风险分级** - 三种安全等级提示，避免误删重要数据
  - 🟢 Safe - 安全清理，通常是临时文件和缓存
  - 🟡 Caution - 谨慎处理，可能包含部分用户数据
  - 🔴 Danger - 危险，不建议清理（如浏览器数据、游戏存档）
- **安全清理** - 文件移入回收站而非永久删除，误删可恢复
- **清晰界面** - 直观展示文件大小、路径和安全等级

## 系统要求

- Windows 10 / Windows 11
- 无需安装 .NET 运行时（自包含版本）

## 下载使用

### 方式一：直接下载可执行文件

从 [Releases](../../releases) 页面下载最新版本的 `AppDataCleaner.exe`，双击运行即可。

### 方式二：从源码编译

```bash
# 克隆仓库
git clone <repository-url>
cd AppDataCleaner

# 编译 Release 版本
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

# 运行
./publish/AppDataCleaner.exe
```

## 使用说明

1. **开始扫描** - 点击"🔍 开始扫描"按钮，程序会自动分析 AppData 目录
2. **选择项目** - 勾选需要清理的项目（建议只选择 🟢 Safe 等级）
3. **执行清理** - 点击"🗑 清理选中"，确认后将文件移入回收站
4. **恢复文件** - 如有误删，可从回收站还原

## 安全提示

- 🔴 **Danger** 等级的项目已禁用勾选，通常包含重要数据
- 清理前请确认选择的项目，避免误删
- 所有删除操作都会移入回收站，不会永久删除

## 技术栈

- **.NET 8** - 跨平台开发框架
- **WPF** - Windows 桌面 UI 框架
- **MVVM** - 使用 CommunityToolkit.Mvvm 实现数据绑定
- **CsWin32** - Windows API 互操作

## 项目结构

```
AppDataCleaner/
├── Models/              # 数据模型
│   ├── AppDataItem.cs   # 应用数据项
│   └── RiskLevel.cs     # 风险等级枚举
├── ViewModels/          # 视图模型
│   ├── MainViewModel.cs # 主窗口视图模型
│   └── AppDataItemViewModel.cs
├── Services/            # 业务服务
│   ├── ScannerService.cs      # 扫描服务
│   ├── SafetyEvaluator.cs     # 安全评估
│   └── CleanerService.cs      # 清理服务
├── Converters/          # 值转换器
└── MainWindow.xaml      # 主界面
```

## 免责声明

本工具仅用于清理应用程序缓存文件。使用本工具造成的任何数据丢失，开发者不承担责任。请在清理前确认所选项，重要数据请提前备份。
