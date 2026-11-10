using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Services;

namespace VicServoPriceWatch.Benchmarks;

/// <summary>
/// Measures the existing postcode loading path, including JSON deserialization.
/// </summary>
[MemoryDiagnoser]
public class PostcodeDeserializationBenchmark
{
    /// <summary>
    /// Executes and consumes the real postcode download/cache and JSON deserialization path.
    /// </summary>
    /// <returns>The number of loaded postcode records.</returns>
    [Benchmark]
    public async Task<int> LoadPostcodeDataAsync()
    {
        IEnumerable<PostcodeData>? records = await PostcodeService.DownloadPostcodeDataAsync();
        return records?.Count() ?? 0;
    }
}