"""
SACGUI Backend API - Community Request and Honor System
Comprehensive backend for Steam Auto-Cracker GUI Request System
"""

from fastapi import APIRouter, HTTPException, Request, Query, Body, Depends
from fastapi.responses import JSONResponse
from pydantic import BaseModel, Field, validator
from typing import Optional, Dict, Any
from datetime import datetime, timedelta
from uuid import UUID, uuid4
import asyncpg
import hashlib
import os
import json
import time
from collections import defaultdict
import redis.asyncio as redis
from dotenv import load_dotenv
import re


HWID_REGEX = re.compile(r'^[a-f0-9]{64}$')


def parse_vote_choice(value: Any) -> str:
    if isinstance(value, bool):
        return 'agree' if value else 'disagree'
    if isinstance(value, (int, float)):
        return 'agree' if value else 'disagree'
    if isinstance(value, str):
        normalized = value.strip().lower()
        if normalized in {'agree', 'support', 'yes', 'true', '1', 'uncrackable'}:
            return 'agree'
        if normalized in {'disagree', 'oppose', 'no', 'false', '0', 'crackable'}:
            return 'disagree'
    raise ValueError('Invalid vote option')

load_dotenv()

# Router configuration
sacgui_api = APIRouter(prefix="/sacgui/api", tags=["sacgui-api"])

# Database configuration
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://sacgui:password@localhost/sacgui")
REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379/0")
STEAM_API_KEY = os.getenv("STEAM_API_KEY", "")

# Admin HWIDs (can mark games as uncrackable)
ADMIN_HWIDS = os.getenv("ADMIN_HWIDS", "").split(",")

# Cache TTL settings
CACHE_ACTIVE_REQUESTS_TTL = 300  # 5 minutes
CACHE_USER_STATS_TTL = 60  # 1 minute
CACHE_GLOBAL_STATS_TTL = 900  # 15 minutes

# Rate limiting settings
RATE_LIMIT_REQUESTS_PER_DAY = 5
RATE_LIMIT_HWID_CHANGES_PER_MONTH = 3
RATE_LIMIT_API_CALLS_PER_HOUR = 100

# Honor point bonuses
HONOR_BONUS_10_REQUESTS = 1
HONOR_BONUS_20_REQUESTS = 2
HONOR_BONUS_50_REQUESTS = 5

# Database connection pool
db_pool = None
redis_client = None


# ======================== Pydantic Models ========================

class RegisterUserRequest(BaseModel):
    hwid: str = Field(..., min_length=64, max_length=64, pattern="^[a-f0-9]{64}$")
    timestamp: datetime
    version: str
    machine: Optional[str] = None

    @validator('hwid')
    def validate_hwid(cls, v):
        if not re.match(r'^[a-f0-9]{64}$', v):
            raise ValueError('Invalid HWID format')
        return v


class UpdateHWIDRequest(BaseModel):
    oldHwid: str = Field(..., min_length=64, max_length=64)
    newHwid: str = Field(..., min_length=64, max_length=64)
    userId: UUID
    timestamp: datetime
    reason: str = "hardware_change"


class VerifyUserRequest(BaseModel):
    hwid: str = Field(..., min_length=64, max_length=64)
    userId: UUID


class GameRequestModel(BaseModel):
    appId: str = Field(..., max_length=20)
    gameName: str = Field(..., max_length=500)
    requestType: str = Field(..., pattern="^(Clean|Cracked|Both)$")
    userId: UUID
    hwid: str = Field(..., min_length=64, max_length=64)
    timestamp: datetime


class RecordShareRequest(BaseModel):
    userId: UUID
    appId: str = Field(..., max_length=20)
    shareType: str = Field(..., pattern="^(clean|cracked)$")
    timestamp: datetime


class RecordRequestModel(BaseModel):
    userId: UUID
    appId: str = Field(..., max_length=20)
    requestType: str = Field(..., pattern="^(Clean|Cracked|Both)$")
    timestamp: datetime


class UploadCompleteRequest(BaseModel):
    appId: str = Field(..., max_length=20)
    uploadType: str = Field(..., pattern="^(clean|cracked)$")
    uploadUrl: str
    userId: UUID
    hwid: str = Field(..., min_length=64, max_length=64)
    timestamp: datetime
    fileSize: int
    uploadMethod: str = "pydrive"


class MarkUncrackableRequest(BaseModel):
    appId: str = Field(..., max_length=20)
    reason: str = Field(..., max_length=500)
    evidenceUrl: Optional[str] = None
    adminId: str = Field(..., min_length=64, max_length=64)
    isPermanent: bool = True


class BlacklistGameRequest(BaseModel):
    appId: str = Field(..., max_length=20)
    reason: str = Field(..., max_length=500)
    adminId: str = Field(..., min_length=64, max_length=64)


class RemoveRequestsRequest(BaseModel):
    appId: str = Field(..., max_length=20)
    reason: Optional[str] = Field(None, max_length=500)
    adminId: str = Field(..., min_length=64, max_length=64)


class UncrackableVoteRequest(BaseModel):
    appId: str = Field(..., max_length=20)
    userId: UUID
    hwid: str = Field(..., min_length=64, max_length=64)
    vote: str

    @validator('hwid')
    def validate_hwid(cls, value):
        if not HWID_REGEX.fullmatch(value):
            raise ValueError('Invalid HWID format')
        return value

    @validator('vote')
    def validate_vote(cls, value):
        return parse_vote_choice(value)


class IncrementHonorRequest(BaseModel):
    userId: UUID
    honorIncrement: int = Field(..., ge=1, le=100)


# ======================== Database Functions ========================

async def init_db():
    """Initialize database connection pool"""
    global db_pool, redis_client

    if not db_pool:
        db_pool = await asyncpg.create_pool(DATABASE_URL, min_size=5, max_size=20)

    if not redis_client:
        redis_client = await redis.from_url(REDIS_URL, decode_responses=True)


async def close_db():
    """Close database connections"""
    global db_pool, redis_client

    if db_pool:
        await db_pool.close()

    if redis_client:
        await redis_client.close()


async def create_tables():
    """Create all required database tables"""
    async with db_pool.acquire() as conn:
        # Users table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS users (
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
            )
        ''')

        await conn.execute('CREATE INDEX IF NOT EXISTS idx_users_hwid ON users(hwid)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_users_honor ON users(honor_score DESC)')

        # HWID History table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS hwid_history (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID REFERENCES users(id),
                old_hwid VARCHAR(255),
                new_hwid VARCHAR(255),
                change_reason VARCHAR(100),
                changed_at TIMESTAMP DEFAULT NOW(),
                ip_address INET,
                machine_name VARCHAR(255)
            )
        ''')

        # Game requests table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS game_requests (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                app_id VARCHAR(20) NOT NULL,
                game_name VARCHAR(500) NOT NULL,
                request_type VARCHAR(20) NOT NULL,
                requester_user_id UUID REFERENCES users(id),
                requester_hwid VARCHAR(255) NOT NULL,
                created_at TIMESTAMP DEFAULT NOW(),
                is_fulfilled BOOLEAN DEFAULT FALSE,
                fulfilled_at TIMESTAMP NULL,
                fulfilled_by_user_id UUID REFERENCES users(id) NULL,
                UNIQUE(requester_user_id, app_id)
            )
        ''')

        await conn.execute('CREATE INDEX IF NOT EXISTS idx_requests_app_id ON game_requests(app_id)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_requests_active ON game_requests(is_fulfilled) WHERE is_fulfilled = FALSE')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_requests_user ON game_requests(requester_user_id)')

        # Game uploads table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS game_uploads (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                app_id VARCHAR(20) NOT NULL,
                game_name VARCHAR(500) NOT NULL,
                upload_type VARCHAR(20) NOT NULL,
                uploader_user_id UUID REFERENCES users(id),
                uploader_hwid VARCHAR(255) NOT NULL,
                upload_url TEXT NOT NULL,
                file_size BIGINT,
                upload_method VARCHAR(50),
                created_at TIMESTAMP DEFAULT NOW(),
                is_verified BOOLEAN DEFAULT FALSE,
                download_count INTEGER DEFAULT 0,
                honor_points_awarded INTEGER DEFAULT 1
            )
        ''')

        await conn.execute('CREATE INDEX IF NOT EXISTS idx_uploads_app_id ON game_uploads(app_id)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_uploads_user ON game_uploads(uploader_user_id)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_uploads_date ON game_uploads(created_at DESC)')

        # Uncrackable games table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS uncrackable_games (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                app_id VARCHAR(20) UNIQUE NOT NULL,
                game_name VARCHAR(500) NOT NULL,
                marked_by_admin_id UUID,
                marked_at TIMESTAMP DEFAULT NOW(),
                reason VARCHAR(500),
                evidence_url TEXT,
                community_votes_agree INTEGER DEFAULT 0,
                community_votes_disagree INTEGER DEFAULT 0,
                is_permanent BOOLEAN DEFAULT FALSE
            )
        ''')

        await conn.execute('CREATE INDEX IF NOT EXISTS idx_uncrackable_app_id ON uncrackable_games(app_id)')

        await conn.execute('''
            CREATE TABLE IF NOT EXISTS uncrackable_votes (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                app_id VARCHAR(20) NOT NULL,
                user_id UUID REFERENCES users(id),
                voter_hwid VARCHAR(255) NOT NULL,
                vote BOOLEAN NOT NULL,
                created_at TIMESTAMP DEFAULT NOW(),
                updated_at TIMESTAMP DEFAULT NOW(),
                UNIQUE(app_id, user_id)
            )
        ''')

        await conn.execute('CREATE INDEX IF NOT EXISTS idx_votes_app_id ON uncrackable_votes(app_id)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_votes_user_id ON uncrackable_votes(user_id)')

        # Blacklisted games table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS blacklisted_games (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                app_id VARCHAR(20) UNIQUE NOT NULL,
                game_name VARCHAR(500) NOT NULL,
                blacklisted_by_admin_id UUID,
                blacklisted_at TIMESTAMP DEFAULT NOW(),
                reason VARCHAR(500),
                is_permanent BOOLEAN DEFAULT TRUE
            )
        ''')

        # Request fulfillments table
        await conn.execute('''
            CREATE TABLE IF NOT EXISTS request_fulfillments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                request_id UUID REFERENCES game_requests(id),
                upload_id UUID REFERENCES game_uploads(id),
                fulfilled_at TIMESTAMP DEFAULT NOW(),
                honor_points_awarded INTEGER DEFAULT 1
            )
        ''')


# ======================== Helper Functions ========================

def hash_hwid(hwid: str) -> str:
    """Generate SHA512 hash of HWID for security"""
    return hashlib.sha512(hwid.encode()).hexdigest()


async def check_rate_limit(key: str, max_count: int, window_seconds: int) -> bool:
    """Check if rate limit is exceeded"""
    current = await redis_client.incr(key)
    if current == 1:
        await redis_client.expire(key, window_seconds)
    return current <= max_count


async def validate_steam_app_id(app_id: str) -> bool:
    """Validate if Steam app ID exists (mock for now)"""
    # TODO: Implement actual Steam API validation
    return app_id.isdigit() and len(app_id) <= 10


async def check_game_status(app_id: str) -> str:
    """Check if game is blacklisted or uncrackable"""
    async with db_pool.acquire() as conn:
        result = await conn.fetchrow('''
            SELECT
                CASE
                    WHEN b.app_id IS NOT NULL THEN 'BLACKLISTED'
                    WHEN u.app_id IS NOT NULL AND u.is_permanent = TRUE THEN 'UNCRACKABLE_PERMANENT'
                    WHEN u.app_id IS NOT NULL THEN 'UNCRACKABLE_TEMPORARY'
                    ELSE 'ALLOWED'
                END as status
            FROM (SELECT $1::VARCHAR as app_id) input
            LEFT JOIN blacklisted_games b ON b.app_id = input.app_id
            LEFT JOIN uncrackable_games u ON u.app_id = input.app_id
        ''', app_id)

        return result['status']


async def build_uncrackable_status(conn, app_id: str, user_id: Optional[UUID] = None) -> Dict[str, Any]:
    status_row = await conn.fetchrow('''
        WITH input_app AS (SELECT $1::VARCHAR AS app_id)
        SELECT
            COALESCE(agg.votes_agree, 0) AS votes_agree,
            COALESCE(agg.votes_disagree, 0) AS votes_disagree,
            ug.is_permanent,
            ug.reason
        FROM input_app
        LEFT JOIN (
            SELECT
                app_id,
                COUNT(*) FILTER (WHERE vote = TRUE) AS votes_agree,
                COUNT(*) FILTER (WHERE vote = FALSE) AS votes_disagree
            FROM uncrackable_votes
            WHERE app_id = $1
            GROUP BY app_id
        ) agg ON agg.app_id = input_app.app_id
        LEFT JOIN uncrackable_games ug ON ug.app_id = input_app.app_id
    ''', app_id)

    votes_agree = status_row['votes_agree'] if status_row and status_row['votes_agree'] is not None else 0
    votes_disagree = status_row['votes_disagree'] if status_row and status_row['votes_disagree'] is not None else 0
    is_marked = bool(
        status_row and (
            status_row['is_permanent'] is not None or status_row['reason'] is not None
        )
    )
    reason = status_row['reason'] if status_row else None
    is_permanent = status_row['is_permanent'] if status_row and status_row['is_permanent'] is not None else False

    user_vote = None
    if user_id:
        vote_row = await conn.fetchrow(
            'SELECT vote FROM uncrackable_votes WHERE app_id = $1 AND user_id = $2',
            app_id,
            user_id
        )
        if vote_row is not None:
            user_vote = 'agree' if vote_row['vote'] else 'disagree'

    return {
        'appId': app_id,
        'votesAgree': votes_agree,
        'votesDisagree': votes_disagree,
        'totalVotes': votes_agree + votes_disagree,
        'userVote': user_vote,
        'isMarkedUncrackable': is_marked,
        'isPermanent': is_permanent,
        'reason': reason
    }


# ======================== User Management Endpoints ========================

@sacgui_api.post("/register")
async def register_user(request: RegisterUserRequest):
    """Register a new user with HWID"""
    try:
        async with db_pool.acquire() as conn:
            # Check if HWID already exists
            existing = await conn.fetchrow('SELECT id FROM users WHERE hwid = $1', request.hwid)
            if existing:
                return JSONResponse(content={
                    "success": False,
                    "error": "HWID_EXISTS",
                    "message": "HWID already registered",
                    "statusCode": 409
                }, status_code=409)

            # Create new user
            user_id = await conn.fetchval('''
                INSERT INTO users (hwid, hwid_hash, machine_name, app_version)
                VALUES ($1, $2, $3, $4)
                RETURNING id
            ''', request.hwid, hash_hwid(request.hwid), request.machine, request.version)

            return {
                "success": True,
                "userId": str(user_id),
                "message": "User registered successfully"
            }

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.post("/updatehwid")
async def update_hwid(request: UpdateHWIDRequest):
    """Update HWID when hardware changes"""
    try:
        # Check rate limit for HWID changes
        rate_limit_key = f"hwid_change:{request.userId}"
        if not await check_rate_limit(rate_limit_key, RATE_LIMIT_HWID_CHANGES_PER_MONTH, 30*24*3600):
            raise HTTPException(
                status_code=429,
                detail="HWID_CHANGE_LIMIT: Too many HWID changes this month"
            )

        async with db_pool.acquire() as conn:
            # Verify old HWID belongs to user
            user = await conn.fetchrow(
                'SELECT id, honor_score, total_shares, total_requests FROM users WHERE id = $1 AND hwid = $2',
                request.userId, request.oldHwid
            )

            if not user:
                raise HTTPException(
                    status_code=404,
                    detail="User not found or HWID mismatch"
                )

            # Update HWID
            await conn.execute(
                'UPDATE users SET hwid = $1, hwid_hash = $2, last_seen = NOW() WHERE id = $3',
                request.newHwid, hash_hwid(request.newHwid), request.userId
            )

            # Log change in history
            await conn.execute('''
                INSERT INTO hwid_history (user_id, old_hwid, new_hwid, change_reason)
                VALUES ($1, $2, $3, $4)
            ''', request.userId, request.oldHwid, request.newHwid, request.reason)

            return {
                "success": True,
                "honorScore": user['honor_score'],
                "totalShares": user['total_shares'],
                "totalRequests": user['total_requests'],
                "message": "HWID updated successfully"
            }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.post("/verify")
async def verify_user(request: VerifyUserRequest):
    """Verify existing user and get updated stats"""
    try:
        async with db_pool.acquire() as conn:
            user = await conn.fetchrow('''
                SELECT id, honor_score, total_shares, total_requests, last_seen
                FROM users
                WHERE id = $1 AND hwid = $2
            ''', request.userId, request.hwid)

            if not user:
                raise HTTPException(
                    status_code=404,
                    detail="User not found or HWID mismatch"
                )

            # Update last seen
            await conn.execute(
                'UPDATE users SET last_seen = NOW() WHERE id = $1',
                request.userId
            )

            return {
                "success": True,
                "honorScore": user['honor_score'],
                "totalShares": user['total_shares'],
                "totalRequests": user['total_requests'],
                "lastSeen": user['last_seen'].isoformat()
            }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ======================== Request Management Endpoints ========================

@sacgui_api.post("/requests")
async def submit_game_request(request: GameRequestModel):
    """Submit a new game request (ONE PER USER PER GAME)"""
    try:
        # Check rate limit
        rate_limit_key = f"requests:{request.userId}"
        if not await check_rate_limit(rate_limit_key, RATE_LIMIT_REQUESTS_PER_DAY, 24*3600):
            raise HTTPException(
                status_code=429,
                detail="RATE_LIMITED: Too many requests today"
            )

        # Check game status
        game_status = await check_game_status(request.appId)
        if game_status == 'BLACKLISTED':
            raise HTTPException(
                status_code=403,
                detail="GAME_BLACKLISTED: This game is blacklisted from requests"
            )
        elif game_status in ['UNCRACKABLE_PERMANENT', 'UNCRACKABLE_TEMPORARY']:
            raise HTTPException(
                status_code=403,
                detail=f"GAME_UNCRACKABLE: This game is marked as uncrackable"
            )

        # Validate Steam app ID
        if not await validate_steam_app_id(request.appId):
            raise HTTPException(
                status_code=400,
                detail="INVALID_APP_ID: Steam app ID not found"
            )

        async with db_pool.acquire() as conn:
            try:
                # Insert new request
                request_id = await conn.fetchval('''
                    INSERT INTO game_requests (
                        app_id, game_name, request_type,
                        requester_user_id, requester_hwid
                    ) VALUES ($1, $2, $3, $4, $5)
                    RETURNING id
                ''', request.appId, request.gameName, request.requestType,
                    request.userId, request.hwid)

                # Update user request count
                await conn.execute(
                    'UPDATE users SET total_requests = total_requests + 1 WHERE id = $1',
                    request.userId
                )

                # Invalidate cache
                await redis_client.delete(f"cache:active_requests")

                return {
                    "success": True,
                    "requestId": str(request_id),
                    "message": "Request submitted successfully"
                }

            except asyncpg.UniqueViolationError:
                return JSONResponse(content={
                    "success": False,
                    "error": "DUPLICATE_REQUEST",
                    "message": "You already have an active request for this game",
                    "statusCode": 409
                }, status_code=409)

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/requests/active")
async def get_active_requests():
    """Get all active (unfulfilled) requests with counts"""
    try:
        # Check cache first
        cached = await redis_client.get("cache:active_requests")
        if cached:
            return json.loads(cached)

        async with db_pool.acquire() as conn:
            results = await conn.fetch('''
                SELECT
                    app_id as "appId",
                    game_name as "gameName",
                    COUNT(*) as "requestCount",
                    COUNT(CASE WHEN request_type IN ('Clean', 'Both') THEN 1 END) as "cleanRequests",
                    COUNT(CASE WHEN request_type IN ('Cracked', 'Both') THEN 1 END) as "crackedRequests",
                    MIN(created_at) as "firstRequested"
                FROM game_requests
                WHERE is_fulfilled = FALSE
                GROUP BY app_id, game_name
                ORDER BY "requestCount" DESC
            ''')

            response = []
            for row in results:
                response.append({
                    "appId": row['appId'],
                    "gameName": row['gameName'],
                    "requestType": "Both",  # Aggregate type
                    "requestCount": row['requestCount'],
                    "cleanRequests": row['cleanRequests'],
                    "crackedRequests": row['crackedRequests'],
                    "firstRequested": row['firstRequested'].isoformat()
                })

            # Cache the response
            await redis_client.setex(
                "cache:active_requests",
                CACHE_ACTIVE_REQUESTS_TTL,
                json.dumps(response)
            )

            return response

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/requests/user/{user_id}")
async def get_user_requests(user_id: UUID):
    """Get all active requests by a specific user"""
    try:
        async with db_pool.acquire() as conn:
            results = await conn.fetch('''
                SELECT
                    app_id as "appId",
                    game_name as "gameName",
                    request_type as "requestType",
                    created_at as "requestedAt"
                FROM game_requests
                WHERE requester_user_id = $1 AND is_fulfilled = FALSE
                ORDER BY created_at DESC
            ''', user_id)

            return [{
                "appId": row['appId'],
                "gameName": row['gameName'],
                "requestType": row['requestType'],
                "requestedAt": row['requestedAt'].isoformat()
            } for row in results]

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.delete("/remove-request/{app_id}")
async def remove_request(app_id: str, user_id: UUID = Query(...), hwid: str = Query(...)):
    """Remove an active request for the authenticated user"""
    if not HWID_REGEX.fullmatch(hwid):
        raise HTTPException(status_code=400, detail="INVALID_HWID: Provided HWID format is invalid")

    try:
        async with db_pool.acquire() as conn:
            user_record = await conn.fetchrow('SELECT hwid FROM users WHERE id = $1', user_id)
            if not user_record or user_record['hwid'] != hwid:
                raise HTTPException(status_code=403, detail="HWID_MISMATCH: HWID does not match user")

            deleted_row = await conn.fetchrow('''
                DELETE FROM game_requests
                WHERE app_id = $1
                  AND requester_user_id = $2
                  AND is_fulfilled = FALSE
                RETURNING game_name
            ''', app_id, user_id)

            if not deleted_row:
                raise HTTPException(status_code=404, detail="REQUEST_NOT_FOUND: No active request found to remove")

            await redis_client.delete("cache:active_requests")

            return {
                "success": True,
                "appId": app_id,
                "gameName": deleted_row['game_name'],
                "message": "Request removed successfully"
            }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/requests/top")
async def get_top_requests(limit: int = Query(10, ge=1, le=100)):
    """Get top requested games with aging consideration"""
    try:
        async with db_pool.acquire() as conn:
            results = await conn.fetch('''
                SELECT
                    app_id as "appId",
                    game_name as "gameName",
                    COUNT(*) as "requestCount",
                    MIN(created_at) as "firstRequested",
                    EXTRACT(DAY FROM NOW() - MIN(created_at)) as "daysSinceFirstRequest"
                FROM game_requests
                WHERE is_fulfilled = FALSE
                GROUP BY app_id, game_name
                ORDER BY "requestCount" DESC
                LIMIT $1
            ''', limit)

            response = []
            for row in results:
                days_old = int(row['daysSinceFirstRequest'] or 0)
                response.append({
                    "appId": row['appId'],
                    "gameName": row['gameName'],
                    "requestType": "Both",
                    "requestCount": row['requestCount'],
                    "firstRequested": row['firstRequested'].isoformat(),
                    "daysSinceFirstRequest": days_old,
                    "isStale": days_old > 30
                })

            return response

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/requests/stats")
async def get_request_stats():
    """Get global request statistics"""
    try:
        # Check cache first
        cached = await redis_client.get("cache:global_stats")
        if cached:
            return json.loads(cached)

        async with db_pool.acquire() as conn:
            stats = await conn.fetchrow('''
                SELECT
                    COUNT(DISTINCT gr.id) FILTER (WHERE gr.is_fulfilled = FALSE) as active_requests,
                    COUNT(DISTINCT u.id) as total_users,
                    COUNT(DISTINCT gu.id) as total_uploads,
                    CASE
                        WHEN COUNT(DISTINCT gr.id) > 0
                        THEN CAST(COUNT(DISTINCT gr.id) FILTER (WHERE gr.is_fulfilled = TRUE) AS FLOAT) / COUNT(DISTINCT gr.id)
                        ELSE 0
                    END as fulfillment_rate
                FROM users u
                LEFT JOIN game_requests gr ON TRUE
                LEFT JOIN game_uploads gu ON TRUE
            ''')

            top_games = await conn.fetch('''
                SELECT
                    app_id as "appId",
                    game_name as "gameName",
                    COUNT(*) as "requestCount"
                FROM game_requests
                WHERE is_fulfilled = FALSE
                GROUP BY app_id, game_name
                ORDER BY "requestCount" DESC
                LIMIT 5
            ''')

            response = {
                "totalActiveRequests": stats['active_requests'] or 0,
                "totalUsers": stats['total_users'] or 0,
                "totalUploads": stats['total_uploads'] or 0,
                "fulfillmentRate": round(stats['fulfillment_rate'] or 0, 2),
                "topRequestedGames": [{
                    "appId": row['appId'],
                    "gameName": row['gameName'],
                    "requestCount": row['requestCount']
                } for row in top_games]
            }

            # Cache the response
            await redis_client.setex(
                "cache:global_stats",
                CACHE_GLOBAL_STATS_TTL,
                json.dumps(response)
            )

            return response

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ======================== Upload and Fulfillment Endpoints ========================

@sacgui_api.post("/recordshare")
async def record_share(request: RecordShareRequest):
    """Record a share/upload (called when user starts upload)"""
    try:
        # This is called when upload starts, actual completion handled by uploadcomplete
        return {
            "success": True,
            "message": "Share intent recorded"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.post("/recordrequest")
async def record_request(request: RecordRequestModel):
    """Record a request (for user stats tracking)"""
    try:
        # This is handled by the main /requests endpoint
        return {
            "success": True,
            "message": "Request recorded"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.post("/uploadcomplete")
async def upload_complete(request: UploadCompleteRequest):
    """CRITICAL ENDPOINT - Called when upload finishes"""
    try:
        async with db_pool.acquire() as conn:
            async with conn.transaction():
                # Record the upload
                upload_id = await conn.fetchval('''
                    INSERT INTO game_uploads (
                        app_id, game_name, upload_type,
                        uploader_user_id, uploader_hwid,
                        upload_url, file_size, upload_method
                    ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
                    RETURNING id
                ''', request.appId, "", request.uploadType,
                    request.userId, request.hwid,
                    request.uploadUrl, request.fileSize, request.uploadMethod)

                # Find matching unfulfilled requests
                matching_requests = await conn.fetch('''
                    UPDATE game_requests
                    SET is_fulfilled = TRUE,
                        fulfilled_at = NOW(),
                        fulfilled_by_user_id = $1
                    WHERE app_id = $2
                      AND is_fulfilled = FALSE
                      AND (
                        ($3 = 'clean' AND request_type IN ('Clean', 'Both')) OR
                        ($3 = 'cracked' AND request_type IN ('Cracked', 'Both'))
                      )
                    RETURNING id
                ''', request.userId, request.appId, request.uploadType)

                requests_fulfilled = len(matching_requests)

                # Calculate honor points with bonuses
                honor_points = requests_fulfilled
                if requests_fulfilled >= 50:
                    honor_points += HONOR_BONUS_50_REQUESTS
                elif requests_fulfilled >= 20:
                    honor_points += HONOR_BONUS_20_REQUESTS
                elif requests_fulfilled >= 10:
                    honor_points += HONOR_BONUS_10_REQUESTS

                # Record fulfillments
                for req in matching_requests:
                    await conn.execute('''
                        INSERT INTO request_fulfillments (
                            request_id, upload_id, honor_points_awarded
                        ) VALUES ($1, $2, $3)
                    ''', req['id'], upload_id, 1)

                # Update user stats
                user_stats = await conn.fetchrow('''
                    UPDATE users
                    SET honor_score = honor_score + $1,
                        total_shares = total_shares + 1,
                        last_seen = NOW()
                    WHERE id = $2
                    RETURNING honor_score
                ''', honor_points, request.userId)

                # Update upload with honor points
                await conn.execute(
                    'UPDATE game_uploads SET honor_points_awarded = $1 WHERE id = $2',
                    honor_points, upload_id
                )

                # Invalidate caches
                await redis_client.delete("cache:active_requests")
                await redis_client.delete("cache:global_stats")

                return {
                    "success": True,
                    "fulfilledRequest": requests_fulfilled > 0,
                    "requestsFulfilled": requests_fulfilled,
                    "honorPoints": honor_points,
                    "newHonorScore": user_stats['honor_score'],
                    "message": f"Upload recorded and {requests_fulfilled} requests fulfilled!"
                }

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ======================== Uncrackable Voting Endpoints ========================

@sacgui_api.post("/vote-uncrackable")
async def vote_uncrackable(request: UncrackableVoteRequest):
    """Submit or update a community vote on uncrackable status"""
    if not await validate_steam_app_id(request.appId):
        raise HTTPException(status_code=400, detail="INVALID_APP_ID: Steam app ID not found")

    try:
        async with db_pool.acquire() as conn:
            user_record = await conn.fetchrow('SELECT hwid FROM users WHERE id = $1', request.userId)
            if not user_record or user_record['hwid'] != request.hwid:
                raise HTTPException(status_code=403, detail="HWID_MISMATCH: HWID does not match user")

            vote_bool = request.vote == 'agree'
            existing_vote = await conn.fetchrow(
                'SELECT vote FROM uncrackable_votes WHERE app_id = $1 AND user_id = $2',
                request.appId,
                request.userId
            )

            if existing_vote is None:
                action = 'recorded'
            elif existing_vote['vote'] == vote_bool:
                action = 'unchanged'
            else:
                action = 'updated'

            await conn.execute('''
                INSERT INTO uncrackable_votes (app_id, user_id, voter_hwid, vote)
                VALUES ($1, $2, $3, $4)
                ON CONFLICT (app_id, user_id)
                DO UPDATE SET
                    vote = EXCLUDED.vote,
                    voter_hwid = EXCLUDED.voter_hwid,
                    updated_at = NOW()
            ''', request.appId, request.userId, request.hwid, vote_bool)

            status = await build_uncrackable_status(conn, request.appId, request.userId)

            await conn.execute('''
                UPDATE uncrackable_games
                SET community_votes_agree = $1,
                    community_votes_disagree = $2,
                    marked_at = COALESCE(marked_at, NOW())
                WHERE app_id = $3
            ''', status['votesAgree'], status['votesDisagree'], request.appId)

            message_map = {
                'recorded': 'Vote recorded successfully',
                'updated': 'Vote updated successfully',
                'unchanged': 'Vote already recorded'
            }

            response = {
                "success": True,
                "action": action,
                "message": message_map[action]
            }
            response.update(status)
            return response

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/uncrackable-status/{app_id}")
async def get_uncrackable_status(app_id: str, user_id: Optional[UUID] = Query(None)):
    """Get vote totals and user vote for a specific game"""
    try:
        async with db_pool.acquire() as conn:
            status = await build_uncrackable_status(conn, app_id, user_id)
            return status

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/is-uncrackable/{app_id}")
async def is_game_uncrackable(app_id: str):
    """Quick check if a game has been marked uncrackable"""
    try:
        async with db_pool.acquire() as conn:
            record = await conn.fetchrow(
                'SELECT is_permanent, reason, community_votes_agree, community_votes_disagree FROM uncrackable_games WHERE app_id = $1',
                app_id
            )

            if not record:
                return {
                    "appId": app_id,
                    "isUncrackable": False
                }

            return {
                "appId": app_id,
                "isUncrackable": True,
                "isPermanent": record['is_permanent'],
                "reason": record['reason'],
                "votesAgree": record['community_votes_agree'],
                "votesDisagree": record['community_votes_disagree']
            }

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ======================== Honor System Endpoints ========================

@sacgui_api.post("/incrementhonor")
async def increment_honor(request: IncrementHonorRequest):
    """Manually increment honor score (admin only)"""
    # TODO: Add admin authentication
    try:
        async with db_pool.acquire() as conn:
            new_score = await conn.fetchval('''
                UPDATE users
                SET honor_score = honor_score + $1
                WHERE id = $2
                RETURNING honor_score
            ''', request.honorIncrement, request.userId)

            if new_score is None:
                raise HTTPException(status_code=404, detail="User not found")

            return {
                "success": True,
                "newHonorScore": new_score,
                "message": f"Honor score increased by {request.honorIncrement}"
            }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/leaderboard")
async def get_leaderboard(limit: int = Query(10, ge=1, le=100)):
    """Get top contributors"""
    try:
        async with db_pool.acquire() as conn:
            results = await conn.fetch('''
                SELECT
                    id,
                    honor_score,
                    total_shares,
                    ROW_NUMBER() OVER (ORDER BY honor_score DESC) as rank
                FROM users
                WHERE is_active = TRUE
                ORDER BY honor_score DESC
                LIMIT $1
            ''', limit)

            return [{
                "userId": str(row['id']),
                "honorScore": row['honor_score'],
                "totalShares": row['total_shares'],
                "rank": row['rank'],
                "isCurrentUser": False  # Client will determine this
            } for row in results]

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ======================== Admin Endpoints ========================

@sacgui_api.post("/admin/mark-uncrackable")
async def mark_uncrackable(request: MarkUncrackableRequest):
    """Mark a game as uncrackable (removes all requests)"""
    try:
        # Verify admin HWID
        if request.adminId not in ADMIN_HWIDS:
            raise HTTPException(
                status_code=403,
                detail="ADMIN_ONLY: Operation requires admin privileges"
            )

        async with db_pool.acquire() as conn:
            async with conn.transaction():
                # Get game name from existing requests
                game_info = await conn.fetchrow(
                    'SELECT game_name FROM game_requests WHERE app_id = $1 LIMIT 1',
                    request.appId
                )

                game_name = game_info['game_name'] if game_info else "Unknown Game"

                # Mark as uncrackable
                await conn.execute('''
                    INSERT INTO uncrackable_games (
                        app_id, game_name, reason,
                        evidence_url, is_permanent
                    ) VALUES ($1, $2, $3, $4, $5)
                    ON CONFLICT (app_id)
                    DO UPDATE SET
                        reason = $3,
                        evidence_url = $4,
                        is_permanent = $5,
                        marked_at = NOW()
                ''', request.appId, game_name, request.reason,
                    request.evidenceUrl, request.isPermanent)

                # Remove all active requests for this game
                deleted = await conn.fetchval('''
                    DELETE FROM game_requests
                    WHERE app_id = $1 AND is_fulfilled = FALSE
                    RETURNING COUNT(*)
                ''', request.appId)

                # Invalidate cache
                await redis_client.delete("cache:active_requests")

                return {
                    "success": True,
                    "requestsRemoved": deleted or 0,
                    "message": f"Marked {game_name} as uncrackable and removed {deleted or 0} requests"
                }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.post("/admin/remove-requests")
async def remove_requests(request: RemoveRequestsRequest):
    """Remove all requests for a specific game"""
    try:
        # Verify admin HWID
        if request.adminId not in ADMIN_HWIDS:
            raise HTTPException(
                status_code=403,
                detail="ADMIN_ONLY: Operation requires admin privileges"
            )

        async with db_pool.acquire() as conn:
            deleted = await conn.fetchval('''
                DELETE FROM game_requests
                WHERE app_id = $1 AND is_fulfilled = FALSE
                RETURNING COUNT(*)
            ''', request.appId)

            # Invalidate cache
            await redis_client.delete("cache:active_requests")

            return {
                "success": True,
                "requestsRemoved": deleted or 0,
                "message": f"Removed {deleted or 0} requests for app_id {request.appId}",
                "reason": request.reason
            }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.post("/admin/blacklist")
async def blacklist_game(request: BlacklistGameRequest):
    """Blacklist a game from future requests"""
    try:
        # Verify admin HWID
        if request.adminId not in ADMIN_HWIDS:
            raise HTTPException(
                status_code=403,
                detail="ADMIN_ONLY: Operation requires admin privileges"
            )

        async with db_pool.acquire() as conn:
            async with conn.transaction():
                # Get game name
                game_info = await conn.fetchrow(
                    'SELECT game_name FROM game_requests WHERE app_id = $1 LIMIT 1',
                    request.appId
                )

                game_name = game_info['game_name'] if game_info else "Unknown Game"

                # Add to blacklist
                await conn.execute('''
                    INSERT INTO blacklisted_games (
                        app_id, game_name, reason, is_permanent
                    ) VALUES ($1, $2, $3, TRUE)
                    ON CONFLICT (app_id)
                    DO UPDATE SET
                        reason = $3,
                        blacklisted_at = NOW()
                ''', request.appId, game_name, request.reason)

                # Remove existing requests
                deleted = await conn.fetchval('''
                    DELETE FROM game_requests
                    WHERE app_id = $1
                    RETURNING COUNT(*)
                ''', request.appId)

                # Invalidate cache
                await redis_client.delete("cache:active_requests")

                return {
                    "success": True,
                    "requestsRemoved": deleted or 0,
                    "message": f"Blacklisted {game_name} and removed {deleted or 0} requests"
                }

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@sacgui_api.get("/admin/uncrackable")
async def get_uncrackable_games():
    """Get list of uncrackable games"""
    try:
        async with db_pool.acquire() as conn:
            results = await conn.fetch('''
                SELECT
                    app_id as "appId",
                    game_name as "gameName",
                    reason,
                    marked_at as "markedAt",
                    is_permanent as "isPermanent",
                    community_votes_agree as "votesAgree",
                    community_votes_disagree as "votesDisagree"
                FROM uncrackable_games
                ORDER BY marked_at DESC
            ''')

            return [{
                "appId": row['appId'],
                "gameName": row['gameName'],
                "reason": row['reason'],
                "markedAt": row['markedAt'].isoformat(),
                "isPermanent": row['isPermanent'],
                "votesAgree": row['votesAgree'],
                "votesDisagree": row['votesDisagree']
            } for row in results]

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ======================== Startup and Shutdown Events ========================

@sacgui_api.on_event("startup")
async def startup_event():
    """Initialize database and create tables on startup"""
    await init_db()
    await create_tables()
    print("SACGUI Backend API started successfully")


@sacgui_api.on_event("shutdown")
async def shutdown_event():
    """Clean up connections on shutdown"""
    await close_db()
    print("SACGUI Backend API shut down")


# Export the router
__all__ = ['sacgui_api']