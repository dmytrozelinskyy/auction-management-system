using System.ComponentModel.DataAnnotations;

namespace MAS_Implementation.Models;

public class ConsignmentAgreement
{
    private string _title = string.Empty;
    private double _commissionRate;
    
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
                throw new ArgumentException("Title cannot be empty");
            if (value.Length > 150)
                throw new ArgumentException("Title cannot exceed 150 characters");
            _title = value;
        }
    }
    
    public double CommissionRate
    {
        get => _commissionRate;
        set
        {
            if (value < 0)
                throw new ArgumentException("Commission rate cannot be less than zero");
            _commissionRate = value;
        }
    }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }
    
    public int ConsignorId { get; private set; }
    public Consignor Consignor { get; private set; } = null!;
    public List<Lot> Lots { get; private set; } = new();
    
    public ConsignmentAgreement(double commisionRate, DateTime validFrom, DateTime validTo)
    {
        if (validFrom == default) throw new ArgumentException("ValidFrom must be specified.");
        if (validTo == default) throw new ArgumentException("ValidTo must be specified.");
        if (validTo < validFrom) throw new ArgumentException("ValidTo cannot be before ValidFrom.");
        
        CommissionRate = commisionRate;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
    
    protected ConsignmentAgreement() { }
    
    public void AssignConsignor(Consignor consignor)
    {
        ArgumentNullException.ThrowIfNull(consignor);
        if (Consignor == consignor) return;
        
        Consignor = consignor;
        ConsignorId = consignor.Id;
        consignor.AddConsignmentAgreement(this);
    }

    public void AddLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        if (Lots.Contains(lot)) return;
        
        Lots.Add(lot);
        lot.AssignConsignmentAgreement(this);
    }
    
    public bool IsActive() => ValidTo >= DateTime.Now;

    public override string ToString() => $"{Title} - {Consignor?.Name} ({CommissionRate}%)";
}