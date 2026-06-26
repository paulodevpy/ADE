using ADE.Core.Audit;
using ADE.Core.Cases;
using ADE.Core.Metadata;
using ADE.Core.Models;

namespace ADE.Core.Collection;

public sealed class CollectionFactory
{
    public CollectionCreationResult Create(
        string rootFolder,
        string unitName,
        string boNumber,
        string procedureNumber,
        string officerName,
        string notes)
    {
        var manager = new CaseManager();

        manager.CreateCase(rootFolder);

        var metadata = new CaseMetadata
        {
            CaseId = manager.CaseId,
            UnitName = unitName,
            BoNumber = boNumber,
            ProcedureNumber = procedureNumber,
            OfficerName = officerName,
            Notes = notes,
            ComputerName = Environment.MachineName,
            WindowsUser = Environment.UserName,
            CreatedAtUtc = DateTime.UtcNow
        };

        new CaseMetadataManager()
            .Save(
                metadata,
                Path.Combine(
                    manager.RootPath,
                    "metadata"));

        var audit = new AuditChain();

        audit.AddEvent(
            "CASE_CREATED",
            manager.CaseId);

        audit.Save(
            Path.Combine(
                manager.RootPath,
                "logs",
                "audit.jsonl"));

        var collection = new CollectionRecord
        {
            CaseId = manager.CaseId,
            GeneratedAtUtc = DateTime.UtcNow,
            CollectionState = CollectionState.InProgress,
            EvidenceFiles = new List<EvidenceFile>()
        };

        return new CollectionCreationResult
        {
            Collection = collection,
            CaseId = manager.CaseId,
            RootPath = manager.RootPath
        };
    }
}