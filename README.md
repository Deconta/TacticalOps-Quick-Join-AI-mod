# 🎮 Tactical Ops Quick Join - AI-Mod

A modern, fast server browser for Tactical Ops with advanced features and an improved user interface.

**The project was completely modified by AI.**

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)
![License](https://img.shields.io/badge/License-MIT-green)

**Light Mode:** 

<img width="879" height="708" alt="image" src="https://github.com/user-attachments/assets/d63bee69-a359-462e-ab2f-442fd952e4e7" />

## ✨ Features

- **🌐 Real-time Server Browser** - Browse all available Tactical Ops servers instantly
- **🗺️ Fast Map Preview** - Optimized image loading with cache (256x144 thumbnails)
- **📊 Ping Display** - Shows latency to each server with color coding
- **👥 Player Details** - Detailed player information and statistics
- **⭐ Favorites System** - Mark your favorite servers for quick access
- **🎨 Dark/Light Theme** - Switchable design for your preference
- **🔄 Auto-Refresh** - Automatic server list updates
- **⌨️ Keyboard Shortcuts** - Quick actions with F5, Enter, and Escape
- **🎯 Version Filter** - Filter servers by TO version (2.2, 3.4, 3.5)

**Dark Mode:** 

<img width="879" height="708" alt="image" src="https://github.com/user-attachments/assets/f12adaac-36c7-430e-a4cc-e399f2d6d325" />

**Map Preview**(Hover your mouse over a map name)**:** 

<img width="875" height="703" alt="image" src="https://github.com/user-attachments/assets/5bb33775-8a07-4abd-a884-f8fab637cf25" />

## 📥 Installation

### Requirements
- Windows 10/11
- .NET 8 Runtime (included in self-contained build)
- Tactical Ops installed

### Download
1. Download the latest `TacticalOpsQuickJoin.exe` from [Releases](../../releases)
2. Install .NET Desktop Runtime 8.0.22 [Microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.22-windows-x64-installer)
3. Run the executable
4. That's it! No installation needed.

## 🎮 Usage

### Joining Servers
1. Select a server from the list
2. Click "Join Server" or double-click the server

### Map Preview
- Hover your mouse over a map name
- A preview window appears after a short delay
- Move away to close the preview

### Favorites
- Click the ☆ icon to add a server to favorites
- Favorite servers appear at the top of the list

## ⌨️ Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **F5** | Refresh server list |
| **Escape** | Minimize application |
| **Enter** | Join selected server |

## 🎨 Themes

Switch between light and dark mode:
- **Menu → Toggle Theme**
- Setting is saved automatically

## ⚙️ Configuration

The application saves settings automatically:
- Theme preference (Light/Dark)
- Window position and size
- "Close on join" setting
- Favorite servers
- Master server list

## 🛠️ Development

### Build from Source
```bash
dotnet build --configuration Release
```

### Create Release Build
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 📊 Map Data

The program loads map preview images from:
1. **Online**: Tactical-Ops.eu/map-screenshots/ (automatic)
2. **Local**: `maps.json` in program folder (fallback)

Map data provided by [Tactical-Ops.eu](https://mirror.tactical-ops.eu/map-screenshots/)

## 🐛 Known Issues

- Map preview requires internet connection
- Some older TO versions may not be recognized
- Firewall can block server ping

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📝 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- Tactical Ops Community
- Map data provided by [Tactical-Ops.eu](https://mirror.tactical-ops.eu/map-screenshots/)
- Original project inspiration [Original-Author](https://github.com/jilderthoekstra/Tactical-Ops-Quick-Join)

## 📞 Support

For issues or questions:
- Open an [Issue](../../issues)
- Contact the TO Community

---
