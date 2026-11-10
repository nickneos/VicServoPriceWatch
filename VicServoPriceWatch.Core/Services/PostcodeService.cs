using System.IO;
using System.IO.Compression;
using System.Text.Json;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Utils;

namespace VicServoPriceWatch.Core.Services
{
    /// <summary>
    /// Loads, caches, and searches postcode data.
    /// </summary>
    public class PostcodeService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string postcodeDataUrl = "https://github.com/matthewproctor/australianpostcodes/raw/refs/heads/master/australian_postcodes_xml.zip";
        private static readonly string zipFileName = "australian_postcodes_xml.zip";
        private static readonly string jsonFileName = "australian_postcodes.json";
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromDays(30);
        private readonly SemaphoreSlim memoryCacheLock = new(1, 1);
        private IReadOnlyList<PostcodeData>? cachedPostcodeData;
        private Dictionary<string, List<PostcodeData>>? suggestionIndex;

        /// <summary>
        /// Gets the postcode data from the in-memory cache, loading it once when necessary.
        /// </summary>
        /// <returns>The cached postcode records.</returns>
        public async Task<IReadOnlyList<PostcodeData>> GetPostcodeDataAsync()
        {
            if (cachedPostcodeData is not null)
            {
                return cachedPostcodeData;
            }

            await memoryCacheLock.WaitAsync().ConfigureAwait(false);
            try
            {

                if (cachedPostcodeData is not null)
                {
                    return cachedPostcodeData;
                }

                // download the postcode data and extract the JSON file from the zip archive
                string jsonFile = await DownloadPostcodeDataAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Failed to download postcode data.");

                // Read the extracted JSON file and deserialize it into a list of PostcodeData objects
                await using FileStream jsonFileStream = File.OpenRead(jsonFile);
                IEnumerable<PostcodeData>? downloadedData = await JsonSerializer.DeserializeAsync(jsonFileStream, PostcodeJsonSerializerContext.Default.ListPostcodeData).ConfigureAwait(false);
                //return FilterPostcodeData(postcodeData, "VIC");

                //IEnumerable<PostcodeData>? downloadedData = await DownloadPostcodeDataAsync().ConfigureAwait(false);
                cachedPostcodeData = downloadedData?
                    .Where(record => !string.IsNullOrWhiteSpace(record.Locality)
                        && !string.IsNullOrWhiteSpace(record.Postcode))
                    .ToArray()
                    ?? Array.Empty<PostcodeData>();
                suggestionIndex = BuildSuggestionIndex(cachedPostcodeData);
                return cachedPostcodeData;
            }
            finally
            {
                memoryCacheLock.Release();
            }
        }

        private static Dictionary<string, List<PostcodeData>> BuildSuggestionIndex(
            IEnumerable<PostcodeData> postcodeData)
        {
            var index = new Dictionary<string, List<PostcodeData>>(StringComparer.OrdinalIgnoreCase);
            var uniqueRecords = new Dictionary<string, PostcodeData>(StringComparer.OrdinalIgnoreCase);

            foreach (PostcodeData record in postcodeData)
            {
                string displayKey = string.Concat(record.Locality, '\u001F', record.Postcode);
                uniqueRecords.TryAdd(displayKey, record);
            }

            foreach (PostcodeData record in uniqueRecords.Values)
            {
                AddPrefixes(index, record.Locality, record);
                AddPrefixes(index, record.Postcode, record);
            }

            return index;
        }

        private static void AddPrefixes(
            Dictionary<string, List<PostcodeData>> index,
            string value,
            PostcodeData record)
        {
            string normalizedValue = value.Trim().ToUpperInvariant();
            if (normalizedValue.Length < 2)
            {
                return;
            }

            string prefix = normalizedValue[..2];
            if (!index.TryGetValue(prefix, out List<PostcodeData>? records))
            {
                records = new List<PostcodeData>();
                index[prefix] = records;
            }

            records.Add(record);
        }

        /// <summary>
        /// Finds postcode records whose locality or postcode starts with the supplied query.
        /// </summary>
        /// <param name="query">The locality or postcode prefix to find.</param>
        /// <param name="maximumResults">The maximum number of records to return.</param>
        /// <returns>Matching postcode records, ordered by locality and postcode.</returns>
        public IReadOnlyList<PostcodeData> FindSuggestions(string? query, int maximumResults = 8)
        {
            if (string.IsNullOrWhiteSpace(query) || maximumResults <= 0 || suggestionIndex is null)
            {
                return Array.Empty<PostcodeData>();
            }

            string normalizedQuery = query.Trim().ToUpperInvariant();
            if (normalizedQuery.Length < 2
                || !suggestionIndex.TryGetValue(normalizedQuery[..2], out List<PostcodeData>? matches))
            {
                return Array.Empty<PostcodeData>();
            }

            return matches
            .Where(record => record.Locality.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || record.Postcode.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(record => record.Locality, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.Postcode, StringComparer.OrdinalIgnoreCase)
                .Take(maximumResults)
                .ToArray();
        }

        public static async Task<string?> DownloadPostcodeDataAsync()
        {
            string? cachedFile = await TryReadFreshCacheAsync();
            if (cachedFile is not null)
            {
                return cachedFile;
            }

            // download the zipped data from the URL
            string zipFilePath = Path.Combine(AppDataPaths.DirectoryPath, zipFileName);
            var zipFile = await client.GetAsync(postcodeDataUrl).ConfigureAwait(false);

            // save the zip file to the local directory
            using (var fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await zipFile.Content.CopyToAsync(fileStream).ConfigureAwait(false);
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

                        return destinationPath;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Reads the cached file if it exists and is fresh enough to use. Returns null if the cache is missing or stale.
        /// </summary>
        /// <returns>Path to the cached file, or null if the cache is missing or stale.</returns>
        private static async Task<string?> TryReadFreshCacheAsync()
        {
            string cacheFilePath = Path.Combine(AppDataPaths.DirectoryPath, jsonFileName);

            if (!File.Exists(cacheFilePath) || DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFilePath) > cacheLifetime)
            {
                return null;
            }

            return cacheFilePath;
        }

        private static IEnumerable<PostcodeData> FilterPostcodeData(IEnumerable<PostcodeData>? postcodeData, string state)
        {
            return postcodeData?.Where(record => record.State.Equals(state, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<PostcodeData>();
        }
    }
}
