<div align="center">

# 🔬 OpenResearch Desktop for Windows

**Enhanced Native Windows Distribution & Desktop Application for OpenResearch**

[![Platform](https://img.shields.io/badge/platform-Windows%20x86__64-blue?logo=windows)](https://github.com/Mohammad-Zahed-Hossen/OpenResearch)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Release](https://img.shields.io/badge/release-v0.2.6--enhanced-purple)](https://github.com/Mohammad-Zahed-Hossen/OpenResearch)

</div>

---

## 🌟 Overview

**OpenResearch Desktop for Windows** is an enhanced, self-contained distribution of [OpenResearch](https://github.com/alphaXiv/OpenResearch) tailored for Windows environments. It bundles a native C# system-tray launcher, standalone desktop application window (PWA mode), API-first pristine markdown export capabilities, and deliverable download tooling.

---

## ✨ Key Features & Enhancements

### 1. 🖥️ Native Windows Desktop Launcher (`OpenResearchLauncher.exe`)
- **Terminal-Free Execution:** Launches the backend `orx.exe` daemon silently in the background without keeping an unsightly terminal window open.
- **Dedicated Desktop App Window:** Automatically opens OpenResearch in standalone application mode (`--app=http://127.0.0.1:4791`) using the best available browser engine (**Comet**, **Brave**, **Edge**, or **Chrome**).
- **System Tray Management:** Minimizes unobtrusively to the Windows system tray with right-click menu options:
  - *Open OpenResearch*
  - *Open in Default Browser*
  - *Exit OpenResearch* (gracefully stops `orx.exe` and background worker processes)
- **Single-Instance Enforcement:** Prevents duplicate processes and port conflicts.

### 2. 📝 Pristine Markdown Export & Copy
- **API-First Markdown Extraction:** Unlike default DOM scrapers that smash tables onto single lines and strip hyperlinks, the exporter directly queries `/api/chat/sessions/{id}/messages` to retrieve the original, uncorrupted markdown.
- **Perfect Table & Link Preservation:** Preserves GitHub-Flavored Markdown (GFM) pipe tables (`|---|---|`), nested lists, and full paper links (such as `<https://arxiv.org/...>`).
- **Offline GFM Fallback:** Fallback parser reconstructs HTML tables and links with GFM formatting if offline.

### 3. 💬 Minimalist Conversation Toolbar
- **Bottom Placement:** Sits neatly at the bottom of each assistant message following modern chat design patterns.
- **Icon-Only Interface:** Compact icons (`[ 📋 ]` Copy Response, `[ 💾 ]` Export `.md`) with hover tooltips and interactive green checkmark confirmations (`✓`).

### 4. 📦 Deliverable File Downloader
- **Direct-to-Disk Download:** Places a dedicated download icon beside deliverable file chips (e.g. `fl-vlm-reopen-audit.md`) in the chat conversation.
- **Event Isolation:** Placed as a sibling element to the chip, preventing accidental activation of the right-hand preview pane.
- **BiDi Unicode Sanitization:** Strips hidden bidirectional isolate characters (`\u2066` and `\u2069`) to ensure reliable API file resolution.
- **Project Tree Auto-Resolution:** Automatically queries `/api/projects/{id}/files` to locate and download files anywhere in the workspace hierarchy.
- **Chromium Stream Safety:** Employs delayed object URL revocation to prevent Chromium download cancellation.

---

## 📁 Repository Structure

```
├── OpenResearchLauncher.cs       # Native C# launcher source code
├── OpenResearchLauncher.exe      # Compiled Windows desktop launcher
├── build_launcher.bat            # One-click C# compiler script (using csc.exe)
├── Stop-OpenResearch.bat         # Helper script to cleanly terminate processes
├── create_shortcuts.ps1          # PowerShell utility to create Desktop & Start Menu shortcuts
├── orx.exe                       # Enhanced OpenResearch server & CLI engine (Windows x86_64)
├── app.ico / app.png             # Application branding and tray icons
├── extension/                    # Chrome / Chromium unpacked browser extension
│   ├── manifest.json             # Extension manifest (MV3)
│   └── content.js                # Content script with UI enhancements
├── openresearch-enhancements.user.js # Tampermonkey / Violentmonkey userscript
├── fl-vlm-reopen-audit.md        # Sample generated research audit deliverable
├── phase-1---landscape-map...md  # Sample generated research landscape scan
├── .gitignore                    # Git ignore file
└── LICENSE                       # MIT License
```

---

## 🚀 Getting Started

### Prerequisites
- **Windows 10 / 11 (64-bit)**
- **Git for Windows** (recommended for full research agent tooling)
- Modern Chromium-based browser (Brave, Edge, Chrome, or Comet)

### Running OpenResearch
1. Clone the repository:
   ```cmd
   git clone https://github.com/Mohammad-Zahed-Hossen/OpenResearch.git
   cd OpenResearch
   ```
2. Double-click **`OpenResearchLauncher.exe`** (or create desktop shortcuts with `create_shortcuts.ps1`).
3. The OpenResearch desktop window will launch immediately on `http://127.0.0.1:4791`.

### Stopping OpenResearch
- Right-click the system tray icon and select **Exit OpenResearch**, or
- Double-click **`Stop-OpenResearch.bat`**.

### Recompiling the Launcher
If you modify `OpenResearchLauncher.cs`, run:
```cmd
build_launcher.bat
```
*(Uses the standard built-in .NET Framework C# compiler `csc.exe` — no external SDK required).*

---

## 🛠️ Browser Extension & Userscript

If you access OpenResearch from a regular browser tab instead of the desktop web app:
- **Browser Extension:** Go to `chrome://extensions/`, enable **Developer mode**, click **Load unpacked**, and select the `extension/` folder.
- **Userscript:** Install `openresearch-enhancements.user.js` via Violentmonkey or Tampermonkey.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
OpenResearch is an open-source project created by [alphaXiv](https://github.com/alphaXiv/OpenResearch).
