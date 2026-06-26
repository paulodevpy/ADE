using ADE.Core.Metadata;
using ADE.Core.Models;

namespace ADE.Core.Collection;

public sealed class CollectionService
{
    private readonly CollectionSessionManager _session;
    private readonly CollectionRepository _repository;

    public CollectionService(
        CollectionSessionManager session,
        CollectionRepository repository)
    {
        _session = session;
        _repository = repository;
    }

    public CollectionRecord Current =>
        _session.CurrentCollection!;

    public void SaveCollection()
    {
        if (!_session.HasCollection)
            return;

        _session.Save(_repository);
    }

    public void SaveMetadata(
        string casePath,
        CaseMetadata metadata)
    {
        new CaseMetadataManager()
            .Save(
                metadata,
                Path.Combine(
                    casePath,
                    "metadata"));
    }

    public CaseMetadata? LoadMetadata(
        string casePath)
    {
        string file =
            Path.Combine(
                casePath,
                "metadata",
                "case.json");

        if (!File.Exists(file))
            return null;

        return System.Text.Json.JsonSerializer.Deserialize<CaseMetadata>(
            File.ReadAllText(file));
    }
}