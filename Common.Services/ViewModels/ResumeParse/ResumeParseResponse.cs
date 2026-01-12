using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Services.ViewModels.ResumeParse
{
    public class ResumeParseResult
    {
        public bool Success { get; set; }
        public ResumeProfile? Profile { get; set; }
        public string? Error { get; set; }
    }

    public class ResumeProfile
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public double? TotalExperienceYears { get; set; }

        [JsonConverter(typeof(StringOrArrayConverter))]
        public List<string> Skills { get; set; } = new();

        public SoftSkills SoftSkills { get; set; } = new();

        [JsonConverter(typeof(StringOrArrayConverter))]
        public List<string> Education { get; set; } = new();

        public string? CurrentCompany { get; set; }
        public string? CurrentRole { get; set; }
        public double? CurrentSalary { get; set; }
        public double? ExpectedSalary { get; set; }
        public string? Summary { get; set; }
    }

    public class SoftSkills
    {
        public int? Communication { get; set; }
        public int? Teamwork { get; set; }
        public int? Leadership { get; set; }
    }

    public class StringOrArrayConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<string>();

            // Case 1: Array
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.String)
                        result.Add(reader.GetString()!);
                }

                return result;
            }

            // Case 2: Single string
            if (reader.TokenType == JsonTokenType.String)
            {
                result.Add(reader.GetString()!);
                return result;
            }

            // Case 3: Null
            if (reader.TokenType == JsonTokenType.Null)
            {
                return result;
            }

            throw new JsonException("Invalid format for string or array");
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in value)
            {
                writer.WriteStringValue(item);
            }

            writer.WriteEndArray();
        }
    }
}
