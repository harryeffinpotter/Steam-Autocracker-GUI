using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace APPID
{
    /// <summary>
    /// Pulls a game's build/branch info from the free steamcmd.net JSON mirror of
    /// PICS app info. Steam exposes the current head of each branch (public, beta,
    /// previous, steam_legacy) with its buildid + update time, so we can:
    ///   - tell whether the INSTALLED buildid is the latest (== public buildid), and
    ///   - look up the actual publish date for the installed buildid if it matches
    ///     any branch head.
    /// (There is no full historical archive of every build, so an arbitrary old
    /// build that isn't a current branch head won't be found here.)
    /// Cached in memory and on disk so the library list hits the network once/day.
    /// </summary>
    public static class SteamUpdateInfo
    {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private static readonly ConcurrentDictionary<string, AppBuildInfo> MemCache = new ConcurrentDictionary<string, AppBuildInfo>();
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(4);

        private static readonly object FileLock = new object();
        private static Dictionary<string, AppBuildInfo> _fileCache;
        private static readonly string CachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SACGUI", "steam_update_cache_v2.json");
        private const long TtlSeconds = 24 * 60 * 60;

        public class AppBuildInfo
        {
            [JsonProperty("pubBuild")] public string PublicBuildId { get; set; }
            [JsonProperty("pubTime")] public long PublicTimeUpdated { get; set; }
            // buildid -> publish time (unix seconds), one entry per branch head.
            [JsonProperty("builds")] public Dictionary<string, long> BuildDates { get; set; } = new Dictionary<string, long>();
            [JsonProperty("f")] public long FetchedAt { get; set; }

            /// <summary>The installed buildid's publish date, or 0 if not a known branch head.</summary>
            public long DateForBuild(string buildId)
                => (!string.IsNullOrEmpty(buildId) && BuildDates.TryGetValue(buildId, out var t)) ? t : 0;

            public bool IsLatest(string buildId)
                => !string.IsNullOrEmpty(buildId) && buildId == PublicBuildId;
        }

        /// <summary>
        /// Version label for a release post: the real build date when the installed
        /// buildid is the latest public build, else "Out of date". Never uses file
        /// times. Always carries the build id.
        /// </summary>
        public static async Task<string> GetBuildVersionLabelAsync(string appId, string buildId)
        {
            var info = await GetInfoAsync(appId);
            if (info == null || info.PublicTimeUpdated <= 0)
                return $"Unknown [Build {buildId}]";
            if (info.IsLatest(buildId))
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(info.PublicTimeUpdated).UtcDateTime;
                return $"{dt:MMM dd, yyyy - HH:mm:ss} UTC [Build {buildId}]";
            }
            return $"Out of date [Build {buildId}]";
        }

        public static async Task<AppBuildInfo> GetInfoAsync(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId) || !long.TryParse(appId, out _))
                return null;

            if (MemCache.TryGetValue(appId, out var cached))
                return cached;

            long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            EnsureFileCacheLoaded();
            lock (FileLock)
            {
                if (_fileCache.TryGetValue(appId, out var entry) && entry.PublicTimeUpdated > 0 &&
                    (nowUnix - entry.FetchedAt) < TtlSeconds)
                {
                    MemCache[appId] = entry;
                    return entry;
                }
            }

            await Gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (MemCache.TryGetValue(appId, out cached))
                    return cached;

                string json = await Http.GetStringAsync($"https://api.steamcmd.net/v1/info/{appId}").ConfigureAwait(false);
                var branches = JObject.Parse(json)["data"]?[appId]?["depots"]?["branches"] as JObject;
                if (branches == null) return null;

                var info = new AppBuildInfo { FetchedAt = nowUnix };
                foreach (var branch in branches.Properties())
                {
                    string buildId = branch.Value?["buildid"]?.ToString();
                    long time = 0;
                    // timebuildupdated = when the actual BUILD CONTENT changed.
                    // timeupdated just tracks branch-pointer touches (metadata
                    // re-points), which can be recent for a months-old build, so it
                    // is only a fallback.
                    long.TryParse(branch.Value?["timebuildupdated"]?.ToString(), out time);
                    if (time <= 0) long.TryParse(branch.Value?["timeupdated"]?.ToString(), out time);

                    if (!string.IsNullOrEmpty(buildId) && time > 0)
                        info.BuildDates[buildId] = time;

                    if (branch.Name == "public")
                    {
                        info.PublicBuildId = buildId;
                        info.PublicTimeUpdated = time;
                    }
                }

                if (info.PublicTimeUpdated <= 0) return null;

                MemCache[appId] = info;
                lock (FileLock)
                {
                    _fileCache[appId] = info;
                    SaveFileCache();
                }
                return info;
            }
            catch
            {
                return null; // offline / API hiccup -> caller keeps the local fallback
            }
            finally
            {
                Gate.Release();
            }
        }

        private static void EnsureFileCacheLoaded()
        {
            if (_fileCache != null) return;
            lock (FileLock)
            {
                if (_fileCache != null) return;
                try
                {
                    if (File.Exists(CachePath))
                        _fileCache = JsonConvert.DeserializeObject<Dictionary<string, AppBuildInfo>>(File.ReadAllText(CachePath));
                }
                catch { }
                _fileCache ??= new Dictionary<string, AppBuildInfo>();
            }
        }

        private static void SaveFileCache()
        {
            try
            {
                string dir = Path.GetDirectoryName(CachePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(CachePath, JsonConvert.SerializeObject(_fileCache));
            }
            catch { }
        }
    }
}
