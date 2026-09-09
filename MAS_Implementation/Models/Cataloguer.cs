using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

[Table("Cataloguers")]
public class Cataloguer : StaffMember
{
    private string _specialization = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Specialization
    {
        get => _specialization;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Specialization cannot be null or empty.");
            if (value.Length > 100)
                throw new ArgumentException("Specialization cannot be longer than 100 characters.");
            _specialization = value;
        }
    }

    public List<Consignor> Consignors { get; private set; } = new();
    public List<Lot> Lots { get; private set; } = new();

    public void AddConsignor(Consignor consignor)
    {
        ArgumentNullException.ThrowIfNull(consignor);
        if (Consignors.Contains(consignor)) return;
        
        Consignors.Add(consignor);
        consignor.AssignCataloguer(this);
    }

    public void AddLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        if (Lots.Contains(lot)) return;
        
        Lots.Add(lot);
        lot.AssignCataloguer(this);
    }

    public Cataloguer(string name, DateTime dateOfBirth, ContactInfo contactInformation, string specialization)
        : base(name, dateOfBirth, contactInformation)
    {
        Specialization = specialization;
    }
    
    protected Cataloguer() { }

    public Consignor RegisterConsignor(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(email)) 
            throw new ArgumentException("Email cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or whitespace.");
        
        return new Consignor(name, email, phoneNumber);
    }

    public ConsignmentAgreement RecordConsignment(Consignor consignor, double commissionRate, DateTime validFrom, DateTime validTo)
    {
        ArgumentNullException.ThrowIfNull(consignor);

        if (commissionRate is <= 0 or > 100)
            throw new ArgumentException("Commission rate must be between 0 and 100.");
        if (validFrom >= validTo)
            throw new ArgumentException("Valid from must be before valid to.");

        var agreement = new ConsignmentAgreement(commissionRate, validFrom, validTo);
        agreement.AssignConsignor(consignor);
        return agreement;
    }

    public Lot CatalogLot(string title, string condition, double estimateLow, double estimateHigh, double reservePrice, string countryOfOrigin, string category, ConsignmentAgreement agreement, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(agreement);
        
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or whitespace.");
        
        var lot = new Lot(title, description, condition, estimateLow,
            estimateHigh, reservePrice, countryOfOrigin, category);
        
        lot.AssignCataloguer(this);
        lot.AssignConsignmentAgreement(agreement);
        
        lot.EvaluateRestrictionProfile(
            AmsConfig.HighValueThreshold,
            AmsConfig.RegulatedCategories,
            AmsConfig.SanctionedCountries,
            category);
        
        if (lot.Status != LotStatus.Blocked)
            lot.DetermineStatus(lot.VerificationTasks.Count == 0 ? LotStatus.Eligible : LotStatus.PendingVerification);

        return lot;
    }

    public void WithdrawLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        lot.Withdraw();
    }

    public override string GetRole() => "Cataloguer";

    public override string ToString() => base.ToString() + $"\nSpecialization: {Specialization}";
}