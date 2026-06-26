using ADE.Core.Models;

namespace ADE.Core.Collection;

public sealed class CollectionCreationResult
{
    public required CollectionRecord Collection { get; init; }

    public required string CaseId { get; init; }

    public required string RootPath { get; init; }
}