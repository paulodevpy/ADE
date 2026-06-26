using System.Text.Json;
using ADE.Core.Models;

namespace ADE.Core.Collection;

public sealed class CollectionRepository
{
    private const string FileName = "collection.json";

    public bool Exists(string casePath)
    {
        return File.Exists(GetFile(casePath));
    }

    public CollectionRecord? Load(string casePath)
    {
        string file = GetFile(casePath);

        if (!File.Exists(file))
            return null;

        string json = File.ReadAllText(file);

        return JsonSerializer.Deserialize<CollectionRecord>(json);
    }

    public void Save(
        string casePath,
        CollectionRecord collection)
    {
        Directory.CreateDirectory(
            Path.Combine(casePath, "metadata"));

        string json =
            JsonSerializer.Serialize(
                collection,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            GetFile(casePath),
            json);
    }

    public void Delete(string casePath)
    {
        string file = GetFile(casePath);

        if (File.Exists(file))
            File.Delete(file);
    }

    public IEnumerable<string> FindCollections(string rootFolder)
    {
        if (!Directory.Exists(rootFolder))
            yield break;

        foreach (var directory in Directory.GetDirectories(rootFolder))
        {
            string collectionFile =
                Path.Combine(
                    directory,
                    "metadata",
                    FileName);

            if (File.Exists(collectionFile))
                yield return directory;
        }
    }

    public CollectionRecord? FindOpenCollection(string rootFolder)
    {
        foreach (var folder in FindCollections(rootFolder))
        {
            var collection = Load(folder);

            if (collection is null)
                continue;

            if (collection.CollectionState ==
                CollectionState.ReportGenerated)
                continue;

            return collection;
        }

        return null;
    }

    private static string GetFile(string casePath)
    {
        return Path.Combine(
            casePath,
            "metadata",
            FileName);
    }
}