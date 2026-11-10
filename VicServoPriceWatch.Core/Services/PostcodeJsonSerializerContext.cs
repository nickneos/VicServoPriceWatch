using System.Text.Json.Serialization;
using VicServoPriceWatch.Core.Models;

namespace VicServoPriceWatch.Core.Services;

/// <summary>
/// Provides source-generated JSON metadata for postcode records.
/// </summary>
[JsonSerializable(typeof(List<PostcodeData>))]
internal partial class PostcodeJsonSerializerContext : JsonSerializerContext
{
}
