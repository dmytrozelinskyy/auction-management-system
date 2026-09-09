using System.ComponentModel.DataAnnotations;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

public class VerificationTask
{
    [Key]
    public int Id { get; set; }
    public RestrictionType Type { get; private set; }

    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;

    public List<Document> Documents { get; private set; } = new();

    public int LotId { get; private set; }
    public Lot Lot { get; private set; } = null!;

    public VerificationTask(RestrictionType type)
    {
        Type = type;
    }
    
    protected VerificationTask() { }

    public void AssignLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        if (Lot == lot) return;
        
        lot.AddVerificationTask(this);
        Lot = lot;
        LotId = lot.Id;
    }

    public void AddDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (Documents.Contains(document)) return;
        
        Documents.Add(document);
    }

    public bool Validate()
    {
        return Type switch
        {
            RestrictionType.Regulated => ValidateRegulated(),
            RestrictionType.HighValue => ValidateHighValue(),
            RestrictionType.Imported => ValidateImported(),
            _ => throw new InvalidOperationException("Unknown restriction type.")
        };
    }

    private bool ValidateRegulated()
    {
        var permit = Documents.FirstOrDefault(d =>
            d.Type == DocumentType.RegulatoryLicense || d.Type == DocumentType.RegulatoryPermit);
        if (permit == null)
            throw new InvalidOperationException("Regulated: A regulatory permit or license document is required.");
        return true;
    }

    private bool ValidateHighValue()
    {
        var appraisal = Documents.FirstOrDefault(d => d.Type == DocumentType.ThirdPartyAppraisal);
        
        if (appraisal == null)
            throw new InvalidOperationException("High value: A third-party appraisal document is required.");
        return true;
    }

    private bool ValidateImported()
    {
        var customsDocument = Documents.FirstOrDefault(d => d.Type == DocumentType.ProvenanceChainDocument);
        
        if (customsDocument == null)
            throw new InvalidOperationException("Imported: A provenance chain document is required.");
        return true;
    }
    public string? CustomCleranceNumber { get; set; }
    public void MarkComplete()
    {
        Validate();
        Status = VerificationStatus.Complete;
    }
    public void MarkPending()
    {
        if (Status == VerificationStatus.Complete)
            throw new InvalidOperationException("Verification task cannot be reverted to Pending from Complete.");
        Status = VerificationStatus.Pending;
    }

    public override string ToString() => $"Verification Task [{Type}] - {Status}";
}