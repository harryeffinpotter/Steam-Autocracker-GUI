# Steam Auto-Cracker GUI 2.0 - Changelog

## Latest Updates (October 2025)

### 🔧 Critical Fixes
- **Fixed UI Freezing**: Resolved deadlocks that completely froze the application during cracking and DLC generation
- **Removed Broken DLC Generation**: Disabled generate_game_infos.exe due to expired Steam API key
- **Async Operations**: Made all long-running operations properly asynchronous to keep UI responsive

---

## Major Version Release: 2.0
*From version 1.1.0 (November 2023) to 2.0 (October 2025)*

---

## 🎨 UI/UX Overhaul

### Modern Visual Design
- **Acrylic Blur Effects**: Implemented Windows 11-style frosted glass blur with customizable transparency
- **Rounded Corners**: Applied modern rounded window corners on Windows 11
- **Borderless Design**: Sleek frameless window with custom draggable title bar
- **RGB Animated Progress**: Dynamic color-cycling progress indicators throughout the app
- **Refined Color Palette**: Updated to modern dark theme with subtle blue accents

### Window Management
- **Independent Window Behavior**: Main and share windows can be minimized/maximized independently
- **Synchronized Restoration**: Restoring either window automatically restores both
- **Smart Window Stacking**: Share window stays above main window without blocking other applications
- **Pin to Top**: Optional always-on-top mode via pin button

---

## 🚀 Performance & Stability

### Asynchronous Operations
- **Non-Blocking Updates**: All updater checks now run asynchronously - no more UI freezes
- **Background Processing**: Steam game database loads asynchronously on startup
- **Smooth Compression**: 7-Zip and archive operations run in background threads
- **Responsive UI**: All long-running operations moved to async tasks

### Improved Reliability
- **Null-Safe Search**: Fixed crashes when typing during app initialization
- **Filename Sanitization**: Automatic removal of invalid path characters (colons, etc.)
- **Error Handling**: Comprehensive exception handling with user-friendly messages
- **Startup Stability**: Settings load before any UI operations begin

---

## 🔧 Auto-Crack System

### Intelligent Auto-Cracking
- **One-Click Automation**: Toggle auto-crack mode with visual green/red indicator
- **Immediate Execution**: Automatically cracks games upon selection when enabled
- **Persistent Settings**: Auto-crack preference saved between sessions
- **Visual Feedback**: Clear status messages ("Auto-cracking..." vs "Ready to crack")
- **No Interruptions**: Seamless workflow from folder selection to cracked game

---

## 🔍 Enhanced Game Search

### Vastly Improved Fuzzy Matching
- **Multi-Level Search Strategy**: 6-tier search algorithm for maximum accuracy
- **CamelCase Detection**: Automatically handles "DeadSpace" → "Dead Space"
- **Special Character Handling**: Strips symbols, handles apostrophes and ampersands
- **Gaming Term Removal**: Filters out "GOTY", "Deluxe", "Remastered", etc.
- **Partial Word Matching**: Finds games even with incomplete folder names
- **Significant Word Priority**: Matches longest/most relevant words first
- **Auto-Refresh Fallback**: Reloads database and retries if no match found

### Smart Selection
- **Single Result Auto-Select**: Automatically selects when only one game matches
- **Auto-Lock Feature**: One result locks and loads instantly (togglable)
- **Clear Visual Feedback**: Status messages guide user through search process

---

## 📦 Advanced Compression & Sharing

### Zip Directory Feature
- **Compression Settings Modal**: Choose format (ZIP/7Z), level (0-9), and optional RIN password encryption
- **Remember Settings**: Save compression preferences for future use
- **Real-Time Progress**: Accurate percentage tracking from 7-Zip output with RGB animation
- **Cancel Anytime**: Live cancellation button (turns orange) to abort compression
- **Smart Cleanup**: Automatically deletes partial files on cancellation
- **Show Zip Button**: After compression, button becomes "Show Zip" to reveal file in Explorer
- **Persistent Access**: "Show Zip" remains available until next game crack

### Share Window System
- **Dual-Pane Interface**: Modern grid view of all installed Steam games
- **Share Clean or Cracked**: Choose to share original or cracked versions
- **Automatic File Preservation**: Clean files backed up as .bak before cracking
- **Clean File Restoration**: Sharing clean versions automatically restores .bak files
- **Re-Crack Protection**: Smart detection prevents re-cracking already cracked files
- **Compression Options**: Full settings control (format, level, password)
- **Upload Integration**: Direct upload to backend with progress tracking
- **Modern Progress Bar**: Sleek blue gradient progress indicator matching app theme
- **Game Size Display**: Shows actual disk space usage for each game
- **Sortable Columns**: Click headers to sort by name, size, or status
- **Search & Filter**: Quick search box to find games instantly
- **Read-Only Grid**: Prevents accidental edits while browsing

---

## 🎮 Emulator Improvements

### GBE → GBE_FORK Migration
- **Updated Emulator**: Switched from deprecated Goldberg Emulator to actively maintained GBE_FORK
- **Async Updates**: Goldberg/GBE updates download in background without freezing UI
- **LAN Multiplayer Support**: Toggle LAN shortcuts for Goldberg (Host/Join LAN game)
- **Settings Persistence**: LAN multiplayer preference saved between sessions
- **Dual Emulator Support**: Goldberg (GBE_FORK) and ALI213 both fully supported

---

## 🛡️ File Management & Safety

### Intelligent Backup System
- **Automatic Backups**: Original DLLs backed up as .bak before replacement
- **Smart Re-Crack Detection**: Checks if file is already cracked before replacing
- **Clean File Preservation**: .bak files preserved when re-cracking same game
- **Restoration on Share Clean**: Automatically restores .bak to original when sharing clean
- **Recursive Backup Support**: Handles nested directory structures
- **No Crackable Files Alert**: Clear notification when no Steam DLLs found

### Path & Filename Safety
- **Invalid Character Removal**: Automatically strips colons, pipes, and other invalid chars
- **Space Preservation**: Keeps spaces in filenames while removing problematic characters
- **Path Length Handling**: Manages long Windows paths correctly
- **Unicode Support**: Handles international characters in game names

---

## 📊 Progress Tracking & Feedback

### Visual Progress Indicators
- **RGB Color Cycling**: Dynamic rainbow progress animation during operations
- **Percentage Tracking**: Real-time progress from 0-100% for compressions
- **Status Messages**: Clear, color-coded status updates (success=green, error=red, etc.)
- **Title Bar Updates**: App title shows current operation progress
- **Completion Notifications**: Success messages with next-step guidance

### Improved User Guidance
- **Context-Sensitive Messages**: Different messages for manual vs auto-crack modes
- **Operation State Clarity**: Button text changes reflect current state (Zip Dir → Cancel → Show Zip)
- **Error Messages**: Descriptive error text instead of cryptic codes
- **No Results Feedback**: Clear message when search finds no games

---

## 🔄 Window & Session Management

### Session Persistence
- **Remember Window State**: Pin status, auto-crack mode, LAN settings all saved
- **Last Directory**: Remembers last selected game folder
- **Compression Preferences**: Saves format, level, and password choices
- **Goldberg/ALI213 Choice**: DLL selector preference persists

### Multi-Window Handling
- **Single Share Window**: Prevents multiple share windows from opening
- **Focus Management**: Clicking share button brings existing window to front
- **Owner Relationship**: Share window properly linked to main window
- **Proper Cleanup**: Share window resources released on close

---

## 🐛 Bug Fixes

### Critical Fixes
- Fixed crash when typing in search box during startup
- Fixed auto-crack not triggering on first app launch
- Fixed compression failing with games containing colons in names
- Fixed double-click required on auto-crack toggle button
- Fixed share window not staying above main window
- Fixed progress bar getting stuck at 77% during compression
- Fixed Designer file missing for EnhancedShareWindow
- Fixed DataGridView horizontal scrollbar with auto-fill columns

### Minor Fixes
- Fixed title bar being too dark to read
- Fixed DataGridView cells being editable
- Fixed "Zip Dir" not resetting to "Show Zip" on errors
- Fixed cancellation token not properly aborting operations
- Fixed Settings loading after UI initialization causing race conditions

---

## ⚙️ Technical Improvements

### Code Quality
- Removed duplicate code paths and consolidated functions
- Improved async/await patterns throughout
- Better exception handling with try-catch-finally blocks
- Cleaner separation of concerns (UI vs business logic)
- Standardized debug logging for troubleshooting

### Architecture
- Modern .NET 4.8 SDK-style project structure
- Embedded resources properly configured
- Designer files correctly linked in .csproj
- Costura.Fody for dependency embedding
- Clean build output structure

---

## 📝 Notes

### Breaking Changes
- None - fully backward compatible with existing game installations

### Known Issues
- None currently reported

### Credits
- GBE_FORK emulator by community maintainers
- Steamless by atom0s
- 7-Zip compression library

---

**Full Changelog**: v1.1.0...v2.0
