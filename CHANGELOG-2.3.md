# SACGUI 2.3 Release Notes

## Cracking Improvements
- **Steamless fixed for all games** - Steamless unpacking now works reliably across all game types
- **Nested directory games supported** - Games with steam_api.dll in subdirectories are now properly detected and cracked
- **Steam API DLL validation** - Folders are only recognized as games if they contain steam_api.dll, preventing false positives
- **Root drive protection** - Selecting a root drive (C:\, D:\, etc.) now shows an error instead of scanning the entire drive
- **Empty directory filtering** - Empty game directories are automatically filtered out from crackable/zippable/shareable lists

## Batch Processing Overhaul
- **Pipeline architecture** - Zipping runs sequentially while uploads run in parallel
  - Each game zips one at a time
  - As soon as a zip completes, it immediately starts uploading
  - Up to 3 uploads run simultaneously (capped for stability)
- **Visual upload slots** - Stacked progress bars below the grid showing active uploads
  - Each slot shows: game name, progress bar, size, speed, ETA
  - Slots appear/disappear dynamically as uploads start/finish
  - Individual **Skip button** per upload to skip just that game
  - **Cancel All** button to stop everything
- **Real-time progress tracking**
  - Status column shows upload speed and ETA per game
  - Overall ETA displayed in main window titlebar during batch operations
- **Minimize to indicator** - When batch window is minimized, an icon appears on the main window showing:
  - Current batch progress percentage as a number
  - Click to restore the batch window

## Share Window Updates
- **Multi-select to batch** - Selecting multiple games in share window now sends paths to batch processor
- **DDL conversion opt-out** - New checkbox to skip debrid conversion of completed uploads
- **Info button fixed** - Now correctly shows PyDrive links after debrid conversion completes

## UI Polish
- Clear black tinted glass effect on all windows (like car window tint)
- Anti-aliased rounded button corners throughout
- Reduced window shadows on popups
- Consistent dark theme color scheme across all forms
