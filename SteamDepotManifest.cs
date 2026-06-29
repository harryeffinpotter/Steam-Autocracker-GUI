using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace APPID
{
    /// <summary>
    /// Reads the real "build date" of an INSTALLED game from its depot manifests.
    /// Each depot .manifest in steamapps\depotcache embeds a creation_time = when
    /// Valve built that depot content. Unlike the appmanifest LastUpdated (which is
    /// just the local install time) this is correct even for an old build freshly
    /// installed, and needs no network. Binary format per SteamKit2 DepotManifest.
    /// </summary>
    public static class SteamDepotManifest
    {
        private const uint MetadataMagic = 0x1F4812BE;
        private const uint EndOfManifestMagic = 0x32C415AB;

        /// <summary>
        /// Returns the newest depot creation_time (unix seconds) across the game's
        /// installed depots, or 0 if it can't be determined.
        /// </summary>
        public static long GetInstalledBuildTimeUtc(string acfContent, string steamappsPath)
        {
            try
            {
                string depotcache = Path.Combine(steamappsPath, "depotcache");
                if (!Directory.Exists(depotcache)) return 0;

                long newest = 0;
                foreach (var (depotId, manifestId) in ParseInstalledDepots(acfContent))
                {
                    string file = Path.Combine(depotcache, $"{depotId}_{manifestId}.manifest");
                    if (!File.Exists(file)) continue;

                    long ct = ReadCreationTime(file);
                    if (ct > newest) newest = ct;
                }
                return newest;
            }
            catch
            {
                return 0;
            }
        }

        private static List<(string depotId, string manifestId)> ParseInstalledDepots(string acfContent)
        {
            var depots = new List<(string, string)>();
            var section = Regex.Match(acfContent, @"""InstalledDepots"".*?\{(.*?)\n\t\}", RegexOptions.Singleline);
            if (section.Success)
            {
                var body = section.Groups[1].Value;
                var ids = Regex.Matches(body, @"""(\d+)""\s*\{");
                var mans = Regex.Matches(body, @"""manifest""\s+""(\d+)""");
                for (int i = 0; i < Math.Min(ids.Count, mans.Count); i++)
                    depots.Add((ids[i].Groups[1].Value, mans[i].Groups[1].Value));
            }
            return depots;
        }

        // Walks the manifest's magic-prefixed chunks; the metadata chunk is a
        // ContentManifestMetadata protobuf whose field 3 is creation_time (uint32).
        private static long ReadCreationTime(string path)
        {
            byte[] b = File.ReadAllBytes(path);
            int pos = 0;
            while (pos + 4 <= b.Length)
            {
                uint magic = BitConverter.ToUInt32(b, pos); pos += 4;
                if (magic == EndOfManifestMagic) break;
                if (pos + 4 > b.Length) break;
                int chunkLen = (int)BitConverter.ToUInt32(b, pos); pos += 4;
                if (chunkLen < 0 || pos + chunkLen > b.Length) break;

                if (magic == MetadataMagic)
                {
                    long ct = ReadProtoCreationTime(b, pos, chunkLen);
                    if (ct > 0) return ct;
                }
                pos += chunkLen;
            }
            return 0;
        }

        // Minimal protobuf scan for field 3 (creation_time, varint).
        private static long ReadProtoCreationTime(byte[] b, int start, int len)
        {
            int p = start, end = start + len;
            while (p < end)
            {
                ulong tag = ReadVarint(b, ref p, end);
                int field = (int)(tag >> 3);
                int wire = (int)(tag & 7);

                if (field == 3 && wire == 0)
                    return (long)ReadVarint(b, ref p, end);

                switch (wire)
                {
                    case 0: ReadVarint(b, ref p, end); break;       // varint
                    case 1: p += 8; break;                          // 64-bit
                    case 2: p += (int)ReadVarint(b, ref p, end); break; // length-delimited
                    case 5: p += 4; break;                          // 32-bit
                    default: return 0;
                }
            }
            return 0;
        }

        private static ulong ReadVarint(byte[] b, ref int p, int end)
        {
            ulong result = 0;
            int shift = 0;
            while (p < end && shift < 64)
            {
                byte by = b[p++];
                result |= (ulong)(by & 0x7F) << shift;
                if ((by & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }
    }
}
