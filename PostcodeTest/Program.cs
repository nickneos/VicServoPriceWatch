using VicServoPriceWatch.Core.Services;
using VicServoPriceWatch.Core.Models;
using System.Text.Json;

namespace PostcodeTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IEnumerable<PostcodeData>? postcodeDataList = await PostcodeService.DownloadPostcodeDataAsync();

            if (postcodeDataList == null)
            {
                Console.WriteLine("Failed to download postcode data.");
                return;
            }

            foreach (PostcodeData postcodeData in postcodeDataList)
            {
                if (int.TryParse(postcodeData.Postcode, out int postcodeNumber) && postcodeNumber >= 3000 && postcodeNumber < 3100)
                {
                    Console.WriteLine($"Postcode: {postcodeData.Postcode}, Locality: {postcodeData.Locality}, State: {postcodeData.State}");
                }
            }
        }
    }
}
