# 🗑️ AppDataKiller

> **C盘又双叒叕红了？** 别慌，你可能只是被缓存偷袭了。

AppDataKiller 是一款专治 Windows 缓存囤积症的清理工具。它帮你找出躲在 AppData 犄角旮旯里的缓存垃圾，**安全地**把它们送进回收站，让 C 盘喘口气。

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-purple)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-brightgreen)

---

## 🤔 这玩意儿能干嘛？

想象一下你的 C 盘是个衣柜：
- **🟢 Safe** = 旧袜子旧T恤（放心扔）
- **🟡 Caution** = 可能还有点用的外套（想好了再扔）
- **🔴 Danger** = 户口本房产证（别碰！）

AppDataKiller 就是那个帮你分类整理的管家，还会把"垃圾"先放进回收站——**删错了还能捡回来**。

### ✨ 特点一览

| 功能 | 说明 |
|------|------|
| 🔍 一键扫描 | 自动翻遍 Local、Roaming、LocalLow，找出缓存老巢 |
| 🚦 红绿灯提示 | Safe/Caution/Danger 三级预警，小白也能看懂 |
| ♻️ 回收站兜底 | 不直接删除，误删随时恢复 |
| 📦 解压即用 | 无需安装，解压文件夹后双击运行 |
| 🚫 零依赖 | 自带 .NET 运行时，裸机也能跑 |

---

## 📥 下载 & 使用

### 懒人版（推荐）

1. 去 [Releases](../../releases) 下载 `AppDataCleaner.zip`
2. **解压到文件夹**（右键 → 解压到 AppDataCleaner\）
3. 打开解压后的文件夹，双击 `AppDataCleaner.exe`
4. 点击"🔍 开始扫描"
5. 勾选想删的项目（建议只选 🟢）
6. 点击"🗑 清理选中"
7. 见证 C 盘瘦身奇迹 ✨

> ⚠️ **不要直接双击 zip 里的 exe！** 必须先解压整个文件夹，否则程序无法正常运行。

### 极客版（自己编译）

```bash
git clone https://github.com/Vae078/AppDataKiller.git
cd AppDataCleaner
# 构建多文件版本（推荐，更稳定）
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
# 或构建单文件版本
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish_singlefile
```

---

## 🎯 清理前后对比

```
清理前：C盘剩余 2.3 GB 😱
清理后：C盘剩余 23.5 GB 😎

释放空间：21.2 GB
主要来源：
- Chrome 缓存 (8.5 GB)
- VS Code 日志 (3.2 GB) 
- 各种 Temp 文件 (5.1 GB)
- 其他乱七八糟 (4.4 GB)
```

---

## ⚠️ 重要提示

1. **🔴 Danger 项目默认锁死**，想删也删不了（为你好）
2. 清理前看一眼列表，**别把自己存档清了**
3. 所有文件进回收站，**反悔还来得及**
4. 首次使用建议先扫一遍、**不清理**，熟悉一下都有啥

---

## 🛠️ 技术栈

- [.NET 8](https://dotnet.microsoft.com/) - 微软亲儿子
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) - 经典桌面框架
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM 神器
- [CsWin32](https://github.com/microsoft/CsWin32) - 调用 Win32 API

---

## 📂 项目结构

```
AppDataCleaner/
├── Models/           # 数据模型（AppDataItem、RiskLevel）
├── ViewModels/       # 视图模型（MainViewModel）
├── Services/         # 核心业务（扫描、评估、清理）
├── Converters/       # 值转换器（颜色、布尔）
└── MainWindow.xaml   # 主界面
```

---

## 📝 License

MIT License - 拿去用，别搞破坏就行。

---

## ☕ 最后

如果这工具帮你救了 C 盘，**给个 Star ⭐** 呗！

有问题？开 [Issue](../../issues) 或者自己改（PR 欢迎）～

**⚠️ 免责声明**：使用本工具造成的数据丢失，开发者概不负责。重要文件请自行备份，手滑别找我 😅
