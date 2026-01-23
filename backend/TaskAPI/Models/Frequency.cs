using System.Text.Json.Serialization;

namespace TaskAPI.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Frequency
{
    Daily,
    Weekly,
    Monthly
}
