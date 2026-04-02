# 🗑️ AppDataKiller

> **C Drive running out of space again?** Don't panic, it's probably just cache files hoarding your disk.

AppDataKiller is a Windows cache cleanup tool that hunts down junk files hiding in the dark corners of your AppData folder and **safely** sends them to the Recycle Bin, giving your C drive some breathing room.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-purple)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-brightgreen)

---

## 🤔 What Does It Do?

Imagine your C drive is a closet:
- **🟢 Safe** = Old socks and t-shirts (toss without worry)
- **🟡 Caution** = Coats you might still need (think before tossing)
- **🔴 Danger** = Passports and property deeds (hands off!)

AppDataKiller is the organizer that sorts everything for you, and puts the "trash" in the Recycle Bin first — **you can always get it back**.

### ✨ Features

| Feature | Description |
|---------|-------------|
| 🔍 One-Click Scan | Automatically searches Local, Roaming, LocalLow for cache hideouts |
| 🚦 Traffic Light System | Safe/Caution/Danger three-level warning, easy to understand |
| ♻️ Recycle Bin Safety Net | No permanent deletion, recover anytime |
| 📦 Single Executable | No installation, download and run |
| 🚫 Zero Dependencies | Bundled .NET runtime, works on fresh Windows |

---

## 📥 Download & Usage

### Lazy Version (Recommended)

1. Go to [Releases](../../releases) and download `AppDataCleaner.exe`
2. Double-click to run
3. Click "🔍 Start Scan"
4. Check items to delete (stick to 🟢 recommended)
5. Click "🗑 Clean Selected"
6. Witness the C drive miracle ✨

### Geek Version (Build Yourself)

```bash
git clone https://github.com/Vae078/AppDataKiller.git
cd AppDataCleaner
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
./publish/AppDataCleaner.exe
```

---

## 🎯 Before & After

```
Before: C Drive 2.3 GB free 😱
After:  C Drive 23.5 GB free 😎

Space freed: 21.2 GB
Main sources:
- Chrome cache (8.5 GB)
- VS Code logs (3.2 GB)
- Various Temp files (5.1 GB)
- Other random junk (4.4 GB)
```

---

## ⚠️ Important Notes

1. **🔴 Danger items are locked by default**, can't delete even if you want to (it's for your own good)
2. Glance through the list before cleaning, **don't delete your game saves**
3. Everything goes to Recycle Bin, **you can still change your mind**
4. First time user? Try scanning without cleaning to see what it finds

---

## 🛠️ Tech Stack

- [.NET 8](https://dotnet.microsoft.com/) - Microsoft's pride and joy
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) - Classic desktop framework
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM magic
- [CsWin32](https://github.com/microsoft/CsWin32) - Win32 API bindings

---

## 📂 Project Structure

```
AppDataCleaner/
├── Models/           # Data models (AppDataItem, RiskLevel)
├── ViewModels/       # View models (MainViewModel)
├── Services/         # Core business (scan, evaluate, clean)
├── Converters/       # Value converters (colors, booleans)
└── MainWindow.xaml   # Main UI
```

---

## 📝 License

MIT License - Use it, just don't be evil.

---

## ☕ Finally

If this tool saved your C drive, **give it a Star ⭐**!

Issues? Open an [Issue](../../issues) or fix it yourself (PRs welcome)~

**⚠️ Disclaimer**: The developer is not responsible for any data loss. Backup important files, don't blame me for butterfingers 😅
