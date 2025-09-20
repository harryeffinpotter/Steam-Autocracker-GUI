# Backend Requirements for Steam Auto-Cracker GUI Request System

## Overview
We need a comprehensive backend API to support our community-driven request and sharing system. This backend will manage user tracking via HWID, handle game requests, track uploads, and maintain honor scores.

## Database Schema Requirements

### Users Table
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    hwid VARCHAR(255) UNIQUE NOT NULL,
    hwid_hash VARCHAR(512) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    last_seen TIMESTAMP DEFAULT NOW(),
    machine_name VARCHAR(255),
    app_version VARCHAR(50),
    honor_score INTEGER DEFAULT 0,
    total_shares INTEGER DEFAULT 0,
    total_requests INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_users_hwid ON users(hwid);
CREATE INDEX idx_users_honor ON users(honor_score DESC);
```

### HWID History Table (for tracking hardware changes)
```sql
CREATE TABLE hwid_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id),
    old_hwid VARCHAR(255),
    new_hwid VARCHAR(255),
    change_reason VARCHAR(100),
    changed_at TIMESTAMP DEFAULT NOW(),
    ip_address INET,
    machine_name VARCHAR(255)
);
```

### Game Requests Table
```sql
CREATE TABLE game_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    app_id VARCHAR(20) NOT NULL,
    game_name VARCHAR(500) NOT NULL,
    request_type VARCHAR(20) NOT NULL, -- 'Clean', 'Cracked', 'Both'
    requester_user_id UUID REFERENCES users(id),
    requester_hwid VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    is_fulfilled BOOLEAN DEFAULT FALSE,
    fulfilled_at TIMESTAMP NULL,
    fulfilled_by_user_id UUID REFERENCES users(id) NULL,

    -- Constraint: One request per user per game
    UNIQUE(requester_user_id, app_id)
);

CREATE INDEX idx_requests_app_id ON game_requests(app_id);
CREATE INDEX idx_requests_active ON game_requests(is_fulfilled) WHERE is_fulfilled = FALSE;
CREATE INDEX idx_requests_user ON game_requests(requester_user_id);
```

### Game Uploads Table
```sql
CREATE TABLE game_uploads (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    app_id VARCHAR(20) NOT NULL,
    game_name VARCHAR(500) NOT NULL,
    upload_type VARCHAR(20) NOT NULL, -- 'clean', 'cracked'
    uploader_user_id UUID REFERENCES users(id),
    uploader_hwid VARCHAR(255) NOT NULL,
    upload_url TEXT NOT NULL,
    file_size BIGINT,
    upload_method VARCHAR(50), -- 'pydrive', 'mega', etc.
    created_at TIMESTAMP DEFAULT NOW(),
    is_verified BOOLEAN DEFAULT FALSE,
    download_count INTEGER DEFAULT 0,
    honor_points_awarded INTEGER DEFAULT 1
);

CREATE INDEX idx_uploads_app_id ON game_uploads(app_id);
CREATE INDEX idx_uploads_user ON game_uploads(uploader_user_id);
CREATE INDEX idx_uploads_date ON game_uploads(created_at DESC);
```

### Uncrackable Games Table
```sql
CREATE TABLE uncrackable_games (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    app_id VARCHAR(20) UNIQUE NOT NULL,
    game_name VARCHAR(500) NOT NULL,
    marked_by_admin_id UUID REFERENCES users(id),
    marked_at TIMESTAMP DEFAULT NOW(),
    reason VARCHAR(500), -- 'denuvo_v12', 'always_online', 'subscription_only', etc.
    evidence_url TEXT, -- Link to forum post or proof
    community_votes_agree INTEGER DEFAULT 0,
    community_votes_disagree INTEGER DEFAULT 0,
    is_permanent BOOLEAN DEFAULT FALSE -- Some might become crackable later
);

CREATE INDEX idx_uncrackable_app_id ON uncrackable_games(app_id);
```

### Blacklisted Games Table (for games that shouldn't be requested at all)
```sql
CREATE TABLE blacklisted_games (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    app_id VARCHAR(20) UNIQUE NOT NULL,
    game_name VARCHAR(500) NOT NULL,
    blacklisted_by_admin_id UUID REFERENCES users(id),
    blacklisted_at TIMESTAMP DEFAULT NOW(),
    reason VARCHAR(500), -- 'spam_requests', 'inappropriate', 'legal_issues'
    is_permanent BOOLEAN DEFAULT TRUE
);
```

### Request Fulfillments Table (links requests to uploads)
```sql
CREATE TABLE request_fulfillments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    request_id UUID REFERENCES game_requests(id),
    upload_id UUID REFERENCES game_uploads(id),
    fulfilled_at TIMESTAMP DEFAULT NOW(),
    honor_points_awarded INTEGER DEFAULT 1
);
```

## API Endpoints

### Base URL: `https://pydrive.harryeffingpotter.com/sacgui/api`

### User Management

#### POST `/register`
Register a new user with HWID
```json
{
    "hwid": "ABC123DEF456...",
    "timestamp": "2025-01-20T10:30:00Z",
    "version": "1.3.0",
    "machine": "GAMING-PC"
}
```
Response:
```json
{
    "success": true,
    "userId": "uuid-here",
    "message": "User registered successfully"
}
```

#### POST `/updatehwid`
Update HWID when hardware changes
```json
{
    "oldHwid": "OLD123...",
    "newHwid": "NEW456...",
    "userId": "uuid-here",
    "timestamp": "2025-01-20T10:30:00Z",
    "reason": "hardware_change"
}
```
Response:
```json
{
    "success": true,
    "honorScore": 25,
    "totalShares": 10,
    "totalRequests": 5,
    "message": "HWID updated successfully"
}
```

#### POST `/verify`
Verify existing user and get updated stats
```json
{
    "hwid": "ABC123...",
    "userId": "uuid-here"
}
```
Response:
```json
{
    "success": true,
    "honorScore": 25,
    "totalShares": 10,
    "totalRequests": 5,
    "lastSeen": "2025-01-20T10:30:00Z"
}
```

### Request Management

#### POST `/requests`
Submit a new game request (ONE PER USER PER GAME)
```json
{
    "appId": "271590",
    "gameName": "Grand Theft Auto V",
    "requestType": "Both", // Clean, Cracked, or Both
    "userId": "uuid-here",
    "hwid": "ABC123...",
    "timestamp": "2025-01-20T10:30:00Z"
}
```
Response:
```json
{
    "success": true,
    "requestId": "uuid-here",
    "message": "Request submitted successfully"
}
```
Error Response (if already requested):
```json
{
    "success": false,
    "error": "DUPLICATE_REQUEST",
    "message": "You already have an active request for this game",
    "statusCode": 409
}
```

#### GET `/requests/active`
Get all active (unfulfilled) requests with counts
```json
[
    {
        "appId": "271590",
        "gameName": "Grand Theft Auto V",
        "requestType": "Both",
        "requestCount": 15,
        "cleanRequests": 8,
        "crackedRequests": 12,
        "firstRequested": "2025-01-15T10:30:00Z"
    }
]
```

#### GET `/requests/user/{userId}`
Get all active requests by a specific user
```json
[
    {
        "appId": "271590",
        "gameName": "Grand Theft Auto V",
        "requestType": "Clean",
        "requestedAt": "2025-01-20T10:30:00Z"
    }
]
```

#### GET `/requests/top?limit={limit}`
Get top requested games with aging consideration
```json
[
    {
        "appId": "271590",
        "gameName": "Grand Theft Auto V",
        "requestType": "Both",
        "requestCount": 25,
        "firstRequested": "2025-01-05T10:30:00Z",
        "daysSinceFirstRequest": 15,
        "isStale": false
    }
]
```

#### GET `/requests/stats`
Get global request statistics
```json
{
    "totalActiveRequests": 150,
    "totalUsers": 1200,
    "totalUploads": 89,
    "fulfillmentRate": 0.68,
    "topRequestedGames": [
        {
            "appId": "271590",
            "gameName": "Grand Theft Auto V",
            "requestCount": 25
        }
    ]
}
```

### Upload and Fulfillment

#### POST `/recordshare`
Record a share/upload (called when user starts upload)
```json
{
    "userId": "uuid-here",
    "appId": "271590",
    "shareType": "clean", // clean or cracked
    "timestamp": "2025-01-20T10:30:00Z"
}
```

#### POST `/recordrequest`
Record a request (for user stats tracking)
```json
{
    "userId": "uuid-here",
    "appId": "271590",
    "requestType": "Both",
    "timestamp": "2025-01-20T10:30:00Z"
}
```

#### POST `/uploadcomplete?type={type}&appid={appid}`
**CRITICAL ENDPOINT** - Called when upload finishes
```json
{
    "appId": "271590",
    "uploadType": "clean", // clean or cracked
    "uploadUrl": "https://pydrive.com/file/abc123",
    "userId": "uuid-here",
    "hwid": "ABC123...",
    "timestamp": "2025-01-20T10:30:00Z",
    "fileSize": 85629440,
    "uploadMethod": "pydrive"
}
```

Response:
```json
{
    "success": true,
    "fulfilledRequest": true,
    "requestsFulfilled": 3, // How many requests this satisfied
    "honorPoints": 3, // Points awarded (usually = requestsFulfilled)
    "newHonorScore": 28,
    "message": "Upload recorded and 3 requests fulfilled!"
}
```

### Honor System

### Admin Endpoints (HWID-restricted)

#### POST `/admin/mark-uncrackable`
Mark a game as uncrackable (removes all requests)
```json
{
    "appId": "271590",
    "reason": "Denuvo v12 + always online",
    "evidenceUrl": "https://cs.rin.ru/forum/thread-123456",
    "adminId": "admin-hwid-hash",
    "isPermanent": true
}
```
Response:
```json
{
    "success": true,
    "requestsRemoved": 25,
    "message": "Marked Grand Theft Auto V as uncrackable and removed 25 requests"
}
```

#### POST `/admin/remove-requests`
Remove all requests for a specific game
```json
{
    "appId": "271590",
    "reason": "admin_removal",
    "adminId": "admin-hwid-hash"
}
```

#### POST `/admin/blacklist`
Blacklist a game from future requests
```json
{
    "appId": "271590",
    "reason": "Inappropriate content",
    "adminId": "admin-hwid-hash"
}
```

#### GET `/admin/uncrackable`
Get list of uncrackable games
```json
[
    {
        "appId": "271590",
        "gameName": "Some Uncrackable Game",
        "reason": "Denuvo v12",
        "markedAt": "2025-01-20T10:30:00Z",
        "isPermanent": true
    }
]
```

#### POST `/incrementhonor`
Manually increment honor score
```json
{
    "userId": "uuid-here",
    "honorIncrement": 1
}
```

#### GET `/leaderboard`
Get top contributors
```json
[
    {
        "userId": "uuid-here",
        "honorScore": 150,
        "totalShares": 45,
        "rank": 1,
        "isCurrentUser": false
    }
]
```

## Business Logic Requirements

### Request Validation
1. **One request per user per game** - Use UNIQUE constraint on (requester_user_id, app_id)
2. **HWID verification** - Ensure HWID matches the user making the request
3. **Request type validation** - Only allow "Clean", "Cracked", "Both"
4. **Game name validation** - Steam API lookup to verify app_id exists
5. **Uncrackable check** - Reject requests for games marked as uncrackable
6. **Blacklist check** - Reject requests for blacklisted games

#### Request Submission Logic
```sql
-- Before allowing new request, check if game is blacklisted or uncrackable
SELECT
    CASE
        WHEN b.app_id IS NOT NULL THEN 'BLACKLISTED'
        WHEN u.app_id IS NOT NULL AND u.is_permanent = TRUE THEN 'UNCRACKABLE_PERMANENT'
        WHEN u.app_id IS NOT NULL THEN 'UNCRACKABLE_TEMPORARY'
        ELSE 'ALLOWED'
    END as status
FROM (SELECT ? as app_id) input
LEFT JOIN blacklisted_games b ON b.app_id = input.app_id
LEFT JOIN uncrackable_games u ON u.app_id = input.app_id;
```

### Upload Processing
1. **When `/uploadcomplete` is called:**
   - Record the upload in `game_uploads` table
   - Find all matching unfulfilled requests for this app_id and upload_type
   - Mark matching requests as fulfilled
   - Award honor points = number of requests fulfilled
   - Update user's honor score and total_shares counter
   - Return fulfillment details

2. **Request matching logic:**
   ```sql
   -- For clean upload, match requests for "Clean" or "Both"
   -- For cracked upload, match requests for "Cracked" or "Both"

   UPDATE game_requests
   SET is_fulfilled = TRUE,
       fulfilled_at = NOW(),
       fulfilled_by_user_id = ?
   WHERE app_id = ?
     AND is_fulfilled = FALSE
     AND (
       (? = 'clean' AND request_type IN ('Clean', 'Both')) OR
       (? = 'cracked' AND request_type IN ('Cracked', 'Both'))
     )
   RETURNING id;
   ```

### HWID Change Handling
1. **When HWID changes:**
   - Verify old HWID belongs to the user
   - Update user record with new HWID
   - Log change in `hwid_history` table
   - Return updated user stats
   - Keep all honor/requests/shares intact

### Honor Point System
- **1 point per request fulfilled**
- **Bonus points for highly requested games:**
  - 10+ requests = +1 bonus point
  - 20+ requests = +2 bonus points
  - 50+ requests = +5 bonus points

### Rate Limiting
- **Requests per user:** 5 new requests per day
- **HWID changes:** 3 per month (prevent abuse)
- **API calls:** 100 per user per hour

## Security Requirements

### Authentication
- No traditional auth - HWID-based identification
- Validate HWID format and uniqueness
- Rate limit by IP address

### Data Validation
- Sanitize all user inputs
- Validate Steam app_ids against Steam API
- Limit string lengths (game names, etc.)
- Check file URLs are valid

### Abuse Prevention
- Track IP addresses for rate limiting
- Monitor for suspicious HWID changes
- Flag users with excessive requests but no uploads
- Blacklist known VPN/proxy IPs for registration
- **Admin HWID whitelist** - Only specific HWIDs can mark games as uncrackable
- **Community voting** - Users can vote if they agree/disagree with uncrackable marking
- **Automatic stale request detection** - Games with 30+ day old requests flagged for review

## Response Formats

### Success Response
```json
{
    "success": true,
    "data": { ... },
    "message": "Operation completed successfully"
}
```

### Error Response
```json
{
    "success": false,
    "error": "ERROR_CODE",
    "message": "Human readable error message",
    "statusCode": 400,
    "details": { ... }
}
```

### Error Codes
- `DUPLICATE_REQUEST` - User already requested this game
- `INVALID_HWID` - HWID format invalid or not found
- `RATE_LIMITED` - Too many requests
- `INVALID_APP_ID` - Steam app ID not found
- `HWID_CHANGE_LIMIT` - Too many HWID changes
- `VALIDATION_ERROR` - Input validation failed
- `GAME_UNCRACKABLE` - Game is marked as uncrackable
- `GAME_BLACKLISTED` - Game is blacklisted from requests
- `ADMIN_ONLY` - Operation requires admin privileges
- `STALE_REQUEST` - Request is too old (30+ days)

## Performance Requirements

### Database Indexes
- Index on `app_id` for fast request lookups
- Index on `hwid` for user identification
- Index on `is_fulfilled` for active request queries
- Composite index on `(requester_user_id, app_id)` for duplicate checking

### Caching
- Cache active requests for 5 minutes
- Cache user stats for 1 minute
- Cache global stats for 15 minutes

### Database Cleanup
- Archive fulfilled requests older than 30 days
- Clean up orphaned HWID history records
- Purge old upload records (keep metadata, remove URLs after 90 days)
- **Stale request handling:**
  - Flag requests older than 30 days as "stale"
  - Auto-suggest uncrackable marking for 60+ day requests
  - Archive requests older than 90 days
- **Review uncrackable games periodically** - Some might become crackable with new techniques

## Monitoring and Analytics

### Metrics to Track
- New user registrations per day
- Active requests vs fulfillments
- Top requested games
- User retention and activity
- Honor score distribution
- Upload success rates

### Alerts
- Unusual HWID change patterns
- Spike in failed requests
- High error rates on specific endpoints
- Database performance issues

## Deployment Notes

### Environment Variables
```
DATABASE_URL=postgresql://...
STEAM_API_KEY=your_steam_key_here
RATE_LIMIT_REDIS_URL=redis://...
LOG_LEVEL=INFO
API_PORT=8080
```

### Required Services
- PostgreSQL 14+ (with UUID support)
- Redis (for rate limiting and caching)
- Steam Web API access (for game validation)

This backend will create a complete ecosystem where users can request games, track fulfillments, build reputation through honor scores, and maintain their identity across hardware changes. The system prevents abuse while encouraging community participation and rewarding helpful users.