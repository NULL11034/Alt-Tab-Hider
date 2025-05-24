<h1 align="center">AltTabHider</h1>
<p align="center">
  <em>Hide any window from Alt + Tab and the Windows taskbar—without flashing or minimising.</em><br/>
  <sub>Built with .NET 8 WinForms · MIT License</sub>
</p>

---

## ✨ Key Features

| | |
| :- | :- |
| **One‑click Hide** | Instantly removes a window from Alt + Tab **and** the taskbar while keeping it usable on‑screen. |
| **Tray Control** | Each hidden window gets its own tray icon with **Show (no Alt‑Tab)**, **Minimise**, and **Restore (full)** actions. |
| **Batch Actions** | Multi‑select rows in the main GUI and hide / show / minimise / restore in bulk. |
| **Auto UAC Elevation** | Restarts itself as administrator if not already elevated—no manual right‑click needed. |
| **Safe Exit** | Closing the app (× or tray → Exit) automatically restores *all* hidden windows, leaving no stragglers. |
| **High‑DPI Ready** | Flow‑layout buttons, larger fonts, and a 32 px row height—no overlap at any scaling factor. |

---

## 📷 Screenshots

![image](https://github.com/user-attachments/assets/de7b7a35-54a8-43ce-a879-4402ba582ab9)
![image](https://github.com/user-attachments/assets/babb502c-bc8c-4e31-b981-c8036765b33f)
![image](https://github.com/user-attachments/assets/4786be14-872b-4434-896a-bebb7406fe88)

---

## 🚀 Quick Start

```console
git clone https://github.com/NULL11034/Alt-Tab-Hider.git
cd alttabhidegui
dotnet build -c Release
bin\Release\net8.0-windows\alttabhidegui.exe
```

> **Heads‑up:** the first launch prompts for administrator rights (UAC).

---

## 🛠️ Building From Source

```console
dotnet restore
dotnet build
dotnet publish -c Release -p:PublishSingleFile=true   # optional
```

---

## 🗂️ Usage

1. Select one or more windows from the list.  
2. Use the top‑row buttons: **Hide**, **Show (no Alt‑Tab)**, **Minimise**, **Restore (full)**.  
3. Each hidden window can also be controlled from its tray icon.  
4. Closing AltTabHider restores *all* hidden windows automatically.

---

## 📄 License

MIT © 2025 NULL11034

---

<details>
<summary><b>点击展开中文说明</b></summary>

## ✨ 功能亮点

| | |
| :- | :- |
| **一键隐藏** | 让窗口瞬间从 Alt + Tab 和任务栏消失，仍可在桌面操作 |
| **托盘控制** | 每个隐藏窗口对应托盘图标：显示、最小化、完全恢复 |
| **批量操作** | 主界面多选后可批量隐藏 / 显示 / 最小化 / 恢复 |
| **自动提权** | 无管理员权限时自动重启请求 UAC |
| **安全退出** | 退出程序或点击 × 时自动恢复所有隐藏窗口 |
| **高 DPI 适配** | 采用 FlowLayout + 大字体，任何缩放比例不挤压 |

---

## 🚀 快速开始

```powershell
git clone https://github.com/NULL11034/Alt-Tab-Hider.git
cd alttabhidegui
dotnet build -c Release
bin\Release\net8.0-windows\alttabhidegui.exe
```

> ⚠️ 首次启动会弹出 UAC 请求管理员权限，否则无法修改其他进程的窗口属性。

---

## 🛠️ 源码构建

```powershell
dotnet restore
dotnet build
dotnet publish -c Release -p:PublishSingleFile=true   # 可选单文件发布
```

---

## 🗂️ 使用方法

1. 在列表中勾选窗口  
2. 点击上方按钮：**Hide**、**Show (no Alt‑Tab)**、**Minimise**、**Restore (full)**  
3. 隐藏后可通过托盘图标执行同样操作  
4. 关闭 AltTabHider（× 或 托盘 Exit）时自动恢复所有隐藏窗口  

</details>
