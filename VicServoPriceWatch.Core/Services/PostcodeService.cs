using System.IO;
using System.IO.Compression;
using System.Text.Json;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Utils;

namespace VicServoPriceWatch.Core.Services
{
    public class PostcodeService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string postcodeDataUrl = "https://github.com/matthewproctor/australianpostcodes/raw/refs/heads/master/australian_postcodes_xml.zip";
        private static readonly string zipFileName = "australian_postcodes_xml.zip";
        private static readonly string jsonFileName = "australian_postcodes.json";
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromDays(30);

        public static async Task<IEnumerable<PostcodeData>?> DownloadPostcodeDataAsync()
        {
            IEnumerable<PostcodeData>? cachedResult = await TryReadFreshCacheAsync();
            if (cachedResult is not null)
            {
                return cachedResult;
            }

            // download the zipped data from the URL
            string zipFilePath = Path.Combine(AppDataPaths.DirectoryPath, zipFileName);
            var zipFile = await client.GetAsync(postcodeDataUrl);

            // save the zip file to the local directory
            using (var fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await zipFile.Content.CopyToAsync(fileStream);
            }

            //Extract the JSON file from the zip archive
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.Equals(jsonFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.Combine(AppDataPaths.DirectoryPath, entry.FullName);

                        // Ensure subdirectories inside the zip are created on disk
                        Directory.CreateDirectory(AppDataPaths.DirectoryPath);

                        // Extract the individual file
                        entry.ExtractToFile(destinationPath, overwrite: true);

                        // Modify the last write time to now to indicate freshness
                        File.SetLastWriteTimeUtc(destinationPath, DateTime.UtcNow);

                        // delete the zip file after extraction
                        File.Delete(zipFilePath);

                        // Read the extracted JSON file and deserialize it into a list of PostcodeData objects
                        string jsonString = File.ReadAllText(destinationPath);
                        return FilterPostcodeData(JsonSerializer.Deserialize<List<PostcodeData>>(jsonString), "VIC");
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Reads the cached file if it exists and is fresh enough to use. Returns null if the cache is missing or stale.
        /// </summary>
        /// <returns>Contents of the cached file as a string, or null if the cache is missing or stale.</returns>
        private static async Task<IEnumerable<PostcodeData>?> TryReadFreshCacheAsync()
        {
            string cacheFilePath = Path.Combine(AppDataPaths.DirectoryPath, jsonFileName);

            if (!File.Exists(cacheFilePath) || DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFilePath) > cacheLifetime)
            {
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(cacheFilePath);
                return FilterPostcodeData(JsonSerializer.Deserialize<List<PostcodeData>>(jsonString), "VIC");
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<PostcodeData> FilterPostcodeData(IEnumerable<PostcodeData>? postcodeData, string state)
        {
            return postcodeData?.Where(record => record.State.Equals(state, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<PostcodeData>();
        }
    }
}
