using ADE.Core.Models;

namespace ADE.Core.Collection;

public sealed class CollectionEvidenceManager
{
    private readonly CollectionSessionManager _session;

    public CollectionEvidenceManager(
        CollectionSessionManager session)
    {
        _session = session;
    }

    public IReadOnlyList<EvidenceFile> All()
    {
        return _session.CurrentCollection?.EvidenceFiles
            ?? new List<EvidenceFile>();
    }

    public IReadOnlyList<EvidenceFile> Active()
    {
        return All()
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.CollectedAtUtc)
            .ToList();
    }

    public IReadOnlyList<EvidenceFile> Deleted()
    {
        return All()
            .Where(e => e.IsDeleted)
            .OrderBy(e => e.DeletedAtUtc)
            .ToList();
    }

    public bool Add(EvidenceFile evidence)
    {
        if (!_session.CanAddEvidence)
            throw new InvalidOperationException(
                "A coleta não permite novas evidências.");

        if (Contains(evidence))
            return false;

        _session.CurrentCollection!
            .EvidenceFiles
            .Add(evidence);

        return true;
    }

    public void Delete(EvidenceFile evidence)
    {
        if (!_session.CanDeleteEvidence)
            throw new InvalidOperationException(
                "A coleta já foi finalizada.");

        evidence.IsDeleted = true;
        evidence.DeletedAtUtc = DateTime.UtcNow;
    }

    public bool Contains(EvidenceFile evidence)
    {
        return All().Any(existing =>

            existing.Sha256.Equals(
                evidence.Sha256,
                StringComparison.OrdinalIgnoreCase)

            &&

            existing.CollectionMethod ==
                evidence.CollectionMethod

            &&

            existing.SourceType ==
                evidence.SourceType

            &&

            existing.SourceDescription.Equals(
                evidence.SourceDescription,
                StringComparison.OrdinalIgnoreCase));
    }

    public int Count()
    {
        return Active().Count;
    }

    public void Clear()
    {
        if (_session.CurrentCollection is null)
            return;

        _session.CurrentCollection
            .EvidenceFiles
            .Clear();
    }

    public void RemoveDeleted()
    {
        if (_session.CurrentCollection is null)
            return;

        _session.CurrentCollection
            .EvidenceFiles
            .RemoveAll(e => e.IsDeleted);
    }

    public IEnumerable<EvidenceFile> ReportFiles()
    {
        return Active();
    }
}