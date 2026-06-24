using System.Text.Json;

namespace ADE.Core.Manifest;

public static class ManifestWriter
{
    public static void Save(
        ManifestModel manifest,
        string filePath)
    {
        string json =
            JsonSerializer.Serialize(
                manifest,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            filePath,
            json);
    }
}