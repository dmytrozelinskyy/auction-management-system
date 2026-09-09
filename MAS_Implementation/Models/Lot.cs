using System.ComponentModel.DataAnnotations;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

public class Lot
{
    private string _title = string.Empty;
    private string? _description;
    private string _condition = string.Empty;
    private double _estimateLow;
    private double _estimateHigh;
    private double _reservePrice;
    private LotStatus _status;
    private string _countryOfOrigin = string.Empty;
    private string _category = string.Empty;

    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty.");
            if (value.Length > 150) 
                throw new ArgumentException("Title cannot exceed 150 characters.");
            _title = value;
        }
    }
    
    public string? Description
    {
        get => _description;
        set => _description = string.IsNullOrWhiteSpace(value) ? null : value;
    }
    
    [Required]
    [MaxLength(100)]
    public string Condition
    {
        get => _condition;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Condition cannot be empty.");
            if (value.Length > 100)
                throw new ArgumentException("Condition cannot exceed 100 characters.");
            _condition = value;
        }
    }

    public double EstimateLow
    {
        get => _estimateLow;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "EstimateLow cannot be negative.");
            _estimateLow = value;
        }
    }

    public double EstimateHigh
    {
        get => _estimateHigh;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "EstimateHigh cannot be negative.");
            _estimateHigh = value;
        }
    }

    public double ReservePrice
    {
        get => _reservePrice;
        set
        {
            if (value < 0)
                throw new ArgumentException("ReservePrice cannot be less than zero.");
            _reservePrice = value;
        }
    }

    public LotStatus Status { get; private set; } = LotStatus.Catalogued;

    [Required]
    [MaxLength(100)]
    public string CountryOfOrigin
    {
        get => _countryOfOrigin;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CountryOfOrigin cannot be null or whitespace.");
            if (value.Length > 100)
                throw new ArgumentException("CountryOfOrigin cannot exceed 100 characters.");
            _countryOfOrigin = value;
        }
    }

    [Required]
    [MaxLength(100)]
    public string Category
    {
        get => _category;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category cannot be null or whitespace.");
            _category = value;
        }
    }
    
    public int? AuctionId { get; private set; }
    public int CataloguerId { get; private set; }
    public int ConsignmentAgreementId { get; private set; }
    
    public Auction Auction { get; private set; }
    public Cataloguer Cataloguer { get; private set; } = null!;
    public ConsignmentAgreement ConsignmentAgreement { get; private set; } = null!;
    public List<VerificationTask> VerificationTasks { get; private set; } = new();
    
    public Lot(string title, string? description, string condition, double estimateLow, double estimateHigh,
        double reservePrice, string countryOfOrigin, string category)
    {
        Title = title;
        Description = description;
        Condition = condition;
        EstimateLow = estimateLow;
        EstimateHigh = estimateHigh;
        ReservePrice = reservePrice;
        CountryOfOrigin = countryOfOrigin;
        Category = category;
    }
    
    protected Lot() { }
    
    public void AssignAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (Auction == auction) return;
        Auction = auction;
        AuctionId = auction.Id; 
    }

    public void AssignCataloguer(Cataloguer cataloguer)
    {
        ArgumentNullException.ThrowIfNull(cataloguer);
        if (Cataloguer == cataloguer) return;
        
        Cataloguer = cataloguer;
        CataloguerId = cataloguer.Id;
        cataloguer.AddLot(this);
    }

    public void AssignConsignmentAgreement(ConsignmentAgreement consignmentAgreement)
    {
        ArgumentNullException.ThrowIfNull(consignmentAgreement);
        if (ConsignmentAgreement == consignmentAgreement) return;
        
        ConsignmentAgreement = consignmentAgreement;
        ConsignmentAgreementId = consignmentAgreement.Id;
        consignmentAgreement.AddLot(this);
    }
    
    public void AddVerificationTask(VerificationTask verificationTask)
    {
        ArgumentNullException.ThrowIfNull(verificationTask);
        if (VerificationTasks.Contains(verificationTask)) return;
        if (VerificationTasks.Count >= 3) return;
        
        VerificationTasks.Add(verificationTask);
        verificationTask.AssignLot(this);
    }
    
    public void EvaluateRestrictionProfile(double highValueThreshold,
        List<string> regulatedCategories, List<string> sanctionedCountries,
        string category)
    {
        if (sanctionedCountries.Contains(CountryOfOrigin, StringComparer.OrdinalIgnoreCase))
        {
            Status = LotStatus.Blocked;
            return;
        }
        
        if (regulatedCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            AddVerificationTask(new VerificationTask(RestrictionType.Regulated));
        
        if (EstimateHigh >= highValueThreshold)
            AddVerificationTask(new VerificationTask(RestrictionType.HighValue));
        
        if (!string.Equals(CountryOfOrigin, "Poland", StringComparison.OrdinalIgnoreCase))
            AddVerificationTask(new VerificationTask(RestrictionType.Imported));
    }

    public bool AreAllVerificationsComplete() =>
        VerificationTasks.All(vt => vt.Status == VerificationStatus.Complete);

    public void DetermineStatus(LotStatus newStatus)
    {
        if (Status is LotStatus.Sold or LotStatus.Passed or LotStatus.Cancelled) 
            throw new InvalidOperationException($"Lot is in a terminal state {Status} and cannot be transitioned to another status.");
        if (Status == LotStatus.Blocked && newStatus != LotStatus.Blocked)
            throw new InvalidOperationException("A blocked lot cannot be transitioned to any other status.");
        if (Status == LotStatus.OnAuction && newStatus == LotStatus.Eligible)
            throw new InvalidOperationException(
                "Use special RevertToEligible() method to transition to Eligible from OnAuction.");
        if (newStatus == LotStatus.Eligible && !AreAllVerificationsComplete())
            throw new InvalidOperationException(
                "Cannot mark lot as Eligible until not all verifications are completed.");
        Status = newStatus;
    }

    public void AssignToRound(Auction auction) // status == ELIGIBLE
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (Status != LotStatus.Eligible)
            throw new ArgumentException($"Cannot assign lot to an auction- status must be Eligible, but instead is {Status}.");
        Auction = auction;
        AuctionId = auction.Id;
        Status = LotStatus.Assigned;
        auction.AddLot(this);
    }
    
    public void Withdraw()
    {
        if (Status is LotStatus.Sold or LotStatus.Passed or LotStatus.Cancelled)
            throw new InvalidOperationException($"Cannot withdraw lot in {Status} status.");
        Status = LotStatus.Cancelled;
    }

    public void MarkOnAuction()
    {
        if (Status != LotStatus.Assigned)
            throw new InvalidOperationException($"Lot cannot be marked as OnAuction from {Status} status.");
        Status = LotStatus.OnAuction;
    }

    public void MarkSold()
    {
        if (Status != LotStatus.OnAuction)
            throw new InvalidCastException($"Lot cannot be marked as Sold from {Status} status.");
        Status = LotStatus.Sold;
    }

    public void MarkPassed()
    {
        if (Status != LotStatus.OnAuction)
            throw new InvalidOperationException($"Lot cannot be marked as Passed from {Status} status.");
        Status = LotStatus.Passed;
    }

    public void RevertToEligible()
    {
        if (Status is not (LotStatus.Assigned or LotStatus.OnAuction))
            throw new InvalidOperationException($"Lot cannot be reverted to Eligible from {Status} status.");
        Status = LotStatus.Eligible;
        Auction = null;
        AuctionId = null;
    }

    public override string ToString() => $"{Title} [{Status}] - {Category}";
}