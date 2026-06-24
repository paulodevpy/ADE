using System.Text.Json;
using ADE.Core.Models;

namespace ADE.Core.Metadata;

public class CaseMetadataManager
{
    public string Save(
        CaseMetadata metadata,
        string metadataFolder)
    {
        string filePath =
            Path.Combine(
                metadataFolder,
                "case.json");

        string json =
            JsonSerializer.Serialize(
                metadata,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            filePath,
            json);

        return filePath;
    }
}