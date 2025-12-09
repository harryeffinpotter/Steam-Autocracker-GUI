using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SteamAppIdIdentifier
{
    public static class ResourceExtractor
    {
        private static readonly string BinPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, "_bin");

        public static void ExtractBinFiles()
        {
            try
            {
                // Get current assembly (use typeof for .NET 8 single-file compatibility)
                var assembly = typeof(ResourceExtractor).Assembly;

                // Get all embedded resource names that start with _bin.
                var resourceNames = assembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith("_bin."))
                    .ToArray();

                if (resourceNames.Length == 0)
                {
                    // No embedded resources, assume _bin is already extracted
                    return;
                }

                // Create _bin directory if it doesn't exist
                if (!Directory.Exists(BinPath))
                {
                    Directory.CreateDirectory(BinPath);
                }

                foreach (var resourceName in resourceNames)
                {
                    // Convert resource name back to file path
                    // Format: _bin.folder.subfolder.file.ext
                    var relativePath = resourceName.Substring(5); // Remove "_bin."

                    // Handle nested directories
                    var parts = relativePath.Split('.');
                    string fileName;
                    string dirPath = "";

                    // The last two parts are usually filename.extension
                    if (parts.Length >= 2)
                    {
                        // Check if it's a file with extension
                        var possibleExtension = parts[parts.Length - 1];
                        var possibleName = parts[parts.Length - 2];

                        // Common executable and image extensions
                        if (possibleExtension == "exe" || possibleExtension == "dll" ||
                            possibleExtension == "png" || possibleExtension == "jpg" ||
                            possibleExtension == "bat" || possibleExtension == "ver" ||
                            possibleExtension == "old" || possibleExtension == "txt" ||
                            possibleExtension == "ini" || possibleExtension == "json")
                        {
                            fileName = possibleName + "." + possibleExtension;

                            // Build directory path from remaining parts
                            if (parts.Length > 2)
                            {
                                dirPath = string.Join(Path.DirectorySeparatorChar.ToString(),
                                    parts.Take(parts.Length - 2));
                            }
                        }
                        else
                        {
                            // Assume it's all directory path
                            fileName = relativePath.Replace('.', Path.DirectorySeparatorChar);
                        }
                    }
                    else
                    {
                        fileName = relativePath;
                    }

                    // Create full file path
                    var fullPath = string.IsNullOrEmpty(dirPath)
                        ? Path.Combine(BinPath, fileName)
                        : Path.Combine(BinPath, dirPath, fileName);

                    // Skip if file already exists and is not older
                    if (File.Exists(fullPath))
                    {
                        // You can add version checking here if needed
                        continue;
                    }

                    // Create directory structure if needed
                    var directory = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Extract the resource
                    using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (resourceStream != null)
                        {
                            using (var fileStream = File.Create(fullPath))
                            {
                                resourceStream.CopyTo(fileStream);
                            }

                            // Make exe files executable
                            if (fullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                File.SetAttributes(fullPath, FileAttributes.Normal);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash - maybe _bin files are already there
                System.Diagnostics.Debug.WriteLine($"Resource extraction error: {ex.Message}");
            }
        }

        public static string GetBinFilePath(string relativePath)
        {
            // Ensure resources are extracted first
            ExtractBinFiles();

            return Path.Combine(BinPath, relativePath);
        }
    }
}