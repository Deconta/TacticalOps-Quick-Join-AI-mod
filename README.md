# 🎮 Tactical Ops Quick Join - AI Mod

A fast Windows server browser for Tactical Ops with favorites, map previews, player details, and a compact dark/light UI.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features

- 🌐 Real-time Tactical Ops server browser
- 📊 Ping display with color coding
- 👥 Player details with fake/min-player filler filtering
- ⭐ Favorites system
- 🎨 Dark/light theme
- 🔄 Auto-refresh
- 🗺️ Map preview on hover with cache and mirror fallback URLs
- 🎯 Improved map matching for common server display names
- ⚡ Single-server refresh including ping, status, player count, and player list
- 📦 Standalone Windows release build

<img width="977" height="730" alt="image" src="https://github.com/user-attachments/assets/264225ac-932b-4ff0-b0a6-49d19440f14d" />
<img width="976" height="727" alt="image" src="https://github.com/user-attachments/assets/a0e0b6b4-9fe6-4914-ae08-cf6ab8673a62" />
<img width="976" height="726" alt="image" src="https://github.com/user-attachments/assets/7132411b-d42f-41df-bae2-889df723e327" />


## 📥 Installation

### Standalone build

Download `TacticalOpsQuickJoin.exe` from the release package and run it.

The standalone build is self-contained, so the .NET Desktop Runtime does not need to be installed separately.

### Requirements

- Windows 10/11 x64
- Tactical Ops installed locally for joining servers
- Internet access for server list and map previews

## 🎮 Usage

### Joining Servers

1. Select a server from the list.
2. Click `Join Server`, press `Enter`, or double-click the server.

### Map Preview

- Hover over a map name.
- A preview window appears after a short delay.
- Move the mouse away to close the preview.

### Favorites

- Click the star column to toggle a server as favorite.
- Favorite servers are sorted above regular servers.

### Refresh

- Press `F5` or use `Menu > Refresh Servers` to refresh the full server list.
- Right-click a server and choose `Diesen Server aktualisieren` to refresh only that server, including ping and player details.

## ⌨️ Keyboard Shortcuts

| Key | Action |
| --- | --- |
| `F5` | Refresh server list |
| `Escape` | Minimize application |
| `Enter` | Join selected server |

## ⚙️ Configuration

Settings are saved automatically:

- Theme preference
- Close-on-join setting
- Favorite servers
- Master server list
- Tactical Ops executable paths

## 🛠️ Development

### Build

```powershell
dotnet build TacticalOpsQuickJoin-Mod.sln
```

### Standalone Release EXE

```powershell
dotnet publish TacticalOpsQuickJoin.csproj -c Release -r win-x64 --self-contained true -o .\publish\win-x64-single-exe
```

The standalone executable is written to:

```text
publish\win-x64-single-exe\TacticalOpsQuickJoin.exe
```

## 🗺️ Map Data

Map metadata is loaded from the online `custom_maps.json` source and cached under:

```text
%AppData%\TacticalOpsQuickJoin\maps.json
```

If a map is missing from the JSON cache, the app falls back to direct Tactical-Ops.eu mirror screenshot URLs.

Map previews are provided by:

https://mirror.tactical-ops.eu/map-screenshots/

## 🐛 Known Notes

- Map preview requires internet access.
- Some servers report fake/min-player filler entries. Known fake player entries are filtered from display and sorting.
- Firewall rules can block UDP server queries or Tactical Ops joining.

## 📝 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- Tactical Ops community
- Tactical-Ops.eu mirror and map screenshots
- Original project inspiration: https://github.com/jilderthoekstra/Tactical-Ops-Quick-Join
