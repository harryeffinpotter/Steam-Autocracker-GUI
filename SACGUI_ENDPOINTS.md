# SACGUI API Endpoints Documentation

## Base Path: `/sacgui`

All SACGUI endpoints are prefixed with `/sacgui` (e.g., `https://pydrive.harryeffingpotter.com/sacgui/upload`)

## Endpoints

### 1. Upload Game Stream
**POST** `/sacgui/upload`

Uploads game archives with streaming progress tracking.

**Parameters (Form Data):**
- `file`: UploadFile - The game archive (.zip or .7z)
- `hwid`: string - 64-character hex Hardware ID
- `version`: string - SACGUI version
- `game_name`: string - Name of the game
- `client_ip`: string - Client's IP address

**Validations:**
- Filename must start with `[SACGUI]`
- File must be .zip or .7z archive
- HWID must be 64-char hex string
- Filename must match format: `[SACGUI] {game_name}{.zip|.7z}`

**Rate Limits:**
- 1 upload per minute per IP
- 3 uploads per hour per IP
- 10 uploads per day per IP
- 20 uploads per day per HWID

**Response:** Server-Sent Events stream with:
- Progress updates (bytes, percent, speed, ETA)
- Complete message with share URL
- Error messages if validation/processing fails

---

### 2. Download File
**GET** `/sacgui/download/{file_id}`

Downloads a previously uploaded file.

**Path Parameters:**
- `file_id`: string - Unique file identifier

**Response:**
- Binary file stream with original filename
- 404 if file not found or expired
- 410 if file has expired

**Notes:**
- Increments download counter
- Checks expiration before serving

---

### 3. List Files
**GET** `/sacgui/list`

Lists uploaded files with optional filtering.

**Query Parameters:**
- `hwid`: string (optional) - Filter by HWID
- `game_name`: string (optional) - Filter by game name (partial match)
- `limit`: integer (optional, default=50, max=100) - Maximum results

**Response:**
```json
{
  "total_count": 10,
  "files": [
    {
      "file_id": "abc123",
      "game_name": "Game Name",
      "original_filename": "[SACGUI] Game Name.zip",
      "file_size_mb": 1024.5,
      "upload_timestamp": "2025-01-20T10:30:00",
      "expires_at": "2025-02-20T10:30:00",
      "download_count": 5,
      "download_url": "https://pydrive.harryeffingpotter.com/sacgui/download/abc123"
    }
  ],
  "storage_keep_days": 30
}
```

---

### 4. Statistics
**GET** `/sacgui/stats`

Returns upload statistics.

**Response:**
```json
{
  "total_files": 100,
  "total_size_gb": 250.5,
  "unique_hwids": 25,
  "unique_games": 15,
  "total_downloads": 500,
  "storage_keep_days": 30,
  "top_games": [
    {"name": "Game 1", "count": 20},
    {"name": "Game 2", "count": 15}
  ]
}
```

---

### 5. Health Check
**GET** `/sacgui/health`

Checks service health status.

**Response:**
```json
{
  "status": "healthy",
  "storage_available": true,
  "keep_length": "30 days",
  "upload_dir": "/tmp/sacgui_temp"
}
```

---

### 6. Delete File
**DELETE** `/sacgui/delete/{file_id}`

Deletes an uploaded file (only by original uploader).

**Path Parameters:**
- `file_id`: string - Unique file identifier

**Query Parameters:**
- `hwid`: string (required) - HWID for authentication

**Response:**
```json
{
  "success": true,
  "message": "File deleted successfully"
}
```

**Errors:**
- 404 if file not found
- 403 if HWID doesn't match uploader

---

## Configuration

The service uses configuration from `/etc/sacugi/config.txt` with:
- Line 1: Zipline API key
- Line 2: Retention days (default: 30)
- Line 3: Max file size in GB (default: 5)
- Line 4: Allowed versions (comma-separated, default: SACUGI-2.0,SACUGI-1.9)

## Storage

Files are stored in:
- Primary: Zipline service at `https://share.harryeffingpotter.com/`
- Fallback: Local storage in `/tmp/sacgui_temp/`

Files automatically expire and are deleted after the configured retention period (default 30 days).

## Security Features

- HWID validation and tracking
- IP-based rate limiting
- Archive integrity validation
- Suspicious pattern detection (VPN, multiple IPs per HWID, etc.)
- Automatic expiration and cleanup of old files
