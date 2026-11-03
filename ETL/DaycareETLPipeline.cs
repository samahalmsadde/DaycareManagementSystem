
using System.Text.Json;

namespace DaycareAPI2026.ETL
{
    public class DaycareETLPipeline
    {
        public static async Task<string> RunAsync(string rootPath)
        {
            // EXTRACT: Read source data
            string source = Path.Combine(
                rootPath, "App_Data", "daycare.json");

            if (!File.Exists(source))
                throw new FileNotFoundException(
                    "Source daycare.json was not found.", source);

            string json = await File.ReadAllTextAsync(source);
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            JsonElement children = default;

            if (root.ValueKind == JsonValueKind.Array)
            {
                children = root;
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    if (property.Name.Equals(
                        "Children",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        children = property.Value;
                        break;
                    }
                }
            }

            if (children.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException(
                    "Children array was not found.");

            // TRANSFORM: Create aggregated statistics
            var records = children.EnumerateArray().ToList();

            string GetField(JsonElement item, string field)
            {
                if (item.ValueKind != JsonValueKind.Object)
                    return "Unknown";

                foreach (JsonProperty property in item.EnumerateObject())
                {
                    if (property.Name.Equals(
                        field,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return property.Value.ToString();
                    }
                }

                return "Unknown";
            }

            var genderSummary = records
                .GroupBy(child => GetField(child, "Gender"))
                .ToDictionary(g => g.Key, g => g.Count());

            var groupSummary = records
                .GroupBy(child => GetField(child, "Group"))
                .ToDictionary(g => g.Key, g => g.Count());

            var report = new
            {
                Pipeline = "Daycare ETL",
                Status = "Success",
                ExecutedAt = DateTime.UtcNow,
                Source = "App_Data/daycare.json",
                TotalChildren = records.Count,
                ByGender = genderSummary,
                ByGroup = groupSummary
            };

            // LOAD: Save transformed results
            string outputFolder = Path.Combine(
                rootPath, "ETL_Output");

            Directory.CreateDirectory(outputFolder);

            string destination = Path.Combine(
                outputFolder, "daycare_summary.json");

            string result = JsonSerializer.Serialize(
                report,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            await File.WriteAllTextAsync(destination, result);

            return destination;
        }
    }
}
