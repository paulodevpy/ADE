using ADE.Core.Models;

namespace ADE.Core.Collection;

public sealed class CollectionSessionManager
{
    public CollectionRecord? CurrentCollection { get; private set; }

    public IReadOnlyList<EvidenceFile> Evidences =>
        CurrentCollection?.EvidenceFiles ??
        new List<EvidenceFile>();
    public string? CaseId { get; private set; }

    public string? RootPath { get; private set; }
    
    public CollectionState State =>
        CurrentCollection?.CollectionState ?? CollectionState.None;

    public bool HasCollection =>
        CurrentCollection is not null;

    public bool CanAddEvidence =>
        State == CollectionState.InProgress;

    public bool CanDeleteEvidence =>
        State == CollectionState.InProgress;

    public bool CanFinalizeCollection =>
        State == CollectionState.InProgress;

    public bool CanGenerateReport =>
        State == CollectionState.Finalized;

    public bool IsReadOnly =>
        State == CollectionState.ReportGenerated;

    public void Start(
        CollectionRecord record,
        string rootPath)
    {
        record.CollectionState =
            CollectionState.InProgress;

        record.GeneratedAtUtc =
            DateTime.UtcNow;

        CurrentCollection =
            record;

        CaseId =
            record.CaseId;

        RootPath =
            rootPath;
    }

    public void Load(
        CollectionRecord record,
        CollectionState state)
    {
        record.CollectionState = state;

        CurrentCollection = record;

        CaseId = record.CaseId;
    }

    public void FinalizeCollection()
    {
        if (CurrentCollection is null)
            throw new InvalidOperationException(
                "Nenhuma coleta iniciada.");

        CurrentCollection.CollectionState =
            CollectionState.Finalized;

        CurrentCollection.FinalizedAtUtc =
            DateTime.UtcNow;
    }

    public void ReportGenerated()
    {
        if (CurrentCollection is null)
            throw new InvalidOperationException(
                "Nenhuma coleta iniciada.");

        CurrentCollection.CollectionState =
            CollectionState.ReportGenerated;

        CurrentCollection.ReportGeneratedAtUtc =
            DateTime.UtcNow;
    }

    public bool HasActiveCollection()
    {
        return CurrentCollection is not null
            && State != CollectionState.ReportGenerated;
    }

    public bool CanStartNewCollection()
    {
        return CurrentCollection is null
            || State == CollectionState.ReportGenerated;
    }

    public void Reset()
    {
        CurrentCollection = null;

        CaseId = null;

        RootPath = null;
    }

    public void Save(CollectionRepository repository)
    {
        if (CurrentCollection is null)
            return;

        if (string.IsNullOrWhiteSpace(RootPath))
            return;

        repository.Save(
            RootPath,
            CurrentCollection);
    }

    public void Load(
        CollectionRepository repository,
        string casePath)
    {
        var collection =
            repository.Load(casePath);

        if (collection is null)
            return;

        CurrentCollection = collection;

        RootPath = casePath;

        CaseId = collection.CaseId;
    }

    public bool HasReportGenerated()
    {
        return State == CollectionState.ReportGenerated;
    }
    public void Close()
    {
        CurrentCollection = null;

        CaseId = null;

        RootPath = null;
    }
    public bool Resume(
        CollectionRepository repository,
        string caseFolder)
    {
        var collection =
            repository.Load(caseFolder);

        if (collection == null)
            return false;

        CurrentCollection = collection;

        RootPath = caseFolder;

        CaseId = collection.CaseId;

        return true;
    }
}