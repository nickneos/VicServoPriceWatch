using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using VicServoPriceWatch.Core.Services;
using Microsoft.VSDiagnostics;

namespace VicServoPriceWatch.Benchmarks;
[CPUUsageDiagnoser]
public class PostcodeIndexBenchmark
{
    private PostcodeService postcodeService = null!;
    /// <summary>
    /// Creates a fresh service so each iteration rebuilds the in-memory index.
    /// </summary>
    [IterationSetup]
    public void SetUp()
    {
        postcodeService = new PostcodeService();
    }

    /// <summary>
    /// Executes the real postcode loading and suggestion-index construction path.
    /// </summary>
    /// <returns>The number of loaded postcode records.</returns>
    [Benchmark]
    public async Task<int> LoadAndIndexPostcodeDataAsync()
    {
        return (await postcodeService.GetPostcodeDataAsync()).Count;
    }
}