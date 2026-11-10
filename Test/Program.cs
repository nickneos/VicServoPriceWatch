using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Services;

namespace Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            PostcodeService postcodeService = new PostcodeService();
            IReadOnlyList<PostcodeData> postcodeData = await postcodeService.GetPostcodeDataAsync();
            postcodeService.FindSuggestions("Derr").ToList().ForEach(s => Console.WriteLine(s.Locality));
        }
    }
}
